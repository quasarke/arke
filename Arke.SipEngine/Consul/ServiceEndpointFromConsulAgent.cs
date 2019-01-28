using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arke.SipEngine.Exceptions;
using Arke.SipEngine.Web;
using Arke.SipEngine.Web.Default;
using NLog;

namespace Arke.SipEngine.Consul
{
    public class ServiceEndpointFromConsulAgent
    {
        private readonly Uri _consulNetworkUrl = new Uri("http://172.28.128.3:8500");
        private const string ServiceQueryParameter = "/v1/catalog/service/{appName}";
        private const string NodeQueryParameter = "/v1/catalog/nodes";
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        
        public async Task<string> GetServiceEndpointFromConsulAgent(string appName)
        {
            IRestCommandResult<List<CatalogService>> results = null;
            try
            {
                results = await GetServiceEndpointsFromConsul(appName);
            }
            catch (NullReferenceException ex)
            {
                _logger.Error(ex, appName);
            }
            catch (ServiceNotRunningException ex)
            {
                _logger.Error(ex, appName);
            }

            var selectedEndpoint = GetRandomEndpointFromResults(results);
            if (selectedEndpoint != null)
                return $"http://{selectedEndpoint.ServiceAddress}:{selectedEndpoint.ServicePort}";
            return "error";
        }

        private async Task<IRestCommandResult<List<CatalogService>>> GetServiceEndpointsFromConsul(string appName)
        {
            var action = new RestActionConsumer(_consulNetworkUrl.AbsoluteUri);
            var command = action.GetRestCommand(HttpMethod.GET, ServiceQueryParameter);
            command.AddUrlSegment("appName", appName);
            var results = await action.ProcessRestCommand<List<CatalogService>>(command);
            _logger.Info("StatusCode " + results.StatusCode);
            return results;
        }

        private CatalogService GetRandomEndpointFromResults(IRestCommandResult<List<CatalogService>> results)
        {
            var randomizer = new Random(DateTime.Now.Millisecond);
            if (results.Data != null)
                return results.Data[randomizer.Next(0, results.Data.Count - 1)];
            return null;
        }

        public async Task<List<CatalogNode>> GetConsulCatalogNodeFromConsul(string appName)
        {
            var results = await GetCatalogNodeFromConsul(appName);
            if (results == null)
                throw new ServiceNotRunningException();
            if (results.Count == 0)
                throw new ServiceNotRunningException();
            return results;
        }

        private async Task<List<CatalogNode>> GetCatalogNodeFromConsul(string appName)
        {
            var action = new RestActionConsumer(_consulNetworkUrl.AbsoluteUri);
            var command = action.GetRestCommand(HttpMethod.GET, NodeQueryParameter);
            command.AddUrlSegment("appName", appName);
            var results = await action.ProcessRestCommand<List<CatalogNode>>(command);
            return results.Data;
        }
    }
}
