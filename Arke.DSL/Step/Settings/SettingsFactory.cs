using System;
using System.Linq;
using Arke.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Arke.DSL.Step.Settings
{
    public class SettingsFactory
    {
        public NodeProperties CreateProperties(string category, JObject jObject)
        {
            var properties = CreatePropertiesObject(category);
            return ((NodeProperties) properties).ConvertFromJObject(jObject);
        }
        
        public virtual object CreatePropertiesObject(string category)
        {
            var propertiesName = category + "Settings";
            var propertiesType = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.FullName.Contains("xunit"))
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(NodeProperties).IsAssignableFrom(type))
                .SingleOrDefault(type => type.Name == propertiesName);

            if (propertiesType == null)
                throw new StepNotFoundException($"Step {category} is missing from Assembly");

            var property = ObjectContainer.GetInstance().GetObjectInstance(propertiesType);
            return property;
        }
    }
}
