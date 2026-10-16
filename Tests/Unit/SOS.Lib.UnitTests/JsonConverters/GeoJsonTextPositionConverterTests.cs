using GeoJSON.Text.Geometry;
using Newtonsoft.Json;
using SOS.Lib.JsonConverters;
using Xunit;

namespace SOS.Lib.UnitTests.JsonConverters;

public class GeoJsonTextPositionConverterTests
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        Converters = { new GeoJsonTextPositionConverter() }
    };

    [Theory]
    [InlineData("""{"Latitude":57.6,"Longitude":12.2}""", null)]
    [InlineData("""{"Latitude":57.6,"Longitude":12.2,"Altitude":null}""", null)]
    [InlineData("""{"Latitude":57.6,"Longitude":12.2,"Altitude":-10.5}""", -10.5)]
    [InlineData("""{"latitude":57.6,"longitude":12.2,"altitude":10}""", 10.0)]
    public void Deserialize_StoredPosition_PreservesCoordinates(string json, double? altitude)
    {
        foreach (var type in new[] { typeof(Position), typeof(IPosition) })
        {
            var position = Assert.IsType<Position>(JsonConvert.DeserializeObject(json, type, Settings));

            Assert.Equal(57.6, position.Latitude);
            Assert.Equal(12.2, position.Longitude);
            Assert.Equal(altitude, position.Altitude);
        }
    }

    [Fact]
    public void Deserialize_NullPosition_ReturnsNull()
    {
        Assert.Null(JsonConvert.DeserializeObject<IPosition>("null", Settings));
    }

    [Theory]
    [InlineData("""{"Longitude":12.2}""")]
    [InlineData("""{"Latitude":57.6}""")]
    [InlineData("""{"Latitude":null,"Longitude":12.2}""")]
    [InlineData("""{"Latitude":57.6,"Longitude":null}""")]
    [InlineData("""{"Latitude":"invalid","Longitude":12.2}""")]
    [InlineData("""{"Latitude":57.6,"Longitude":{}}""")]
    [InlineData("""{"Latitude":57.6,"Longitude":12.2,"Altitude":false}""")]
    public void Deserialize_InvalidPosition_ThrowsInsteadOfDefaultingCoordinates(string json)
    {
        Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<IPosition>(json, Settings));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(10.5)]
    public void Serialize_Position_PreservesExistingStoredFormat(double? altitude)
    {
        var position = new Position(57.6, 12.2, altitude);

        Assert.Equal(JsonConvert.SerializeObject(position), JsonConvert.SerializeObject(position, Settings));
    }
}
