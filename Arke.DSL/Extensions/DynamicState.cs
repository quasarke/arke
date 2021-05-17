using System.Collections.Generic;

namespace Arke.DSL.Extensions
{
    public static class DynamicState
    {
        public static object GetProperty(object value, string path)
        {
            var currentType = value.GetType();

            foreach (var propertyName in path.Split('.'))
            {
                var property = currentType?.GetProperty(propertyName);
                value = property?.GetValue(value, null);
                currentType = property?.PropertyType;
            }

            return value;
        }

        public static void SetProperty(object obj, string path, object value)
        {
            var propQueue = new Queue<string>(path.Split('.'));
            
            SetValue(obj, propQueue, value);
        }

        private static void SetValue(object obj, Queue<string> properties, object value)
        {
            var prop = properties.Dequeue();
            if (properties.Count == 0)
                obj.GetType().GetProperty(prop)?.SetValue(obj, value);
            else
            {
                var nextObj = obj.GetType().GetProperty(prop)?.GetValue(obj, null);
                SetValue(nextObj, properties, value);
            }
        }
    }
}
