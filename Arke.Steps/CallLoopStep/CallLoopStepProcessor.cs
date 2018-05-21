using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Arke.DSL.Step;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Processors;

namespace Arke.Steps.CallLoopStep
{
    public class CallLoopStepProcessor : IStepProcessor
    {
        public string Name => "CallLoopStep";
        private ICall _call;
        private Step _step;

        public async Task DoStep(Step step, ICall call)
        {
            var callTimer = new Timer(1000d);
            callTimer.Elapsed += CallTimer_Elapsed;
            _call = call;
            _step = step;
            _call.SipApiClient.OnLineHangupEvent += SipApiClient_OnLineHangupEvent;
        }

        private void SipApiClient_OnLineHangupEvent(SipEngine.Api.ISipApiClient sender, SipEngine.Events.LineHangupEvent e)
        {
            if (e.LineId == _call.CallState.GetIncomingLineId()
                || e.LineId == _call.CallState.GetOutgoingLineId())
            {
                _call.SipApiClient.OnLineHangupEvent -= SipApiClient_OnLineHangupEvent;
                _call.AddStepToProcessQueue(_step.LinkedSteps.Single(s => s.FromPort == "NextIncomingStep").To);
                _call.AddStepToProcessQueue(_step.LinkedSteps.Single(s => s.FromPort == "NextOutgoingStep").To);
            }
        }

        private void CallTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            
        }
    }
}
