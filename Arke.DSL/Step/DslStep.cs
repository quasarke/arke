using Arke.DSL.Step.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Arke.DSL.Step
{
    public class DslStep
    {
        public string Type { get; set; }
        public ISettings Settings { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Direction Direction { get; set; }

        [JsonConstructor]
        public DslStep(string type, string direction, object settings)
        {
            var settingsFactory = new SettingsFactory();
            Type = type;
            SetDirection(direction);
            Settings = settingsFactory.CopyJObject(type, (JObject)settings);
        }

        public DslStep(string type, string direction, ISettings settings)
        {
            Type = type;
            SetDirection(direction);
            Settings = settings;
        }

        public void SetDirection(string direction)
        {
            switch (direction)
            {
                case "Incoming":
                    Direction = Direction.Incoming;
                    break;
                case "Outgoing":
                    Direction = Direction.Outgoing;
                    break;
                default:
                    Direction = Direction.Both;
                    break;
            }
        }
    }
}
