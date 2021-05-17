using System.Collections.Generic;
using Arke.DSL.Step;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.OutboundCallStep
{
    public class CallOutboundSettings : NodeProperties
    {
        public string OutboundEndpointName { get; set; }
        public override NodeProperties ConvertFromJObject(JObject jObject)
        {
            base.ConvertFromJObject(jObject);
            OutboundEndpointName = jObject.GetValue("OutboundEndpointName").Value<string>();
            return this;
        }

        public new static List<string> GetOutputNodes()
        {
            return new List<string>() { "Error", "NoAnswer", "NextStep" };
        }
    }
}
