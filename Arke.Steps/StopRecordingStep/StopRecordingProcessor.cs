using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Processors;

namespace Arke.Steps.StopRecordingStep
{
    public class StopRecordingProcessor : IStepProcessor
    {
        public string Name => "StopRecording";

        public async Task DoStep(Step settings, ICall call)
        {
            call.Logger.Info($"Stop recording {call.CallState.GetIncomingLineId()}");
            await call.RecordingManager.StopRecordingOnLine(call.CallState.GetIncomingLineId());
        }
    }
}
