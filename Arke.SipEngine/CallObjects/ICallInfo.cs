using System;
using System.Collections.Generic;
using Arke.SipEngine.Bridging;
using Arke.SipEngine.Device;

namespace Arke.SipEngine.CallObjects
{
    public interface ICallInfo
    {
        Guid CallId { get; }
        bool CallStarted { get; set; }
        string Destination { get; set; }
        DeviceConfig Device { get; set; }
        string Endpoint { get; set; }
        string FileName { get; set; }
        string InputData { get; set; }
        string LanguageCode { get; set; }
        string PortId { get; set; }
        bool ProcessOutgoingQueue { get; set; }
        string TerminationCode { get; set; }
        DateTimeOffset TimeOffHook { get; set; }
        int InputRetryCount { get; set; }
        bool CallCanBeAbandoned { get; set; }
        int AttemptCount { get; set; }
        Queue<string> OutboundEndpoint { get; set; }

        void AddStepToIncomingQueue(int stepId);
        void AddStepToOutgoingQueue(int stepId);
        void CreateOutgoingLine(object sipLine);
        string GetBridgeId();
        string GetIncomingLineId();
        int GetNextIncomingStep();
        int GetNextOutgoingStep();
        string GetOutgoingLineId();
        string GetPortId();
        int GetStepsOnIncomingQueue();
        int GetStepsOnOutgoingQueue();
        void SetBridge(IBridge bridge);
    }
}