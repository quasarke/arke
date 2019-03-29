using System.Threading.Tasks;

namespace Arke.SipEngine.Api
{
    public interface ISipPromptApi
    {
        Task<string> PlayPromptToLineAsync(string lineId, string promptFile, string languageCode);
        Task StopPromptAsync(string playbackId);
        Task<string> PlayPromptToBridgeAsync(string bridgeId, string promptFile, string languageCode);
        Task<string> PlayRecordingToLineAsync(string lineId, string recordingName);
        Task<string> PlayNumberToLineAsync(string lineId, string number, string languageCode);
    }
}
