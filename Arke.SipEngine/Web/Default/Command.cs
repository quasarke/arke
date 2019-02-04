using System;
using Newtonsoft.Json;
using RestSharp;

namespace Arke.SipEngine.Web.Default
{
    public class Command : IRestCommand
    {
        internal RestClient Client;
        internal RestRequest Request;

        public Command(string endpoint, string path)
        {
            Client = new RestClient(endpoint);
            Request = new RestRequest(path);
        }

        public string UniqueId { get; set; }
        public string Url { get; set; }

        public string Method
        {
            get => Request.Method.ToString();
            set => Request.Method = (Method) Enum.Parse(typeof(Method), value);
        }

        public string Body { get; private set; }

        public void AddUrlSegment(string segName, string value)
        {
            Request.AddUrlSegment(segName, value);
        }

        public void AddParameter(string name, object value, RequestParameterType type)
        {
            if (type == RequestParameterType.RequestBody)
            {
                Request.JsonSerializer = new RestSharp.Serializers.JsonSerializer();
                //Request.AddParameter(name, JsonConvert.SerializeObject(value), 
                //    (ParameterType)Enum.Parse(typeof(ParameterType), 
                //    type.ToString()));
                Request.AddJsonBody(value);
            }
            else
            {
                Request.AddParameter(name, value, 
                    (ParameterType)Enum.Parse(typeof(ParameterType), 
                    type.ToString()));
            }
        }
    }
}
