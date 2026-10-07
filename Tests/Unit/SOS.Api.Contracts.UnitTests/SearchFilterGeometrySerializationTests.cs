using FluentAssertions;
using GeoJSON.Text.Geometry;
using SOS.Api.Contracts.Observations.Requests;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace SOS.Api.Contracts.UnitTests;

public class SearchFilterGeometrySerializationTests
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    [Theory]
    [InlineData("""{"type":"Point","coordinates":[16,62,10]}""")]
    [InlineData("""{"type":"MultiPoint","coordinates":[[16,62],[17,63]]}""")]
    [InlineData("""{"type":"LineString","coordinates":[[16,62],[17,63]]}""")]
    [InlineData("""{"type":"MultiLineString","coordinates":[[[16,62],[17,63]],[[18,64],[19,65]]]}""")]
    [InlineData("""{"type":"Polygon","coordinates":[[[16,62,10],[17,62,20],[17,63,30],[16,62,10]],[[16.1,62.1,40],[16.2,62.1,50],[16.2,62.2,60],[16.1,62.1,40]]]}""")]
    [InlineData("""{"type":"MultiPolygon","coordinates":[[[[16,62],[17,62],[17,63],[16,62]]],[[[18,64],[19,64],[19,65],[18,64]]]]}""")]
    [InlineData("""{"type":"GeometryCollection","geometries":[{"type":"Point","coordinates":[16,62,10]},{"type":"LineString","coordinates":[[16,62],[17,63]]}]}""")]
    public void Filter_RoundTripsGeoJsonGeometriesIncludingHolesAndAltitude(string geometryJson)
    {
        var json = "{\"geographics\":{\"geometries\":[" + geometryJson + "]}}";

        var filter = JsonSerializer.Deserialize<SearchFilterDto>(json, Options)!;
        var result = JsonSerializer.SerializeToNode(filter, Options)!;

        filter.Geographics!.Geometries.Should().ContainSingle().Which.Should().BeAssignableTo<IGeometryObject>();
        JsonNode.DeepEquals(JsonNode.Parse(geometryJson), result["geographics"]!["geometries"]![0]).Should().BeTrue();
    }

    [Theory]
    [InlineData("""{"geographics":{}}""", true)]
    [InlineData("""{"geographics":{"geometries":null}}""", true)]
    [InlineData("""{"geographics":{"geometries":[]}}""", false)]
    public void Filter_PreservesMissingNullAndEmptyGeometries(string json, bool expectNull)
    {
        var filter = JsonSerializer.Deserialize<SearchFilterDto>(json, Options)!;
        var result = JsonSerializer.SerializeToNode(filter, Options)!;

        if (expectNull)
        {
            filter.Geographics!.Geometries.Should().BeNull();
            result["geographics"]!["geometries"].Should().BeNull();
        }
        else
        {
            filter.Geographics!.Geometries.Should().BeEmpty();
            result["geographics"]!["geometries"]!.AsArray().Should().BeEmpty();
        }
    }
}
