using System;

namespace Arke.SipEngine.Exceptions
{
    public class InvalidEventStateException : Exception
    {
        public InvalidEventStateException()
        {
        }

        public InvalidEventStateException(string message) : base(message)
        {
        }

        public InvalidEventStateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
