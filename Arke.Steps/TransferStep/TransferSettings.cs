using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Arke.DSL.Step;
using Newtonsoft.Json.Linq;

namespace Arke.Steps.TransferStep
{
    [StepDescription("Transfers a call to a new endpoint in Asterisk.")]
    public class TransferSettings : NodeProperties
    {
        public string Endpoint { get; set; }
        public string DialString { get; set; }

        public override NodeProperties ConvertFromJObject(JObject jObject)
        {
            base.ConvertFromJObject(jObject);
            Endpoint = jObject.GetValue("Endpoint").Value<string>();
            DialString = jObject.GetValue("DialString").Value<string>();
            return this;
        }
    }
}
