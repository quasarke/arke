using Arke.Steps.ArkeInitStep;
using Arke.Steps.BridgeCallStep;
using Arke.Steps.CallLoopStep;
using Arke.Steps.CheckAttemptStep;
using Arke.Steps.DeviceConnectedStep;
using Arke.Steps.DeviceDisconnectedStep;
using Arke.Steps.HoldStep;
using Arke.Steps.InputStep;
using Arke.Steps.LanguageStep;
using Arke.Steps.OutboundCallStep;
using Arke.Steps.ParallelStep;
using Arke.Steps.PlayPromptStep;
using Arke.Steps.PlayValueStep;
using Arke.Steps.StartRecordingStep;
using Arke.Steps.StopRecordingStep;
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
            container.Register<CallLoopProcessor>();
            container.Register<CheckAttemptStepProcessor>();
            container.Register<LanguageStepProcessor>();
            container.Register<CallOutboundProcessor>();
            container.Register<ParallelStartProcessor>();
            container.Register<PlayPromptProcessor>();
            container.Register<PlayValueStepProcessor>();
            container.Register<StartRecordingLineProcessor>();
            container.Register<StopRecordingProcessor>();
        }

        private void RegisterSettings(Container container)
        {
            container.Register<DeviceConnectedSettings>();
            container.Register<DisconnectDeviceSettings>();
            container.Register<HoldStepSettings>();
            container.Register<GetInputSettings>();
            container.Register<ArkeInitSettings>();
            container.Register<BridgeCallSettings>();
            container.Register<CallLoopSettings>();
            container.Register<CheckAttemptStepSettings>();
            container.Register<LanguageStepSettings>();
            container.Register<CallOutboundSettings>();
            container.Register<ParallelStartSettings>();
            container.Register<PlayPromptSettings>();
            container.Register<PlayValueStepSettings>();
            container.Register<StartRecordingLineSettings>();
            container.Register<StopRecordingSettings>();
        }
    }
}
