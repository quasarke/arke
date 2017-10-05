using System.Linq;
using System.Threading;
using Arke.SipEngine.Api;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Events;
using Arke.SipEngine.FSM;

namespace Arke.IVR.Input
{
    public class AsteriskPhoneInputHandler : IPhoneInputHandler
    {
        private readonly ICall _call;
        private readonly IPromptPlayer _promptPlayer;
        private PhoneInputHandlerSettings _settings;
        private readonly AutoResetEvent _resetEvent;
        public AsteriskPhoneInputHandler(ICall call, IPromptPlayer promptPlayer)
        {
            _call = call;
            _promptPlayer = promptPlayer;
            DigitsReceived = "";
            _resetEvent = new AutoResetEvent(false);
            DigitTimeoutTimer = new Timer(DigitTimeoutEvent,_resetEvent, int.MaxValue, int.MaxValue);
            MaxDigitTimeoutInSeconds = 0;
            NumberOfDigitsToWaitForNextStep = 0;
            TerminationDigit = "#";
        }
        
        public string DigitsReceived { get; set; }
        public Timer DigitTimeoutTimer { get; set; }
        public int MaxDigitTimeoutInSeconds { get; set; }
        public int NumberOfDigitsToWaitForNextStep { get; set; }
        public string TerminationDigit { get; set; }

        public void ChangeInputSettings(PhoneInputHandlerSettings settings)
        {
            _settings = settings;
            MaxDigitTimeoutInSeconds = _settings.MaxDigitTimeoutInSeconds;
            NumberOfDigitsToWaitForNextStep = _settings.NumberOfDigitsToWaitForNextStep;
            TerminationDigit = _settings.TerminationDigit;
        }

        public void StartUserInput(bool reset)
        {
            if (reset)
            {
                DigitsReceived = "";
            }
            _call.CallState.InputRetryCount++;
            if (MaxDigitTimeoutInSeconds > 0)
                DigitTimeoutTimer = new Timer(DigitTimeoutEvent, _resetEvent, MaxDigitTimeoutInSeconds * 1000, int.MaxValue);
        }

        public void AriClient_OnChannelDtmfReceivedEvent(ISipApiClient sipApiClient, DtmfReceivedEvent dtmfReceivedEvent)
        {
            DigitTimeoutTimer.Change(int.MaxValue, int.MaxValue);
            _call.Logger.Debug($"OnChannel Dtmf Received Event {dtmfReceivedEvent.LineId}");
            if (_call.GetCurrentState() == State.LanguagePrompts)
                return;
            if (dtmfReceivedEvent.LineId != _call.CallState.GetIncomingLineId())
                return;

            LogDtmfValue(dtmfReceivedEvent);

            CaptureDigitIfInValidState(dtmfReceivedEvent);

            if (_call.GetCurrentState() == State.PlayingInterruptiblePrompt)
                _promptPlayer.StopPrompt();

            ProcessDigitsReceived();
        }

        private void DigitTimeoutEvent(object sender)
        {
            FailCaptureWhenTimeoutExpires();
        }

        public void FailCaptureWhenTimeoutExpires()
        {
            if (_call.GetCurrentState() != State.CapturingInput)
                return;

            if (_call.CallState.InputRetryCount > _settings.MaxRetryCount && _settings.MaxRetryCount > 0)
            {
                ResetInputRetryCount();
                _call.AddStepToProcessQueue(_settings.MaxAttemptsReachedStep);
            }
            else
                _call.CallState.AddStepToIncomingQueue(_settings.NoAction);
            _call.FireStateChange(Trigger.FailedInputCapture);
        }

        private void ResetInputRetryCount()
        {
            _call.CallState.InputRetryCount = 0;
        }

        public void CaptureDigitIfInValidState(DtmfReceivedEvent e)
        {
            if (_call.GetCurrentState() == State.PlayingInterruptiblePrompt ||
                _call.GetCurrentState() == State.CapturingInput)
                DigitsReceived += e.Digit;
        }

        private void LogDtmfValue(DtmfReceivedEvent e)
        {
            if (_call.LogData.ContainsKey("LastDTMF"))
                _call.LogData["LastDTMF"] = e.Digit;
            else
                _call.LogData.Add("LastDTMF", e.Digit);

            _call.Logger.Debug("DTMF Received", _call.LogData);
        }

        public void ProcessDigitsReceived()
        {
            if (_call.GetCurrentState() != State.CapturingInput ||
                DigitsReceived.Length < NumberOfDigitsToWaitForNextStep)
            {
                DigitTimeoutTimer = new Timer(DigitTimeoutEvent, _resetEvent, MaxDigitTimeoutInSeconds * 1000, int.MaxValue);
                return;
            }

            if (NumberOfDigitsToWaitForNextStep == 0
                && !DigitsReceived.EndsWith(TerminationDigit))
            {
                DigitTimeoutTimer = new Timer(DigitTimeoutEvent, _resetEvent, MaxDigitTimeoutInSeconds * 1000, int.MaxValue);
                return;
            }

            if (TerminationDigit != "")
            {
                if (DigitsReceived.EndsWith(TerminationDigit))
                {
                    if (_settings.SetValueAsDestination)
                        _call.CallState.Destination = DigitsReceived.Substring(0, DigitsReceived.Length - 1);

                    _call.CallState.AddStepToIncomingQueue(_settings.NextStep);
                    _call.CallState.InputData = DigitsReceived.Substring(0, DigitsReceived.Length - 1);
                    ResetInputRetryCount();
                    _call.FireStateChange(Trigger.InputReceived);
                    return;
                }
            }
            else if (_settings.SetValueAsDestination)
            {
                _call.CallState.Destination = DigitsReceived.Substring(0, DigitsReceived.Length);
                _call.CallState.AddStepToIncomingQueue(_settings.NextStep);
                _call.CallState.InputData = DigitsReceived.Substring(0, DigitsReceived.Length);
                ResetInputRetryCount();
                _call.FireStateChange(Trigger.InputReceived);
                return;
            }
            if (NumberOfDigitsToWaitForNextStep == 0)
            {
                DigitTimeoutTimer = new Timer(DigitTimeoutEvent, _resetEvent, MaxDigitTimeoutInSeconds * 1000, int.MaxValue);
                return;
            }

            var validStep = false;
            foreach (var option in _settings.Options.Where(option => option.Input == DigitsReceived))
            {
                ResetInputRetryCount();
                _call.CallState.AddStepToIncomingQueue(option.NextStep);
                validStep = true;
            }

            if (!validStep)
            {
                if (_call.CallState.InputRetryCount > _settings.MaxRetryCount && _settings.MaxRetryCount > 0)
                {
                    ResetInputRetryCount();
                    _call.AddStepToProcessQueue(_settings.MaxAttemptsReachedStep);
                }
                else
                    _call.CallState.AddStepToIncomingQueue(_settings.Invalid);
            }
            _call.FireStateChange(Trigger.InputReceived);
        }
    }
}