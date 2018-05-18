using System.Collections.Generic;
using Arke.DSL.Step;
using Newtonsoft.Json;

namespace Arke.SipEngine.Device
{
    public abstract class DeviceConfig
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public string Name { get; set; }
        
        public DeviceType DeviceType { get; set; }
        public List<Feature> Features { get; set; }
        public List<Setting> Settings { get; set; }
        public List<Workflow> Workflows { get; set; }
    }

    public class Workflow
    {
        private CallFlowDsl _value;
        public int Id { get; set; }
        public string Key { get; set; }
        public object Value {
            get => _value;
            set
            {
                var s = value as string;
                if (s != null)
                {
                    _value = JsonConvert.DeserializeObject<CallFlowDsl>(s);
                }
                else if (value is CallFlowDsl)
                {
                    _value = (CallFlowDsl) value;
                }
            } 
        }
    }
}
