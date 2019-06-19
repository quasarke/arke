using System.Threading.Tasks;

namespace Arke.SipEngine.Api
{
    public interface ISipRecordingApi
    {
        Task<string> StartRecordingOnLineAsync(string lineId, string fileName);
        Task StopRecordingAsync(string recordingId);
        Task<string> StartRecordingOnBridgeAsync(string bridgeId, string fileName);

        Task<string> StartShortRecordingForLineAsync(string lineId, string fileName, int maxDurationSeconds,
            int maxSilenceSeconds, bool beepOnStart);

        event RecordingFinishedEventHandler OnRecordingFinishedAsyncEvent;
    }
}
