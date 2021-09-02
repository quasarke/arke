using System.Threading.Tasks;
using Arke.SipEngine.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Arke.ManagementApi.Extensions;
using Arke.SipEngine.Api.Models;
using System.Collections.Generic;

namespace Arke.ManagementApi.Controllers
{
    //[Authorize]
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
        public async Task<IActionResult> GetSoundsForEngine(int? page, int? pageSize)
        {
            var prompts = (await soundsApi.GetSoundsOnEngineAsync().ConfigureAwait(false)).AsEnumerable();

            var sounds = new PaginatedSounds()
            {
                Sounds = prompts,
                Count = prompts.Count(),
                TotalCount = prompts.Count(),
                CurrentPage = page.HasValue ? page.Value : 0,
                TotalPages = pageSize.HasValue ? prompts.Count() / pageSize.Value : 1
            };

            if (page.HasValue && pageSize.HasValue)
            {
                sounds.Sounds = prompts.Paginate(page.Value, pageSize.Value);
                sounds.Count = sounds.Sounds.Count();
            }
            return new OkObjectResult(sounds);
        }
    }

    public class PaginatedSounds
    {
        public IEnumerable<Sound> Sounds { get; set; }
        public int Count { get; set; }
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
