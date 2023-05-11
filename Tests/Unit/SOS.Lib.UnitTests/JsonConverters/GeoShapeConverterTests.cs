using SOS.Lib.JsonConverters;
using System.Text.Json.Serialization;
using System.Text.Json;
using Xunit;
using Nest;
using FluentAssertions;

namespace SOS.Lib.UnitTests.JsonConverters
{
    public  class GeoShapeConverterTests
    {
        [Fact]
        public void DeserializeJsonString_CanParsePointGeoShape_GivenValidInput()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var jsonSerializerOptions = new JsonSerializerOptions {                
                Converters = {                    
                    new GeoShapeConverter()                    
                }
            };
            
            var strJson = """
                         {"type":"point","coordinates":[13.44774, 55.97684]}
                         """;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var point = JsonSerializer.Deserialize<PointGeoShape>(strJson, jsonSerializerOptions);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            point.Should().NotBeNull();
            point.Coordinates.Longitude.Should().BeApproximately(13.44774, 0.001);
            point.Coordinates.Latitude.Should().BeApproximately(55.97684, 0.001);
        }
    }
}
