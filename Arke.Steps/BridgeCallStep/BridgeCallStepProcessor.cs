using System.Threading.Tasks;
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
            if (!string.IsNullOrEmpty(call.CallState.GetBridgeId()))
                await call.StopHoldingBridge().ConfigureAwait(false);
            _callBridge = await call.CreateBridge(BridgeType.NoDTMF).ConfigureAwait(false);

            call.CallState.SetBridge(_callBridge);

            await call.AddLineToBridge(call.CallState.GetIncomingLineId(), _callBridge.Id).ConfigureAwait(false);
            await call.AddLineToBridge(call.CallState.GetOutgoingLineId(), _callBridge.Id).ConfigureAwait(false);

            call.AddStepToProcessQueue(step.GetStepFromConnector(NextStep));
            call.FireStateChange(Trigger.NextCallFlowStep);
        }
    }
}
