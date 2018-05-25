using System.Collections.Generic;
using Arke.DSL.Step;

namespace Arke.SipEngine.CallObjects
{
    public class PlayerPromptSettings
    {
        public List<string> Prompts { get; set; }
        public bool IsInterruptible { get; set; }
        public int NextStep { get; set; }
        public Direction Direction { get; set; }
    }
}