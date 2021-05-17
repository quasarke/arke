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

        public HoldStepSettings GetSettings()
        {
            return _settings;
        }

        public async Task DoStepAsync(Step step, ICall call)
        {
            _call = call;
            _call.OnWorkflowStep += OnWorkflowStep;
            _call.Logger.Information("Hold processor start {@Call}", call.CallState);
            _settings = (HoldStepSettings)step.NodeData.Properties;
            _timeoutStep = step.GetStepFromConnector("TimeoutStep");
            
            _holdingBridge = await call.CreateBridgeAsync(BridgeType.Holding);
            _call.CallState.SetBridge(_holdingBridge);
            try
            {
                await _call.SipBridgingApi.AddLineToBridgeAsync(_call.CallState.GetIncomingLineId(), _call.CallState.GetBridgeId());
            }
            catch (Exception e)
            {
                _call.Logger.Error(e, "Failed to place line on hold. Probably hungup. {@Call}", call.CallState);
                await _call.FireStateChange(Trigger.FailedCallFlow);
                await _call.ProcessCallLogicAsync();
                return;
            }
            
            if (_settings.HoldMusic)
                await _call.SipBridgingApi.PlayMusicOnHoldToBridgeAsync(_call.CallState.GetBridgeId());
            else
            {
                _call.CallState.HoldPrompt = _settings.WaitPrompt;
                _call.SipApiClient.OnPromptPlaybackFinishedAsyncEvent += AriClient_OnPlaybackFinishedAsyncEvent;
                _currentPlaybackId = await _call.SipBridgingApi.PlayPromptToBridgeAsync(_call.CallState.GetBridgeId(), _settings.WaitPrompt, call.CallState.LanguageCode);
            }
            call.SipApiClient.OnLineHangupAsyncEvent += SipApiClientOnOnLineHangupEvent;

            await _call.FireStateChange(Trigger.PlaceOnHold);
            _call.Logger.Information("Hold processor fired {@Call}", call.CallState);
            
#pragma warning disable 4014 // we want to keep going without waiting.
            _call.ProcessCallLogicAsync();
#pragma warning restore 4014

            await Task.Delay(_settings.HoldTimeoutInSeconds * 1000).ContinueWith(HoldTimeoutAction());
        }

        private Action<Task> HoldTimeoutAction()
        {
            return (status) =>
            {
                if (_call.CallState.Bridge.Id == _holdingBridge.Id)
                    HoldTimeoutCallBack();
                else
                {
                    // we're not on hold anymore, so we can just remove our events and continue
                    _call.SipApiClient.OnPromptPlaybackFinishedAsyncEvent -= AriClient_OnPlaybackFinishedAsyncEvent;
                    _call.OnWorkflowStep -= OnWorkflowStep;
                    _call.SipApiClient.OnLineHangupAsyncEvent -= SipApiClientOnOnLineHangupEvent;
                }
            };
        }

        private void HoldTimeoutCallBack()
        {
            // we somehow got stuck in the holding bridge. Lets move on.
            // we're not on hold anymore, so we can just remove our events and continue
            _call.Logger.Warning("On Hold Timeout for call {@Call}", _call.CallState);
            _call.SipApiClient.OnPromptPlaybackFinishedAsyncEvent -= AriClient_OnPlaybackFinishedAsyncEvent;
            _call.OnWorkflowStep -= OnWorkflowStep;
            _call.SipApiClient.OnLineHangupAsyncEvent -= SipApiClientOnOnLineHangupEvent;
            _call.AddStepToProcessQueue(_timeoutStep);
            _call.FireStateChange(Trigger.NextCallFlowStep);
        }

        private async Task SipApiClientOnOnLineHangupEvent(ISipApiClient sender, LineHangupEvent e)
        {
            if (e.LineId != _call.CallState.GetOutgoingLineId()) return;
            if (!_call.CallState.TalkTimeStart.HasValue)
            {
                _call.CallState.CallCanBeAbandoned = true;
                await _call.FireStateChange(Trigger.FinishCall);
            }
            else
            {
                // we're not on hold anymore, so we can just remove our events and continue
                _call.SipApiClient.OnPromptPlaybackFinishedAsyncEvent -= AriClient_OnPlaybackFinishedAsyncEvent;
                _call.OnWorkflowStep -= OnWorkflowStep;
                _call.SipApiClient.OnLineHangupAsyncEvent -= SipApiClientOnOnLineHangupEvent;
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
                TriggerWorkflowStepEvent(onWorkflowStepEvent);
            }

            if (_settings.PromptChanges.ContainsKey(onWorkflowStepEvent.StepId.ToString()))
            {
                _call.CallState.HoldPrompt = _settings.PromptChanges[onWorkflowStepEvent.StepId.ToString()];
            }

            if (call.CallState.Bridge.Id != _holdingBridge.Id)
            {
                RemoveEventSubscriptions();
            }
        }

        private void RemoveEventSubscriptions()
        {
// we're not on hold anymore, so we can just remove our events and continue
            _call.SipApiClient.OnPromptPlaybackFinishedAsyncEvent -= AriClient_OnPlaybackFinishedAsyncEvent;
            _call.OnWorkflowStep -= OnWorkflowStep;
            _call.SipApiClient.OnLineHangupAsyncEvent -= SipApiClientOnOnLineHangupEvent;
        }

        private void TriggerWorkflowStepEvent(OnWorkflowStepEvent onWorkflowStepEvent)
        {
            _call.CallState.AddStepToIncomingQueue(
                int.Parse(_settings.Triggers[onWorkflowStepEvent.StepId.ToString()]));
            _call.FireStateChange(Trigger.NextCallFlowStep);
            _call.OnWorkflowStep -= OnWorkflowStep;
        }

        private async Task AriClient_OnPlaybackFinishedAsyncEvent(ISipApiClient sipApiClient, PromptPlaybackFinishedEvent e)
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
            
            try
            {
                _currentPlaybackId = await _call.SipBridgingApi.PlayPromptToBridgeAsync(_call.CallState.Bridge.Id, _call.CallState.HoldPrompt, _call.CallState.LanguageCode);
            }
            catch (Exception exception)
            {
                _call.Logger.Warning(exception, "Think the call has probably hungup");
                RemoveEventSubscriptions();
            }
            
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
