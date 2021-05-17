using System;

namespace Arke.SipEngine.Exceptions
{
    public class InvalidDeviceConfigException : Exception
    {
        public InvalidDeviceConfigException(string message) : base(message)
        {
        }
    }
}