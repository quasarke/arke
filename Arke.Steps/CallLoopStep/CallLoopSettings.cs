using System;
using System.Collections.Generic;
using System.Text;
using Arke.DSL.Step;

namespace Arke.Steps.CallLoopStep
{
    [StepDescription("Process a call loop waiting for max time limits.")]
    public class CallLoopSettings : NodeProperties
    {
        public new static List<string> GetOutputNodes()
        {
            return new List<string>() {"NextIncomingStep", "NextOutgoingStep"};
        }
    }
}
