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
}
