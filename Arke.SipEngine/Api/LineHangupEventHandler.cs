using System.Threading.Tasks;
using Arke.SipEngine.Events;

namespace Arke.SipEngine.Api
{
    public delegate Task LineHangupEventHandler(ISipApiClient sender, LineHangupEvent e);
}