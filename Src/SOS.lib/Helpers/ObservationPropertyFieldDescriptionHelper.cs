using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        public static readonly Dictionary<string, PropertyFieldDescription> FieldByPropertyPath;
        public static readonly Dictionary<OutputFieldSet, List<PropertyFieldDescription>> FieldsByFieldSet;
        public static readonly Dictionary<OutputFieldSet, List<string>> OutputFieldsByFieldSet;

        static ObservationPropertyFieldDescriptionHelper()
        {
            AllFields = LoadFieldDescriptionsFromJson();
            InitDataTypeEnum(AllFields);
            FieldByPropertyPath = AllFields.ToDictionary(x => x.PropertyPath, x => x);
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
                case PropertyLabelType.PropertyPath:
                    return field.PropertyPath;
                case PropertyLabelType.PropertyName:
                    return field.PropertyName;
                case PropertyLabelType.Swedish:
                    return field.GetSwedishTitle();
                case PropertyLabelType.English:
                    return field.GetEnglishTitle();
                default:
                    return field.PropertyPath;
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
                    case "TimeSpan":
                        field.DataTypeEnum = PropertyFieldDataType.TimeSpan;
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

                // string data type is always nullable.
                if (field.DataTypeEnum == PropertyFieldDataType.String)
                {
                    field.DataTypeIsNullable = true;
                }
            }
        }

        public static bool ValidateUniquePropertyNames()
        {
            var propertyPathSet = new HashSet<string>();
            var propertyNameSet = new HashSet<string>();
            var swedishNameSet = new HashSet<string>();
            var englishNameSet = new HashSet<string>();

            foreach (var field in FieldsByFieldSet[OutputFieldSet.All])
            {
                if (propertyPathSet.Contains(field.PropertyPath.ToLowerInvariant()))
                    return false;
                if (propertyNameSet.Contains(field.PropertyName.ToLowerInvariant()))
                    return false;
                if (swedishNameSet.Contains(field.GetSwedishTitle().ToLowerInvariant()))
                    return false;
                if (englishNameSet.Contains(field.GetEnglishTitle().ToLowerInvariant()))
                    return false;

                propertyPathSet.Add(field.PropertyPath.ToLowerInvariant());
                propertyNameSet.Add(field.PropertyName.ToLowerInvariant());
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
                {OutputFieldSet.AllWithValues, new List<PropertyFieldDescription>()},
                {OutputFieldSet.All, new List<PropertyFieldDescription>()}
            };
            
            foreach (var field in fields)
            {
                if (string.IsNullOrEmpty(field.FieldSet)) continue;
                if (field.FieldSet == "Minimum")
                {
                    fieldsByFieldSet[OutputFieldSet.Minimum].Add(field);
                    fieldsByFieldSet[OutputFieldSet.Extended].Add(field);
                    fieldsByFieldSet[OutputFieldSet.AllWithValues].Add(field);
                    fieldsByFieldSet[OutputFieldSet.All].Add(field);
                    field.FieldSets = new List<OutputFieldSet>
                    {
                        OutputFieldSet.Minimum, OutputFieldSet.Extended, OutputFieldSet.AllWithValues,
                        OutputFieldSet.All
                    };
                    field.FieldSetEnum = OutputFieldSet.Minimum;
                }
                else if (field.FieldSet == "Extended")
                {
                    fieldsByFieldSet[OutputFieldSet.Extended].Add(field);
                    fieldsByFieldSet[OutputFieldSet.AllWithValues].Add(field);
                    fieldsByFieldSet[OutputFieldSet.All].Add(field);
                    field.FieldSets = new List<OutputFieldSet>
                    {
                        OutputFieldSet.Extended, OutputFieldSet.AllWithValues, OutputFieldSet.All
                    };
                    field.FieldSetEnum = OutputFieldSet.Extended;
                }
                else if (field.FieldSet == "AllWithValues")
                {
                    fieldsByFieldSet[OutputFieldSet.AllWithValues].Add(field);
                    fieldsByFieldSet[OutputFieldSet.All].Add(field);
                    field.FieldSets = new List<OutputFieldSet>
                    {
                        OutputFieldSet.AllWithValues, OutputFieldSet.All
                    };
                    field.FieldSetEnum = OutputFieldSet.AllWithValues;
                }
                else if (field.FieldSet == "All")
                {
                    fieldsByFieldSet[OutputFieldSet.All].Add(field);
                    field.FieldSets = new List<OutputFieldSet>
                    {
                        OutputFieldSet.All
                    };
                    field.FieldSetEnum = OutputFieldSet.All;
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
                {OutputFieldSet.AllWithValues, new List<string>()},
                {OutputFieldSet.All, new List<string>()}
            };

            foreach (var pair in fieldsByFieldSet)
            {
                if (pair.Key == OutputFieldSet.All || pair.Key == OutputFieldSet.AllWithValues)
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
