using System.Collections.Generic;
using System.Threading.Tasks;
using Arke.SipEngine.CallObjects;

namespace Arke.SipEngine.Processors
{
    public interface ILanguageSelectionPromptPlayer
    {
        void AddPromptsToQueue(List<string> prompts);
        void PlayPromptTextToSpeech(string promptText);
        Task PlayNextPromptInQueue();
        Task PlayPromptFile(string promptFile);
        Task HaltPromptPlayback();
        void AddPromptToQueue(IPrompt prompt);
        void CleanupEventHooks();
        void SetStepProcessor(ILanguageStepProcessor languageStepProcessor);
    }
}
