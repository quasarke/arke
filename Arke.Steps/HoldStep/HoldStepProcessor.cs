using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.Api;
using Arke.SipEngine.Bridging;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Events;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

// ReSharper disable FormatStringProblem - NLog extension causing this

namespace Arke.Steps.HoldStep
{
    public class HoldStepProcessor : IStepProcessor
    {
        private ICall _call;
        private string _currentPlaybackId;
        private HoldStepSettings _settings;
        public Dictionary<string, string> LogData = new Dictionary<string, string>();
        public string Name => "HoldStep";
        
        public string HoldPrompt { get; private set; }

        public async Task DoStep(ISettings settings, ICall call)
        {
            _call = call;
            _call.OnWorkflowStep += OnWorkflowStep;
            _call.Logger.Info("Hold processor start");
            _settings = (HoldStepSettings)settings;
            var holdingBridge = await call.CreateBridge(BridgeType.Holding);
            _call.CallState.SetBridge(holdingBridge);
            await _call.SipBridgingApi.AddLineToBridge(_call.CallState.GetIncomingLineId(), _call.CallState.GetBridgeId());
            if (_settings.HoldMusic)
                await _call.SipBridgingApi.PlayMusicOnHoldToBridge(_call.CallState.GetBridgeId());
            else
            {
                HoldPrompt = _settings.WaitPrompt;
                _call.SipApiClient.OnPromptPlaybackFinishedEvent += AriClient_OnPlaybackFinishedEvent;
                _currentPlaybackId = await _call.SipBridgingApi.PlayPromptToBridge(_call.CallState.GetBridgeId(), _settings.WaitPrompt, call.CallState.LanguageCode);
            }
            _call.FireStateChange(Trigger.PlaceOnHold);
            _call.Logger.Info("Hold processor fired");
            _call.ProcessCallLogic();
        }
        
        private void OnWorkflowStep(ICall call, OnWorkflowStepEvent onWorkflowStepEvent)
        {
            if (onWorkflowStepEvent.LineId != _call.CallState.GetOutgoingLineId())
                return;

            if (_settings?.Triggers == null)
                return;

            if (_settings.Triggers.ContainsKey(onWorkflowStepEvent.StepId.ToString()))
            {
                _call.CallState.AddStepToIncomingQueue(
                    _settings.Triggers[onWorkflowStepEvent.StepId.ToString()]);
            }

            if (_settings.PromptChanges.ContainsKey(onWorkflowStepEvent.StepId.ToString()))
            {
                HoldPrompt = _settings.PromptChanges[onWorkflowStepEvent.StepId.ToString()];
            }
        }

        private async Task AriClient_OnPlaybackFinishedEvent(ISipApiClient sipApiClient, PromptPlaybackFinishedEvent e)
        {
            LogData.Add("PlaybackId", e.PlaybackId);
            if (e.PlaybackId != _currentPlaybackId)
                return;

            await Task.Delay(TimeSpan.FromSeconds(5));

            if (_call.GetCurrentState() != State.OnHold)
                return;

            _currentPlaybackId = await _call.SipBridgingApi.PlayPromptToBridge(_call.CallState.GetBridgeId(), HoldPrompt, "en");
            _call.Logger.Info("Playback finished", LogData);
        }
    }
}
