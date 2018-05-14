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
using Datadog.Trace;
using Microsoft.Extensions.Configuration;
using NLog;

#if !MONO
#endif

namespace Arke.IVR
{
    public class ArkeCallFlowService : ICallFlowService
    {
        private readonly IAriClient _ariClient;
        public Dictionary<string, ICall> ConnectedLines { get; set; }
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public static IConfiguration Configuration { get; set; }
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ArkeSipApiClient _sipApi;
        
        public ArkeCallFlowService()

        {
            _logger.Info("ArkeCallFlowService Created");
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
                _logger.Info("ArkeCallFlowService Start() Initiated");
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
            _logger.Info("Registering Events");
            _sipApi.SubscribeToDtmfReceivedEvents();
            _sipApi.SubscribeToLineHangupEvents();
            _sipApi.SubscribeToPlaybackFinishedEvents();
        }
        
        public bool Stop()
        {
            _logger.Info("Unregistering events...");
            var disconnectionTask = DisconnectAri();
            disconnectionTask.Wait();
            _cancellationTokenSource.Cancel();
            _logger.Info("Shutdown complete.");
            return true;
        }

        public virtual async Task DisconnectAri()
        {
            _sipApi.UnsubscribeToDtmfReceivedEvents();
            _sipApi.UnsubscribeToPlaybackFinishedEvents();
            if (!((AriClient) _ariClient).Connected)
                return;
            await EndAllCalls();
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
            var callScope = Tracer.Instance.StartActive("NewCall");
            ICall line;
            using (var answerScope = Tracer.Instance.StartActive("AnswerCall"))
            {
                if (e.Args.Contains("dialed") || e.Args.Contains("SnoopChannel"))
                    return;
                callScope.Span.SetTag("ChannelId", e.Channel.Id);
                _logger.Info("Line Offhook", new
                {
                    ChannelId = e.Channel.Id,
                    CallerIdName = e.Channel.Caller.Number,
                    CallerIdNumber = e.Channel.Caller.Name
                });
                line = ArkeCallFactory.CreateArkeCall(e.Channel, callScope);
                ConnectedLines.Add(e.Channel.Id, line);
                _logger.Info("Starting Call Script", new
                {
                    ChannelId = e.Channel.Id
                });
            }
            // call answered and started
            await line.RunCallScript();
            _logger.Info("Call Script Complete", new { ChannelId = e.Channel.Id });
            callScope.Close();
        }

        private async void AriClientOnStasisEndEvent(IAriClient sender, StasisEndEvent stasisEndEvent)
        {
            _logger.Info(stasisEndEvent.Channel.Id);
            if (!ConnectedLines.ContainsKey(stasisEndEvent.Channel.Id))
                return;
            ConnectedLines[stasisEndEvent.Channel.Id].Hangup();
            
            while (!ConnectedLines[stasisEndEvent.Channel.Id].CallState.CallCanBeAbandoned)
            {
                await Task.Delay(1000);
            }
            ConnectedLines.Remove(stasisEndEvent.Channel.Id);
        }

        private async Task EndAllCalls()
        {
            foreach (var line in ConnectedLines.Where(c => c.Value.CallState.CallCanBeAbandoned))
            {
                line.Value.Hangup();
            }
            foreach (var line in ConnectedLines.Where(c => !c.Value.CallState.CallCanBeAbandoned))
            {
                while (!line.Value.CallState.CallCanBeAbandoned)
                    await Task.Delay(1000);
                line.Value.Hangup();
            }
        }
    }
}