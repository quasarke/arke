using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Processors;

namespace Arke.Steps.SetValueStep
{
    public class SetValueStepProcessor : IStepProcessor
    {
        public string Name => "SetValueStep";
        private const string next_step = "NextStep";
        public Task DoStepAsync(Step step, ICall call)
        {
            if (!(step.NodeData.Properties is SetValueStepSettings))
                throw new ArgumentException("SetValueStep called with invalid settings");

            var settings = step.NodeData.Properties as SetValueStepSettings;
            call.CallState.GetType().GetProperty(settings?.Property ?? throw new ArgumentException("Invalid Property on SetValue"))?.SetValue(call.CallState, settings.Value, null);

            switch (settings.Direction)
            {
                case Direction.Incoming:
                    call.CallState.AddStepToIncomingQueue(step.GetStepFromConnector(next_step));
                    break;
                case Direction.Outgoing:
                    call.CallState.AddStepToOutgoingQueue(step.GetStepFromConnector(next_step));
                    break;
                case Direction.Both:
                    call.CallState.AddStepToIncomingQueue(step.GetStepFromConnector(next_step));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Task.CompletedTask;
        }
    }
}
