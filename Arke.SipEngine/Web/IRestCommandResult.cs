﻿using System.Net;

namespace Arke.SipEngine.Web
{
    public interface IRestCommandResult<T>
        where T : new()
    {
        string UniqueId { get; set; }
        HttpStatusCode StatusCode { get; set; }
        T Data { get; set; }
    }

    public interface IRestCommandResult
    {
        string UniqueId { get; set; }
        HttpStatusCode StatusCode { get; set; }
    }
}
