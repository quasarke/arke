using Arke.DSL.Step;
using Arke.SipEngine.CallObjects;

namespace Arke.IVR.Prompts
{
    public class Prompt : IPrompt
    {
        public string PromptFile { get; set; }
        public Direction Direction { get; set; }
    }
}