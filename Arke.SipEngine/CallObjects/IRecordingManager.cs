using System.Threading.Tasks;
using Arke.SipEngine.Api;

namespace Arke.SipEngine.CallObjects
{
    public interface IRecordingManager
    {
        Task StartRecordingOnLine(string lineId, string direction, ICallInfo callState);
        Task StopRecordingOnLine(string lineId);
        Task StartRecordingOnBridge(string bridgeId, ICallInfo callState);
        Task StopRecordingOnBridge(string bridgeId);
        Task StopAllRecordings();
        Task AriClient_OnRecordingFinishedEvent(ISipApiClient sender, RecordingFinishedEventHandlerArgs args);
    }
}