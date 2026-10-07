using FluentAssertions;
using NetTopologySuite.Geometries;
using SOS.Lib.JsonConverters;
using System.Text.Json;
using Xunit;

namespace SOS.Lib.UnitTests.JsonConverters;

public class GeometryConverterTests
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters = { new GeometryConverter() }
    };

    [Fact]
    public void Deserialize_WithValidPoint_ReturnsPoint()
    {
        const string json = """
            { "coordinates": [16.98, 62.90], "type": "point" }
            """;

        var geometry = JsonSerializer.Deserialize<Geometry>(json, JsonSerializerOptions);

        geometry.Should().BeOfType<Point>();
    }

    [Fact]
    public void Deserialize_WithValidPolygon_ReturnsPolygon()
    {
        const string json = """
            {
              "coordinates": [
                [
                  [16.0, 62.0],
                  [17.0, 62.0],
                  [17.0, 63.0],
                  [16.0, 63.0],
                  [16.0, 62.0]
                ]
              ],
              "type": "polygon"
            }
            """;

        var geometry = JsonSerializer.Deserialize<Geometry>(json, JsonSerializerOptions);

        geometry.Should().BeOfType<Polygon>();
    }

    [Fact]
    public void Deserialize_WithDegeneratePolygon_ThrowsJsonException()
    {
        const string json = """
            {
              "coordinates": [
                [
                  [16.98472499942703, 62.90148608256228],
                  [16.98472499942703, 62.90148608256228]
                ]
              ],
              "type": "polygon"
            }
            """;

        var act = () => JsonSerializer.Deserialize<Geometry>(json, JsonSerializerOptions);

        act.Should().Throw<JsonException>().WithMessage("*Invalid geometry*");
    }

    [Fact]
    public void Deserialize_WithNullGeometryInCollection_ThrowsJsonException()
    {
        var act = () => JsonSerializer.Deserialize<List<Geometry>>("[null]", JsonSerializerOptions);

        act.Should().Throw<JsonException>().WithMessage("*Invalid geometry*");
    }

    [Theory]
    [InlineData("""{"type":"Point","coordinates":[1.698e1,6.29e1]}""", "POINT (16.98 62.9)")]
    [InlineData("""{"coordinates":[[16,62],[17,63]],"type":"LineString"}""", "LINESTRING (16 62, 17 63)")]
    [InlineData("""{"type":"MultiLineString","coordinates":[[[16,62],[17,63]],[[18,64],[19,65]]]}""", "MULTILINESTRING ((16 62, 17 63), (18 64, 19 65))")]
    [InlineData("""{"type":"Polygon","coordinates":[[[16,62],[17,62],[17,63],[16,62]],[[16.1,62.1],[16.2,62.1],[16.2,62.2],[16.1,62.1]]]}""",
        "POLYGON ((16 62, 17 62, 17 63, 16 62), (16.1 62.1, 16.2 62.1, 16.2 62.2, 16.1 62.1))")]
    [InlineData("""{"type":"MultiPolygon","coordinates":[[[[16,62],[17,62],[17,63],[16,62]]],[[[18,64],[19,64],[19,65],[18,64]]]]}""",
        "MULTIPOLYGON (((16 62, 17 62, 17 63, 16 62)), ((18 64, 19 64, 19 65, 18 64)))")]
    public void Deserialize_PreservesCoordinatesRingsAndGeometryParts(string json, string expectedWkt)
    {
        JsonSerializer.Deserialize<Geometry>(json, JsonSerializerOptions)!.AsText().Should().Be(expectedWkt);
    }

    [Fact]
    public void Deserialize_SkipsUnknownNestedProperties()
    {
        const string json = """
            {"extra":{"coordinates":[1,2],"nested":{}},"type":"Point","bbox":[16,62,16,62],"coordinates":[16,62]}
            """;

        JsonSerializer.Deserialize<Geometry>(json, JsonSerializerOptions)!.AsText().Should().Be("POINT (16 62)");
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("""{"type":"Point","coordinates":[16]}""")]
    [InlineData("""{"type":"Point","coordinates":[16,62,"bad"]}""")]
    [InlineData("""{"type":"Polygon","coordinates":[]}""")]
    [InlineData("""{"type":"Polygon","coordinates":[[[16,62],[17,62],[17,63],[16,63]]]}""")]
    [InlineData("""{"type":"LineString","coordinates":[[16,62]]}""")]
    [InlineData("""{"type":"Unsupported","coordinates":[16,62]}""")]
    public void Deserialize_InvalidCoordinatesThrowJsonException(string json)
    {
        var action = () => JsonSerializer.Deserialize<Geometry>(json, JsonSerializerOptions);
        action.Should().Throw<JsonException>().WithMessage("*Invalid geometry*");
    }

    [Fact]
    public void Deserialize_PreservesExistingTwoDimensionalSearchBehavior()
    {
        const string json = """{"type":"Point","coordinates":[16,62,10]}""";
        var geometry = JsonSerializer.Deserialize<Geometry>(json, JsonSerializerOptions)!;

        geometry.AsText().Should().Be("POINT (16 62)");
        double.IsNaN(geometry.Coordinate.Z).Should().BeTrue();
    }
}
