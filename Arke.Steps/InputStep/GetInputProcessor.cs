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
            call.Logger.Information("GetInputStep {@Call}", call.CallState);
            call.CallState.InputRetryCount++;
            call.Logger.Information("Attempt count {InputRetryCount} {@Call}", call.CallState.InputRetryCount, call.CallState);
            var inputHandlerSettings = (GetInputSettings) step.NodeData.Properties;
            
            call.FireStateChange(Trigger.CaptureInput);
            call.InputProcessor.ChangeInputSettings(inputHandlerSettings.GetPhoneInputHandlerSettings(step));
            call.InputProcessor.StartUserInput(false);
            call.Logger.Information("Input Processor begin! {@Call}", call.CallState);
            return Task.CompletedTask;
        }
    }
}
