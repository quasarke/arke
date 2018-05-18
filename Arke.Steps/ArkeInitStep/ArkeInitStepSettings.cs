using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.ArkeInitStep
{
    public class ArkeInitStepSettings : NodeProperties
    {
        public string DeviceConfigEndpoint { get; set; }

        public override NodeProperties ConvertFromJObject(JObject jObject)
        {
            base.ConvertFromJObject(jObject);
            DeviceConfigEndpoint = jObject.GetValue("DeviceConfigEndpoint").Value<string>();
            return this;
        }
    }
}