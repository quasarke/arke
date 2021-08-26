using System.Collections.Generic;
using System.Linq;
using Arke.DSL.Extensions;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.InputStep
{
    [StepDescription("Gets input from the user.")]
    public class GetInputSettings : NodeProperties
    {
        [ApiValue("Numeric", "StepId")]
        public List<InputOptions> Options { get; set; }
        public int MaxDigitTimeoutInSeconds { get; set; }
        public int NumberOfDigitsToWaitForNextStep { get; set; }
        public string TerminationDigit { get; set; }
        public bool SetValueAsDestination { get; set; }
        public int MaxAttempts { get; set; }
        public string SetValueAs { get; set; }

        public override NodeProperties ConvertFromJObject(JObject jObject)
        {
            base.ConvertFromJObject(jObject);
            MaxDigitTimeoutInSeconds = jObject.GetValue("MaxDigitTimeoutInSeconds").Value<int>();
            NumberOfDigitsToWaitForNextStep = jObject.GetValue("NumberOfDigitsToWaitForNextStep").Value<int>();
            TerminationDigit = jObject.GetValue("TerminationDigit").Value<string>();
            SetValueAsDestination = jObject.GetValue("SetValueAsDestination").Value<bool>();
            SetValueAs = jObject.GetValue("SetValueAs").Value<string>();
            MaxAttempts = jObject.GetValue("MaxAttempts").Value<int>();
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

        public PhoneInputHandlerSettings GetPhoneInputHandlerSettings(Step step)
        {
            return new PhoneInputHandlerSettings()
            {
                Direction = Direction,
                Invalid = step.GetStepFromConnector("Invalid"),
                MaxDigitTimeoutInSeconds = MaxDigitTimeoutInSeconds,
                NextStep = step.GetStepFromConnector("NextStep"),
                NoAction = step.GetStepFromConnector("NoAction"),
                NumberOfDigitsToWaitForNextStep = NumberOfDigitsToWaitForNextStep,
                Options = Options,
                SetValueAsDestination = SetValueAsDestination,
                SetValueAs = SetValueAs,
                TerminationDigit = TerminationDigit,
                MaxAttemptsReachedStep = step.GetStepFromConnector("MaxAttemptsReachedStep"),
                MaxRetryCount = MaxAttempts
            };
        }

        public new static List<string> GetOutputNodes()
        {
            return new List<string>() { "Invalid", "NextStep", "NoAction", "MaxAttemptsReachedStep" };
        }
    }
}
