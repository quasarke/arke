using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Arke.DependencyInjection;
using Arke.DSL.Step;
using Microsoft.AspNetCore.Mvc;

namespace Arke.ManagementApi.Controllers
{
    [Produces("application/json")]
    [Route("api/steps")]
    public class StepsController
    {
        [HttpGet]
        public async Task<List<StepDescription>> GetSteps()
        {
            var steps = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                steps.AddRange(assembly.GetTypes().Where(type => typeof(NodeProperties).IsAssignableFrom(type) && type.FullName != typeof(NodeProperties).FullName));
            }

            var stepApi = new List<StepDescription>();
            foreach (var step in steps)
            {
                var stepType = new StepDescription(step.FullName.Split('.').Last().Replace("Settings", ""));

                foreach (var property in step.GetProperties())
                {
                    stepType.Properties.Add(new StepProperty()
                    {
                        PropertyType = GetPropertyTypeName(property.PropertyType.FullName), 
                        PropertyName = property.Name
                    });
                }

                try
                {
                    stepType.OutboundConnectors =
                        (List<string>)step.GetMethod("GetOutputNodes", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[]{});
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Step package {step.FullName} missing GetOutputNodes method: {e}");
                    stepType.OutboundConnectors = (List<string>)step.BaseType.GetMethod("GetOutputNodes", BindingFlags.Static | BindingFlags.Public)
                        .Invoke(null, null);
                }

                stepType.Description = GetDescriptionForStep(step);

                stepApi.Add(stepType);
            }

            return stepApi;
        }

        private string GetDescriptionForStep(Type step)
        {
            var attributes = Attribute.GetCustomAttributes(step);

            foreach (var attribute in attributes)
            {
                if (attribute is DSL.Step.StepDescription description)
                    return description.GetDescription();
            }

            return "";
        }

        private string GetPropertyTypeName(string propertyFullName)
        {
            switch (propertyFullName)
            {
                case "System.Collections.Generic.List`1[[Arke.DSL.Step.RecordingItems, Arke.DSL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]":
                    return "Arke.DSL.Step.RecordingItems";
                case "System.Collections.Generic.List`1[[System.String, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]":
                    return "string[]";
                case "System.Collections.Generic.Dictionary`2[[System.String, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e],[System.String, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]":
                    return "Dictionary<string,string>";
                case "System.Collections.Generic.List`1[[Arke.DSL.Step.Settings.InputOptions, Arke.DSL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]":
                    return "Arke.DSL.Step.Settings.InputOptions";
                default:
                    return propertyFullName;
            }
        }
    }

    public class StepProperty
    {
        public string PropertyType { get; set; }
        public string PropertyName { get; set; }
    }

    public class StepDescription
    {
        public string StepName { get; set; }
        public List<StepProperty> Properties { get; set; }
        public List<string> OutboundConnectors { get; set; }
        public string Description { get; set; }

        public StepDescription(string stepFullName)
        {
            StepName = stepFullName;
            Properties = new List<StepProperty>();
        }
    }
}
