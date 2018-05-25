using System.Threading.Tasks;

namespace Arke.SipEngine.Api
{
    public interface ISipPromptApi
    {
        Task<string> PlayPromptToLine(string lineId, string promptFile, string languageCode);
        Task StopPrompt(string playbackId);
        Task<string> PlayPromptToBridge(string bridgeId, string promptFile, string languageCode);
    }
}
