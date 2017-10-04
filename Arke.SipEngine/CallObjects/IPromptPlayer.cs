using System.Collections.Generic;
using System.Threading.Tasks;

namespace Arke.SipEngine.CallObjects
{
    public interface IPromptPlayer
    {
        void AddPromptsToQueue(List<string> prompts);
        void PlayPromptTextToSpeech(string promptText);
        void PlayPromptsInQueue();
        Task PlayPromptFile(string promptFile);
        void StopPrompt();
        void AddPromptToQueue(IPrompt prompt);
        void DoStep(PlayerPromptSettings settings);
    }
}