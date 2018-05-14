using System;
using Arke.DependencyInjection;
using Arke.SipEngine.CallObjects;
using AsterNET.ARI.Models;
using Datadog.Trace;

namespace Arke.IVR.CallObjects
{
    public static class ArkeCallFactory
    {
        public static ICall CreateArkeCall(Channel channel, Scope callScope)
        {
            var call = ObjectContainer.GetInstance().GetObjectInstance<ICall>();
            call.CallId = Guid.NewGuid();
            call.CallState = new ArkeCallState()
                {
                    IncomingSipChannel = new ArkeSipChannel
                    {
                        Channel = channel
                    },
                    CallCanBeAbandoned = true,
                    TraceScope = callScope
                };
            return call;
        }
    }
}
