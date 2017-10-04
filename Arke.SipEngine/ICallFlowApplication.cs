using Arke.SipEngine.CallObjects;

namespace Arke.SipEngine
{
    public interface ICallFlowApplication
    {
        ICall CallApi { get; set; }
        ICallInfo CallInfo { get; set; }
        void HandleCall();
    }
}
