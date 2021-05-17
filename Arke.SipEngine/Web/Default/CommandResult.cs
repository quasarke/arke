using System.Net;

namespace Arke.SipEngine.Web.Default
{
    public class CommandResult<T> : IRestCommandResult<T>
        where T : new ()
    {
        public string UniqueId { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public T Data { get; set; }
    }

    public class CommandResult : IRestCommandResult
    {
        public string UniqueId { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}
