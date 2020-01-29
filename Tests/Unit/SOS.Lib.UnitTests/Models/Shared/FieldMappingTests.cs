using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Shared;
using SOS.TestHelpers.JsonConverters;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SOS.Lib.UnitTests.Models.Shared
{
    public class FieldMappingTests
    {

        [Fact]
        public void Reads_Gender_Field_Mapping_Json_file()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            string filename = @"Resources\GenderFieldMapping.json";

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var fieldMappings = CreateFieldMappingFromFile(filename);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            fieldMappings.Should().NotBeNull();
        }

        private FieldMapping CreateFieldMappingFromFile(string filename)
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, filename);
            string str = File.ReadAllText(filePath, Encoding.UTF8);
            var serializerSettings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter> { new FieldMappingValueConverter() }
            };
            var fieldMappings = JsonConvert.DeserializeObject<FieldMapping>(str, serializerSettings);
            return fieldMappings;
        }
    }
}