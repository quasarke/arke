using System.Timers;
using System.Linq;
using System.Threading.Tasks;
using Arke.DSL.Extensions;
using Arke.DSL.Step;
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

        public AsteriskPhoneInputHandler(ICall call, IPromptPlayer promptPlayer)
        {
            _call = call;
            _promptPlayer = promptPlayer;
            DigitsReceived = "";
            DigitTimeoutTimer = new Timer();
            DigitTimeoutTimer.Elapsed += DigitTimeoutEvent;
            MaxDigitTimeoutInSeconds = 0;
            NumberOfDigitsToWaitForNextStep = 0;
            TerminationDigit = "#";
        }
        
        public string DigitsReceived { get; set; }
        public Timer DigitTimeoutTimer { get; set; }
        public int MaxDigitTimeoutInSeconds { get; set; }
        public int NumberOfDigitsToWaitForNextStep { get; set; }
        public string TerminationDigit { get; set; }
        public string SetValueAs { get; set; }

        public void ChangeInputSettings(PhoneInputHandlerSettings settings)
        {
            _settings = settings;
            MaxDigitTimeoutInSeconds = _settings.MaxDigitTimeoutInSeconds;
            SetValueAs = _settings.SetValueAs;
            SetTimerInterval();
            NumberOfDigitsToWaitForNextStep = _settings.NumberOfDigitsToWaitForNextStep;
            TerminationDigit = _settings.TerminationDigit;
        }

        private void SetTimerInterval()
        {
            if (MaxDigitTimeoutInSeconds > 0)
            {
                DigitTimeoutTimer.Interval = MaxDigitTimeoutInSeconds * 1000;
            }
        }

        public void StartUserInput(bool reset)
        {
            if (reset)
            {
                DigitsReceived = "";
            }
            SetTimerInterval();
            if (MaxDigitTimeoutInSeconds > 0)
                DigitTimeoutTimer.Start();
        }

        public void AriClient_OnChannelDtmfReceivedEvent(ISipApiClient sipApiClient, DtmfReceivedEvent dtmfReceivedEvent)
        {
            DigitTimeoutTimer.Stop();
            _call.Logger.Debug($"OnChannel Dtmf Received Event {dtmfReceivedEvent.LineId}");
            if (_call.GetCurrentState() == State.LanguagePrompts)
                return;
            if (dtmfReceivedEvent.LineId != _call.CallState.GetIncomingLineId())
                return;

            LogDtmfValue(dtmfReceivedEvent);

            CaptureDigitIfInValidState(dtmfReceivedEvent);

            if (_call.GetCurrentState() == State.PlayingInterruptiblePrompt)
                _promptPlayer.StopPromptAsync();

            ProcessDigitsReceived();
        }

        private void DigitTimeoutEvent(object sender, ElapsedEventArgs e)
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
                if (_settings.Direction != Direction.Outgoing)
                    _call.CallState.AddStepToIncomingQueue(_settings.MaxAttemptsReachedStep);
                else
                    _call.CallState.AddStepToOutgoingQueue(_settings.MaxAttemptsReachedStep);
            }
            else
            {
                if (_settings.Direction != Direction.Outgoing)
                    _call.CallState.AddStepToIncomingQueue(_settings.NoAction);
                else
                    _call.CallState.AddStepToOutgoingQueue(_settings.NoAction);
            }

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

        public async Task ProcessDigitsReceived()
        {
            if (_call.GetCurrentState() != State.CapturingInput ||
                DigitsReceived.Length < NumberOfDigitsToWaitForNextStep)
            {
                DigitTimeoutTimer.Start();
                return;
            }

            if (NumberOfDigitsToWaitForNextStep == 0
                && !DigitsReceived.EndsWith(TerminationDigit))
            {
                DigitTimeoutTimer.Start();
                return;
            }

            if (TerminationDigit != "")
            {
                if (DigitsReceived.EndsWith(TerminationDigit))
                {
                    if (_settings.SetValueAsDestination)
                        _call.CallState.Destination = DigitsReceived.Substring(0, DigitsReceived.Length - 1);

                    if (_settings.Direction != Direction.Outgoing)
                        _call.CallState.AddStepToIncomingQueue(_settings.NextStep);
                    else
                        _call.CallState.AddStepToOutgoingQueue(_settings.NextStep);

                    _call.CallState.InputData = DigitsReceived.Substring(0, DigitsReceived.Length - 1);
                    ResetInputRetryCount();
                    await _call.FireStateChange(Trigger.InputReceived);
                    return;
                }
            }
            else if (_settings.SetValueAsDestination)
            {
                _call.CallState.Destination = DigitsReceived.Substring(0, DigitsReceived.Length);
                
                if (_settings.Direction != Direction.Outgoing)
                    _call.CallState.AddStepToIncomingQueue(_settings.NextStep);
                else
                    _call.CallState.AddStepToOutgoingQueue(_settings.NextStep);

                _call.CallState.InputData = DigitsReceived.Substring(0, DigitsReceived.Length);
                ResetInputRetryCount();
                await _call.FireStateChange(Trigger.InputReceived);
                return;
            }
            else if (DigitsReceived.Length == NumberOfDigitsToWaitForNextStep
                     && !string.IsNullOrEmpty(SetValueAs))
            {
                DynamicState.SetProperty(_call.CallState, SetValueAs, DigitsReceived);
                ResetInputRetryCount();
                await _call.FireStateChange(Trigger.InputReceived);
                return;
            }

            if (NumberOfDigitsToWaitForNextStep == 0)
            {
                DigitTimeoutTimer.Start();
                return;
            }

            var validStep = false;
            foreach (var option in _settings.Options.Where(option => option.Input == DigitsReceived.Substring(0, NumberOfDigitsToWaitForNextStep)))
            {
                ResetInputRetryCount();
                
                if (_settings.Direction != Direction.Outgoing)
                    _call.CallState.AddStepToIncomingQueue(option.NextStep);
                else
                    _call.CallState.AddStepToOutgoingQueue(option.NextStep);

                validStep = true;
            }

            if (!validStep)
            {
                if (_call.CallState.InputRetryCount > _settings.MaxRetryCount && _settings.MaxRetryCount > 0)
                {
                    ResetInputRetryCount();

                    if (_settings.Direction != Direction.Outgoing)
                        _call.CallState.AddStepToIncomingQueue(_settings.MaxAttemptsReachedStep);
                    else
                        _call.CallState.AddStepToOutgoingQueue(_settings.MaxAttemptsReachedStep);
                }
                else
                {
                    if (_settings.Direction != Direction.Outgoing)
                        _call.CallState.AddStepToIncomingQueue(_settings.Invalid);
                    else
                        _call.CallState.AddStepToOutgoingQueue(_settings.Invalid);
                }
            }
            await _call.FireStateChange(Trigger.InputReceived);
        }
    }
}