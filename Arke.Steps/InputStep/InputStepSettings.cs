using System.Collections.Generic;
using System.Linq;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.InputStep
{
    public class InputStepSettings : ISettings
    {
        public List<InputOptions> Options { get; set; }
        public int NoAction { get; set; }
        public int Invalid { get; set; }
        public int MaxDigitTimeoutInSeconds { get; set; }
        public int NumberOfDigitsToWaitForNextStep { get; set; }
        public string TerminationDigit { get; set; }
        public int NextStep { get; set; }
        public bool SetValueAsDestination { get; set; }
        public int MaxAttemptsReachedStep { get; set; }
        public int MaxAttempts { get; set; }

        public ISettings ConvertFromJObject(JObject jObject)
        {
            Invalid = jObject.GetValue("Invalid").Value<int>();
            NoAction = jObject.GetValue("NoAction").Value<int>();
            MaxDigitTimeoutInSeconds = jObject.GetValue("MaxDigitTimeoutInSeconds").Value<int>();
            NumberOfDigitsToWaitForNextStep = jObject.GetValue("NumberOfDigitsToWaitForNextStep").Value<int>();
            TerminationDigit = jObject.GetValue("TerminationDigit").Value<string>();
            NextStep = jObject.GetValue("NextStep").Value<int>();
            SetValueAsDestination = jObject.GetValue("SetValueAsDestination").Value<bool>();
            MaxAttempts = jObject.GetValue("MaxAttempts").Value<int>();
            MaxAttemptsReachedStep = jObject.GetValue("MaxAttemptsReachedStep").Value<int>();
            Options = new List<InputOptions>();

            var options = jObject.GetValue("Options").Value<JArray>();
            foreach (var option in options.Select(o => new InputOptions()
            {
                Input = o.Value<JObject>().GetValue("Input").Value<string>(),
                NextStep = o.Value<JObject>().GetValue("NextStep").Value<int>()
            }))
            {
                Options.Add(option);
            }
            return this;
        }

        public PhoneInputHandlerSettings GetPhoneInputHandlerSettings()
        {
            return new PhoneInputHandlerSettings()
            {
                Invalid = Invalid,
                MaxDigitTimeoutInSeconds = MaxDigitTimeoutInSeconds,
                NextStep = NextStep,
                NoAction = NoAction,
                NumberOfDigitsToWaitForNextStep = NumberOfDigitsToWaitForNextStep,
                Options = Options,
                SetValueAsDestination = SetValueAsDestination,
                TerminationDigit = TerminationDigit,
                MaxAttemptsReachedStep = MaxAttemptsReachedStep,
                MaxRetryCount = MaxAttempts
            };
        }
    }
}
