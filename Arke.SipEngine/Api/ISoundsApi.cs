using System.Collections.Generic;
using System.Threading.Tasks;
using Arke.SipEngine.Api.Models;

namespace Arke.SipEngine.Api
{
    public interface ISoundsApi
    {
        Task<ICollection<Sound>> GetSoundsOnEngineAsync();
    }
}
