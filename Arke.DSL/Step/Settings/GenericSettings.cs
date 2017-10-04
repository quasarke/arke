using Newtonsoft.Json.Linq;

namespace Arke.DSL.Step.Settings
{
    public class GenericSettings : ISettings
    {
        public int FailStep { get; set; }
        public int NextStep { get; set; }
        public ISettings ConvertFromJObject(JObject jObject)
        {
            NextStep = jObject.GetValue("NextStep").Value<int>();
            FailStep = jObject.GetValue("FailStep").Value<int>();
            return this;
        }
    }
}