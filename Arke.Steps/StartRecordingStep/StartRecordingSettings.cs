using System;
using System.Collections.Generic;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.StartRecordingStep
{
    public class StartRecordingSettings : NodeProperties
    {
        public List<RecordingItems> ItemsToRecord { get; set; }

        public override NodeProperties ConvertFromJObject(JObject jObject)
        {
            base.ConvertFromJObject(jObject);
            ItemsToRecord = new List<RecordingItems>();

            var itemsToRecord = jObject.GetValue("ItemsToRecord").Value<JArray>();
            foreach (var i in itemsToRecord)
            {
                ItemsToRecord.Add((RecordingItems)Enum.Parse(typeof(RecordingItems), i.ToString()));
            }
            return this;
        }
    }
}
