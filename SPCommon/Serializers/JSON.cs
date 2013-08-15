using System;
using System.Collections.Generic;
using System.Linq;

namespace SPCommon.Serializers
{
    public sealed class JSON
    {
        private readonly Dictionary<string, string> _propertyMap;
        public Dictionary<string, string> PropertyMap
        {
            get { return _propertyMap; }
        }

        public JSON(Dictionary<string, string> propertyMap)
        {
            _propertyMap = propertyMap;
        }

        public override string ToString()
        {
            // JSON format is: { [Name:Value],..n }
            return "{" +
                        String.Join(",",
                        _propertyMap.Keys.Select(key =>
                        String.Format(@"""{0}"":""{1}""", key, EncodeJSONString(_propertyMap[key]))).ToArray()) +
                    "}";
        }

        private static string EncodeJSONString(string input)
        {
            // TODO: check string to ensure valid avlue
            return input;
        }
    }
}
