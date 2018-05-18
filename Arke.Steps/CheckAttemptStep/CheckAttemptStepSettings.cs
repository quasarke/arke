using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.CheckAttemptStep
{
    public class CheckAttemptStepSettings : NodeProperties
    {
        public override NodeProperties ConvertFromJObject(JObject jObject)
        {
            base.ConvertFromJObject(jObject);
            MaxAttempts = jObject.GetValue("MaxAttempts").Value<int>();
            return this;
        }

        public int MaxAttempts { get; set; }
    }
}
