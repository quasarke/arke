using System.Collections.Generic;
using System.Linq;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.InputStep
{
    public class GetInputSettings : NodeProperties
    {
        private const string NextStep = "NextStep";
        private const string NoAction = "NoAction";
        private const string Invalid = "Invalid";
        private const string MaxAttemptsReachedStep = "MaxAttemptsReachedStep";
        private const string PhoneInputs = "0123456789*#";

        public int MaxDigitTimeoutInSeconds { get; set; }
        public int NumberOfDigitsToWaitForNextStep { get; set; }
        public string TerminationDigit { get; set; }
        public bool SetValueAsDestination { get; set; }
        public int MaxAttempts { get; set; }

        public override NodeProperties ConvertFromJObject(JObject jObject)
        {
            base.ConvertFromJObject(jObject);
            MaxDigitTimeoutInSeconds = jObject.GetValue("MaxDigitTimeoutInSeconds").Value<int>();
            NumberOfDigitsToWaitForNextStep = jObject.GetValue("NumberOfDigitsToWaitForNextStep").Value<int>();
            TerminationDigit = jObject.GetValue("TerminationDigit").Value<string>();
            SetValueAsDestination = jObject.GetValue("SetValueAsDestination").Value<bool>();
            MaxAttempts = jObject.GetValue("MaxAttempts").Value<int>();
            return this;
        }

        public PhoneInputHandlerSettings GetPhoneInputHandlerSettings(Step step)
        {
            return new PhoneInputHandlerSettings()
            {
                Invalid = step.GetStepFromConnector(Invalid),
                MaxDigitTimeoutInSeconds = MaxDigitTimeoutInSeconds,
                NextStep = step.GetStepFromConnector(NextStep),
                NoAction = step.GetStepFromConnector(NoAction),
                NumberOfDigitsToWaitForNextStep = NumberOfDigitsToWaitForNextStep,
                Options = step.LinkedSteps
                    .Where(s => PhoneInputs.Contains(s.FromPort))
                    .Select(s => new InputOptions { Input = s.FromPort, NextStep = s.To }).ToList(),
                SetValueAsDestination = SetValueAsDestination,
                TerminationDigit = TerminationDigit,
                MaxAttemptsReachedStep = step.GetStepFromConnector(MaxAttemptsReachedStep),
                MaxRetryCount = MaxAttempts
            };
        }
    }
}
