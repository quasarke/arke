using System.Collections.Generic;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.PlayPromptStep
{
    public class PlayPromptSettings : NodeProperties
    {
        private const string NextStep = "NextStep";
        public bool IsInterruptible { get; set; }
        public List<string> Prompts { get; set; }

        public override NodeProperties ConvertFromJObject(JObject jObject)
        {
            base.ConvertFromJObject(jObject);
            IsInterruptible = jObject.GetValue("IsInterruptible").Value<bool>();
            Prompts = new List<string>();

            var prompts = jObject.GetValue("Prompts").Value<JArray>();
            foreach (var p in prompts)
            {
                Prompts.Add(p.Value<string>());
            }
            return this;
        }

        public PlayerPromptSettings GetPromptPlayerSettings(Step step, Direction direction)
        {
            return new PlayerPromptSettings()
            {
                IsInterruptible = IsInterruptible,
                Prompts = Prompts,
                NextStep = step.GetStepFromConnector(NextStep),
                Direction = direction
            };
        }
    }
}
