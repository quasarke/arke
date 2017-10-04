using Arke.SipEngine.Events;

namespace Arke.SipEngine.Api
{
    public delegate void LineHangupEventHandler(ISipApiClient sender, LineHangupEvent e);
}