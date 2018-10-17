using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.Api;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Events;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;
using Arke.SipEngine.Prompts;
using Arke.DSL.Step;

// ReSharper disable FormatStringProblem - Disabled as NLog has a format extension

namespace Arke.Steps.LanguageStep
{
    public class LanguageStepProcessor : IStepProcessor, ILanguageStepProcessor
    {
        private const int MaxRetries = 3;
        private const string NextStep = "NextStep";

        private ICall _call;
        private readonly Timer _inputTimeout;
        private ILanguageSelectionPromptPlayer _promptPlayer;
        private int _currentRetryCount;
        private LanguageStepSettings _settings;
        private Step _step;

        public Dictionary<string, string> LogData = new Dictionary<string, string>();

        public string Name => "LanguageStep";

        public LanguageStepProcessor()
        {
            _inputTimeout = new Timer();
            _inputTimeout.Elapsed += InputTimeout;
            _inputTimeout.AutoReset = false;
            _currentRetryCount = 0;
        }

        public async Task DoStep(Step step, ICall call)
        {
            _promptPlayer = call.LanguageSelectionPromptPlayer;
            _promptPlayer.SetStepProcessor(this);
            _call = call;
            _call.Logger.Info("Get Language Step Start");
            _step = step;
            _settings = (LanguageStepSettings)step.NodeData.Properties;
            call.SipApiClient.OnDtmfReceivedEvent += DTMF_ReceivedEvent;
            call.FireStateChange(Trigger.PlayLanguagePrompts);
            _promptPlayer.AddPromptsToQueue(_settings.Prompts);
            await _promptPlayer.PlayNextPromptInQueue();
            _inputTimeout.Interval = _settings.MaxDigitTimeoutInSeconds * 1000;
            _call.Logger.Info("Get Language Step End");
        }

        private void InputTimeout(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _call.FireStateChange(_currentRetryCount > MaxRetries
                ? Trigger.FailedCallFlow
                : Trigger.PlayLanguagePrompts);
            _currentRetryCount++;
            DoStep(_step, _call).Start();
        }

        public void StartTimeoutTimer()
        {
            _inputTimeout.Start();
        }

        private void DTMF_ReceivedEvent(ISipApiClient sipApiClient, DtmfReceivedEvent e)
        {
            if (!LogData.ContainsKey(e.LineId))
                LogData.Add(e.LineId, _call.CallState.GetIncomingLineId());

            if (_call.CallState != null && e.LineId != _call.CallState.GetIncomingLineId())
                return;

            try
            {
                if (_call.GetCurrentState() != State.LanguageInput && _call.GetCurrentState() != State.LanguagePrompts)
                    return;
                if (_call.GetCurrentState() == State.LanguagePrompts)
                    _call.FireStateChange(Trigger.GetLanguageInput);
                GetLanguageChoiceForDigit(e.Digit);
                _inputTimeout.Stop();
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
            _call.AddStepToProcessQueue(_step.GetStepFromConnector(NextStep));
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
