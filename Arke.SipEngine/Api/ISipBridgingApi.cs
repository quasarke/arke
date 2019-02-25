using System.Threading.Tasks;
using Arke.SipEngine.Bridging;

namespace Arke.SipEngine.Api
{
    public interface ISipBridgingApi
    {
        Task<IBridge> CreateBridge(string bridgeType, string bridgeName);
        Task AddLineToBridge(string lineId, string bridgeId);
        Task PlayMusicOnHoldToBridge(string bridgeId);
        Task RemoveLineFromBridge(string lineId, string bridgeId);
        Task DestroyBridge(string bridgeId);
        Task MuteLine(string lineId);
        Task UnmuteLine(string lineId);
        Task<string> PlayPromptToBridge(string bridgeId, string promptFile, string languageCode);
    }
}