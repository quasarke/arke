using System;
using Arke.DependencyInjection;
using Arke.SipEngine.CallObjects;
using AsterNET.ARI.Models;

namespace Arke.IVR.CallObjects
{
    public static class ArkeCallFactory
    {
        public static ICall CreateArkeCall(Channel channel)
        {
            var call = ObjectContainer.GetInstance().GetObjectInstance<ICall>();
            call.CallId = Guid.NewGuid();
            call.CallState = new ArkeCallState()
                {
                    IncomingSipChannel = new ArkeSipChannel
                    {
                        Channel = channel
                    },
                    CallCanBeAbandoned = true
                };
            return call;
        }
    }
}
