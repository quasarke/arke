using System.Collections.Generic;
using Arke.DSL.Step;

namespace Arke.Steps.ArkeInitStep
{
    public class ArkeInitSettings : NodeProperties
    {
        public new static List<string> GetOutputNodes()
        {
            return new List<string>() {"NextStep", "FailStep"};
        }
    }
}