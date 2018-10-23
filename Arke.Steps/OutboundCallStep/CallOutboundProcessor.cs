using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.SipEngine.Bridging;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.Steps.OutboundCallStep
{
    public class CallOutboundProcessor : IStepProcessor
    {
        private const string NextStep = "NextStep";
        private const string FailedStep = "Error";
        private const string NoAnswer = "NoAnswer";
        private ICall _call;
        private Step _step;
        public string Name => "CallOutbound";

        public async Task DoStep(Step step, ICall call)
        {
            _step = step;
            _call = call;
            await CallOutbound(_call.CallState.Destination).ConfigureAwait(false);
        }

        public void GoToNextStep()
        {
            var next = _step.GetStepFromConnector(NextStep);
            _call.Logger.Info("Outbound connected, go to next step " + next);
            _call.CallState.AddStepToOutgoingQueue(next);
            _call.CallState.ProcessOutgoingQueue = true;
            _call.FireStateChange(Trigger.NextCallFlowStep);
        }

        public async Task CallOutbound(string dialString)
        {
            _call.Logger.Info("Outbound start " + dialString);
            try
            {
                _call.CallState.CreateOutgoingLine(
                await _call.SipLineApi.CreateOutboundCall(dialString, (_step.NodeData.Properties as CallOutboundSettings)?.OutboundEndpointName).ConfigureAwait(false));
                var outgoingLineId = _call.CallState.GetOutgoingLineId();
                var currentCallState = await _call.SipLineApi.GetLineState(outgoingLineId).ConfigureAwait(false);
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
                    await Task.Delay(100).ConfigureAwait(false);
                    currentCallState = await _call.SipLineApi.GetLineState(outgoingLineId).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _call.Logger.Error(ex, "Outbound call can't get through ");
                _call.AddStepToProcessQueue(_step.GetStepFromConnector(FailedStep));
                _call.FireStateChange(Trigger.NextCallFlowStep);
                return;
            }
            //await _call.SipLineApi.AnswerLine(_call.CallState.GetOutgoingLineId()).ConfigureAwait(false);
            GoToNextStep();
        }
    }
}
