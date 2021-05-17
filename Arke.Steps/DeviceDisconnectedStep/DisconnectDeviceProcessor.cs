using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.Steps.DeviceDisconnectedStep
{
    public class DisconnectDeviceProcessor : IStepProcessor
    {
        public string Name => "DisconnectDevice";
        public Task DoStepAsync(Step step, ICall call)
        {
            var stepSettings = step.NodeData.Properties as DisconnectDeviceSettings;
            if (stepSettings.HangUp)
                switch (stepSettings.Direction)
                {
                    case Direction.Incoming:
                        call.SipLineApi.HangupLineAsync(call.CallState.GetIncomingLineId());
                        break;
                    case Direction.Outgoing:
                        call.SipLineApi.HangupLineAsync(call.CallState.GetOutgoingLineId());
                        break;
                    case Direction.Both:
                        call.SipLineApi.HangupLineAsync(call.CallState.GetIncomingLineId());
                        call.SipLineApi.HangupLineAsync(call.CallState.GetOutgoingLineId());
                        break;
                    default:
                        break;
                }

            call.FireStateChange(Trigger.FinishCall);
            return Task.CompletedTask;
        }
    }
}
