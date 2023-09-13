using FluentAssertions;
using MongoDB.Driver;
using SharpCompress.Common;
using SOS.Lib.Helpers;
using SOS.Lib.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Xunit;

namespace SOS.Lib.UnitTests.Helpers
{
    public class ObservationPropertyFieldDescriptionHelperTool
    {       
        [Fact(Skip = "Intended to run on demand")]
        [Trait("Category", "Tool")]
        public void ReOrderProperties()
        {
            // Arrange
            string json = File.ReadAllText("c:/temp/2023-09-10/sortorder2.json");
            var sortOrders = System.Text.Json.JsonSerializer.Deserialize<List<FieldSortOrder>>(json);
            var allFields = new List<PropertyFieldDescription>(ObservationPropertyFieldDescriptionHelper.AllFields);
            var newOrder = new List<PropertyFieldDescription>();
            string strJsonOutput1 = JsonSerializer.Serialize(allFields, JsonSerializerOptions);

            // Act
            foreach (var field in sortOrders)
            {
                var item = allFields.Single(m => m.PropertyPath == field.PropertyPath);
                allFields.Remove(item);
                newOrder.Add(item);
            }
            newOrder.AddRange(allFields);

            string strJsonOutput = System.Text.Json.JsonSerializer.Serialize(newOrder, JsonSerializerOptions);
            File.WriteAllText("c:/temp/2023-09-10/newsortorder2.json", strJsonOutput);

            // Assert
            strJsonOutput.Should().NotBeNull();            
        }

        private class FieldSortOrder
        {
            public string Order { get; set; }
            public int SortOrder => int.Parse(Order);
            public string PropertyName { get; set; }
            public string PropertyPath { get; set; }
        }

        private JsonSerializerOptions JsonSerializerOptions => new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter()
            },
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement)
        };
    }
}
