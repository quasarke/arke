using System;
using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.Bridging;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.Steps.OutboundCallStep
{
    public class OutboundCallStepProcessor : IStepProcessor
    {
        private const string NextStep = "NextStep";
        private const string FailedStep = "FailedStep";
        private ICall _call;
        private Step _step;
        public string Name => "OutboundCallStep";

        public async Task DoStep(Step step, ICall call)
        {
            _step = step;
            _call = call;
            await CallOutbound(_call.CallState.Destination);
        }

        public void GoToNextStep()
        {
            _call.CallState.AddStepToOutgoingQueue(_step.GetStepFromConnector(NextStep));
            _call.FireStateChange(Trigger.NextCallFlowStep);
        }

        public async Task CallOutbound(string dialingId)
        {
            _call.Logger.Info("Outbound start " + dialingId);
            _call.CallState.CreateOutgoingLine(
                await _call.SipLineApi.CreateOutboundCall(dialingId));
            try
            {
                var outgoingLineId = _call.CallState.GetOutgoingLineId();
                var currentCallState = await _call.SipLineApi.GetLineState(outgoingLineId);
                while (currentCallState != "Up")
                {
                    await Task.Delay(100);
                    currentCallState = await _call.SipLineApi.GetLineState(outgoingLineId);
                }
            }
            catch (Exception ex)
            {
                _call.Logger.Error(ex, "Outbound call can't get through ");
                _call.AddStepToProcessQueue(_step.GetStepFromConnector(FailedStep));
                _call.FireStateChange(Trigger.NextCallFlowStep);
                return;
            }
            
            var bridge = await _call.CreateBridge(BridgeType.WithDTMF);
            await _call.SipBridgingApi.AddLineToBridge(
                _call.CallState.GetIncomingLineId(), bridge.Id);
            await _call.SipBridgingApi.AddLineToBridge(
                _call.CallState.GetOutgoingLineId(), bridge.Id);

            GoToNextStep();
            _call.Logger.Info("Outbound end, go to next step " + dialingId);
        }
    }
}
