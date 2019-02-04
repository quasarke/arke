using System;
using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.Steps.StartRecordingStep
{
    public class StartRecordingLineProcessor : IStepProcessor
    {
        private ICall _call;
        private Step _step;
        private const string NextStep = "NextStep";

        public string Name => "StartRecordingLine";
        
        public async Task DoStep(Step step, ICall call)
        {
            _call = call;
            _step = step;
            try
            {
                await StartAllRecordings();
                GoToNextStep();
            }
            catch (Exception ex)
            {
                if (ex.InnerException?.InnerException != null)
                    _call.Logger.Debug(ex.InnerException.InnerException.Message);
                _call.Logger.Error(ex);
            }

        }

        protected virtual async Task StartAllRecordings()
        {
            foreach (var recordingItem in ((StartRecordingLineSettings)_step.NodeData.Properties).ItemsToRecord)
            {
                await StartRecording(recordingItem);
            }
        }

        protected virtual async Task StartRecording(RecordingItems recordingItem)
        {
            switch (recordingItem)
            {
                case RecordingItems.InboundLine:
                    await StartRecordingOnInboundLine();
                    break;
                case RecordingItems.OutboundLine:
                    await StartRecordingOnOuboundLine();
                    break;
                case RecordingItems.Bridge:
                    await StartRecordingOnBridge();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void GoToNextStep()
        {
            var stepSettings = _step.NodeData.Properties as StartRecordingLineSettings;
            if (stepSettings.Direction != Direction.Outgoing)
                _call.CallState.AddStepToIncomingQueue(_step.GetStepFromConnector(NextStep));
            else
                _call.CallState.AddStepToOutgoingQueue(_step.GetStepFromConnector(NextStep));
            _call.FireStateChange(Trigger.NextCallFlowStep);
        }

        public async Task StartRecordingOnInboundLine()
        {
            if (_call.CallState.GetIncomingLineId() != null)
            {
                _call.Logger.Info("Start recording on inbound line "+ _call.CallState.GetIncomingLineId());
                await _call.StartRecordingOnLine(_call.CallState.GetIncomingLineId(), "I");
            }
        }

        public async Task StartRecordingOnOuboundLine()
        {
            if (_call.CallState.GetOutgoingLineId() != null)
            {
                _call.Logger.Info("Start recording on outbound line "+_call.CallState.GetOutgoingLineId());
                await _call.StartRecordingOnLine(_call.CallState.GetOutgoingLineId(), "O");
            }            
        }

        public async Task StartRecordingOnBridge()
        {
            if (_call.CallState.GetBridgeId() != null)
            {
                _call.Logger.Info("Start recording on bridge: " + _call.CallState.GetBridgeId());
                await _call.StartRecordingOnBridge(_call.CallState.GetBridgeId());
            }
        }
    }
}
