namespace Arke.SipEngine
{
    public interface IApplicationFactory
    {
        ICallFlowApplication GetApplication(byte applicationId);
    }
}
