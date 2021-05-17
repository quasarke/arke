using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Arke.SipEngine.Api;
using Microsoft.AspNetCore.Mvc;

namespace Arke.ManagementApi.Controllers
{
    [Produces("application/json")]
    [Route("api/sounds")]
    public class SoundsController : ControllerBase
    {
        private readonly ISoundsApi soundsApi;

        public SoundsController(ISoundsApi soundsApi)
        {
            this.soundsApi = soundsApi;
        }

        [HttpGet]
        public async Task<IActionResult> GetSoundsForEngine()
        {
            return new OkObjectResult(await soundsApi.GetSoundsOnEngineAsync().ConfigureAwait(false));
        }
    }
}
