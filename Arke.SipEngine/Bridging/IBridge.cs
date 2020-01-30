namespace Arke.SipEngine.Bridging
{
    public interface IBridge
    {
        string Id { get; set; }
        string Type { get; set; }
        string Name { get; set; }
    }
}