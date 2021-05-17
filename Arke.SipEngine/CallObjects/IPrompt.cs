using Arke.DSL.Step;

namespace Arke.SipEngine.CallObjects
{
    public interface IPrompt
    {
        string PromptFile { get; set; }
        Direction Direction { get; set; }
    }
}
