using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.SharePoint;
using SPCommon.Entity;
using Microsoft.SharePoint.Publishing.Fields;
using Microsoft.SharePoint.Taxonomy;
using SPCommon.Entity.DataTypes;

namespace SPCommon.Infrastructure.Common
{
    internal class SharePointItemMapper<T> where T : BaseListItem, new()
    {
        public T BuildEntityFromItem(SPListItem item)
        {
            var entity = new T();
            var entityType = typeof(T);

            // Map non-field type information
            MapNonFieldValues(entity, item);

            var entityProperties = new List<PropertyInfo>();
            entityProperties.AddRange(entityType.GetProperties().ToList());

            /*
            var fieldsToMap = (from prop in entityProperties where item.Fields.ContainsFieldWithStaticName(prop.Name) 
                               select item.Fields[prop.Name])
                               .ToList();
            */

            var fields = item.Fields;
            foreach (SPField field in fields)
            {
                MapProperty(entity, entityType, item, field);
            }

            return entity;
        }

        private static void MapNonFieldValues(T entity, SPListItem item)
        {
            entity.Id = item.ID;
            entity.Guid = item.UniqueId;
            if (item.ContentType != null)
                entity.ContentTypeName = item.ContentType.Name;
        }

        private static void MapProperty(T entity, Type entityType, SPListItem item, SPField field)
        {
            var fieldName = field.StaticName;

            // Don't do anything if field doesn't countain a value
            try
            {
                if (item[fieldName] == null || String.IsNullOrEmpty(item[fieldName].ToString())) return;
            }
            catch (ArgumentException)
            {
                return;
            }

            var prop = entityType.GetProperty(fieldName);

            // No mapping from SPItem internal name to entity object -- don't do anything
            if (prop == null) return;

            try
            {
                MapFieldToValue(entity, prop, item, field);
            }
            catch (Exception e)
            {
                var msg = "Exception converting SP item to domain with field: " + fieldName + " for item: " + item["Title"];
                throw new Exception(msg, e.InnerException);
            }
        }

        private static void MapFieldToValue(T entity, PropertyInfo property, SPListItem item, SPField field)
        {
            var fieldName = field.StaticName;

            if (fieldName == "ID") return;

            switch (field.Type)
            {
                case SPFieldType.DateTime:
                    GetDateValue(entity, property, item, fieldName);
                    break;
                case SPFieldType.Integer:
                    property.SetValue(entity, (int)item[fieldName], null);
                    break;
                case SPFieldType.ModStat:
                    property.SetValue(entity, Convert.ToInt16((string)item[fieldName]), null);
                    break;
                case SPFieldType.Lookup:
                    GetLookupValue(entity, property, item, fieldName);
                    break;
                case SPFieldType.URL:
                    GetUrlValue(entity, property, item, fieldName);
                    break;
                case SPFieldType.Boolean:
                    GetBooleanValue(entity, property, item, fieldName);
                    break;
                default:
                    switch (field.TypeAsString)
                    {
                        case "Image":
                            GetImageValue(entity, property, item, fieldName);
                            break;
                        case "TaxonomyFieldType":
                        case "TaxonomyFieldTypeMulti":
                            GetTaxonomyFieldValue(entity, property, item, fieldName);
                            break;
                        default:
                            property.SetValue(entity, item[fieldName] as string, null);
                            break;
                    }
                    break;
            }
        }

        private static void GetDateValue(T entity, PropertyInfo property, SPListItem item, string fieldName)
        {
            if (property.PropertyType == typeof(DateTime))
            {
                property.SetValue(entity, (DateTime)item[fieldName], null);
            }
            else
            {
                property.SetValue(entity, item[fieldName], null);
            }
        }

        private static void GetTaxonomyFieldValue(T entity, PropertyInfo property, SPListItem item, string fieldName)
        {
            if (item[fieldName] is TaxonomyFieldValueCollection)
            {
                var value = item[fieldName] as TaxonomyFieldValueCollection;

// ReSharper disable HeuristicUnreachableCode
                if (value == null) return;
// ReSharper restore HeuristicUnreachableCode

                var taxValues = new List<TaxonomyValue>();

                taxValues.AddRange(value.Select(t => new TaxonomyValue
                {
                    Label = t.Label,
                    Guid = new Guid(t.TermGuid)
                }));
                property.SetValue(entity, taxValues, null);
            }
            else if (item[fieldName] is TaxonomyFieldValue)
            {
                var value = item[fieldName] as TaxonomyFieldValue;

// ReSharper disable ConditionIsAlwaysTrueOrFalse
                if (value == null) return;
// ReSharper restore ConditionIsAlwaysTrueOrFalse

                var taxonomyValue = new TaxonomyValue
                {
                    Label = value.Label,
                    Guid = new Guid(value.TermGuid)
                };

                property.SetValue(entity, taxonomyValue, null);
            }
        }

        private static void GetImageValue(T entity, PropertyInfo property, SPListItem item, string fieldName)
        {
            if (property.PropertyType == typeof(PublishingImage))
            {
                var imageFieldValue = item[fieldName] as ImageFieldValue;

                if (imageFieldValue == null) return;

                var image = new PublishingImage
                {
                    AlternateText = imageFieldValue.AlternateText,
                    Hyperlink = imageFieldValue.Hyperlink,
                    ImageUrl = imageFieldValue.ImageUrl
                };

                property.SetValue(entity, image, null);
            }
            else
                property.SetValue(entity, item[fieldName] as string, null);
        }

        private static void GetBooleanValue(T entity, PropertyInfo property, SPListItem item, string fieldName)
        {
            if (property.PropertyType == typeof(bool))
            {
                var value = item[fieldName].ToString().Equals("TRUE", StringComparison.InvariantCultureIgnoreCase);
                property.SetValue(entity, value, null);
            }
            else
                property.SetValue(entity, item[fieldName] as string, null);
        }

        private static void GetUrlValue(T entity, PropertyInfo property, SPListItem item, string fieldName)
        {
            if (property.PropertyType == typeof(HyperLink))
            {
                var urlValue = new SPFieldUrlValue(item[fieldName].ToString());
                var hyperLink = new HyperLink
                {
                    Url = urlValue.Url,
                    Description = urlValue.Description
                };
                property.SetValue(entity, hyperLink, null);
            }
            else
                property.SetValue(entity, item[fieldName], null);
        }

        private static void GetLookupValue(T entity, PropertyInfo property, SPListItem item, string fieldName)
        {
            try
            {
                var lookupValue = new SPFieldLookupValue(item[fieldName].ToString());
                property.SetValue(entity, lookupValue.LookupValue, null);
            }
            catch
            {
                property.SetValue(entity, item[fieldName] as string, null);
            }
        }
    }
}
