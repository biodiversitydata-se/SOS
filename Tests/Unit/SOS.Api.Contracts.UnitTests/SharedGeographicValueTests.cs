using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SOS.Api.Contracts.Common.Geo;
using System.Text.Json;
using Xunit;

namespace SOS.Api.Contracts.UnitTests;

public class SharedGeographicValueTests
{
    [Fact]
    public void BoundingBox_PreservesJsonAndBsonFieldsAndCoordinates()
    {
        var boundingBox = LatLonBoundingBoxDto.Create(15, 60, 20, 55);
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var json = JsonSerializer.Serialize(boundingBox, options);
        using var expected = JsonDocument.Parse("""{"bottomRight":{"latitude":55,"longitude":20},"topLeft":{"latitude":60,"longitude":15}}""");
        using var actual = JsonDocument.Parse(json);
        Assert.True(JsonElement.DeepEquals(expected.RootElement, actual.RootElement));
        var restored = JsonSerializer.Deserialize<LatLonBoundingBoxDto>(json, options)!;
        Assert.Equal(60, restored.TopLeft.Latitude);
        Assert.Equal(15, restored.TopLeft.Longitude);
        Assert.Equal(55, restored.BottomRight.Latitude);
        Assert.Equal(20, restored.BottomRight.Longitude);

        var bson = boundingBox.ToBsonDocument();
        Assert.Equal(new[] { "BottomRight", "TopLeft" }, bson.Names.Order());
        Assert.Equal(new[] { "Latitude", "Longitude" }, bson["TopLeft"].AsBsonDocument.Names.Order());
        var fromBson = BsonSerializer.Deserialize<LatLonBoundingBoxDto>(bson);
        using var bsonJson = JsonDocument.Parse(JsonSerializer.Serialize(fromBson, options));
        Assert.True(JsonElement.DeepEquals(expected.RootElement, bsonJson.RootElement));
    }

    [Theory]
    [InlineData(null, 60d, 20d, 55d)]
    [InlineData(15d, null, 20d, 55d)]
    [InlineData(15d, 60d, null, 55d)]
    [InlineData(15d, 60d, 20d, null)]
    public void BoundingBox_CreateWithMissingBoundPreservesWorldExtent(
        double? left, double? top, double? right, double? bottom)
    {
        var box = LatLonBoundingBoxDto.Create(left, top, right, bottom);
        Assert.Equal(90, box.TopLeft.Latitude);
        Assert.Equal(-180, box.TopLeft.Longitude);
        Assert.Equal(-90, box.BottomRight.Latitude);
        Assert.Equal(180, box.BottomRight.Longitude);
    }
}
