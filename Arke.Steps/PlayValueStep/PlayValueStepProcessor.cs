using System;
using System.Threading.Tasks;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Processors;
using Arke.SipEngine.Utility;

namespace Arke.Steps.PlayValueStep
{
    public class PlayValueStepProcessor : IStepProcessor
    {
        public string Name => "PlayValueStep";
        public Task DoStep(ISettings settings, ICall call)
        {
            var stepSettings = settings as PlayValueStepSettings;
            if (stepSettings == null)
                throw new ArgumentException("PlayValueStepProcessor called with invalid Step settings");
            var numbersToPromptsConverter = new NumbersToPrompts();
            var valueToPlay = (decimal?)call.CallState.GetType().GetProperty(stepSettings.Value)?.GetValue(call.CallState);

            if (valueToPlay == null)
                throw new ArgumentException("Value specified does not exist on CallState: " + stepSettings.Value);

            var prompts = numbersToPromptsConverter.GetPromptsForValue(valueToPlay.Value);

            var promptSettings = new PlayerPromptSettings()
            {
                IsInterruptible = stepSettings.IsInterruptible,
                NextStep = stepSettings.NextStep,
                Prompts = prompts
            };

            call.PromptPlayer.DoStep(promptSettings);
            call.AddStepToProcessQueue(stepSettings.NextStep);
            return Task.CompletedTask;
        }
    }
}
