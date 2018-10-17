using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.Steps.InputStep
{
    public class GetInputProcessor : IStepProcessor
    {
        public string Name => "GetInput";
        public Task DoStep(Step step, ICall call)
        {
            call.CallState.InputRetryCount++;
            var inputHandlerSettings = (GetInputSettings) step.NodeData.Properties;
            
            call.FireStateChange(Trigger.CaptureInput);
            call.InputProcessor.ChangeInputSettings(inputHandlerSettings.GetPhoneInputHandlerSettings(step));
            call.InputProcessor.StartUserInput(false);
            return Task.CompletedTask;
        }
    }
}
