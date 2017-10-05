using System.Linq;
using Arke.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Arke.DSL.Step.Settings
{
    public class SettingsFactory
    {
        public virtual object GetSettings(string stepType)
        {
            var settingsName = stepType + "Settings";
            var settingsType = AssemblyTools.GetReferencingAssemblies("Arke")
                // Xunit assemblies cause issues during unit tests, so omit from assembly search.
                .Where(assembly => !assembly.FullName.Contains("xunit"))
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(ISettings).IsAssignableFrom(type))
                .SingleOrDefault(type => type.Name == settingsName);

            if (settingsType == null)
                throw new StepNotFoundException($"Step {stepType} is missing from Assembly.");

            var settings = ObjectContainer.GetInstance().GetObjectInstance(settingsType);
            return settings;
        }

        public ISettings CopyJObject(string stepType, JObject jObject)
        {
            var settings = GetSettings(stepType);
            return ((ISettings) settings).ConvertFromJObject(jObject);
        }
    }
}
