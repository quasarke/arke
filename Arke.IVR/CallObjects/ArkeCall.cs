using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.IVR.Bridging;
using Arke.IVR.DSL;
using Arke.IVR.Input;
using Arke.IVR.Prompts;
using Arke.SipEngine.Api;
using Arke.SipEngine.Bridging;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Device;
using Arke.SipEngine.Events;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;
using Arke.SipEngine.Prompts;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;

namespace Arke.IVR.CallObjects
{
    public class ArkeCall : ICall
    {
        private readonly ArkeBridgeFactory _arkeBridgeFactory;
        private readonly AsteriskPhoneInputHandler _asteriskPhoneInputHandler;
        private readonly IRecordingManager _asteriskRecordingManager;
        private readonly DslProcessor _dslProcessor;
        private readonly Dictionary<string, string> _logFields;
        private readonly ArkePromptPlayer _promptPlayer;
        private readonly ISipApiClient _sipApiClient;
        private readonly ISipBridgingApi _sipBridgeApi;
        private readonly ISipLineApi _sipLineApi;
        private ArkeCallState _callState;
        private CancellationToken _cancellationToken;
        private readonly CallStateMachine _callStateMachine;
        
        public ArkeCall(ISipApiClient sipApiClient, ISipLineApi sipLineApi, ISipBridgingApi sipBridgeApi,
            ISipPromptApi sipPromptApi, IRecordingManager recordingmanager, ILogger logger)
        {
            Logger = logger;
            _sipApiClient = sipApiClient;
            _sipLineApi = sipLineApi;
            _sipBridgeApi = sipBridgeApi;
            _logFields = new Dictionary<string, string>();
            _promptPlayer = new ArkePromptPlayer(this, sipPromptApi);
            _asteriskPhoneInputHandler = new AsteriskPhoneInputHandler(this, _promptPlayer);
            _asteriskRecordingManager = recordingmanager;
            _dslProcessor = new DslProcessor(this);
            _arkeBridgeFactory = new ArkeBridgeFactory(_sipBridgeApi);
            _callStateMachine = new CallStateMachine(this, _promptPlayer);
            _callStateMachine.SetupFiniteStateMachine();
            LanguageSelectionPromptPlayer = new AsteriskLanguageSelectionPromptPlayer(this, sipPromptApi, sipApiClient);
        }
        
        public Guid CallId { get; set; }
        public virtual ICallInfo CallState
        {
            get => _callState;
            set
            {
                var state = value as ArkeCallState;
                if (state != null)
                    _callState = state;
            }
        }

        public CallStateMachine CallStateMachine => _callStateMachine;
        public DslProcessor DslProcessor => _dslProcessor;
        public IPhoneInputHandler InputProcessor => _asteriskPhoneInputHandler;
        public ILanguageSelectionPromptPlayer LanguageSelectionPromptPlayer { get; private set; }
        public Dictionary<string, string> LogData => _logFields;
        public ILogger Logger { get; }
        public IPromptPlayer PromptPlayer => _promptPlayer;
        public IRecordingManager RecordingManager => _asteriskRecordingManager;
        public NodeProperties StepSettings { get; set; }
        public ISipApiClient SipApiClient => _sipApiClient;
        public ISipBridgingApi SipBridgingApi => _sipBridgeApi;
        public ISipLineApi SipLineApi => _sipLineApi;

        public async Task AddLineToBridgeAsync(string lineId, string bridgeId)
        {
            await _arkeBridgeFactory.AddLineToBridge(lineId, bridgeId);
        }

        public void AddOrUpdateStepIdToLogFields(int stepId)
        {
            if (_logFields.ContainsKey("StepId"))
                _logFields["StepId"] = stepId.ToString();
            else
                _logFields.Add("StepId", stepId.ToString());
        }

        public void AddStepToProcessQueue(int stepNumber)
        {
            _callState.AddStepToIncomingQueue(stepNumber);
        }

        public async Task Answer()
        {
            _logFields.Add("ChannelID", _callState.IncomingSipChannel.Channel.Id);
            Logger.Debug("Answering Channel");
            var answerCall = _sipLineApi.AnswerLineAsync(_callState.IncomingSipChannel.Channel.Id);
            await answerCall;
            Logger.Debug("Channel Answered");
            _callState.TimeOffHook = DateTimeOffset.Now;
            _logFields.Add("TimeDeviceOffHook", _callState.TimeOffHook.ToString("s"));
            await CallStateMachine.FireAsync(Trigger.Answered);
            Logger.Debug("New Call Answered");
        }

        private async Task AriClient_OnStasisEndEvent(ISipApiClient sipApiClient, LineHangupEvent e)
        {
            if (_callState.IncomingSipChannel?.Channel == null)
                return;
            if (e.LineId != _callState.IncomingSipChannel.Channel.Id)
                return;
            Logger.Debug("OnStasisEndEvent");
            await CallStateMachine.FireAsync(Trigger.FinishCall);
            _sipApiClient.OnDtmfReceivedEvent -= _asteriskPhoneInputHandler.AriClient_OnChannelDtmfReceivedEvent;
            _sipApiClient.OnPromptPlaybackFinishedAsyncEvent -= _promptPlayer.AriClient_OnPlaybackFinishedEvent;
            
        }

        public async Task<IBridge> CreateBridgeAsync(BridgeType bridgeType)
        {
            return await _arkeBridgeFactory.CreateBridge(bridgeType);
        }

        private async Task DisposeOfBridgeApi()
        {
            if (CallState.GetBridgeId() != null)
            {
                await _sipBridgeApi.DestroyBridgeAsync(CallState.GetBridgeId());
            }
        }

        private async Task DisposeOfCallServices()
        {
            await DisposeOfBridgeApi();
            await DisposeOfOutgoingSipLineChannel();
            await DisposeOfIncomingSipLineChannel();
        }

        private async Task DisposeOfIncomingSipLineChannel()
        {
            if (_callState.IncomingSipChannel?.Channel == null)
            {
                return;
            }
            await _sipLineApi.HangupLineAsync(_callState.IncomingSipChannel.Channel.Id);
        }
        
        private async Task DisposeOfOutgoingSipLineChannel()
        {
            if (_callState.OutgoingSipChannel?.Channel != null)
            {
                await _sipLineApi.HangupLineAsync(_callState.OutgoingSipChannel.Channel.Id);
            }
        }

        public async Task FireStateChange(Trigger trigger)
        {
            await CallStateMachine.FireAsync(trigger);
        }

        public State GetCurrentState()
        {
            return CallStateMachine.StateMachine.State;
        }

        public async Task HangupAsync()
        {
            Logger.Information("HangupAsync");

            await DisposeOfCallServices();
        }

        public event Action<ICall, OnWorkflowStepEvent> OnWorkflowStep;

        public virtual async Task ProcessCallLogicAsync()
        {
            if (_callState.GetStepsOnIncomingQueue() > 0)
            {
                Logger.Debug("Processing next step on incoming line.");
                var step = _callState.GetNextIncomingStep();
                AddOrUpdateStepIdToLogFields(step);
                Logger.Debug($"Processing Step ID {step}");
                await DslProcessor.ProcessStepAsync(step);
                OnWorkflowStep?.Invoke(this, new OnWorkflowStepEvent()
                {
                    LineId = _callState.IncomingSipChannel.Channel.Id,
                    StepId = step
                });
            }
            if (_callState.ProcessOutgoingQueue &&
                _callState.GetStepsOnOutgoingQueue() > 0)
            {
                Logger.Debug("Processing next step on outgoing line.");
                var step = _callState.GetNextOutgoingStep();
                AddOrUpdateStepIdToLogFields(step);
                await DslProcessor.ProcessStepAsync(step);
                OnWorkflowStep?.Invoke(this, new OnWorkflowStepEvent()
                {
                    LineId = _callState.OutgoingSipChannel.Channel.Id,
                    StepId = step
                });
            }
        }

        public async Task RunCallScriptAsync(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _cancellationToken.Register(async () => await HangupAsync());
            await Answer();
            SetupSuccessfulCallStartEvents();
            await StartCallFlowDslProcessor();
            SetFileNameForCall();
            await StartConnectionStep();
            await StartTheCallFlow();
        }

        public void SetCallLanguage(LanguageData languageData)
        {
            _promptPlayer.SetLanguageCode(languageData);
        }

        private async Task StartConnectionStep()
        {
            await DslProcessor.ProcessStepAsync(0);
        }

        private void SetFileNameForCall()
        {
            _callState.FileName =
                $"{ArkeCallFlowService.GetConfigValue("appSettings:AsteriskServerID")}_{_callState.Endpoint}_{_callState.TimeOffHook:yyyyMMddHHmmss}";
        }

        private void SetupSuccessfulCallStartEvents()
        {
            CallState.TerminationCode = TerminationCode.NormalCallCompletion;
            _callState.StepAttempts = 0;
            _sipApiClient.OnDtmfReceivedEvent += _asteriskPhoneInputHandler.AriClient_OnChannelDtmfReceivedEvent;
            _sipApiClient.OnPromptPlaybackFinishedAsyncEvent += _promptPlayer.AriClient_OnPlaybackFinishedEvent;
            _sipApiClient.OnLineHangupAsyncEvent += AriClient_OnStasisEndEvent;
        }

        private async Task StartCallFlowDslProcessor()
        {
            try
            {
                var jsonObject =
                    File.ReadAllText(PlatformServices.Default.Application.ApplicationBasePath +
                                     $"\\{ArkeCallFlowService.GetConfigValue("appSettings:Application")}.json");
                DslProcessor.Dsl = await Task.Factory
                    .StartNew(() => JsonConvert.DeserializeObject<CallFlowDsl>(
                        jsonObject).GetStepsFromDsl())
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Invalid CallFlow DSL");
            }
        }

        public void SetWorkflow(Workflow deviceWorkflow)
        {
            DslProcessor.Dsl = deviceWorkflow.Value as Dictionary<int, Step>;
        }

        public async Task ForceCallEndAsync()
        {
            await HangupAsync();
        }

        public async Task StartCallRecordingAsync()
        {
            if (_callState.IncomingSipChannel != null)
                await _asteriskRecordingManager.StartRecordingOnLine(_callState.IncomingSipChannel.Channel.Id, "I",
                    CallState);
            if (_callState.OutgoingSipChannel != null)
                await _asteriskRecordingManager.StartRecordingOnLine(_callState.OutgoingSipChannel.Channel.Id, "O",
                    CallState);
        }

        public async Task StartRecordingOnBridgeAsync(string bridgeId)
        {
            await RecordingManager.StartRecordingOnBridge(bridgeId, CallState);
        }

        public async Task StartRecordingOnLineAsync(string lineId, string direction)
        {
            await RecordingManager.StartRecordingOnLine(lineId, direction, CallState);
        }

        private async Task StartTheCallFlow()
        {
            await CallStateMachine.FireAsync(Trigger.StartCallFlow);
        }

        public async Task StopCallRecordingAsync()
        {
            if (_callState.IncomingSipChannel != null)
                await _asteriskRecordingManager.StopRecordingOnLine(_callState.IncomingSipChannel.Channel.Id);
            if (_callState.OutgoingSipChannel != null)
                await _asteriskRecordingManager.StopRecordingOnLine(_callState.OutgoingSipChannel.Channel.Id);
        }

        public async Task StopHoldingBridgeAsync()
        {
            await _arkeBridgeFactory.StopHoldingBridge(CallState);
        }
    }
}