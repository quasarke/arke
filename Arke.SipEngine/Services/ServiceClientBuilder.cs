using System;
using Arke.SipEngine.Interfaces;
using Arke.SipEngine.Web;
using Arke.SipEngine.Web.Default;

namespace Arke.SipEngine.Services
{
    public class ServiceClientBuilder : IServiceClientBuilder
    {
        public IActionConsumer Construct(Uri url)
        {
            return new RestActionConsumer(url.AbsoluteUri);
        }
    }
}
