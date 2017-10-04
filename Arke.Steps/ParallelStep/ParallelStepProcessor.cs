using System.Threading.Tasks;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.Steps.ParallelStep
{
    public class ParallelStepProcessor : IStepProcessor
    {
        public string Name => "ParallelStep";

#pragma warning disable 1998
        public async Task DoStep(ISettings settings, ICall call)
#pragma warning restore 1998
        {
            var stepSettings = (ParallelStepSettings) settings;
            call.CallState.AddStepToIncomingQueue(stepSettings.IncomingNextStep);
            call.CallState.AddStepToOutgoingQueue(stepSettings.OutgoingNextStep);
            call.CallState.ProcessOutgoingQueue = true;
            call.FireStateChange(Trigger.NextCallFlowStep);
        }
    }
}
