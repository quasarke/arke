using System.Collections.Generic;

namespace Arke.SipEngine.Device
{
    public class DeviceConfigServiceResponse
    {
        public string Status { get; set; }
        public List<DeviceConfig> Data { get; set; }
    }

    public class FacilitySettingsServiceResponse
    {
        public string Status { get; set; }
        public List<Setting> Data { get; set; }
    }
}
