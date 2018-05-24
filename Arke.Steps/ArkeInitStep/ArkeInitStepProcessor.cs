using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.SipEngine.Api;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Processors;

namespace Arke.Steps.ArkeInitStep
{
    public class ArkeInitStepProcessor : IStepProcessor
    {
        private const string FailStep = "FailStep";
        private const string NextStep = "NextStep";
        private readonly ISipLineApi _sipLineApi;
        private ICall _call;
        private ArkeInitStepSettings _settings;

        public string Name => "ArtemisInitStep";

        public ArkeInitStepProcessor(ISipLineApi sipLineApi)
        {
            _sipLineApi = sipLineApi;
        }

        public async Task DoStep(Step step, ICall call)
        {
            _settings = (ArkeInitStepSettings) step.NodeData.Properties;
            _call = call;
            try
            {
                await SetEndpointFromChannelVariable();
            }
            catch (Exception ex) when (ex is EndpointNotFoundException)
            {
                _call.CallState.TerminationCode = TerminationCode.InvalidDeviceConfig;
                _call.CallState.AddStepToIncomingQueue(step.GetStepFromConnector(FailStep));
            }
            
            _call.CallState.AddStepToIncomingQueue(step.GetStepFromConnector(NextStep));
            _call.FireStateChange(SipEngine.FSM.Trigger.NextCallFlowStep);
        }

        
        public async Task SetEndpointFromChannelVariable()
        {
            await GetEndpointFromAri();
            _call.Logger.Debug($"Phone connected at Endpoint {_call.CallState.Endpoint} PortID: {_call.CallState.PortId}");
        }

        private async Task GetEndpointFromAri()
        {
            try
            {
                _call.CallState.Endpoint =
                    await _sipLineApi.GetEndpoint(_call.CallState.GetIncomingLineId());
                _call.CallState.PortId =
                    await _sipLineApi.GetLineVariable(_call.CallState.GetIncomingLineId(),
                    "CALLERID(num)");
            }
            catch (Exception ex)
            {
                _call.Logger.Error(ex, "Exception getting the Endpoint from ARI");
                throw new EndpointNotFoundException("Exception getting the Endpoint from ARI", ex);
            }
        }
    }
}
