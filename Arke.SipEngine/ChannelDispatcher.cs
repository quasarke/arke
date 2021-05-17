using System;
using System.Collections.Generic;
using Arke.SipEngine.Exceptions;

namespace Arke.SipEngine
{
    public class ChannelDispatcher
    {
        public ChannelDispatcher()
        {
            Events = new Dictionary<string, Stack<IEvent>>();
        }

        public void Dispatch(string channelId, IEvent @event)
        {
            if (!Events.ContainsKey(channelId))
            {
                ExecuteNewEvent(channelId, @event);
            }
            else
            {
                var eventCount = Events[channelId].Count;

                CheckExistingEvents(channelId, @event, eventCount);
            }
        }

        private void CheckExistingEvents(string channelId, IEvent @event, int eventCount)
        {
            for (var i = 0; i < eventCount; i++)
            {
                var eventObject = Events[channelId].Pop();
                ProcessEventIfTriggered(channelId, @event, eventObject);
            }
        }

        private void ProcessEventIfTriggered(string channelId, IEvent @event, IEvent eventObject)
        {
            if (eventObject.Id == @event.Id)
            {
                FinishEventIfStillRunning(@event);
            }
            else
            {
                AddEventBackToStack(channelId, eventObject);
            }
        }

        private void AddEventBackToStack(string channelId, IEvent eventObject)
        {
            Events[channelId].Push(eventObject);
        }

        private static void FinishEventIfStillRunning(IEvent @event)
        {
            switch (@event.State)
            {
                case EventState.Running:
                    @event.State = EventState.Finished;
                    @event.Finish();
                    return;
                case EventState.Finished:
                    throw new EventAlreadyFinishedException("Event: " + @event.Id +
                                                            " has already finished executing. Cannot be dispatched.");
                case EventState.Creating:
                case EventState.Starting:
                    throw new InvalidEventStateException("Event: " + @event.Id +
                                                         " is in an invalid state. Cannot be dispatched.");
                default:
                    throw new ArgumentOutOfRangeException("Invalid state for Event: "+ @event.Id);
            }
        }

        private void ExecuteNewEvent(string channelId, IEvent @event)
        {
            @event.State = EventState.Starting;
            Events.Add(channelId, new Stack<IEvent>());
            @event.Execute();
            @event.State = EventState.Running;
            Events[channelId].Push(@event);
        }

        public Dictionary<string, Stack<IEvent>> Events { get; set; }
    }
}
