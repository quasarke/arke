using System.Collections.Generic;
using Arke.DSL.Step;

namespace Arke.Steps.ParallelStep
{

    public class ParallelStartSettings : NodeProperties
    {
        public new static List<string> GetOutputNodes()
        {
            return new List<string>() { "IncomingNextStep", "OutgoingNextStep" };
        }
    }
}