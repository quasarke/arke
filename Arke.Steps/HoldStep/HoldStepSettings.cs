using System.Collections.Generic;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.HoldStep
{
    public class HoldStepSettings : NodeProperties
    {
        public bool HoldMusic { get; set; }
        public string WaitPrompt { get; set; }
        public int HoldTimeoutInSeconds { get; set; }
        public Dictionary<string, string> PromptChanges { get; set; }
        public Dictionary<string, string> Triggers { get; set; }

        public override NodeProperties ConvertFromJObject(JObject jObject)
        {
            HoldMusic = jObject.GetValue("HoldMusic").Value<bool>();
            WaitPrompt = jObject.GetValue("WaitPrompt").Value<string>();
            PromptChanges = jObject.GetValue("PromptChanges").ToObject<Dictionary<string, string>>();
            Triggers = jObject.GetValue("Triggers").ToObject<Dictionary<string, string>>();
            HoldTimeoutInSeconds = jObject.GetValue("HoldTimeoutInSeconds").Value<int>();
            return this;
        }

        public new static List<string> GetOutputNodes()
        {
            return new List<string>() { "TimeoutStep" };
        }
    }
}
