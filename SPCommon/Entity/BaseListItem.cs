using System;
using System.Collections.Generic;
using System.Globalization;
using SPCommon.Interface;
using SPCommon.Serializers;

namespace SPCommon.Entity
{
    public class BaseListItem : IJSONSerializable
    {
        public Guid Guid { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Created { get; set; }
        public string Author { get; set; }
        public string ContentTypeName { get; set; }

        #region Serializer

        /// <summary>
        /// Override this method in extended types to add mappings of your own.
        /// </summary>
        /// <returns></returns>
        protected virtual Dictionary<string, string> ProvideJSONMapping()
        {
            return new Dictionary<string, string>
            {
                {"Id", Id.ToString(CultureInfo.InvariantCulture)},
                {"Title", Title},
                {"Created", Created.ToShortDateString()},
                {"Author", Author},
                {"ContentTypeName", ContentTypeName},
            };
        }
        
        /// <summary>
        /// Call this method to get a JSON type for the entity object
        /// To support types extended from BaseListItem, override the 'ProvideJSONMapping' method
        /// </summary>
        /// <returns></returns>
        public JSON ToJSON()
        {
            return new JSON(ProvideJSONMapping());
        }

        #endregion
    }
}
