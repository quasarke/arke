namespace Arke.SipEngine
{
    public interface ICallFlowService
    {
        bool Start();
        bool Stop();
        bool Continue();
        bool Pause();
    }
}
