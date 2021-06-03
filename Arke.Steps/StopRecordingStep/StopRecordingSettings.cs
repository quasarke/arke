using System;
using System.Collections.Generic;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.StopRecordingStep
{
    [StepDescription("Stops the recording on a designated line.")]
    public class StopRecordingSettings : NodeProperties
    {
        public List<RecordingItems> ItemsToStop { get; set; }
        
        public override NodeProperties ConvertFromJObject(JObject jObject)
        {
            base.ConvertFromJObject(jObject);
            ItemsToStop = new List<RecordingItems>();

            var itemsToStop = jObject.GetValue("ItemsToStop").Value<JArray>();
            foreach (var i in itemsToStop)
            {
                ItemsToStop.Add((RecordingItems)Enum.Parse(typeof(RecordingItems), i.ToString()));
            }

            return this;
        }
    }
}
