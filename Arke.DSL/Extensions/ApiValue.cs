using System;
using System.Collections.Generic;
using System.Text;

namespace Arke.DSL.Extensions
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class ApiValue : System.Attribute
    {
        private readonly string _key;
        private readonly string _value;

        public string Value => _value;

        public string Key => _key;

        public ApiValue(string key, string value)
        {
            _key = key;
            _value = value;
        }
    }
}
