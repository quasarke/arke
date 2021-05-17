namespace Arke.SipEngine.Events
{
    public class DtmfReceivedEvent
    {
        public string Digit { get; set; }
        public int DurationInMilliseconds { get; set; }
        public string LineId { get; set; }
    }
}