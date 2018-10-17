using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.SipEngine.Api;
using Arke.SipEngine.Bridging;
using Arke.SipEngine.Device;
using Arke.SipEngine.Events;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;
using Arke.SipEngine.Prompts;
using NLog;

namespace Arke.SipEngine.CallObjects
{
    public interface ICall
    {
        Guid CallId { get; set; }
        ICallInfo CallState { get; set; }
        Dictionary<string,string> LogData { get; }
        Logger Logger { get; }
        NodeProperties StepSettings { get; set; }
        IPromptPlayer PromptPlayer { get; }
        IPhoneInputHandler InputProcessor { get; }
        ILanguageSelectionPromptPlayer LanguageSelectionPromptPlayer { get; }
        IRecordingManager RecordingManager { get; }
        ISipApiClient SipApiClient { get; }
        ISipBridgingApi SipBridgingApi { get; }
        ISipLineApi SipLineApi { get; }


        Task RunCallScript();
        void Hangup();
        void ProcessCallLogic();
        Task StartCallRecording();
        Task StopCallRecording();
        void FireStateChange(Trigger trigger);
        void AddStepToProcessQueue(int stepNumber);
        State GetCurrentState();
        event Action<ICall, OnWorkflowStepEvent> OnWorkflowStep;
        void SetCallLanguage(LanguageData languageData);
        Task<IBridge> CreateBridge(BridgeType bridgeType);
        Task StopHoldingBridge();
        Task AddLineToBridge(string lineId, string bridgeId);
        Task StartRecordingOnLine(string lineId, string direction);
        Task StartRecordingOnBridge(string bridgeId);
        void SetWorkflow(Workflow deviceWorkflow);
    }
}
