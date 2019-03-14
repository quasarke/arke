using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Arke.DSL.Step;
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
        private IBridge _holdingBridge;
        private int _timeoutStep = 0;
        
        public async Task DoStep(Step step, ICall call)
        {
            _call = call;
            _call.OnWorkflowStep += OnWorkflowStep;
            _call.Logger.Information("Hold processor start {@Call}", call.CallState);
            _settings = (HoldStepSettings)step.NodeData.Properties;
            _timeoutStep = step.GetStepFromConnector("TimeoutStep");
            
            _holdingBridge = await call.CreateBridge(BridgeType.Holding);
            _call.CallState.SetBridge(_holdingBridge);
            await _call.SipBridgingApi.AddLineToBridge(_call.CallState.GetIncomingLineId(), _call.CallState.GetBridgeId());
            if (_settings.HoldMusic)
                await _call.SipBridgingApi.PlayMusicOnHoldToBridge(_call.CallState.GetBridgeId());
            else
            {
                _call.CallState.HoldPrompt = _settings.WaitPrompt;
                _call.SipApiClient.OnPromptPlaybackFinishedEvent += AriClient_OnPlaybackFinishedEvent;
                _currentPlaybackId = await _call.SipBridgingApi.PlayPromptToBridge(_call.CallState.GetBridgeId(), _settings.WaitPrompt, call.CallState.LanguageCode);
            }
            call.SipApiClient.OnLineHangupEvent += SipApiClientOnOnLineHangupEvent;

            _call.FireStateChange(Trigger.PlaceOnHold);
            _call.Logger.Information("Hold processor fired {@Call}", call.CallState);
            _call.ProcessCallLogic();

            var holdTimeout = Task.Delay(_settings.HoldTimeoutInSeconds * 1000).ContinueWith((status) =>
            {
                if (_call.CallState.Bridge.Id == _holdingBridge.Id)
                    HoldTimeoutCallBack();
                else
                {
                    // we're not on hold anymore, so we can just remove our events and continue
                    _call.SipApiClient.OnPromptPlaybackFinishedEvent -= AriClient_OnPlaybackFinishedEvent;
                    _call.OnWorkflowStep -= OnWorkflowStep;
                    _call.SipApiClient.OnLineHangupEvent -= SipApiClientOnOnLineHangupEvent;
                }
            });
        }

        private void HoldTimeoutCallBack()
        {
            // we somehow got stuck in the holding bridge. Lets move on.
            // we're not on hold anymore, so we can just remove our events and continue
            _call.Logger.Warning("On Hold Timeout for call {@Call}", _call.CallState);
            _call.SipApiClient.OnPromptPlaybackFinishedEvent -= AriClient_OnPlaybackFinishedEvent;
            _call.OnWorkflowStep -= OnWorkflowStep;
            _call.SipApiClient.OnLineHangupEvent -= SipApiClientOnOnLineHangupEvent;
            _call.AddStepToProcessQueue(_timeoutStep);
            _call.FireStateChange(Trigger.NextCallFlowStep);
        }

        private void SipApiClientOnOnLineHangupEvent(ISipApiClient sender, LineHangupEvent e)
        {
            if (e.LineId != _call.CallState.GetOutgoingLineId()) return;
            if (!_call.CallState.TalkTimeStart.HasValue)
            {
                _call.CallState.CallCanBeAbandoned = true;
                _call.FireStateChange(Trigger.FinishCall);
            }
            else
            {
                // we're not on hold anymore, so we can just remove our events and continue
                _call.SipApiClient.OnPromptPlaybackFinishedEvent -= AriClient_OnPlaybackFinishedEvent;
                _call.OnWorkflowStep -= OnWorkflowStep;
                _call.SipApiClient.OnLineHangupEvent -= SipApiClientOnOnLineHangupEvent;
            }
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
                    int.Parse(_settings.Triggers[onWorkflowStepEvent.StepId.ToString()]));
                _call.FireStateChange(Trigger.NextCallFlowStep);
                _call.OnWorkflowStep -= OnWorkflowStep;
            }

            if (_settings.PromptChanges.ContainsKey(onWorkflowStepEvent.StepId.ToString()))
            {
                _call.CallState.HoldPrompt = _settings.PromptChanges[onWorkflowStepEvent.StepId.ToString()];
            }

            if (call.CallState.Bridge.Id != _holdingBridge.Id)
            {
                // we're not on hold anymore, so we can just remove our events and continue
                _call.SipApiClient.OnPromptPlaybackFinishedEvent -= AriClient_OnPlaybackFinishedEvent;
                _call.OnWorkflowStep -= OnWorkflowStep;
                _call.SipApiClient.OnLineHangupEvent -= SipApiClientOnOnLineHangupEvent;
            }
        }

        private async Task AriClient_OnPlaybackFinishedEvent(ISipApiClient sipApiClient, PromptPlaybackFinishedEvent e)
        {
            _call.Logger.Information("Playback finished {PlaybackId} {@Call}", e.PlaybackId, _call.CallState);
            AddPlaybackIdToLogData(e.PlaybackId);

            if (e.PlaybackId != _currentPlaybackId)
                return;
            if (_call.GetCurrentState() == State.InCall)
            {
                return;
            }
            await Task.Delay(TimeSpan.FromSeconds(5));
            if (_call.CallState.Bridge.Id != _holdingBridge.Id) return;
            _currentPlaybackId = await _call.SipBridgingApi.PlayPromptToBridge(_call.CallState.Bridge.Id, _call.CallState.HoldPrompt, _call.CallState.LanguageCode);
            _call.Logger.Information("Playback started {currentPlaybackId}, of prompt {HoldPrompt}, to bridge {BridgeName} {@Call}", _currentPlaybackId, _call.CallState.HoldPrompt, _call.CallState.Bridge.Name, _call.CallState);
        }

        private void AddPlaybackIdToLogData(string id)
        {
            if (!LogData.ContainsKey("PlaybackId"))
                LogData.Add("PlaybackId", id);
            else
                LogData["PlaybackId"] = id;
        }
    }
}
