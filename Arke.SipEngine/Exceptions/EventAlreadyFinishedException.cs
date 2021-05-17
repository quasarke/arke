using System;

namespace Arke.SipEngine.Exceptions
{
    public class EventAlreadyFinishedException : Exception
    {
        public EventAlreadyFinishedException()
        {
        }

        public EventAlreadyFinishedException(string message) : base(message)
        {
        }

        public EventAlreadyFinishedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}