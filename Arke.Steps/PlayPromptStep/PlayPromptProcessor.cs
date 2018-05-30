using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Processors;

namespace Arke.Steps.PlayPromptStep
{
    public class PlayPromptProcessor : IStepProcessor
    {
        private const string NextStep = "NextStep";
        public string Name => "PlayPrompt";

        public Task DoStep(Step step, ICall call)
        {
            var stepSettings = (PlayPromptSettings) step.NodeData.Properties;
            call.StepSettings = stepSettings;
            var nextStep = step.GetStepFromConnector(NextStep);
            call.Logger.Debug("Next step " + nextStep);
            call.PromptPlayer.DoStep(stepSettings.GetPromptPlayerSettings(step));
            call.CallState.AddStepToIncomingQueue(nextStep);
            return Task.CompletedTask;
        }
    }
}
