using FluentAssertions;
using GeoJSON.Text.Geometry;
using GeoJSON.Text.Feature;
using SOS.Api.Contracts.DataStewardship.Responses;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace SOS.Api.Contracts.UnitTests;

public class DataStewardshipResponseSerializationTests
{
    [Theory]
    [InlineData("""{"type":"Point","coordinates":[18,59]}""")]
    [InlineData("""{"type":"LineString","coordinates":[[18,59],[19,60]]}""")]
    [InlineData("""{"type":"Polygon","coordinates":[[[18,59],[19,59],[19,60],[18,59]]]}""")]
    [InlineData("""{"type":"MultiPolygon","coordinates":[[[[18,59],[19,59],[19,60],[18,59]]]]}""")]
    public void SurveyGeometry_RoundTripsWithSystemTextJson(string geometryJson)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var json = "{\"surveyLocation\":{\"emplacement\":" + geometryJson + "}}";

        var dto = JsonSerializer.Deserialize<DsEventDto>(json, options)!;
        var result = JsonSerializer.SerializeToNode(dto, options)!;

        dto.SurveyLocation.Emplacement.Should().BeAssignableTo<IGeometryObject>();
        JsonNode.DeepEquals(JsonNode.Parse(geometryJson), result["surveyLocation"]!["emplacement"])
            .Should().BeTrue();
    }

    [Fact]
    public void GeoJsonFeatureCollection_RoundTripsGeometryAndProperties()
    {
        const string json = """
            {"type":"FeatureCollection","features":[{"type":"Feature","id":"site:1",
            "geometry":{"type":"Point","coordinates":[18,59]},"properties":{"count":7}}]}
            """;
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        var dto = JsonSerializer.Deserialize<FeatureCollection>(json, options)!;
        var feature = dto.Features.Single();
        var result = JsonSerializer.SerializeToNode(dto, options)!;

        feature.Geometry.Should().BeOfType<Point>();
        feature.Id.Should().Be("site:1");
        result["features"]![0]!["properties"]!["count"]!.GetValue<int>().Should().Be(7);
        JsonNode.DeepEquals(JsonNode.Parse(json)!["features"]![0]!["geometry"], result["features"]![0]!["geometry"])
            .Should().BeTrue();
    }

    [Theory]
    [InlineData("""{"type":"FeatureCollection","features":[]}""")]
    [InlineData("""{"type":"FeatureCollection","bbox":[15,59,17,61],"features":[{"type":"Feature","id":"site:1","bbox":[15,59,17,61],"geometry":null,"properties":null}]}""")]
    [InlineData("""{"type":"FeatureCollection","features":[{"type":"Feature","geometry":{"type":"Polygon","coordinates":[[[15,59,10],[17,59,20],[17,61,30],[15,59,10]],[[15.5,59.5,40],[16.5,59.5,50],[16.5,60.5,60],[15.5,59.5,40]]]},"properties":{"count":7}}]}""")]
    [InlineData("""{"type":"FeatureCollection","features":[{"type":"Feature","geometry":{"type":"MultiPolygon","coordinates":[[[[15,59],[16,59],[16,60],[15,59]]],[[[16,60],[17,60],[17,61],[16,60]]]]},"properties":{}}]}""")]
    public void GeoJsonFeatureCollection_RoundTripsStandardFieldsAndGeometry(string json)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var collection = JsonSerializer.Deserialize<FeatureCollection>(json, options)!;

        var serialized = JsonSerializer.SerializeToNode(collection, options);

        JsonNode.DeepEquals(JsonNode.Parse(json), serialized).Should().BeTrue();
    }
}
