using System.Collections.Generic;

namespace Arke.SipEngine.Consul
{
    public class CatalogService
    {
        public string Node { get; set; }
        public string Address { get; set; }
        public string ServiceID { get; set; }
        public string ServiceName { get; set; }
        public List<string> ServiceTags { get; set; }
        public string ServiceAddress { get; set; }
        public string ServicePort { get; set; }
        public bool ServiceEnableTagOverride { get; set; }
        public int CreateIndex { get; set; }
        public int ModifyIndex { get; set; }
    }
}
