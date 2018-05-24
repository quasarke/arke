using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
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
        private const string FailedStep = "Error";
        private const string NoAnswer = "NoAnswer";
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
            try
            {
                _call.CallState.CreateOutgoingLine(
                await _call.SipLineApi.CreateOutboundCall($"1{dialingId}", "PJSIP/100"));
                var outgoingLineId = _call.CallState.GetOutgoingLineId();
                var currentCallState = await _call.SipLineApi.GetLineState(outgoingLineId);
                var noAnswerTimeout = new Stopwatch();
                noAnswerTimeout.Start();
                while (currentCallState != "Up")
                {
                    if (noAnswerTimeout.Elapsed.TotalSeconds > 90)
                    {
                        _call.AddStepToProcessQueue(_step.GetStepFromConnector(NoAnswer));
                        _call.FireStateChange(Trigger.NextCallFlowStep);
                        return;
                    }
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
