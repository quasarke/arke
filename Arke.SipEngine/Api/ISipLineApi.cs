using System.Threading.Tasks;

namespace Arke.SipEngine.Api
{
    public interface ISipLineApi
    {
        Task HangupLineAsync(string lineId);
        Task<object> CreateOutboundCallAsync(string numberToDial, string callerId, string outboundEndpoint);
        Task<object> CreateOutboundCallAsync(string numberToDial, string outboundEndpoint);
        Task<string> GetLineStateAsync(string lineId);
        Task<string> GetEndpointAsync(string lineId);
        Task AnswerLineAsync(string lineId);
        Task<string> GetLineVariableAsync(string lineId, string variableName);
        Task TransferLineAsync(string lineId, string endpoint);
        Task PlayMusicOnHoldToLineAsync(string channelId);
        Task StopMusicOnHoldToLineAsync(string channelId);
    }
}
