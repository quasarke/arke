using System.Threading.Tasks;
using Arke.SipEngine.Bridging;

namespace Arke.SipEngine.Api
{
    public interface ISipBridgingApi
    {
        Task<IBridge> CreateBridgeAsync(string bridgeType, string bridgeName);
        Task AddLineToBridgeAsync(string lineId, string bridgeId);
        Task PlayMusicOnHoldToBridgeAsync(string bridgeId);
        Task RemoveLineFromBridgeAsync(string lineId, string bridgeId);
        Task DestroyBridgeAsync(string bridgeId);
        Task MuteLineAsync(string lineId);
        Task UnmuteLineAsync(string lineId);
        Task<string> PlayPromptToBridgeAsync(string bridgeId, string promptFile, string languageCode);
    }
}