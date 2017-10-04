using Arke.DSL.Step.Settings;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.OutboundCallStep
{
    public class OutboundCallStepSettings : ISettings
    {
        public int NextStep { get; set; }
        public int FailedStep { get; set; }
        public ISettings ConvertFromJObject(JObject jObject)
        {
            NextStep = jObject.GetValue("NextStep").Value<int>();
            return this;
        }
    }
}
