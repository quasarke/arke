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
        public Task DoStep(Step step, ICall call)
        {
            call.FireStateChange(Trigger.FinishCall);
            return Task.CompletedTask;
        }
    }
}
