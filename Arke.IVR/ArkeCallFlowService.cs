using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arke.DependencyInjection;
using Arke.IVR.CallObjects;
using Arke.SipEngine;
using Arke.SipEngine.CallObjects;
using AsterNET.ARI;
using AsterNET.ARI.Models;
using Microsoft.Extensions.Configuration;
using Serilog;

#if !MONO
#endif

namespace Arke.IVR
{
    public class ArkeCallFlowService : ICallFlowService
    {
        private readonly IAriClient _ariClient;
        public Dictionary<string, ICall> ConnectedLines { get; set; }
        private readonly ILogger _logger;
        public static IConfiguration Configuration { get; set; }
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ArkeSipApiClient _sipApi;
        private List<string> processingCalls = new List<string>();
        
        public ArkeCallFlowService(ILogger logger)
        {
            _logger = logger;
            _logger.Information("ArkeCallFlowService Created");
            ConnectedLines = new Dictionary<string, ICall>();
            _cancellationTokenSource = new CancellationTokenSource();
            _ariClient = ObjectContainer.GetInstance().GetObjectInstance<IAriClient>();
            _sipApi = ObjectContainer.GetInstance().GetObjectInstance<ArkeSipApiClient>();
        }
        
        public static string GetConfigValue(string key)
        {
            return Configuration?[key];
        }

        public bool Start()
        {
            try
            {
                _logger.Information("ArkeCallFlowService Start() Initiated");
                SetupAriEvents();
                ConnectAri();
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error Setting up CallFlowServices.");
                return false;
            }
        }
        
        public virtual void ConnectAri()
        {
            ((AriClient)_ariClient).Connect();
        }
        
        public virtual void SetupAriEvents()
        {
            _ariClient.OnStasisStartEvent += AriClientOnStasisStartEvent;
            _ariClient.OnStasisEndEvent += AriClientOnStasisEndEvent;
            _logger.Information("Registering Events");
            _sipApi.SubscribeToDtmfReceivedEvents();
            _sipApi.SubscribeToLineHangupEvents();
            _sipApi.SubscribeToPlaybackFinishedEvents();
        }
        
        public bool Stop()
        {
            _logger.Information("Unregistering events...");
            var disconnectionTask = DisconnectAri();
            disconnectionTask.Wait();
            _cancellationTokenSource.Cancel();
            _logger.Information("Shutdown complete.");
            return true;
        }

        public virtual async Task DisconnectAri()
        {
            _sipApi.UnsubscribeToDtmfReceivedEvents();
            _sipApi.UnsubscribeToPlaybackFinishedEvents();
            if (!((AriClient) _ariClient).Connected)
                return;
            await EndAllCallsAsync();
            ((AriClient) _ariClient).Disconnect();
            _sipApi.UnsubscribeToDtmfReceivedEvents();
            _sipApi.UnsubscribeToPlaybackFinishedEvents();
        }

        public bool Continue()
        {
            ConnectAri();
            return true;
        }

        public bool Pause()
        {
            var disconnectTask = DisconnectAri();
            disconnectTask.Wait();
            return true;
        }

        
        [SuppressMessage("ReSharper", "FormatStringProblem", Justification = "NLog will use args in the output format instead of string format.")]
        private async void AriClientOnStasisStartEvent(IAriClient sender, StasisStartEvent e)
        {
            _logger.Debug($"Line Connecting: {e.Channel.Name}");
            
            ICall line;
            if (e.Args.Contains("dialed") || e.Args.Contains("SnoopChannel"))
                return;
            _logger.Information("Line Offhook", new
            {
                ChannelId = e.Channel.Id,
                CallerIdName = e.Channel.Caller.Number,
                CallerIdNumber = e.Channel.Caller.Name
            });
            line = ArkeCallFactory.CreateArkeCall(e.Channel);
            ConnectedLines.Add(e.Channel.Id, line);
            _logger.Information("Starting Call Script", new
            {
                ChannelId = e.Channel.Id
            });
        
            // call answered and started
            await line.RunCallScriptAsync(_cancellationTokenSource.Token);
            await Task.Delay(1000);
            _logger.Information("Call Script Complete", new { ChannelId = e.Channel.Id });
        }

        private async void AriClientOnStasisEndEvent(IAriClient sender, StasisEndEvent stasisEndEvent)
        {
            _logger.Information("Channel {channelId} hungup", new { channelId = stasisEndEvent.Channel.Id});
            if (!ConnectedLines.ContainsKey(stasisEndEvent.Channel.Id))
                return;

            try
            {
                await ConnectedLines[stasisEndEvent.Channel.Id].HangupAsync();
            }
            catch (AriException e)
            {
                _logger.Warning($"Exception while hanging up, most likely ok: {e.Message}");
            }
            
            while (!ConnectedLines[stasisEndEvent.Channel.Id].CallState.CallCanBeAbandoned)
            {
                await Task.Delay(1000);
            }
            ConnectedLines.Remove(stasisEndEvent.Channel.Id);
        }

        private async Task EndAllCallsAsync()
        {
            _cancellationTokenSource.Cancel();
            foreach (var line in ConnectedLines.Where(c => c.Value.CallState.CallCanBeAbandoned))
            {
                await line.Value.HangupAsync();
            }
            foreach (var line in ConnectedLines.Where(c => !c.Value.CallState.CallCanBeAbandoned))
            {
                while (!line.Value.CallState.CallCanBeAbandoned)
                    await Task.Delay(1000);
                await line.Value.HangupAsync();
            }
        }
    }
}