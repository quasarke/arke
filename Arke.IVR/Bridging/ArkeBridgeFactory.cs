using System.Threading.Tasks;
using Arke.SipEngine.Api;
using Arke.SipEngine.BridgeName;
using Arke.SipEngine.Bridging;
using Arke.SipEngine.CallObjects;

namespace Arke.IVR.Bridging
{
    public class ArkeBridgeFactory
    {
        private readonly ISipBridgingApi _ariClient;
        public ArkeBridgeFactory(ISipBridgingApi ariClient)
        {
            _ariClient = ariClient;
        }

        public async Task AddLineToBridge(string lineId, string bridgeId)
        {
            await _ariClient.AddLineToBridgeAsync(lineId, bridgeId);
        }

        public async Task StopHoldingBridge(ICallInfo callInfo)
        {
            await _ariClient.RemoveLineFromBridgeAsync(callInfo.GetIncomingLineId(), callInfo.GetBridgeId());
            await _ariClient.DestroyBridgeAsync(callInfo.GetBridgeId());
        }

        public async Task<IBridge> CreateBridge(BridgeType bridgeType)
        {
            switch (bridgeType)
            {
                case BridgeType.NoMedia:
                    return await _ariClient.CreateBridgeAsync((new MixingBridgeType()).Type, new BridgeNameGenerator().GetRandomBridgeName());
                case BridgeType.Holding:
                    return await _ariClient.CreateBridgeAsync((new HoldingBridgeType()).Type, new BridgeNameGenerator().GetRandomBridgeName());
                case BridgeType.NoDTMF:
                    return await _ariClient.CreateBridgeAsync((new ProxyMediaBridgeType()).Type, new BridgeNameGenerator().GetRandomBridgeName());
                case BridgeType.WithDTMF:
                    return await _ariClient.CreateBridgeAsync($"{(new DtmfBridgeType()).Type},{(new MixingBridgeType()).Type}", new BridgeNameGenerator().GetRandomBridgeName());
                default:
                    return await _ariClient.CreateBridgeAsync((new DtmfBridgeType()).Type, new BridgeNameGenerator().GetRandomBridgeName());
            }
        }
    }
}
