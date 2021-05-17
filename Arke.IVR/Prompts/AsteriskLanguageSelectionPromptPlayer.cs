using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arke.SipEngine.Api;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Events;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.IVR.Prompts
{
    public class AsteriskLanguageSelectionPromptPlayer : ILanguageSelectionPromptPlayer
    {
        private const string LanguageCode = "en";

        private readonly ICall _asteriskCall;
        private readonly Queue<IPrompt> _promptQueue;
        private ILanguageStepProcessor _stepProcessor;
        private readonly ISipPromptApi _sipPromptApi;
        private readonly ISipApiClient _sipApiClient;

        private string _currentPlaybackId;

        public AsteriskLanguageSelectionPromptPlayer(ICall asteriskCall, ISipPromptApi sipPromptApi, ISipApiClient sipApiClient)
        {
            _asteriskCall = asteriskCall;
            _promptQueue = new Queue<IPrompt>();
            _sipPromptApi = sipPromptApi;
            _sipApiClient = sipApiClient;
            _sipApiClient.OnPromptPlaybackFinishedAsyncEvent += HandlePromptFinishedEvent;
        }

        public void AddPromptsToQueue(List<string> prompts)
        {
            foreach (var prompt in prompts)
                _promptQueue.Enqueue(new Prompt()
                {
                    PromptFile = prompt
                });

            _asteriskCall.Logger.Debug($"Added prompts to queue, queue size: {_promptQueue.Count}", _asteriskCall.LogData);
        }

        public void PlayPromptTextToSpeech(string promptText)
        {
            throw new NotImplementedException("Asterisk Engine currently does not support TTS");
        }

        public async Task PlayNextPromptInQueue()
        {
            if (_asteriskCall.GetCurrentState() == State.StoppingPlayback || _promptQueue.Count == 0)
                return;
            _asteriskCall.Logger.Debug($"Playing next prompt in queue, queue size: {_promptQueue.Count}", _asteriskCall.LogData);
            try
            {
                var prompt = _promptQueue.Dequeue();
                if (prompt != null)
                    await PlayPromptFile(prompt.PromptFile);
            }
            catch (Exception e)
            {
                _asteriskCall.Logger.Error(e, "Error playing prompt in queue.");
                await _asteriskCall.FireStateChange(Trigger.FinishCall);
            }
        }

        public async Task PlayPromptFile(string promptFile)
        {
            try
            {
                _currentPlaybackId = await _sipPromptApi.PlayPromptToLineAsync(
                    _asteriskCall.CallState.GetIncomingLineId(), promptFile, LanguageCode)
                    .ConfigureAwait(false);
                _asteriskCall.Logger.Debug($"Prompt ID: {_currentPlaybackId}", _asteriskCall.LogData);
            }
            catch (Exception ex)
            {
                _asteriskCall.Logger.Error(ex, $"Error Playing Prompt: {ex.Message}");
                CleanupEventHooks();
                if (_asteriskCall.GetCurrentState() != State.HangUp)
                    await _asteriskCall.FireStateChange(Trigger.FinishCall);
            }
        }

        public async Task HaltPromptPlayback()
        {
            if (_asteriskCall.GetCurrentState() == State.PlayingPrompt)
                throw new InvalidOperationException("Cannot stop playback of this prompt.");
            _asteriskCall.Logger.Debug($"Stopping Prompt {_currentPlaybackId}", _asteriskCall.LogData);
            var stopPlaybackTask = _sipPromptApi.StopPromptAsync(_currentPlaybackId).ConfigureAwait(false);
            _promptQueue.Clear();
            try
            {
                await stopPlaybackTask;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("playback cannot be found"))
                    _asteriskCall.Logger.Warning("Playback ID Missing. Probably don't need to call this.");
                else
                    _asteriskCall.Logger.Error(e, "Problem stopping playback.");
            }
        }

        public void AddPromptToQueue(IPrompt prompt)
        {
            _asteriskCall.Logger.Debug($"Adding prompt to queue, queue size: {_promptQueue.Count}", _asteriskCall.LogData);
            _promptQueue.Enqueue(prompt);
        }

        public void CleanupEventHooks()
        {
            _sipApiClient.OnPromptPlaybackFinishedAsyncEvent -= HandlePromptFinishedEvent;
        }

        public void SetStepProcessor(ILanguageStepProcessor languageStepProcessor)
        {
            _stepProcessor = languageStepProcessor;
        }

        public async Task HandlePromptFinishedEvent(ISipApiClient sipApiClient, PromptPlaybackFinishedEvent e)
        {
            if (e.PlaybackId != _currentPlaybackId)
                return;
            if (_asteriskCall.GetCurrentState() == State.StoppingPlayback)
                return;
            if (_promptQueue.Count == 0)
            {
                if (_asteriskCall.GetCurrentState() != State.LanguageInput)
                    await _asteriskCall.FireStateChange(Trigger.GetLanguageInput);
                _stepProcessor.StartTimeoutTimer();
            }
            else
                await PlayNextPromptInQueue();
        }
    }
}
