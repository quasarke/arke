using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.Steps.StopRecordingStep
{
    public class StopRecordingProcessor : IStepProcessor
    {
        public string Name => "StopRecording";
        private const string NextStep = "NextStep";

        public async Task DoStep(Step settings, ICall call)
        {
            call.Logger.Info($"Stop recording {call.CallState.GetIncomingLineId()}");
            await call.RecordingManager.StopRecordingOnLine(call.CallState.GetIncomingLineId());
            GoToNextStep(call, settings);
        }
        
        public void GoToNextStep(ICall call, Step step)
        {
            if (step.NodeData.Properties.Direction != Direction.Outgoing)
                call.CallState.AddStepToIncomingQueue(step.GetStepFromConnector(NextStep));
            else
                call.CallState.AddStepToOutgoingQueue(step.GetStepFromConnector(NextStep));
            call.FireStateChange(Trigger.NextCallFlowStep);
        }
    }
}
