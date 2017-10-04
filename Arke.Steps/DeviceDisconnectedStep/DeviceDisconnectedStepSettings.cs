using Arke.DSL.Step.Settings;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.DeviceDisconnectedStep
{
    public class DeviceDisconnectedStepSettings : ISettings
    {
        public ISettings ConvertFromJObject(JObject jObject)
        {
            HangUp = jObject.GetValue("HangUp").Value<bool>();
            return this;
        }

        public bool HangUp { get; set; }
    }
}