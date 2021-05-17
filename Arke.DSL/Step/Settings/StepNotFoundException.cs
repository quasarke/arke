using System;

namespace Arke.DSL.Step.Settings
{
    public class StepNotFoundException : Exception
    {
        public StepNotFoundException(string message) : base(message)
        {
            
        }
    }
}