using Arke.DSL.Step.Settings;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.PlayValueStep
{
    public class PlayValueStepSettings : ISettings
    {
        public bool IsInterruptible { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
        public int NextStep { get; set; }

        public ISettings ConvertFromJObject(JObject jObject)
        {
            IsInterruptible = jObject.GetValue("IsInterruptible").Value<bool>();
            Value = jObject.GetValue("Value").Value<string>();
            ValueType = jObject.GetValue("ValueType").Value<string>();
            NextStep = jObject.GetValue("NextStep").Value<int>();
            return this;
        }
    }
}
