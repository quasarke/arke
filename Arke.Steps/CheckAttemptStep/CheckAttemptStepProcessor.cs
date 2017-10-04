using System.Threading.Tasks;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.Steps.CheckAttemptStep
{
    public class CheckAttemptStepProcessor : IStepProcessor
    {
        public string Name => "CheckAttemptStep";
        public Task DoStep(ISettings settings, ICall call)
        {
            var stepSettings = settings as CheckAttemptStepSettings;

            if (call.CallState.AttemptCount >= stepSettings.MaxAttempts)
            {
                call.AddStepToProcessQueue(stepSettings.MaxAttemptsStep);
            }
            else
            {
                call.CallState.AttemptCount++;
                call.AddStepToProcessQueue(stepSettings.NextStep);
            }
            call.FireStateChange(Trigger.NextCallFlowStep);
            return Task.CompletedTask;
        }
    }
}
