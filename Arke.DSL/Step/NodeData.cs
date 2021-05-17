using System.Collections.Generic;
using System.Linq;
using Arke.DSL.Step.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Arke.DSL.Step
{
    public class NodeData
    {
        public string Text { get; set; }
        public string Category { get; set; }
        public int Key { get; set; }
        public NodeProperties Properties { get; set; }

        [JsonConstructor]
        public NodeData(string text, string category, int key, object properties)
        {
            var settingsFactory = new SettingsFactory();
            Key = key;
            Text = text;
            Category = category;
            Properties = settingsFactory.CreateProperties(category, (JObject)properties);
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
    public class StepDescription : System.Attribute
    {
        private string _description;

        public StepDescription(string description)
        {
            _description = description;
        }

        public string GetDescription()
        {
            return _description;
        }
    }

    public abstract class NodeProperties
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public Direction Direction { get; set; }

        public virtual NodeProperties ConvertFromJObject(JObject jObject)
        {
            SetDirection(jObject.GetValue("Direction").Value<string>());
            return this;
        }

        private void SetDirection(string direction)
        {
            switch (direction)
            {
                case "INCOMING":
                    Direction = Direction.Incoming;
                    break;
                case "OUTGOING":
                    Direction = Direction.Outgoing;
                    break;
                default:
                    Direction = Direction.Both;
                    break;
            }
        }

        /// <summary>
        /// If you add Output Nodes you should override this method and return your own nodes.
        /// </summary>
        /// <returns>List of Output Node names</returns>
        public static List<string> GetOutputNodes()
        {
            return new List<string>() {"NextStep"};
        }
    }

    public class LinkData
    {
        public int From { get; set; }
        public int To { get; set; }
        public string FromPort { get; set; }
        public string ToPort { get; set; }
    }

    public class CallFlowDsl
    {
        public List<NodeData> NodeDataArray { get; set; }
        public List<LinkData> LinkDataArray { get; set; }

        public Dictionary<int, Step> GetStepsFromDsl()
        {
            var stepList = new Dictionary<int, Step>();
            foreach (var node in NodeDataArray)
            {
                var step = new Step()
                {
                    NodeData = node,
                    LinkedSteps = LinkDataArray.AsQueryable().Where(s => s.From == node.Key).ToList()
                };
                stepList.Add(node.Key, step);
            }
            return stepList;
        }
    }
}
