using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Loader;
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
using NLog;
using NLog.Web;
using SimpleInjector;

namespace Arke.ServiceHost
{
    [SuppressMessage("ReSharper", "FormatStringProblem", Justification = "NLog will use args in the output format instead of string format.")]
    internal static class Program
    {
        private static Logger _logger;
        private static AriClient _ariClient;
        private static ArkeSipApiClient _sipApi;
        private static string _pluginDirectory = "c:\\Arke\\Plugins";

        public static void Main(string[] args)
        {
            _logger = LogManager.GetCurrentClassLogger();
            RegisterDependencies();
            InitializeConfigurationFileDependencies();
            LoadPlugins();
            
            _logger.Debug("Configuration Loaded.");
            SetupAriEndpoint();
            _logger.Info("Verifying DI Container", new { DIContainer = "SimpleInjector"});
            ObjectContainer.GetInstance().Verify();
            
            var service = new ArkeCallFlowService();
            service.Start();
            
            _logger.Info("Service running, press CTRL-C to terminate.");

            try
            {
                _logger.Info("Starting Web Host services.");
                BuildWebHost(args).Run();
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "Host terminated unexpectedly.");
            }
        }

        private static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<WebApiStartup>()
                .UseNLog()
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
            ArkeCallFlowService.Configuration = GetAppSettingsByHostName();
        }
        
        public static void SetupAriEndpoint()
        {
            _logger.Info("Creating Endpoint");
            var appName = ArkeCallFlowService.Configuration.GetSection("appSettings:AsteriskAppName").Value;

            var endpoint = new StasisEndpoint(
                ArkeCallFlowService.Configuration.GetSection("appSettings:AsteriskHost").Value,
                8088,
                ArkeCallFlowService.Configuration.GetSection("appSettings:AsteriskUser").Value,
                ArkeCallFlowService.Configuration.GetSection("appSettings:AsteriskPassword").Value
                );
            _logger.Info("Registering endpoint with AriClient");
            _ariClient = new AriClient(endpoint,
                appName);

            _logger.Info("Adding AriClient to CallFlowService");

            var container = ObjectContainer.GetInstance();
            container.RegisterSingleton<IAriClient>(() => _ariClient);
            _sipApi = new ArkeSipApiClient(_ariClient);
            container.RegisterSingleton(_sipApi);
            _logger.Info("Registering API Layer");
            container.RegisterSingleton<ISipApiClient>(_sipApi);
            container.RegisterSingleton<ISipBridgingApi>(_sipApi);
            container.RegisterSingleton<ISipLineApi>(_sipApi);
            container.RegisterSingleton<ISipPromptApi>(_sipApi);
            container.RegisterSingleton<ISipRecordingApi>(_sipApi);
            container.RegisterSingleton<ISoundsApi>(_sipApi);
        }

        private static void RegisterDependencies()
        {
            _logger.Info("Creating Container.");
            var container = ObjectContainer.GetInstance();
            _logger.Info("Registering Dependencies");
            container.Register<IServiceClientBuilder, ServiceClientBuilder>();
            container.Register<IRecordingManager, ArkeRecordingManager>();
            container.Register<ICallFlowService, ArkeCallFlowService>();
            container.Register<ICall, ArkeCall>(ObjectLifecycle.Transient);
            _logger.Info("Dependencies registered.");
        }
        
        public static IConfiguration GetAppSettingsByHostName()
        {
            _logger.Info("Loading Configuration");
            var hostName = Dns.GetHostName();
            _logger.Info($"Server Hostname is {hostName}");
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("appsettings.json"); 
            _logger.Info("Building config.");
            return configBuilder.Build();
        }
    }
}
