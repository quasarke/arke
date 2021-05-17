using System;
using System.Collections.Generic;
using System.Text;
using Arke.SipEngine.Api;
using Arke.Steps.ArkeInitStep;
using Moq;
using Xunit;

namespace Arke.Tests.Steps.InitStepTests
{
    public class OnDoStep_RetrieveEndpointSetup_FromChannelVariable
    {
        private ISipLineApi _sipLineApiMock;
        private void Setup()
        {
            var mockSipApi = new Mock<ISipLineApi>();
            mockSipApi.Setup(api => api.GetEndpointAsync(It.IsAny<string>()))
                .ReturnsAsync("TEST_ENDPOINT");
            mockSipApi.Setup(api => api.GetLineVariableAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("TEST_PORT");
            mockSipApi.VerifyAll();
            _sipLineApiMock = mockSipApi.Object;
        }

        [Fact]
        public async void EnsureEndpointSetupGetsCalledOnDoStep()
        {
            Setup();
            var initStep = new ArkeInitProcessor(_sipLineApiMock);
            //await initStep.DoStepAsync();
        }
    }
}
