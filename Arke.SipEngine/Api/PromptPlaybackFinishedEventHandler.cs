using System.Threading.Tasks;
using Arke.SipEngine.Events;

namespace Arke.SipEngine.Api
{
    public delegate Task PromptPlaybackFinishedEventHandler(ISipApiClient sender, PromptPlaybackFinishedEvent e);
}
