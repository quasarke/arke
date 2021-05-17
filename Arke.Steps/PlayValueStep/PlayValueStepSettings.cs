using System.Collections.Generic;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.PlayValueStep
{
    public class PlayValueStepSettings : NodeProperties
    {
        public bool IsInterruptible { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
        
        public override NodeProperties ConvertFromJObject(JObject jObject)
        {
            base.ConvertFromJObject(jObject);
            IsInterruptible = jObject.GetValue("IsInterruptible").Value<bool>();
            Value = jObject.GetValue("Value").Value<string>();
            ValueType = jObject.GetValue("ValueType").Value<string>();
            return this;
        }
    }
}
