using Arke.DSL.Step.Settings;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.BridgeCallStep
{
    public class BridgeCallStepSettings : ISettings
    {
        public int NextStep { get; set; }
        public ISettings ConvertFromJObject(JObject jObject)
        {
            NextStep = jObject.GetValue("NextStep").Value<int>();
            return this;
        }
    }
}
