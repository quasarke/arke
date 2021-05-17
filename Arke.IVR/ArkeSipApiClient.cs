using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Arke.IVR.Bridging;
using Arke.SipEngine.Api;
using Arke.SipEngine.Api.Models;
using Arke.SipEngine.Bridging;
using Arke.SipEngine.CallObjects.RecordingFiles;
using Arke.SipEngine.Events;
using AsterNET.ARI;
using Serilog;
using RecordingFinishedEventHandler = Arke.SipEngine.Api.RecordingFinishedEventHandler;

namespace Arke.IVR
{
    public class ArkeSipApiClient : ISipApiClient, ISipBridgingApi, ISipLineApi, ISipPromptApi, ISipRecordingApi, ISoundsApi
    {
        private readonly IAriClient _ariClient;
        private string _appName;
        private readonly ILogger _logger;
        
        [SuppressMessage("NDepend", "ND1901:AvoidNonReadOnlyStaticFields", Justification="Singleton Pattern")]
        private static ArkeSipApiClient _instance;

        public ArkeSipApiClient(IAriClient ariClient, ILogger logger)
        {
            _logger = logger;
            _ariClient = ariClient;
            _appName = ArkeCallFlowService.Configuration.GetSection("appSettings:AsteriskAppName").Value;
        }

        public event DtmfReceivedEventHandler OnDtmfReceivedEvent;
        public event LineHangupEventHandler OnLineHangupAsyncEvent;
        public event RecordingFinishedEventHandler OnRecordingFinishedAsyncEvent;
        public event PromptPlaybackFinishedEventHandler OnPromptPlaybackFinishedAsyncEvent;

        public static ArkeSipApiClient GetInstance(IAriClient ariClient, ILogger logger)
        {
            return _instance ??= new ArkeSipApiClient(ariClient, logger);
        }

        public async Task AnswerLineAsync(string lineId)
        {
            await _ariClient.Channels.AnswerAsync(lineId).ConfigureAwait(false);
        }

        public async Task TransferLineAsync(string lineId, string endpoint)
        {
            try
            {
                await _ariClient.Channels.RedirectAsync(lineId, endpoint);
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Error transferring line");
            }
        }

        public async Task PlayMusicOnHoldToLineAsync(string channelId)
        {
            try
            {
                await _ariClient.Channels.StartMohAsync(channelId).ConfigureAwait(false);
            }
            catch (HttpRequestException e)
            {
                _logger.Warning(e, "Channel probably dead");
            }
        }

        public async Task StopMusicOnHoldToLineAsync(string channelId)
        {
            try
            {
                await _ariClient.Channels.StopMohAsync(channelId).ConfigureAwait(false);
            }
            catch (HttpRequestException e)
            {
                _logger.Warning(e, "Channel probably dead");
            }
        }

        public async Task<string> GetLineVariableAsync(string lineId, string variableName)
        {
            var variable = await _ariClient.Channels.GetChannelVarAsync(lineId, variableName).ConfigureAwait(false);
            return variable.Value;
        }

        public async Task<string> GetEndpointAsync(string lineId)
        {
            var getChannelVarResult = await _ariClient.Channels.GetChannelVarAsync(lineId, "CHANNEL(pjsip,remote_addr)").ConfigureAwait(false);
            _logger.Debug("GetEndpointAsync", new {LineId = lineId, Result = getChannelVarResult.Value});
            return getChannelVarResult.Value.Split(':')[0];
        }

        public async Task<IBridge> CreateBridgeAsync(string bridgeType, string bridgeName)
        {
            var asteriskBridge = await _ariClient.Bridges.CreateAsync(bridgeType, Guid.NewGuid().ToString(), bridgeName).ConfigureAwait(false);
            var artemisBridge = new ArkeBridge()
            {
                Id = asteriskBridge.Id,
                Type = asteriskBridge.Bridge_type
            };
            return artemisBridge;
        }

        public async Task AddLineToBridgeAsync(string lineId, string bridgeId)
        {
            await _ariClient.Bridges.AddChannelAsync(bridgeId, lineId).ConfigureAwait(false);
        }

        public async Task RemoveLineFromBridgeAsync(string lineId, string bridgeId)
        {
            await _ariClient.Bridges.RemoveChannelAsync(bridgeId, lineId).ConfigureAwait(false);
        }

        public async Task DestroyBridgeAsync(string bridgeId)
        {
            await _ariClient.Bridges.DestroyAsync(bridgeId).ConfigureAwait(false);
        }

        public async Task HangupLineAsync(string lineId)
        {
            try
            {
                await _ariClient.Channels.HangupAsync(lineId, reason: "normal").ConfigureAwait(false);
            }
            catch (HttpRequestException e)
            {
                _logger.Warning(e, "Channel probably dead");
            }
        }

        public async Task StopRecordingAsync(string recordingId)
        {
            await _ariClient.Recordings.StopAsync(recordingId).ConfigureAwait(false);
        }

        public async Task<string> StartRecordingOnLineAsync(string lineId, string fileName)
        {
            var snoopingChannel = await _ariClient.Channels.SnoopChannelAsync(lineId,
                _appName,
                "in",
                null,
                "SnoopChannel").ConfigureAwait(false);
            var channelRecording = await _ariClient.Channels.RecordAsync(snoopingChannel.Id, fileName,
                new ArkeFileFormatFactory().GetFileFormat(), 0, 0, "fail", false, "none").ConfigureAwait(false);
            return channelRecording.Name;
        }

        public async Task<string> StartRecordingOnBridgeAsync(string bridgeId, string fileName)
        {
            var recording = await _ariClient.Bridges.RecordAsync(bridgeId,
                fileName,
                new ArkeFileFormatFactory().GetFileFormat(),
                0,
                0,
                "fail",
                false,
                "none").ConfigureAwait(false);
            return recording.Name;
        }

        public async Task<string> StartShortRecordingForLineAsync(string lineId, string fileName, int maxDurationSeconds, int maxSilenceSeconds,
            bool beepOnStart)
        {
            return (await _ariClient.Channels
                .RecordAsync(lineId, fileName, "wav", maxDurationSeconds, maxSilenceSeconds, "overwrite", beepOnStart)
                .ConfigureAwait(false)).Name;
        }

        public async Task StopRecordingOnBridge(string recordingId)
        {
            await _ariClient.Recordings.StopAsync(recordingId).ConfigureAwait(false);
        }

        public async Task<string> PlayPromptToLineAsync(string lineId, string promptFile, string languageCode)
        {
            try
            {
                return (await _ariClient.Channels.PlayAsync(lineId, $"sound:{promptFile}", languageCode).ConfigureAwait(false)).Id;
            }
            catch (HttpRequestException e)
            {
                _logger.Warning(e, "Channel probably dead");
                return "";
            }
        }

        public async Task<string> PlayPromptToBridgeAsync(string bridgeId, string promptFile, string languageCode)
        {
            return (await _ariClient.Bridges.PlayAsync(bridgeId, $"sound:{promptFile}", languageCode).ConfigureAwait(false)).Id;
        }

        public async Task<string> PlayRecordingToLineAsync(string lineId, string recordingName)
        {
            return (await _ariClient.Channels.PlayAsync(lineId, $"recording:{recordingName}").ConfigureAwait(false)).Id;
        }

        public async Task<string> PlayNumberToLineAsync(string lineId, string number, string languageCode)
        {
            try
            {
                return (await _ariClient.Channels.PlayAsync(lineId, $"number:{number}", languageCode).ConfigureAwait(false)).Id;
            }
            catch (HttpRequestException e)
            {
                _logger.Warning(e, "Channel probably dead");
                return "";
            }
        }

        public async Task StopPromptAsync(string playbackId)
        {
            try
            {
                await _ariClient.Playbacks.StopAsync(playbackId).ConfigureAwait(false);
            }
            catch (HttpRequestException e)
            {
                _logger.Warning(e, "Channel probably dead");
            }
        }

        public async Task PlayMusicOnHoldToBridgeAsync(string bridgeId)
        {
            await _ariClient.Bridges.StartMohAsync(bridgeId).ConfigureAwait(false);
        }

        public async Task<object> CreateOutboundCallAsync(string numberToDial, string outboundEndpoint)
        {
            try
            {
                return await _ariClient.Channels.OriginateAsync(
                    outboundEndpoint,
                    numberToDial,
                    app: _appName, // need to test if this is needed
                    appArgs: "dialed").ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error creating an outbound call");
                throw;
            }
        }

        public async Task<ICollection<Sound>> GetSoundsOnEngineAsync()
        {
            var sounds = await _ariClient.Sounds.ListAsync().ConfigureAwait(false);
            var soundsResponse = new List<Sound>();
            foreach (var sound in sounds)
            {
                var soundObj = new Sound()
                {
                    Id = sound.Id,
                    Text = sound.Text,
                    Formats = new List<SoundFormat>()
                };
                foreach (var format in sound.Formats)
                    soundObj.Formats.Add(new SoundFormat() { Format = format.Format, Language = format.Language });
                soundsResponse.Add(soundObj);
            }
            return soundsResponse;
        }

        public async Task<string> GetLineStateAsync(string lineId)
        {
            return (await _ariClient.Channels.GetAsync(lineId).ConfigureAwait(false)).State;
        }

        public void SubscribeToDtmfReceivedEvents()
        {
            _ariClient.OnChannelDtmfReceivedEvent += AriClient_OnChannelDtmfReceivedEvent;
        }

        public void SubscribeToPlaybackFinishedEvents()
        {
            _ariClient.OnPlaybackFinishedEvent += AriClient_OnPlaybackFinishedEvent;
        }

        public void UnsubscribeToDtmfReceivedEvents()
        {
            _ariClient.OnChannelDtmfReceivedEvent -= AriClient_OnChannelDtmfReceivedEvent;
        }

        public void UnsubscribeToPlaybackFinishedEvents()
        {
            _ariClient.OnPlaybackFinishedEvent -= AriClient_OnPlaybackFinishedEvent;
        }

        public void SubscribeToLineHangupEvents()
        {
            _ariClient.OnStasisEndEvent += AriClient_OnStasisEndEvent;
        }

        public void SetAppNameForEvents(string appName)
        {
            _appName = appName;
        }

        private void AriClient_OnStasisEndEvent(IAriClient sender, AsterNET.ARI.Models.StasisEndEvent e)
        {
            OnLineHangupAsyncEvent?.Invoke(this, new LineHangupEvent()
            {
                LineId = e.Channel.Id
            });
        }

        private void AriClient_OnPlaybackFinishedEvent(IAriClient sender, AsterNET.ARI.Models.PlaybackFinishedEvent e)
        {
            OnPromptPlaybackFinishedAsyncEvent?.Invoke(this, new PromptPlaybackFinishedEvent()
            {
                PlaybackId = e.Playback.Id
            });
        }

        private void AriClient_OnChannelDtmfReceivedEvent(IAriClient sender, AsterNET.ARI.Models.ChannelDtmfReceivedEvent e)
        {
            OnDtmfReceivedEvent?.Invoke(this, new DtmfReceivedEvent()
            {
                Digit = e.Digit,
                DurationInMilliseconds = e.Duration_ms,
                LineId = e.Channel.Id
            });
        }

        public async Task<object> CreateOutboundCallAsync(string numberToDial, string callerId, string outboundEndpoint)
        {
            try
            {
                return await _ariClient.Channels.OriginateAsync(
                    outboundEndpoint,
                    numberToDial,
                    callerId: callerId,
                    app: _appName, // need to test if this is needed
                    appArgs: "dialed").ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error creating an outbound call");
                throw;
            }
        }

        public async Task MuteLineAsync(string lineId)
        {
            await _ariClient.Channels.MuteAsync(lineId, "out");
        }

        public async Task UnmuteLineAsync(string lineId)
        {
            await _ariClient.Channels.UnmuteAsync(lineId, "both");
        }
    }
}
