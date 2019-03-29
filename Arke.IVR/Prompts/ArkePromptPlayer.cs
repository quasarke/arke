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

        public async Task DoStepAsync(PlayerPromptSettings settings)
        {
            if (settings.IsInterruptible)
            {
                _arkeCall.InputProcessor.StartUserInput(true);
            }

            AddPromptsToQueue(settings.Prompts, settings.Direction);

            await _arkeCall.CallStateMachine.FireAsync(settings.IsInterruptible
                ? Trigger.PlayInterruptiblePrompt
                : Trigger.PlayPrompt);
        }

        public async Task PlayRecordingToLineAsync(string recordingName, string lineId)
        {
            try
            {
                _currentPlaybackId = await _sipPromptApi.PlayRecordingToLineAsync(lineId, recordingName);
            }
            catch (Exception e)
            {
                _arkeCall.Logger.Error(e, $"Error Playing Prompt: {e.Message}");
                if (_arkeCall.GetCurrentState() != State.HangUp)
                    await _arkeCall.CallStateMachine.FireAsync(Trigger.FinishCall);
            }
        }

        public async Task PlayNumberToLineAsync(string number, string lineId)
        {
            _promptQueue.Clear();
            await _arkeCall.CallStateMachine.FireAsync(Trigger.PlayPrompt);
            _currentPlaybackId =
                await _sipPromptApi.PlayNumberToLineAsync(lineId, number, _languageData.FolderName);
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

        public async Task PlayPromptsInQueueAsync()
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
                            await PlayPromptToIncomingLineAsync(prompt.PromptFile).ConfigureAwait(false);
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
                await _arkeCall.CallStateMachine.FireAsync(Trigger.FinishedPrompt);
            }
        }

        public async Task StopPromptAsync()
        {
            if (_arkeCall.GetCurrentState() == State.PlayingPrompt)
                throw new InvalidOperationException("Cannot stop playback of this prompt.");
            await _arkeCall.CallStateMachine.FireAsync(Trigger.PromptInterrupted);
            _arkeCall.Logger.Debug($"Stopping Prompt {_currentPlaybackId}", _arkeCall.LogData);
            await _sipPromptApi.StopPromptAsync(_currentPlaybackId).ConfigureAwait(false);
            _promptQueue.Clear();
            await _arkeCall.CallStateMachine.FireAsync(Trigger.FinishedPrompt);
        }

        public async Task PlayPromptToBridge(string promptFile)
        {
            try
            {
                _currentPlaybackId = (await _sipPromptApi.PlayPromptToBridgeAsync(
                    _arkeCall.CallState.GetBridgeId(),
                    promptFile, _languageData.FolderName).ConfigureAwait(false));
                _arkeCall.Logger.Debug($"Prompt file is: {promptFile}");
                _arkeCall.Logger.Debug($"Prompt ID: {_currentPlaybackId}", _arkeCall.LogData);
            }
            catch (Exception ex)
            {
                _arkeCall.Logger.Error(ex, $"Error Playing Prompt: {ex.Message}");
                if (_arkeCall.GetCurrentState() != State.HangUp)
                    await _arkeCall.CallStateMachine.FireAsync(Trigger.FinishCall);
            }
        }

        public async Task PlayPromptToOutgoingLine(string promptFile)
        {
            try
            {
                _currentPlaybackId = (await _sipPromptApi.PlayPromptToLineAsync(
                    _arkeCall.CallState.GetOutgoingLineId(),
                    promptFile, _languageData.FolderName).ConfigureAwait(false));
                _arkeCall.Logger.Debug($"Prompt file is: {promptFile}");
                _arkeCall.Logger.Debug($"Prompt ID: {_currentPlaybackId}", _arkeCall.LogData);
            }
            catch (Exception ex)
            {
                _arkeCall.Logger.Error(ex, $"Error Playing Prompt: {ex.Message}");
                if (_arkeCall.GetCurrentState() != State.HangUp)
                    await _arkeCall.CallStateMachine.FireAsync(Trigger.FinishCall);
            }
        }

        public async Task PlayPromptToIncomingLineAsync(string promptFile)
        {
            try
            {
                _currentPlaybackId = (await _sipPromptApi.PlayPromptToLineAsync(
                    _arkeCall.CallState.GetIncomingLineId(),
                    promptFile, _languageData.FolderName).ConfigureAwait(false));
                _arkeCall.Logger.Debug($"Prompt file is: {promptFile}");
                _arkeCall.Logger.Debug($"Prompt ID: {_currentPlaybackId}", _arkeCall.LogData);
            }
            catch (Exception ex)
            {
                _arkeCall.Logger.Error(ex, $"Error Playing Prompt: {ex.Message}");
                if (_arkeCall.GetCurrentState() != State.HangUp)
                    await _arkeCall.CallStateMachine.FireAsync(Trigger.FinishCall);
            }
        }

        public async Task AriClient_OnPlaybackFinishedEvent(ISipApiClient sipApiClient, PromptPlaybackFinishedEvent e)
        {
            if (e.PlaybackId != _currentPlaybackId)
                return;
            if (_arkeCall.GetCurrentState() == State.StoppingPlayback)
                return;

            await _arkeCall.CallStateMachine.FireAsync(
                _promptQueue.Count == 0 ? Trigger.FinishedPrompt : Trigger.PlayNextPrompt);
        }

        public void SetLanguageCode(LanguageData language)
        {
            _languageData = language;
        }
    }
}