using System.Threading.Tasks;

namespace Arke.SipEngine.Api
{
    public interface ISipLineApi
    {
        Task HangupLine(string lineId);
        Task<object> CreateOutboundCall(string numberToDial, string outboundEndpoint);
        Task<string> GetLineState(string lineId);
        Task<string> GetEndpoint(string lineId);
        Task AnswerLine(string lineId);
        Task<string> GetLineVariable(string lineId, string variableName);
        Task PlayMusicOnHoldToLine(string channelId);
        Task StopMusicOnHoldToLine(string channelId);
    }
}
