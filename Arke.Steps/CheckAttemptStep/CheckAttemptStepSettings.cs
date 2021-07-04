using System.Collections.Generic;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.CheckAttemptStep
{
    [StepDescription("Check if the number of attempts at an input have been exceeded.")]
    public class CheckAttemptStepSettings : NodeProperties
    {
        public override NodeProperties ConvertFromJObject(JObject jObject)
        {
            base.ConvertFromJObject(jObject);
            MaxAttempts = jObject.GetValue("MaxAttempts").Value<int>();
            return this;
        }

        public int MaxAttempts { get; set; }

        public new static List<string> GetOutputNodes()
        {
            return new List<string>() { "NextStep", "MaxAttemptsStep" };
        }
    }
}
