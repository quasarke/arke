using System.Collections.Generic;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.PlayPromptStep
{
    public class PlayPromptStepSettings : ISettings
    {
        public int NextStep { get; set; }
        public bool IsInterruptible { get; set; }
        public List<string> Prompts { get; set; }
        public ISettings ConvertFromJObject(JObject jObject)
        {
            IsInterruptible = jObject.GetValue("IsInterruptible").Value<bool>();
            NextStep = jObject.GetValue("NextStep").Value<int>();
            Prompts = new List<string>();

            var prompts = jObject.GetValue("Prompts").Value<JArray>();
            foreach (var p in prompts)
            {
                Prompts.Add(p.Value<string>());
            }
            return this;
        }

        public PlayerPromptSettings GetPromptPlayerSettings()
        {
            return new PlayerPromptSettings()
            {
                IsInterruptible = IsInterruptible,
                Prompts = Prompts,
                NextStep = NextStep
            };
        }
    }
}
