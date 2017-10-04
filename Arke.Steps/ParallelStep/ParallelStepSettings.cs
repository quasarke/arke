using Arke.DSL.Step.Settings;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.ParallelStep
{

    public class ParallelStepSettings : ISettings
    {
        public int IncomingNextStep { get; set; }
        public int OutgoingNextStep { get; set; }
        public ISettings ConvertFromJObject(JObject jObject)
        {
            IncomingNextStep = jObject.GetValue("IncomingNextStep").Value<int>();
            OutgoingNextStep = jObject.GetValue("OutgoingNextStep").Value<int>();
            return this;
        }
    }
}