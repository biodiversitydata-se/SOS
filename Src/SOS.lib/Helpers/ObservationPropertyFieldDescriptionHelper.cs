using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models;

namespace SOS.Lib.Helpers
{
    /// <summary>
    /// Helper class for PropertyFieldDescription associated with the Observation class.
    /// </summary>
    public static class ObservationPropertyFieldDescriptionHelper
    {
        public static readonly List<PropertyFieldDescription> AllFields;
        public static readonly Dictionary<string, PropertyFieldDescription> FieldByName;
        public static readonly Dictionary<OutputFieldSet, List<PropertyFieldDescription>> FieldsByFieldSet;
        public static readonly Dictionary<OutputFieldSet, List<string>> OutputFieldsByFieldSet;

        static ObservationPropertyFieldDescriptionHelper()
        {
            AllFields = LoadFieldDescriptionsFromJson();
            InitDataTypeEnum(AllFields);
            FieldByName = AllFields.ToDictionary(x => x.Name, x => x);
            FieldsByFieldSet = CreateFieldSetDictionary(AllFields);
            OutputFieldsByFieldSet = CreateOutputFieldsDictionary(FieldsByFieldSet);
        }

        /// <summary>
        /// Get property label.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="propertyLabelType"></param>
        /// <returns></returns>
        public static string GetPropertyLabel(PropertyFieldDescription field,
            PropertyLabelType propertyLabelType)
        {
            switch (propertyLabelType)
            {
                case PropertyLabelType.PropertyName:
                    return field.Name;
                case PropertyLabelType.ShortPropertyName:
                    return field.ShortName;
                case PropertyLabelType.Swedish:
                    return field.GetSwedishTitle();
                case PropertyLabelType.English:
                    return field.GetEnglishTitle();
                default:
                    return field.Name;
            }
        }

        private static void InitDataTypeEnum(List<PropertyFieldDescription> fields)
        {
            foreach (var field in fields)
            {
                switch (field.DataType)
                {
                    case "String":
                        field.DataTypeEnum = PropertyFieldDataType.String;
                        break;
                    case "Boolean":
                        field.DataTypeEnum = PropertyFieldDataType.Boolean;
                        break;
                    case "DateTime":
                        field.DataTypeEnum = PropertyFieldDataType.DateTime;
                        break;
                    case "Double":
                        field.DataTypeEnum = PropertyFieldDataType.Double;
                        break;
                    case "Int32":
                        field.DataTypeEnum = PropertyFieldDataType.Int32;
                        break;
                    case "Int64":
                        field.DataTypeEnum = PropertyFieldDataType.Int64;
                        break;
                    default:
                        field.DataTypeEnum = PropertyFieldDataType.String;
                        break;
                }
            }
        }

        public static bool ValidateUniquePropertyNames()
        {
            var propertyNameSet = new HashSet<string>();
            var shortPropertyNameSet = new HashSet<string>();
            var swedishNameSet = new HashSet<string>();
            var englishNameSet = new HashSet<string>();

            foreach (var field in FieldsByFieldSet[OutputFieldSet.All])
            {
                if (propertyNameSet.Contains(field.Name.ToLowerInvariant()))
                    return false;
                if (shortPropertyNameSet.Contains(field.ShortName.ToLowerInvariant()))
                    return false;
                if (swedishNameSet.Contains(field.GetSwedishTitle().ToLowerInvariant()))
                    return false;
                if (englishNameSet.Contains(field.GetEnglishTitle().ToLowerInvariant()))
                    return false;

                propertyNameSet.Add(field.Name.ToLowerInvariant());
                shortPropertyNameSet.Add(field.ShortName.ToLowerInvariant());
                swedishNameSet.Add(field.GetSwedishTitle().ToLowerInvariant());
                englishNameSet.Add(field.GetEnglishTitle().ToLowerInvariant());
            }

            return true;
        }

        private static Dictionary<OutputFieldSet, List<PropertyFieldDescription>> CreateFieldSetDictionary(
            List<PropertyFieldDescription> fields)
        {
            var fieldsByFieldSet = new Dictionary<OutputFieldSet, List<PropertyFieldDescription>>
            {
                {OutputFieldSet.Minimum, new List<PropertyFieldDescription>()},
                {OutputFieldSet.Extended, new List<PropertyFieldDescription>()},
                {OutputFieldSet.AllWithKnownValues, new List<PropertyFieldDescription>()},
                {OutputFieldSet.All, new List<PropertyFieldDescription>()}
            };
            
            foreach (var field in fields.Where(m => m.IsPartOfFlatObservation.GetValueOrDefault()))
            {
                if (string.IsNullOrEmpty(field.FieldSet)) continue;
                if (field.FieldSet == "Minimum")
                {
                    fieldsByFieldSet[OutputFieldSet.Minimum].Add(field);
                    fieldsByFieldSet[OutputFieldSet.Extended].Add(field);
                    fieldsByFieldSet[OutputFieldSet.AllWithKnownValues].Add(field);
                    fieldsByFieldSet[OutputFieldSet.All].Add(field);
                }
                else if (field.FieldSet == "Extended")
                {
                    fieldsByFieldSet[OutputFieldSet.Extended].Add(field);
                    fieldsByFieldSet[OutputFieldSet.AllWithKnownValues].Add(field);
                    fieldsByFieldSet[OutputFieldSet.All].Add(field);
                }
                else if (field.FieldSet == "AllWithKnownValues")
                {
                    fieldsByFieldSet[OutputFieldSet.AllWithKnownValues].Add(field);
                    fieldsByFieldSet[OutputFieldSet.All].Add(field);
                }
                else if (field.FieldSet == "All")
                {
                    fieldsByFieldSet[OutputFieldSet.All].Add(field);
                }
            }

            return fieldsByFieldSet;
        }

        private static Dictionary<OutputFieldSet, List<string>> CreateOutputFieldsDictionary(
            Dictionary<OutputFieldSet, List<PropertyFieldDescription>> fieldsByFieldSet)
        {
            var outputfieldsByFieldSet = new Dictionary<OutputFieldSet, List<string>>
            {
                {OutputFieldSet.Minimum, new List<string>()},
                {OutputFieldSet.Extended, new List<string>()},
                {OutputFieldSet.AllWithKnownValues, new List<string>()},
                {OutputFieldSet.All, new List<string>()}
            };

            foreach (var pair in fieldsByFieldSet)
            {
                if (pair.Key == OutputFieldSet.All || pair.Key == OutputFieldSet.AllWithKnownValues)
                {
                    continue; // retrieve all fields from Elasticsearch
                }

                var outputFieldSet = new HashSet<string>();
                foreach (var field in pair.Value)
                {
                    outputFieldSet.Add(field.DependsOn);
                }

                outputfieldsByFieldSet[pair.Key] = outputFieldSet.ToList();
            }
            
            return outputfieldsByFieldSet;
        }

        private static List<PropertyFieldDescription> LoadFieldDescriptionsFromJson()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, @"Resources\ObservationFieldDescriptions.json");
            using var fs = FileSystemHelper.WaitForFileAndThenOpenIt(filePath);
            var fields = System.Text.Json.JsonSerializer.DeserializeAsync<List<PropertyFieldDescription>>(fs).Result;
            return fields;
        }
    }
}
