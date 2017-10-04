using Newtonsoft.Json.Linq;

namespace Arke.DSL.Step.Settings
{
    public interface ISettings
    {
        ISettings ConvertFromJObject(JObject jObject);
    }
}
