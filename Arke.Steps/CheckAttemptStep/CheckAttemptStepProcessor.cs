using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.Steps.CheckAttemptStep
{
    public class CheckAttemptStepProcessor : IStepProcessor
    {
        private const string MaxAttemptsStep = "MaxAttemptsStep";
        private const string NextStep = "NextStep";
        public string Name => "CheckAttemptStep";
        public Task DoStep(Step step, ICall call)
        {
            var stepSettings = step.NodeData.Properties as CheckAttemptStepSettings;

            if (call.CallState.AttemptCount >= stepSettings.MaxAttempts)
            {
                call.AddStepToProcessQueue(step.GetStepFromConnector(MaxAttemptsStep));
            }
            else
            {
                call.CallState.AttemptCount++;
                call.AddStepToProcessQueue(step.GetStepFromConnector(NextStep));
            }
            call.FireStateChange(Trigger.NextCallFlowStep);
            return Task.CompletedTask;
        }
    }
}
