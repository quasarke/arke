using System;
using Arke.SipEngine.Web;

namespace Arke.SipEngine.Interfaces
{
    public interface IServiceClientBuilder
    {
        IActionConsumer Construct(Uri url);
    }
}
