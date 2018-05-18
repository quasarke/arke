using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.Steps.ParallelStep
{
    public class ParallelStepProcessor : IStepProcessor
    {
        public string Name => "ParallelStep";
        private const string IncomingNextStep = "IncomingNextStep";
        private const string OutgoingNextStep = "OutgoingNextStep";

        public Task DoStep(Step step, ICall call)
        {
            call.CallState.AddStepToIncomingQueue(step.GetStepFromConnector(IncomingNextStep));
            call.CallState.AddStepToOutgoingQueue(step.GetStepFromConnector(OutgoingNextStep));
            call.CallState.ProcessOutgoingQueue = true;
            call.FireStateChange(Trigger.NextCallFlowStep);
            return Task.CompletedTask;
        }
    }
}
