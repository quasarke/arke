using System.Threading.Tasks;

namespace Arke.SipEngine.Api
{
    public interface ISipRecordingApi
    {
        Task<string> StartRecordingOnLine(string lineId, string fileName);
        Task StopRecording(string recordingId);
        Task<string> StartRecordingOnBridge(string bridgeId, string fileName);
    }
}
