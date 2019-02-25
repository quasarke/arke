using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.Steps.TransferStep
{
    public class TransferProcessor : IStepProcessor
    {
        public string Name => "Transfer";
        private const string NextStep = "NextStep";

        public async Task DoStep(Step step, ICall call)
        {
            var transferSettings = step.NodeData.Properties as TransferSettings;
            await call.SipLineApi.TransferLine(
                step.NodeData.Properties.Direction == Direction.Incoming
                    ? call.CallState.GetIncomingLineId()
                    : call.CallState.GetOutgoingLineId(), transferSettings.Endpoint);
            GoToNextStep(call, step);
        }

        public void GoToNextStep(ICall call, Step step)
        {
            if (step.NodeData.Properties.Direction != Direction.Outgoing)
                call.CallState.AddStepToIncomingQueue(step.GetStepFromConnector(NextStep));
            else
                call.CallState.AddStepToOutgoingQueue(step.GetStepFromConnector(NextStep));
            call.FireStateChange(Trigger.NextCallFlowStep);
        }
    }
}
