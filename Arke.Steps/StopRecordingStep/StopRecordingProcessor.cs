using System.Threading.Tasks;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;

namespace Arke.Steps.StopRecordingStep
{
    public class StopRecordingProcessor
    {
        public async Task DoStep(ISettings settings, ICall call)
        {
            call.Logger.Info($"Stop recording {call.CallState.GetIncomingLineId()}");
            await call.RecordingManager.StopRecordingOnLine(call.CallState.GetIncomingLineId());
        }
    }
}
