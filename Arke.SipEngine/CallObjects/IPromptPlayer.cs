using System.Collections.Generic;
using System.Threading.Tasks;
using Arke.DSL.Step;

namespace Arke.SipEngine.CallObjects
{
    public interface IPromptPlayer
    {
        void AddPromptsToQueue(List<string> prompts, Direction direction);
        void PlayPromptTextToSpeech(string promptText);
        void PlayPromptsInQueue();
        Task PlayPromptToIncomingLine(string promptFile);
        void StopPrompt();
        void AddPromptToQueue(IPrompt prompt);
        void DoStep(PlayerPromptSettings settings);
        Task PlayRecordingToLine(string recordingName, string lineId);
    }
}