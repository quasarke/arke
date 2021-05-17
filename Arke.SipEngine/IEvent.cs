using System;

namespace Arke.SipEngine
{
    public interface IEvent
    {
        Guid Id { get; set; }
        EventState State { get; set; }
        void Execute();
        void Finish();
    }
}