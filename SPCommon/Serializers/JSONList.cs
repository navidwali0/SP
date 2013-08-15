using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SPCommon.Interface;

namespace SPCommon.Serializers
{
    public sealed class JSONList : List<IJSONSerializable>
    {
        public string Name { get; set; }
        
        public string ToJSONString()
        {
            return "{ \"" + (Name ?? ToString()) + "\": [" + String.Join(",", this.Select(jsonObj => jsonObj.ToJSON().ToString()).ToArray()) + "]}";
        }

        /// <summary>
        /// Converts a list of IJSONSeriaizable objects into a JSONList. 
        /// Will throw arguement exception if conversion fails
        /// </summary>
        /// <param name="objs">List of IJSONSerializabe objects</param>
        /// <returns>JSONList</returns>
        public static JSONList ToJSONList(IList objs)
        {
            var jsonList = new JSONList();
            jsonList.AddRange(from object obj in objs select obj as IJSONSerializable);
            return jsonList;
        }
    }    
}
