using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.Api;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Events;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;
using Arke.SipEngine.Prompts;

// ReSharper disable FormatStringProblem - Disabled as NLog has a format extension

namespace Arke.Steps.LanguageStep
{
    public class LanguageStepProcessor : IStepProcessor, ILanguageStepProcessor
    {
        private const int MaxRetries = 3;

        private ICall _call;
        private Timer _inputTimeout;
        private ILanguageSelectionPromptPlayer _promptPlayer;

        private int _currentRetryCount;
        private LanguageStepSettings _settings;
        private AutoResetEvent _resetEvent;
        public Dictionary<string, string> LogData = new Dictionary<string, string>();

        public string Name => "LanguageStep";

        public LanguageStepProcessor()
        {
            _currentRetryCount = 0;
            _resetEvent = new AutoResetEvent(false);
        }

        public async Task DoStep(ISettings settings, ICall call)
        {
            _promptPlayer = call.LanguageSelectionPromptPlayer;
            _promptPlayer.SetStepProcessor(this);
            _call = call;
            _call.Logger.Info("Get Language Step Start");
            _settings = (LanguageStepSettings)settings;
            call.SipApiClient.OnDtmfReceivedEvent += DTMF_ReceivedEvent;
            _promptPlayer.AddPromptsToQueue(_settings.Prompts);
            await _promptPlayer.PlayNextPromptInQueue();
            _inputTimeout = new Timer(InputTimeout, _resetEvent,  _settings.MaxDigitTimeoutInSeconds * 1000, int.MaxValue);
            _call.Logger.Info("Get Language Step End");
        }

        private async void InputTimeout(object sender)
        {
            _call.FireStateChange(_currentRetryCount > MaxRetries
                ? Trigger.FailedCallFlow
                : Trigger.PlayLanguagePrompts);
            _currentRetryCount++;
            await DoStep(_settings, _call);
        }
        
        private void DTMF_ReceivedEvent(ISipApiClient sipApiClient, DtmfReceivedEvent e)
        {
            LogData.Add(e.LineId, _call.CallState.GetIncomingLineId());
            if (_call.CallState != null && e.LineId != _call.CallState.GetIncomingLineId())
                return;

            try
            {
                var currentState = _call.GetCurrentState();
                if (currentState != State.LanguageInput && 
                    currentState != State.LanguagePrompts &&
                    currentState != State.CallFlow)
                    return;
                if (_call.GetCurrentState() == State.LanguagePrompts)
                    _call.FireStateChange(Trigger.GetLanguageInput);
                GetLanguageChoiceForDigit(e.Digit);
                _inputTimeout.Change(int.MaxValue, int.MaxValue);
                _call.Logger.Info("DTMF Event", LogData);
                GoToNextStep();
            }
            catch (Exception ex)
            {
                _call.Logger.Error(ex, "DTMF Received event ", LogData);
            }
        }

        public void GoToNextStep()
        {
            _promptPlayer.HaltPromptPlayback();
            CleanupEventHooks();
            _call.AddStepToProcessQueue(_settings.NextStep);
            _call.FireStateChange(Trigger.NextCallFlowStep);
        }

        public void GetLanguageChoiceForDigit(string digitReceived)
        {
            foreach (var pair in _settings.LanguageSettings.Where(pair => pair.Key == digitReceived))
            {
                _call.SetCallLanguage(
                    new LanguageData(pair.Value));
                break;
            }
        }

        private void CleanupEventHooks()
        {
            _call.SipApiClient.OnDtmfReceivedEvent -= DTMF_ReceivedEvent;
            _promptPlayer.CleanupEventHooks();
        }
    }

}
