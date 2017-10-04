using System.Threading.Tasks;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Processors;

namespace Arke.Steps.DeviceConnectedStep
{
    public class DeviceConnectedStepProcessor : IStepProcessor
    {
        public string Name => "DeviceConnectedStep";

        public virtual Task DoStep(ISettings settings, ICall call)
        {
            var deviceConnectedStepSettings = ((DeviceConnectedStepSettings)settings);
            call.Logger.Debug("Next step " + deviceConnectedStepSettings.NextStep);
            call.CallState.AddStepToIncomingQueue(deviceConnectedStepSettings.NextStep);
            return Task.CompletedTask;
        }
    }
}
