using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.IVR.CallObjects;
using Arke.SipEngine.Api;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Events;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Prompts;

namespace Arke.IVR.Prompts
{
    public class ArkePromptPlayer : IPromptPlayer
    {
        private readonly ArkeCall _arkeCall;
        private readonly Queue<IPrompt> _promptQueue;
        private readonly ISipPromptApi _sipPromptApi;
        private LanguageData _languageData;
        private string _currentPlaybackId = "";

        public ArkePromptPlayer(ArkeCall arkeCall, ISipPromptApi sipPromptApi)
        {
            _arkeCall = arkeCall;
            _sipPromptApi = sipPromptApi;
            _promptQueue = new Queue<IPrompt>();
        }

        public void AddPromptToQueue(IPrompt prompt)
        {
            _arkeCall.Logger.Debug($"Adding prompt to queue, queue size: {_promptQueue.Count}", _arkeCall.LogData);
            _promptQueue.Enqueue(prompt);
        }

        public void DoStep(PlayerPromptSettings settings)
        {
            if (settings.IsInterruptible)
            {
                _arkeCall.InputProcessor.StartUserInput(true);
            }

            AddPromptsToQueue(settings.Prompts, settings.Direction);

            _arkeCall.CallStateMachine.Fire(settings.IsInterruptible
                ? Trigger.PlayInterruptiblePrompt
                : Trigger.PlayPrompt);
        }

        public async Task PlayRecordingToLine(string recordingName, string lineId)
        {
            try
            {
                _currentPlaybackId = await _sipPromptApi.PlayRecordingToLine(lineId, recordingName);
            }
            catch (Exception e)
            {
                _arkeCall.Logger.Error(e, $"Error Playing Prompt: {e.Message}");
                if (_arkeCall.GetCurrentState() != State.HangUp)
                    _arkeCall.CallStateMachine.Fire(Trigger.FinishCall);
            }
        }

        public void AddPromptsToQueue(List<string> prompts, Direction direction)
        {
            foreach (var prompt in prompts)
            {
                _promptQueue.Enqueue(new Prompt()
                {
                    PromptFile = prompt,
                    Direction = direction
                });
            }
            _arkeCall.Logger.Debug($"Added prompts to queue, queue size: {_promptQueue.Count}", _arkeCall.LogData);
        }

        public void PlayPromptTextToSpeech(string promptText)
        {
            throw new NotImplementedException("Asterisk Engine currently does not support TTS");
        }

        public async void PlayPromptsInQueue()
        {
            if (_arkeCall.GetCurrentState() == State.StoppingPlayback || _promptQueue.Count == 0)
                return;
            _arkeCall.Logger.Debug($"Playing next prompt in queue, queue size: {_promptQueue.Count}", _arkeCall.LogData);

            try
            {
                var prompt = _promptQueue.Dequeue();

                if (prompt != null)
                {
                    switch (prompt.Direction)
                    {
                        case Direction.Incoming:
                            await PlayPromptToIncomingLine(prompt.PromptFile).ConfigureAwait(false);
                            break;
                        case Direction.Outgoing:
                            await PlayPromptToOutgoingLine(prompt.PromptFile).ConfigureAwait(false);
                            break;
                        default:
                            await PlayPromptToBridge(prompt.PromptFile).ConfigureAwait(false);
                            break;
                    }
                }
            }
            catch (Exception)
            {
                _arkeCall.CallStateMachine.Fire(Trigger.FinishedPrompt);
            }
        }

        public void StopPrompt()
        {
            if (_arkeCall.GetCurrentState() == State.PlayingPrompt)
                throw new InvalidOperationException("Cannot stop playback of this prompt.");
            _arkeCall.CallStateMachine.Fire(Trigger.PromptInterrupted);
            _arkeCall.Logger.Debug($"Stopping Prompt {_currentPlaybackId}", _arkeCall.LogData);
            _sipPromptApi.StopPrompt(_currentPlaybackId);
            _promptQueue.Clear();
            _arkeCall.CallStateMachine.Fire(Trigger.FinishedPrompt);
        }

        public async Task PlayPromptToBridge(string promptFile)
        {
            try
            {
                _currentPlaybackId = (await _sipPromptApi.PlayPromptToBridge(
                    _arkeCall.CallState.GetBridgeId(),
                    promptFile, _languageData.FolderName).ConfigureAwait(false));
                _arkeCall.Logger.Debug($"Prompt file is: {promptFile}");
                _arkeCall.Logger.Debug($"Prompt ID: {_currentPlaybackId}", _arkeCall.LogData);
            }
            catch (Exception ex)
            {
                _arkeCall.Logger.Error(ex, $"Error Playing Prompt: {ex.Message}");
                if (_arkeCall.GetCurrentState() != State.HangUp)
                    _arkeCall.CallStateMachine.Fire(Trigger.FinishCall);
            }
        }

        public async Task PlayPromptToOutgoingLine(string promptFile)
        {
            try
            {
                _currentPlaybackId = (await _sipPromptApi.PlayPromptToLine(
                    _arkeCall.CallState.GetOutgoingLineId(),
                    promptFile, _languageData.FolderName).ConfigureAwait(false));
                _arkeCall.Logger.Debug($"Prompt file is: {promptFile}");
                _arkeCall.Logger.Debug($"Prompt ID: {_currentPlaybackId}", _arkeCall.LogData);
            }
            catch (Exception ex)
            {
                _arkeCall.Logger.Error(ex, $"Error Playing Prompt: {ex.Message}");
                if (_arkeCall.GetCurrentState() != State.HangUp)
                    _arkeCall.CallStateMachine.Fire(Trigger.FinishCall);
            }
        }

        public async Task PlayPromptToIncomingLine(string promptFile)
        {
            try
            {
                _currentPlaybackId = (await _sipPromptApi.PlayPromptToLine(
                    _arkeCall.CallState.GetIncomingLineId(),
                    promptFile, _languageData.FolderName).ConfigureAwait(false));
                _arkeCall.Logger.Debug($"Prompt file is: {promptFile}");
                _arkeCall.Logger.Debug($"Prompt ID: {_currentPlaybackId}", _arkeCall.LogData);
            }
            catch (Exception ex)
            {
                _arkeCall.Logger.Error(ex, $"Error Playing Prompt: {ex.Message}");
                if (_arkeCall.GetCurrentState() != State.HangUp)
                    _arkeCall.CallStateMachine.Fire(Trigger.FinishCall);
            }
        }

        public Task AriClient_OnPlaybackFinishedEvent(ISipApiClient sipApiClient, PromptPlaybackFinishedEvent e)
        {
            if (e.PlaybackId != _currentPlaybackId)
                return Task.CompletedTask;
            if (_arkeCall.GetCurrentState() == State.StoppingPlayback)
                return Task.CompletedTask;

            _arkeCall.CallStateMachine.Fire(
                _promptQueue.Count == 0 ? Trigger.FinishedPrompt : Trigger.PlayNextPrompt);
            return Task.CompletedTask;
        }

        public void SetLanguageCode(LanguageData language)
        {
            _languageData = language;
        }
    }
}