using System.Collections.Generic;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;

namespace Arke.SipEngine.CallObjects
{
    public class PhoneInputHandlerSettings
    {
        public Direction Direction { get; set; }
        public int Invalid { get; set; }
        public int MaxDigitTimeoutInSeconds { get; set; }
        public int NextStep { get; set; }
        public int NoAction { get; set; }
        public int NumberOfDigitsToWaitForNextStep { get; set; }
        public Dictionary<string, int> Options { get; set; }
        public bool SetValueAsDestination { get; set; }
        public string TerminationDigit { get; set; }
        public int MaxRetryCount { get; set; }
        public int MaxAttemptsReachedStep { get; set; }
        public string SetValueAs { get; set; }
    }
}