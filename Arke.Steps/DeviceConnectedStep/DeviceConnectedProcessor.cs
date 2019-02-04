using System;
using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Processors;

namespace Arke.Steps.DeviceConnectedStep
{
    public class DeviceConnectedProcessor : IStepProcessor
    {
        public string Name => "DeviceConnected";
        private const string NextStep = "NextStep";

        public virtual Task DoStep(Step step, ICall call)
        {
            call.Logger.Debug("Next step " + step.GetStepFromConnector(NextStep));
            call.CallState.AddStepToIncomingQueue(step.GetStepFromConnector(NextStep));
            call.CallState.TimeDeviceConnected = DateTimeOffset.Now;
            return Task.CompletedTask;
        }
    }
}
