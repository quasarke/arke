using System.Threading.Tasks;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Processors;

namespace Arke.Steps.PlayPromptStep
{
    public class PlayPromptStepProcessor : IStepProcessor
    {
        public string Name => "PlayPromptStep";
        public Task DoStep(ISettings settings, ICall call)
        {
            var stepSettings = (PlayPromptStepSettings) settings;
            call.StepSettings = settings;
            var nextStep = ((PlayPromptStepSettings)call.StepSettings).NextStep;
            call.Logger.Debug("Next step " + nextStep);
            call.PromptPlayer.DoStep(stepSettings.GetPromptPlayerSettings());
            call.CallState.AddStepToIncomingQueue(nextStep);
            return Task.CompletedTask;
        }
    }
}
