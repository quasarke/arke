using System.Threading.Tasks;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.Steps.InputStep
{
    public class InputStepProcessor : IStepProcessor
    {
        public string Name => "InputStep";
        public Task DoStep(ISettings settings, ICall call)
        {
            call.CallState.InputRetryCount++;
            var inputHandlerSettings = (InputStepSettings) settings;
            
            call.FireStateChange(Trigger.CaptureInput);
            call.InputProcessor.ChangeInputSettings(inputHandlerSettings.GetPhoneInputHandlerSettings());
            call.InputProcessor.StartUserInput(false);
            return Task.CompletedTask;
        }
    }
}
