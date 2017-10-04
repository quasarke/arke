using System;
using System.Collections.Generic;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.StartRecordingStep
{
    public class StartRecordingSettings : ISettings
    {
        public List<RecordingItems> ItemsToRecord { get; set; } 
        public int NextStep { get; set; }
        public ISettings ConvertFromJObject(JObject jObject)
        {
            ItemsToRecord = new List<RecordingItems>();

            var itemsToRecord = jObject.GetValue("ItemsToRecord").Value<JArray>();
            foreach (var i in itemsToRecord)
            {
                ItemsToRecord.Add((RecordingItems)Enum.Parse(typeof(RecordingItems), i.ToString()));
            }
            NextStep = jObject.GetValue("NextStep").Value<int>();
            return this;
        }
    }
}
