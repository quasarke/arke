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
            container.Register<DeviceConnectedStepProcessor>();
            container.Register<DeviceDisconnectedStepProcessor>();
            container.Register<HoldStepProcessor>();
            container.Register<InputStepProcessor>();
        }

        private void RegisterSettings(Container container)
        {
            container.Register<DeviceConnectedStepSettings>();
            container.Register<DeviceDisconnectedStepSettings>();
            container.Register<HoldStepSettings>();
        }
    }
}
