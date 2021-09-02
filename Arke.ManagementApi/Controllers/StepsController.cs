using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Arke.DependencyInjection;
using Arke.DSL.Extensions;
using Arke.DSL.Step;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arke.ManagementApi.Controllers
{
    //[Authorize]
    [Produces("application/json")]
    [Route("api/steps")]
    public class StepsController : ControllerBase
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
                    string[] values = null;
                    if (property.PropertyType.IsEnum)
                    {
                        values = property.PropertyType.GetEnumNames();
                    }
                    string key = null;
                    string value = null;
                    bool array = false;
                    var name = GetPropertyTypeName(property.PropertyType.FullName);

                    if (typeof(System.Collections.IList).IsAssignableFrom(property.PropertyType))
                    {
                        foreach (var prop in property.PropertyType.GetProperties())
                        {
                            if ("Item" == prop.Name && typeof(object) != prop.PropertyType)
                            {
                                var ipa = prop.GetIndexParameters();
                                if (ipa.Length == 1 && ipa[0].ParameterType == typeof(int))
                                {
                                    if (prop.PropertyType.IsEnum)
                                        values = prop.PropertyType.GetEnumNames();
                                }
                            }
                        }
                    }

                    if (property.PropertyType.GetInterfaces().Contains(typeof(System.Collections.IEnumerable))
                        && property.PropertyType.FullName != "System.String")
                    {
                        array = true;
                    }

                    foreach (var attr in System.Attribute.GetCustomAttributes(property))
                    {
                        if (attr is ApiValue a)
                        {
                            key = a.Key;
                            value = a.Value;
                        }
                    }

                    if (property.Name == "Prompts" || value == "Prompt")
                        value = $"{Request.Scheme}://{Request.Host}/api/sounds";

                    stepType.Properties.Add(new StepProperty()
                    {
                        Type = name,
                        Name = property.Name,
                        Options = values,
                        KeyType = key,
                        ValueType = value,
                        IsArray = array
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
                case "System.Collections.Generic.List`1[[Arke.DSL.Step.RecordingItems, Arke.DSL, Version=1.1.0.0, Culture=neutral, PublicKeyToken=null]]":
                    return "Arke.DSL.Step.RecordingItems";
                case "System.Collections.Generic.List`1[[System.String, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]":
                    return "string[]";
                case "System.Collections.Generic.Dictionary`2[[System.String, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e],[System.String, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]":
                    return "Dictionary<string,string>";
                case "System.Collections.Generic.Dictionary`2[[System.String, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e],[System.Int32, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]":
                    return "Dictionary<string,int>";
                case "System.Collections.Generic.List`1[[Arke.DSL.Step.Settings.InputOptions, Arke.DSL, Version=1.1.0.0, Culture=neutral, PublicKeyToken=null]]":
                    return "Arke.DSL.Step.Settings.InputOptions";
                default:
                    return propertyFullName;
            }
        }
    }

    public class StepProperty
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string[] Options { get; set; }
        public string KeyType { get; set; }
        public string ValueType { get; set; }
        public bool IsArray { get; set; }
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
