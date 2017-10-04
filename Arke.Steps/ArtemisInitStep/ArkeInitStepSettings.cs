using Arke.DSL.Step.Settings;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.ArtemisInitStep
{
    public class ArkeInitStepSettings : ISettings
    {
        public int FailStep { get; set; }
        public int NextStep { get; set; }
        public string DeviceConfigEndpoint { get; set; }
        public ISettings ConvertFromJObject(JObject jObject)
        {
            NextStep = jObject.GetValue("NextStep").Value<int>();
            FailStep = jObject.GetValue("FailStep").Value<int>();
            DeviceConfigEndpoint = jObject.GetValue("DeviceConfigEndpoint").Value<string>();
            return this;
        }
    }
}