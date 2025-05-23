﻿using AgileObjects.NetStandardPolyfills;
using FluentAssertions;
using SOS.Export.Models;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Import.LiveIntegrationTests.TestDataTools
{
    public class CreatePropertyFieldDescriptionsTool : TestBase
    {
        public class SsosFieldDescription
        {
            public int Id { get; set; }
            public string Class { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Label { get; set; }
            public string SwedishLabel { get; set; }
            public int SortOrder { get; set; }
            public int Importance { get; set; }
        }

        private async Task<List<SsosFieldDescription>> ReadSsosFieldDescriptions()
        {
            string filePath = @"C:\Temp\SSOS field descriptions.json";
            string strJson = await File.ReadAllTextAsync(filePath);
            var fieldDescriptions = System.Text.Json.JsonSerializer.Deserialize<List<SsosFieldDescription>>(strJson);
            return fieldDescriptions;
        }

        [Fact]
        [Trait("Category", "Tool")]
        public void ValidateUniquePropertyNamesAndTitles()
        {
            // Act
            bool unique = ObservationPropertyFieldDescriptionHelper.ValidateUniquePropertyNames();

            // Assert
            unique.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "Tool")]
        public async Task CreateFlatObservationClassValueSwitchStatements()
        {
            // Arrange
            var propertyFields = ObservationPropertyFieldDescriptionHelper.AllFields;
            StringBuilder sb = new StringBuilder();

            // Act
            foreach (var fieldDescription in propertyFields.Where(m => !string.IsNullOrEmpty(m.FieldSet)))
            {
                sb.AppendLine($"case \"{fieldDescription.PropertyPath}\":");
                sb.AppendLine($"    return {fieldDescription.PropertyName};");
            }
            string result = sb.ToString();

            // Assert
            result.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        ///     
        /// </summary>
        [Fact]
        [Trait("Category", "Tool")]
        public async Task CreateFlatObservationClassProperties()
        {
            // Arrange
            var fieldDescriptions = ObservationPropertyFieldDescriptionHelper.AllFields;
            var sb = new StringBuilder();

            // Act
            foreach (var fieldDescription in fieldDescriptions.Where(m => !string.IsNullOrEmpty(m.FieldSet)))
            {
                string dataType = GetDataTypeString(fieldDescription.DataType, fieldDescription.DataTypeIsNullable.GetValueOrDefault());
                string propertyName = fieldDescription.PropertyName;
                string str = $"public {dataType} {propertyName} => _observation?.{fieldDescription.PropertyPath.Replace(".", "?.")};";
                sb.AppendLine(str);
            }
            string result = sb.ToString();

            // Assert
            result.Should().NotBeNullOrEmpty();
        }

        private string GetDataTypeString(string dataType, bool isNullable)
        {
            switch (dataType)
            {
                case "String":
                    return "string";
                case "Boolean":
                    return "bool?";
                case "DateTime":
                    return "DateTime?";
                case "Double":
                    return "double?";
                case "Int32":
                    return "int?";
                case "Int64":
                    return "long?";
                default:
                    return "string";
                    //throw new ArgumentException($"Mapping for \"{dataType}\" doesn't exist");
            }
        }

        [Fact]
        [Trait("Category", "Tool")]
        public async Task CreateInitialFieldDescriptions()
        {
            // Arrange
            var ssosFieldDescriptions = await ReadSsosFieldDescriptions();
            var sosFieldDescriptions = FieldDescriptionHelper.GetAllFieldDescriptions().ToList();
            var observationType = typeof(Lib.Models.Processed.Observation.Observation);
            var propertyFields = GetAllProperties(observationType, true);

            // Act
            AddSsosTitlesToPropertyFields(propertyFields, ssosFieldDescriptions);
            AddSosDwcInfoToPropertyFields(propertyFields, sosFieldDescriptions);
            string strJson = System.Text.Json.JsonSerializer.Serialize(propertyFields);

            // Assert
            strJson.Should().NotBeNullOrEmpty();
        }

        private static void AddSsosTitlesToPropertyFields(List<PropertyFieldDescription> propertyFields, List<SsosFieldDescription> ssosFieldDescriptions)
        {
            foreach (var propertyField in propertyFields)
            {
                var ssosFieldDescription = ssosFieldDescriptions.FirstOrDefault(
                    m => m.Name.ToLowerInvariant() == propertyField.PropertyName.ToLowerInvariant());
                if (ssosFieldDescription != null)
                {
                    propertyField.SwedishTitle = ssosFieldDescription.SwedishLabel;
                    propertyField.EnglishTitle = ssosFieldDescription.Label;
                }
            }
        }

        private static void AddSosDwcInfoToPropertyFields(List<PropertyFieldDescription> propertyFields, List<FieldDescription> sosFieldDescriptions)
        {
            foreach (var propertyField in propertyFields)
            {
                var sosFieldDescription = sosFieldDescriptions.FirstOrDefault(
                    m => m.Name.ToLowerInvariant() == propertyField.PropertyName.ToLowerInvariant());
                if (sosFieldDescription != null && sosFieldDescription.IsDwC)
                {
                    propertyField.DwcIdentifier = sosFieldDescription.DwcIdentifier;
                    propertyField.DwcName = sosFieldDescription.Name;
                }
            }
        }

        private static List<PropertyFieldDescription> GetAllProperties(Type type, bool includeClassTypes)
        {
            var properties = new List<PropertyFieldDescription>();
            GetAllProperties(type, "", includeClassTypes, properties, null);
            return properties;
        }

        private static void GetAllProperties(Type currentType, string prefix, bool includeClassTypes, List<PropertyFieldDescription> properties, PropertyFieldDescription parent)
        {
            foreach (PropertyInfo pi in currentType.GetProperties().Where(m => m.CanRead && m.IsPublic()))
            {
                PropertyFieldDescription propField = null;
                bool isValueTypeOrString = pi.PropertyType.IsValueTypeOrString();
                if (includeClassTypes || isValueTypeOrString)
                {
                    var propertyField = new PropertyFieldDescription();
                    propertyField.PropertyPath = $"{prefix}{pi.Name}";
                    propertyField.PropertyName = pi.Name;
                    if (isValueTypeOrString)
                    {
                        propertyField.DataType = pi.PropertyType.Name;
                    }

                    propField = propertyField;
                    properties.Add(propertyField);
                }

                if (!pi.PropertyType.IsValueTypeOrString())
                {
                    if (pi.PropertyType.IsIEnumerable())
                    {
                        var types = pi.PropertyType.GetGenericArguments();
                        foreach (var genericType in types)
                        {
                            if (!genericType.IsValueTypeOrString())
                            {
                                GetAllProperties(genericType, prefix + pi.Name + ".", includeClassTypes, properties, propField);
                            }
                        }
                    }
                    else
                    {
                        GetAllProperties(pi.PropertyType, prefix + pi.Name + ".", includeClassTypes, properties, propField);
                    }
                }

            }
        }
    }
}