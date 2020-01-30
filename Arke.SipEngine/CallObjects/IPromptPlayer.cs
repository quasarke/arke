using System.Collections.Generic;
using System.Threading.Tasks;
using Arke.DSL.Step;

namespace Arke.SipEngine.CallObjects
{
    public interface IPromptPlayer
    {
        void AddPromptsToQueue(List<string> prompts, Direction direction);
        void PlayPromptTextToSpeech(string promptText);
        Task PlayPromptsInQueueAsync();
        Task PlayPromptToIncomingLineAsync(string promptFile);
        Task StopPromptAsync();
        void AddPromptToQueue(IPrompt prompt);
        Task DoStepAsync(PlayerPromptSettings settings);
        Task PlayRecordingToLineAsync(string recordingName, string lineId);
        Task PlayNumberToLineAsync(string number, string lineId);
    }
}