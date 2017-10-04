using Arke.DSL.Step.Settings;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.CheckAttemptStep
{
    public class CheckAttemptStepSettings : ISettings
    {
        public ISettings ConvertFromJObject(JObject jObject)
        {
            MaxAttempts = jObject.GetValue("MaxAttempts").Value<int>();
            MaxAttemptsStep = jObject.GetValue("MaxAttemptsStep").Value<int>();
            NextStep = jObject.GetValue("NextStep").Value<int>();
            return this;
        }

        public int MaxAttempts { get; set; }
        public int MaxAttemptsStep { get; set; }
        public int NextStep { get; set; }
    }
}
