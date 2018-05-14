using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.Api;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Processors;

namespace Arke.Steps.ArkeInitStep
{
    public class ArkeInitStepProcessor : IStepProcessor
    {
        private readonly ISipLineApi _sipLineApi;
        private ICall _call;
        private ArkeInitStepSettings _settings;

        public string Name => "ArtemisInitStep";

        public ArkeInitStepProcessor(ISipLineApi sipLineApi)
        {
            _sipLineApi = sipLineApi;
        }

        public async Task DoStep(ISettings settings, ICall call)
        {
            _settings = (ArkeInitStepSettings) settings;
            _call = call;
            try
            {
                await SetEndpointFromChannelVariable();
            }
            catch (Exception ex) when (ex is EndpointNotFoundException)
            {
                _call.CallState.TerminationCode = TerminationCode.InvalidDeviceConfig;
                _call.CallState.AddStepToIncomingQueue(_settings.FailStep);
            }
            
            _call.CallState.AddStepToIncomingQueue(_settings.NextStep);
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
            }
            catch (Exception ex)
            {
                _call.Logger.Error(ex, "Exception getting the Endpoint from ARI");
                throw new EndpointNotFoundException("Exception getting the Endpoint from ARI", ex);
            }
        }
    }
}
