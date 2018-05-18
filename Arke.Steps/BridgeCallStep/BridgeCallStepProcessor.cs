﻿using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.Bridging;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.Steps.BridgeCallStep
{
    public class BridgeCallStepProcessor : IStepProcessor
    {
        private const string NextStep = "NextStep";
        private IBridge _callBridge;

        public string Name => "BridgeCallStep";

        public string GetBridgeId()
        {
            return _callBridge?.Id;
        }

        public async Task DoStep(Step step, ICall call)
        {
            await call.StopHoldingBridge();
            _callBridge = await call.CreateBridge(BridgeType.NoDTMF);

            call.CallState.SetBridge(_callBridge);

            await call.AddLineToBridge(call.CallState.GetIncomingLineId(), _callBridge.Id);
            await call.AddLineToBridge(call.CallState.GetOutgoingLineId(), _callBridge.Id);

            call.AddStepToProcessQueue(step.GetStepFromConnector(NextStep));
            call.FireStateChange(Trigger.NextCallFlowStep);
        }
    }
}
