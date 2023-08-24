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
        public static readonly IEnumerable<PropertyFieldDescription> AllFields;
        public static readonly Dictionary<string, PropertyFieldDescription> FieldByPropertyPath;
        public static readonly Dictionary<OutputFieldSet, ICollection<PropertyFieldDescription>> FieldsByFieldSet;
        public static readonly Dictionary<OutputFieldSet, HashSet<string>> JsonFormatDependencyByFieldSet;
        private static readonly Dictionary<string, string> ExportFormatFieldByJsonFormatField;

        static ObservationPropertyFieldDescriptionHelper()
        {
            AllFields = LoadFieldDescriptionsFromJson();
            InitDataTypeEnum(AllFields);
            FieldByPropertyPath = AllFields.ToDictionary(x => x.PropertyPath.ToLower(), x => x);
            FieldsByFieldSet = CreateFieldSetDictionary(AllFields);
            JsonFormatDependencyByFieldSet = CreateJsonFormatDependencyDictionary(FieldsByFieldSet);
            ExportFormatFieldByJsonFormatField = CreateExportFormatFieldByJsonFormatFieldDictionary(AllFields);
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

        private static void InitDataTypeEnum(IEnumerable<PropertyFieldDescription> fields)
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

        public static bool ValidateUniqueDependencyMapping()
        {
            var dependencySet = new HashSet<string>();
            HashSet<string> dependsOnDuplicates = new HashSet<string>();

            foreach (var field in FieldsByFieldSet[OutputFieldSet.All])
            {
                if (dependencySet.Contains(field.DependsOn))
                {
                    dependsOnDuplicates.Add(field.DependsOn);
                }

                dependencySet.Add(field.DependsOn);
            }

            return dependsOnDuplicates.Count <= 0;
        }


        private static Dictionary<OutputFieldSet, ICollection<PropertyFieldDescription>> CreateFieldSetDictionary(
            IEnumerable<PropertyFieldDescription> fields)
        {
            var fieldsByFieldSet = new Dictionary<OutputFieldSet, ICollection<PropertyFieldDescription>>
            {
                {OutputFieldSet.Minimum, new List<PropertyFieldDescription>()},
                {OutputFieldSet.Extended, new List<PropertyFieldDescription>()},
                {OutputFieldSet.AllWithValues, new List<PropertyFieldDescription>()},
                {OutputFieldSet.All, new List<PropertyFieldDescription>()}
            };
            
            foreach (var field in fields)
            {
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
                else
                {
                    field.FieldSetEnum = OutputFieldSet.None;
                }
            }

            return fieldsByFieldSet;
        }

        private static Dictionary<string, string> CreateExportFormatFieldByJsonFormatFieldDictionary(
            IEnumerable<PropertyFieldDescription> fields)
        {
            var exportFormatFieldByJsonFormatField = new Dictionary<string, string>();
            foreach (var field in fields)
            {
                if (field.FieldSetEnum == OutputFieldSet.None) continue;

                var dependentFields = field.GetJsonFormatDependsOn();
                if (dependentFields.Length > 1)
                {
                    foreach (var dependentField in dependentFields)
                    {
                        exportFormatFieldByJsonFormatField.TryAdd(dependentField.ToLower(), field.PropertyPath);
                    }
                }
            }
            
            return exportFormatFieldByJsonFormatField;
        }

        private static Dictionary<OutputFieldSet, HashSet<string>> CreateJsonFormatDependencyDictionary(
            Dictionary<OutputFieldSet, ICollection<PropertyFieldDescription>> fieldsByFieldSet)
        {
            var jsonFormatDependencyByFieldSet = new Dictionary<OutputFieldSet, HashSet<string>>
            {
                {OutputFieldSet.Minimum, new HashSet<string>()},
                {OutputFieldSet.Extended, new HashSet<string>()}
            };

            foreach (var pair in fieldsByFieldSet)
            {
                if (pair.Key == OutputFieldSet.All || pair.Key == OutputFieldSet.AllWithValues)
                {
                    continue; // retrieve all fields from Elasticsearch
                }

                foreach (var field in pair.Value)
                {
                    foreach (var dependentField in field.GetJsonFormatDependsOn())
                    {
                        jsonFormatDependencyByFieldSet[pair.Key].Add(dependentField);
                    }
                }
            }

            return jsonFormatDependencyByFieldSet;
        }

        private static IEnumerable<PropertyFieldDescription> LoadFieldDescriptionsFromJson()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, @"Resources/ObservationFieldDescriptions.json");
            using var fs = FileSystemHelper.WaitForFileAndThenOpenIt(filePath);
            var fields = System.Text.Json.JsonSerializer.DeserializeAsync<List<PropertyFieldDescription>>(fs).Result;
            return fields;
        }

        public static List<PropertyFieldDescription> GetExportFieldsFromOutputFields(IEnumerable<string> outputFields)
        {
            if (!outputFields?.Any() ?? true) return FieldsByFieldSet[OutputFieldSet.AllWithValues].ToList();

            var fieldsSet = new HashSet<string>();
            foreach (var outputField in outputFields)
            {
                if (ExportFormatFieldByJsonFormatField.TryGetValue(outputField.ToLower(), out string exportField))
                {                    
                    fieldsSet.Add(exportField);
                }
                else
                {
                    fieldsSet.Add(outputField);
                }
            }

            var propertyFields = new List<PropertyFieldDescription>();
            foreach (var field in fieldsSet)
            {
                if (FieldByPropertyPath.TryGetValue(field.ToLower(), out var propertyField))
                {
                    if (propertyField.FieldSetEnum != OutputFieldSet.None) // If its not part of a field set, then the value isn't mapped in FlatObservation.
                    {
                        propertyFields.Add(propertyField);
                    }
                }
            }

            return propertyFields;
        }
    }
}
