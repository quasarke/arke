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
        public async Task DoStepAsync(Step step, ICall call)
        {
            call.Logger.Information("GetInputStep {@Call}", call.CallState);
            call.CallState.InputRetryCount++;
            call.Logger.Information("Attempt count {InputRetryCount} {@Call}", call.CallState.InputRetryCount, call.CallState);
            var inputHandlerSettings = (GetInputSettings) step.NodeData.Properties;
            await call.FireStateChange(Trigger.CaptureInput);
            call.InputProcessor.ChangeInputSettings(inputHandlerSettings.GetPhoneInputHandlerSettings(step));
            call.InputProcessor.StartUserInput(false);
            if (!string.IsNullOrEmpty(call.InputProcessor.DigitsReceived))
                await call.InputProcessor.ProcessDigitsReceived();
            
            call.Logger.Information("Input Processor begin! {@Call}", call.CallState);
        }
    }
}
