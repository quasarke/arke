using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.Steps.ParallelStep
{
    public class ParallelStartProcessor : IStepProcessor
    {
        public string Name => "ParallelStart";
        private const string IncomingNextStep = "IncomingNextStep";
        private const string OutgoingNextStep = "OutgoingNextStep";

        public Task DoStepAsync(Step step, ICall call)
        {
            call.CallState.AddStepToIncomingQueue(step.GetStepFromConnector(IncomingNextStep));
            call.CallState.AddStepToOutgoingQueue(step.GetStepFromConnector(OutgoingNextStep));
            call.CallState.ProcessOutgoingQueue = true;
            call.FireStateChange(Trigger.NextCallFlowStep);
            return Task.CompletedTask;
        }
    }
}
