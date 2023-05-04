using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using SOS.Lib.Enums;
using Xunit;

namespace SOS.Lib.UnitTests.Models.TaxonTree
{
    public class EnumsJsonTests
    {       
        string jsonWithEnumValue = """
        {
            "LocationType": 2 
        }
        """;

        string jsonWithEnumStringValue = """
        {
            "LocationType": "Polygon"
        }
        """;

        private class MyClass
        {
            public LocationType LocationType { get; set; }
        }

        [Fact]
        public void ParseJson_Succeeds_GivenJsonWithEnumValue()
        {            
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var obj = JsonSerializer.Deserialize<MyClass>(jsonWithEnumValue);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            obj.Should().NotBeNull();
        }

        [Fact]
        public void ParseJson_Fails_GivenJsonWithEnumString()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => JsonSerializer.Deserialize<MyClass>(jsonWithEnumStringValue);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<JsonException>();                
        }

        [Fact]
        public void ParseJson_Succeeds_GivenJsonWithEnumStringAndEnumConverter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var jsonOptions = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var obj = JsonSerializer.Deserialize<MyClass>(jsonWithEnumStringValue, jsonOptions);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            obj.Should().NotBeNull();
        }
    }
}