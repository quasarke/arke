using System.Collections.Generic;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.SetValueStep
{
    [StepDescription("Sets a value on the Call State.")]
    public class SetValueStepSettings : NodeProperties
    {
        public string Property { get; set; }
        public object Value { get; set; }

        public override NodeProperties ConvertFromJObject(JObject jObject)
        {
            base.ConvertFromJObject(jObject);
            Property = jObject.GetValue("Property").Value<string>();
            Value = jObject.GetValue("Value").Value<object>();
            return this;
        }
    }
}
