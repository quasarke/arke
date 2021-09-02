using System.Collections.Generic;
using System.Timers;
using System.Linq;
using System.Threading.Tasks;
using Arke.DSL.Extensions;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
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
            _settings = new PhoneInputHandlerSettings();
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
        public Direction Direction { get; set; }

        public void ChangeInputSettings(PhoneInputHandlerSettings settings)
        {
            if (_settings != null)
            {
                _settings = settings;
                MaxDigitTimeoutInSeconds = _settings.MaxDigitTimeoutInSeconds;
                SetValueAs = _settings.SetValueAs;
                SetTimerInterval();
                NumberOfDigitsToWaitForNextStep = _settings.NumberOfDigitsToWaitForNextStep;
                TerminationDigit = _settings.TerminationDigit;
                Direction = _settings.Direction;
            }
            else
            {
                NumberOfDigitsToWaitForNextStep = -1;
                TerminationDigit = "D";
                Direction = Direction.Both;
                _settings.SetValueAs = "";
                _settings.SetValueAsDestination = false;
                _settings.Options = new Dictionary<string, int>();
            }
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

        public async void AriClient_OnChannelDtmfReceivedEvent(ISipApiClient sipApiClient, DtmfReceivedEvent dtmfReceivedEvent)
        {
            DigitTimeoutTimer.Stop();
            if (_call.GetCurrentState() == State.LanguagePrompts)
                return;
            if (!ShouldProcessDigit(dtmfReceivedEvent))
                return;
            _call.Logger.Debug($"OnChannel Dtmf Received Event {dtmfReceivedEvent.LineId}");

            LogDtmfValue(dtmfReceivedEvent);

            CaptureDigitIfInValidState(dtmfReceivedEvent);

            if (_call.GetCurrentState() == State.PlayingInterruptiblePrompt)
                await _promptPlayer.StopPromptAsync();

            await ProcessDigitsReceived();
        }

        private bool ShouldProcessDigit(DtmfReceivedEvent dtmfReceivedEvent)
        {
            switch (Direction)
            {
                case Direction.Incoming:
                    return (dtmfReceivedEvent.LineId == _call.CallState.GetIncomingLineId());
                case Direction.Outgoing:
                    return (dtmfReceivedEvent.LineId == _call.CallState.GetOutgoingLineId());
                case Direction.Both:
                    return (dtmfReceivedEvent.LineId == _call.CallState.GetIncomingLineId()
                        || dtmfReceivedEvent.LineId == _call.CallState.GetOutgoingLineId());
                default:
                    return false;
            }
        }

        private async void DigitTimeoutEvent(object sender, ElapsedEventArgs e)
        {
            await FailCaptureWhenTimeoutExpires();
        }

        public async Task FailCaptureWhenTimeoutExpires()
        {
            if (_call.GetCurrentState() != State.CapturingInput)
                return;

            if (_call.CallState.InputRetryCount > _settings.MaxRetryCount && _settings.MaxRetryCount > 0)
            {
                ResetInputRetryCount();
                AddStepToProperQueue(_settings.MaxAttemptsReachedStep);
            }
            else
            {
                AddStepToProperQueue(_settings.NoAction);
            }

            await _call.FireStateChange(Trigger.FailedInputCapture);
        }

        private void AddStepToProperQueue(int step)
        {
            switch (Direction)
            {
                case Direction.Both:
                    _call.CallState.AddStepToIncomingQueue(step);
                    _call.CallState.AddStepToOutgoingQueue(step);
                    break;
                case Direction.Incoming:
                    _call.CallState.AddStepToIncomingQueue(step);
                    break;
                case Arke.DSL.Step.Direction.Outgoing:
                    _call.CallState.AddStepToOutgoingQueue(step);
                    break;
                default:
                    _call.CallState.AddStepToIncomingQueue(step);
                    break;
            }
        }

        private void ResetInputRetryCount()
        {
            _call.CallState.InputRetryCount = 0;
        }

        public void CaptureDigitIfInValidState(DtmfReceivedEvent e)
        {
            if (_call.GetCurrentState() == State.PlayingInterruptiblePrompt ||
                _call.GetCurrentState() == State.CallFlow ||
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
            if (DigitsReceived.Length < NumberOfDigitsToWaitForNextStep)
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
                    else if (!string.IsNullOrEmpty(_settings.SetValueAs))
                        DynamicState.SetProperty(_call.CallState, _settings.SetValueAs, DigitsReceived);

                    AddStepToProperQueue(_settings.NextStep);
                    _call.CallState.InputData = DigitsReceived.Substring(0, DigitsReceived.Length - 1);
                    ResetInputRetryCount();
                    await _call.FireStateChange(Trigger.InputReceived);
                    return;
                }
            }
            else if (_settings.SetValueAsDestination)
            {
                _call.CallState.Destination = DigitsReceived.Substring(0, DigitsReceived.Length);
                AddStepToProperQueue(_settings.NextStep);
                _call.CallState.InputData = DigitsReceived.Substring(0, DigitsReceived.Length);
                ResetInputRetryCount();
                await _call.FireStateChange(Trigger.InputReceived);
                return;
            }
            else if (!string.IsNullOrEmpty(_settings.SetValueAs))
            {
                DynamicState.SetProperty(_call.CallState, _settings.SetValueAs, DigitsReceived);
                _call.CallState.InputData = DigitsReceived;
                ResetInputRetryCount();
                AddStepToProperQueue(_settings.NextStep);
                await _call.FireStateChange(Trigger.InputReceived);
                return;
            }

            if (NumberOfDigitsToWaitForNextStep == 0)
            {
                DigitTimeoutTimer.Start();
                return;
            }

            var validStep = false;
            foreach (var option in _settings.Options.Where(option => option.Key == DigitsReceived.Substring(0, NumberOfDigitsToWaitForNextStep)))
            {
                ResetInputRetryCount();
                
                AddStepToProperQueue(option.Value);

                validStep = true;
            }

            if (!validStep)
            {
                if (_call.CallState.InputRetryCount > _settings.MaxRetryCount && _settings.MaxRetryCount > 0)
                {
                    ResetInputRetryCount();

                    AddStepToProperQueue(_settings.MaxAttemptsReachedStep);
                }
                else
                {
                    AddStepToProperQueue(_settings.Invalid);
                }
            }
            await _call.FireStateChange(Trigger.InputReceived);
        }
    }
}