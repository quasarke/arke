using Arke.Steps.ArkeInitStep;
using Arke.Steps.BridgeCallStep;
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
            container.Register<ArkeInitProcessor>();
            container.Register<BridgeCallProcessor>();
            container.Register<CallLoopStep.CallLoopProcessor>();
            container.Register<CheckAttemptStep.CheckAttemptStepProcessor>();
            container.Register<LanguageStep.LanguageStepProcessor>();
            container.Register<OutboundCallStep.CallOutboundProcessor>();
            container.Register<ParallelStep.ParallelStartProcessor>();
            container.Register<PlayPromptStep.PlayPromptProcessor>();
            container.Register<PlayValueStep.PlayValueStepProcessor>();
            container.Register<StartRecordingStep.StartRecordingLineProcessor>();
            container.Register<StopRecordingStep.StopRecordingProcessor>();
        }

        private void RegisterSettings(Container container)
        {
            container.Register<DeviceConnectedSettings>();
            container.Register<DisconnectDeviceSettings>();
            container.Register<HoldStepSettings>();
        }
    }
}
