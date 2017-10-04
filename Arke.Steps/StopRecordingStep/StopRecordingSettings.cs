using System;
using System.Collections.Generic;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.StopRecordingStep
{
    public class StopRecordingSettings : ISettings
    {
        public List<RecordingItems> ItemsToStop { get; set; }
        public int NextStep { get; set; }
        public ISettings ConvertFromJObject(JObject jObject)
        {
            ItemsToStop = new List<RecordingItems>();

            var itemsToStop = jObject.GetValue("ItemsToStop").Value<JArray>();
            foreach (var i in itemsToStop)
            {
                ItemsToStop.Add((RecordingItems)Enum.Parse(typeof(RecordingItems), i.ToString()));
            }

            NextStep = jObject.GetValue("NextStep").Value<int>();
            return this;
        }
    }
}
