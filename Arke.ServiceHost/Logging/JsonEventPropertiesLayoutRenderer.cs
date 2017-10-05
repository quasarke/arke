using System.Text;
using Newtonsoft.Json;
using NLog;
using NLog.LayoutRenderers;

namespace Arke.ServiceHost.Logging
{
    [LayoutRenderer("json-event-properties")]
    public class JsonEventPropertiesLayoutRenderer : LayoutRenderer
    {
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (logEvent.Properties == null || logEvent.Properties.Count == 0)
                return;
            var serialized = JsonConvert.SerializeObject(logEvent.Properties);
            builder.Append(serialized);
        }
    }
}
