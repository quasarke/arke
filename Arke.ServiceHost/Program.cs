﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using Arke.DependencyInjection;
using Arke.IVR;
using Arke.IVR.CallObjects;
using Arke.IVR.Recording;
using Arke.SipEngine;
using Arke.SipEngine.Api;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Interfaces;
using Arke.SipEngine.Services;
using AsterNET.ARI;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using SimpleInjector;

namespace Arke.ServiceHost
{
    [SuppressMessage("ReSharper", "FormatStringProblem", Justification = "NLog will use args in the output format instead of string format.")]
    internal static class Program
    {
        private static ILogger _logger;
        private static AriClient _ariClient;
        private static ArkeSipApiClient _sipApi;
        private static string _pluginDirectory = "/app";
        private static IConfiguration _configuration;
        private static ICallFlowService _service;
        private static CancellationTokenSource _cancellationToken;

        public static ICallFlowService GetCurrentCallEngine() => _service;

        public static void Main(string[] args)
        {
            InitializeConfigurationFileDependencies();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
                .CreateLogger();
            _logger = Log.Logger;

            _logger.Information($"Server Hostname is {Dns.GetHostName()}");
            RegisterDependencies();
            LoadPlugins();
            
            _logger.Debug("Configuration Loaded.");
            SetupAriEndpoint();
            _logger.Information("Verifying DI Container", new { DIContainer = "SimpleInjector"});
            ObjectContainer.GetInstance().Verify();
            
            _logger.Information("Service running, press CTRL-C to terminate.");

            try
            {
                _logger.Information("Starting Web Host services.");
                BuildWebHost(args).RunAsync();
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "Host terminated unexpectedly.");
            }

            _service = ObjectContainer.GetInstance().GetObjectInstance<ICallFlowService>();
            _service.Start();
            AssemblyLoadContext.Default.Unloading += SigTermEventHandler;
            Console.CancelKeyPress += CancelHandler;

            _logger.Information("Service running, press CTRL-C to terminate.");
            while (true)
            {
                Thread.Sleep(200);
            }
        }

        private static void CancelHandler(object sender, ConsoleCancelEventArgs e)
        {
            _cancellationToken.Cancel();
            _service.Stop();
        }

        private static void SigTermEventHandler(AssemblyLoadContext obj)
        {
            _cancellationToken.Cancel();
            _service.Stop();
        }

        private static IHost BuildWebHost(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(options =>
                        {
                            options.ListenAnyIP(5000);
                        })
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseStartup<WebApiStartup>();
                })
                .Build();

        private static void LoadPlugins()
        {
            _pluginDirectory = ArkeCallFlowService.GetConfigValue("appSettings:PluginDirectory");
            var assemblies =
                from file in new DirectoryInfo(_pluginDirectory).GetFiles()
                where string.Equals(file.Extension, ".dll", StringComparison.InvariantCultureIgnoreCase)
                select Assembly.Load(AssemblyLoadContext.GetAssemblyName(file.FullName));

            ObjectContainer.GetInstance().GetSimpleInjectorContainer().RegisterPackages(assemblies);
        }

        private static void InitializeConfigurationFileDependencies()
        {
            _configuration = GetAppSettingsByHostName();
            ArkeCallFlowService.Configuration = _configuration;
        }
        
        public static void SetupAriEndpoint()
        {
            _logger.Information("Creating Endpoint");
            var appName = ArkeCallFlowService.Configuration.GetSection("appSettings:AsteriskAppName").Value;

            var endpoint = new StasisEndpoint(
                ArkeCallFlowService.Configuration.GetSection("appSettings:AsteriskHost").Value,
                8088,
                ArkeCallFlowService.Configuration.GetSection("appSettings:AsteriskUser").Value,
                ArkeCallFlowService.Configuration.GetSection("appSettings:AsteriskPassword").Value
                );
            _logger.Information("Registering endpoint with AriClient");
            _ariClient = new AriClient(endpoint,
                appName);

            _logger.Information("Adding AriClient to CallFlowService");

            var container = ObjectContainer.GetInstance();
            container.RegisterSingleton<IAriClient>(() => _ariClient);
            _sipApi = new ArkeSipApiClient(_ariClient, _logger);
            container.RegisterSingleton(_sipApi);
            _logger.Information("Registering API Layer");
            container.RegisterSingleton<ISipApiClient>(_sipApi);
            container.RegisterSingleton<ISipBridgingApi>(_sipApi);
            container.RegisterSingleton<ISipLineApi>(_sipApi);
            container.RegisterSingleton<ISipPromptApi>(_sipApi);
            container.RegisterSingleton<ISipRecordingApi>(_sipApi);
            container.RegisterSingleton<ISoundsApi>(_sipApi);
        }

        private static void RegisterDependencies()
        {
            _logger.Information("Creating Container.");
            var container = ObjectContainer.GetInstance();
            _logger.Information("Registering Dependencies");
            container.RegisterSingleton<ILogger>(Log.Logger);
            container.Register<IServiceClientBuilder, ServiceClientBuilder>();
            container.RegisterSingleton<ICallFlowService>(() => new ArkeCallFlowService(Log.Logger));
            container.Register<ICall, ArkeCall>(ObjectLifecycle.Transient);
            _logger.Information("Dependencies registered.");
        }
        
        public static IConfiguration GetAppSettingsByHostName()
        {
            var hostName = Dns.GetHostName();
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            configBuilder.AddJsonFile($"appsettings.{hostName}.json", optional: true, reloadOnChange: true);
            configBuilder.AddEnvironmentVariables();
            return configBuilder.Build();
        }
    }
}
