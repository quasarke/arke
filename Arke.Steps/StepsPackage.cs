using Arke.Steps.DeviceConnectedStep;
using Arke.Steps.DeviceDisconnectedStep;
using Arke.Steps.HoldStep;
using Arke.Steps.InputStep;
using SimpleInjector;
using SimpleInjector.Packaging;

namespace Arke.Steps
{
    public class StepsPackage : IPackage
    {
        public void RegisterServices(Container container)
        {
            RegisterProcessors(container);
            RegisterSettings(container);
        }

        private void RegisterProcessors(Container container)
        {
            container.Register<DeviceConnectedProcessor>();
            container.Register<DisconnectDeviceProcessor>();
            container.Register<HoldStepProcessor>();
            container.Register<GetInputProcessor>();
        }

        private void RegisterSettings(Container container)
        {
            container.Register<DeviceConnectedSettings>();
            container.Register<DisconnectDeviceSettings>();
            container.Register<HoldStepSettings>();
        }
    }
}
