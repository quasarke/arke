using System.Threading;

namespace Arke.SipEngine
{
    public interface ICallFlowService
    {
        bool Start(CancellationToken cancellationToken);
        bool Stop();
        bool Continue();
        bool Pause();
    }
}
