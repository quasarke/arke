using System;
using System.Threading.Tasks;
using Arke.DSL.Extensions;
using Arke.DSL.Step;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Processors;
using Arke.SipEngine.Utility;

namespace Arke.Steps.PlayValueStep
{
    public class PlayValueStepProcessor : IStepProcessor
    {
        private const string next_step = "NextStep";
        public string Name => "PlayValueStep";
        public async Task DoStepAsync(Step step, ICall call)
        {
            if (!(step.NodeData.Properties is PlayValueStepSettings stepSettings))
                throw new ArgumentException("PlayValueStepProcessor called with invalid Step settings");
            if (stepSettings.ValueType == "money")
            {
                await PlayMoneyValue(step, call, stepSettings);
            }
            else if (stepSettings.ValueType == "number")
            {
                await PlayNumberValue(step, call, stepSettings);
            }
            else if (stepSettings.ValueType == "digits")
            {
                throw new NotImplementedException("Digits not supported yet. Please request if needed.");
            }
        }

        private static async Task PlayNumberValue(Step step, ICall call, PlayValueStepSettings stepSettings)
        {
            var promptSettings = new PlayerPromptSettings()
            {
                IsInterruptible = stepSettings.IsInterruptible,
                NextStep = step.GetStepFromConnector(next_step),
                Direction = stepSettings.Direction
            };

            await call.PromptPlayer.DoStepAsync(promptSettings);

            var valueToPlay = DynamicState.GetProperty(call.CallState, stepSettings.Value);

            if (valueToPlay == null)
                throw new ArgumentException("Value specified does not exist on CallState: " + stepSettings.Value);

            await call.PromptPlayer.PlayNumberToLineAsync(valueToPlay.ToString(),
                stepSettings.Direction == Direction.Incoming
                    ? call.CallState.GetIncomingLineId()
                    : call.CallState.GetOutgoingLineId());

            if (stepSettings.Direction != Direction.Outgoing)
                call.CallState.AddStepToIncomingQueue(step.GetStepFromConnector(next_step));
            else
                call.CallState.AddStepToOutgoingQueue(step.GetStepFromConnector(next_step));
        }

        private static Task PlayMoneyValue(Step step, ICall call, PlayValueStepSettings stepSettings)
        {
            var numbersToPromptsConverter = new MoneyValueToPrompts(call.Logger);
            var valueToPlay = (decimal?) DynamicState.GetProperty(call.CallState, stepSettings.Value);

            if (valueToPlay == null)
                throw new ArgumentException("Value specified does not exist on CallState: " + stepSettings.Value);

            var prompts = numbersToPromptsConverter.GetPromptsForValue(valueToPlay.Value);

            var promptSettings = new PlayerPromptSettings()
            {
                IsInterruptible = stepSettings.IsInterruptible,
                NextStep = step.GetStepFromConnector(next_step),
                Prompts = prompts,
                Direction = stepSettings.Direction
            };

            call.PromptPlayer.DoStepAsync(promptSettings);
            if (stepSettings.Direction != Direction.Outgoing)
                call.CallState.AddStepToIncomingQueue(step.GetStepFromConnector(next_step));
            else
                call.CallState.AddStepToOutgoingQueue(step.GetStepFromConnector(next_step));
            return Task.CompletedTask;
        }
    }
}
