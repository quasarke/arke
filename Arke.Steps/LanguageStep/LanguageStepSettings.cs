using System.Collections.Generic;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.LanguageStep
{
    public class LanguageStepSettings : NodeProperties
    {
        public List<string> Prompts { get; set; }
        public Dictionary<string, string> LanguageSettings { get; set; }
        public int MaxDigitTimeoutInSeconds { get; set; }
        public override NodeProperties ConvertFromJObject(JObject jObject)
        {
            base.ConvertFromJObject(jObject);
            Prompts = new List<string>();
            MaxDigitTimeoutInSeconds = jObject.GetValue("MaxDigitTimeoutInSeconds").Value<int>();
            var prompts = jObject.GetValue("Prompts").Value<JArray>();
            foreach (var p in prompts)
            {
                Prompts.Add(p.Value<string>());
            }
            LanguageSettings = jObject.GetValue("LanguageSettings").ToObject<Dictionary<string, string>>();
            return this;
        }
    }
}
