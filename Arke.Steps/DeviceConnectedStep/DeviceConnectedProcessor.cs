using System;
using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.Steps.DeviceConnectedStep
{
    public class DeviceConnectedProcessor : IStepProcessor
    {
        public string Name => "DeviceConnected";
        private const string NextStep = "NextStep";

        public virtual async Task DoStepAsync(Step step, ICall call)
        {
            call.Logger.Information("Call flow begin for call {@Call}", call.CallState);
            call.Logger.Debug("Next step " + step.GetStepFromConnector(NextStep));
            call.CallState.AddStepToIncomingQueue(step.GetStepFromConnector(NextStep));
            call.CallState.TimeDeviceConnected = DateTimeOffset.Now;
            await call.FireStateChange(Trigger.NextCallFlowStep);
        }
    }
}
