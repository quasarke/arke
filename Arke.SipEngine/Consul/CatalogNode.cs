namespace Arke.SipEngine.Consul
{
    public class CatalogNode
    {
        public string Node { get; set; }
        public string Address { get; set; }
        public TaggedAddresses TaggedAddresses { get; set; }
        public int CreateIndex { get; set; }
        public int ModifyIndex { get; set; }
    }
}
