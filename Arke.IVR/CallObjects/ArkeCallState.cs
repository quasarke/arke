using System;
using System.Collections.Generic;
using System.Linq;
using Arke.IVR.Bridging;
using Arke.SipEngine.Bridging;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Device;
using Arke.SipEngine.FSM;
using AsterNET.ARI.Models;

namespace Arke.IVR.CallObjects
{
    public class ArkeCallState : ICallInfo, IEquatable<ArkeCallState>
    {
        private readonly Queue<int> _incomingStepQueue;
        private readonly Queue<int> _outgoingStepQueue;

        public ArkeCallState()
        {
            CallId = Guid.NewGuid();
            _incomingStepQueue = new Queue<int>();
            _outgoingStepQueue = new Queue<int>();
            ProcessOutgoingQueue = false;
        }

        public ArkeBridge Bridge { get; set; }
        public Guid CallId { get; set; }
        public bool CallStarted { get; set; }
        public string Destination { get; set; }
        public DeviceConfig Device { get; set; }
        public string Endpoint { get; set; }
        public string FileName { get; set; }
        public ArkeSipChannel IncomingSipChannel { get; set; }
        public string InputData { get; set; }
        public string LanguageCode { get; set; }
        public ArkeSipChannel MonitoringSipChannel { get; set; }
        public ArkeSipChannel OutgoingSipChannel { get; set; }
        public string PortId { get; set; }
        public bool ProcessOutgoingQueue { get; set; }
        public int StepAttempts { get; set; }
        public string TerminationCode { get; set; }
        public DateTimeOffset TimeOffHook { get; set; }

        public int InputRetryCount { get; set; }

        public bool CallCanBeAbandoned { get; set; }
        public int AttemptCount { get; set; }

        public void AddStepToIncomingQueue(int stepId)
        {
            _incomingStepQueue.Enqueue(stepId);
        }

        public void AddStepToOutgoingQueue(int stepId)
        {
            _outgoingStepQueue.Enqueue(stepId);
        }

        public void CreateOutgoingLine(object sipLine)
        {
            if (sipLine is Channel channel)
            {
                OutgoingSipChannel = new ArkeSipChannel
                {
                    Channel = channel,
                    CurrentState = State.Initialization
                };
            }
        }

        public bool Equals(ArkeCallState other)
        {
            if (other == null)
                return false;

            if (other.CallId != CallId)
                return false;

            if (other.LanguageCode != LanguageCode)
                return false;

            if (!other.IncomingSipChannel.Equals(IncomingSipChannel))
                return false;

            if (other.OutgoingSipChannel == null ^ OutgoingSipChannel == null)
                return false;

            if (other.OutgoingSipChannel?.Equals(OutgoingSipChannel) == false)
                return false;

            if (other.MonitoringSipChannel == null ^ MonitoringSipChannel == null)
                return false;

            if (other.MonitoringSipChannel?.Equals(MonitoringSipChannel) == false)
                return false;

            if (other.Endpoint != Endpoint)
                return false;

            if (other.PortId != PortId)
                return false;

            if (other.StepAttempts != StepAttempts)
                return false;

            if (!other._incomingStepQueue.SequenceEqual(_incomingStepQueue))
                return false;

            if (other.InputData != InputData)
                return false;

            return true;
        }

        public virtual string GetBridgeId()
        {
            return Bridge != null ? Bridge.Bridge.Id : string.Empty;
        }

        public virtual string GetIncomingLineId()
        {
            return IncomingSipChannel != null ? IncomingSipChannel.Channel.Id : string.Empty;
        }

        public virtual int GetNextIncomingStep()
        {
            if (_incomingStepQueue.Count == 0)
                throw new Exception("No More incoming steps remaining.");
            return _incomingStepQueue.Dequeue();
        }

        public virtual int GetNextOutgoingStep()
        {
            if (_outgoingStepQueue.Count == 0)
                throw new Exception("No More outgoing steps remaining.");
            return _outgoingStepQueue.Dequeue();
        }

        public virtual string GetOutgoingLineId()
        {
            return OutgoingSipChannel != null ? OutgoingSipChannel.Channel.Id : string.Empty;
        }

        public string GetPortId()
        {
            return IncomingSipChannel.Channel.Caller.Number;
        }

        public int GetStepsOnIncomingQueue()
        {
            return _incomingStepQueue.Count;
        }

        public int GetStepsOnOutgoingQueue()
        {
            return _outgoingStepQueue.Count;
        }

        public void SetBridge(IBridge bridge)
        {
            Bridge = bridge as ArkeBridge;
        }
    }
}