using System;
using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.Bridging;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.Steps.BridgeCallStep
{
    public class BridgeCallProcessor : IStepProcessor
    {
        private const string NextStep = "NextStep";
        private IBridge _callBridge;

        public string Name => "BridgeCall";

        public string GetBridgeId()
        {
            return _callBridge?.Id;
        }

        public async Task DoStepAsync(Step step, ICall call)
        {
            if (!string.IsNullOrEmpty(call.CallState.GetBridgeId()))
                await call.StopHoldingBridgeAsync().ConfigureAwait(false);
            _callBridge = await call.CreateBridgeAsync(BridgeType.NoDTMF).ConfigureAwait(false);
            call.CallState.CalledPartyAcceptTime = DateTimeOffset.Now;
            call.CallState.SetBridge(_callBridge);
            call.InputProcessor.ChangeInputSettings(null);

            await call.AddLineToBridgeAsync(call.CallState.GetIncomingLineId(), _callBridge.Id).ConfigureAwait(false);
            //await call.SipBridgingApi.MuteLineAsync(call.CallState.GetIncomingLineId());
            await call.AddLineToBridgeAsync(call.CallState.GetOutgoingLineId(), _callBridge.Id).ConfigureAwait(false);
            //await call.SipBridgingApi.MuteLineAsync(call.CallState.GetOutgoingLineId());

            call.CallState.AddStepToIncomingQueue(step.GetStepFromConnector(NextStep));
            await call.FireStateChange(Trigger.NextCallFlowStep);
        }
    }
}
