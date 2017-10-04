using System;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using AsterNET.ARI.Models;

namespace Arke.IVR.CallObjects
{
    public class ArkeSipChannel : ISipChannel, IEquatable<ArkeSipChannel>
    {
        public Channel Channel { get; set; }
        public State CurrentState { get; set; }
        public object Id => Channel.Id;
        public Channel SnoopingChannel { get; set; }
        public DateTimeOffset TimeRecordingEnd { get; set; }
        public DateTimeOffset TimeRecordingStart { get; set; }

        public bool Equals(ArkeSipChannel other)
        {
            if (other == null)
                return false;

            if (other.Channel.Id != Channel.Id)
                return false;

            if (other.CurrentState != CurrentState)
                return false;

            if (other.SnoopingChannel == null ^ SnoopingChannel == null)
                return false;
            
            if (other.SnoopingChannel != null && other.SnoopingChannel.Id != SnoopingChannel?.Id)
                return false;

            if (other.TimeRecordingStart != TimeRecordingStart)
                return false;

            if (other.TimeRecordingEnd != TimeRecordingEnd)
                return false;

            return true;
        }
    }
}
