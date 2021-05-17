using Arke.SipEngine.Events;

namespace Arke.SipEngine.Api
{
    public delegate void DtmfReceivedEventHandler(ISipApiClient sender, DtmfReceivedEvent e);
}
