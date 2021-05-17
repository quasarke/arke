using System.Collections.Generic;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.DeviceDisconnectedStep
{
    public class DisconnectDeviceSettings : NodeProperties
    {
        public override NodeProperties ConvertFromJObject(JObject jObject)
        {
            base.ConvertFromJObject(jObject);
            HangUp = jObject.GetValue("HangUp").Value<bool>();
            return this;
        }

        public bool HangUp { get; set; }

        public new static List<string> GetOutputNodes()
        {
            return new List<string>() {  };
        }
    }
}