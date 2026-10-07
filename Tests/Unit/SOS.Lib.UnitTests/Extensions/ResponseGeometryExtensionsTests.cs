using SOS.Api.Contracts.Common.Geo;
using GeoJSON.Text.Geometry;
using NetTopologySuite.IO;
using SOS.Lib.Enums;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Shared;
using System.Text.Json;
using Xunit;

namespace SOS.Lib.UnitTests.Extensions;

public class ResponseGeometryExtensionsTests
{
    [Theory]
    [InlineData("POINT Z (18 59 120)", """{"type":"Point","coordinates":[18,59,120]}""")]
    [InlineData("LINESTRING Z (18 59 120, 19 60 130)", """{"type":"LineString","coordinates":[[18,59,120],[19,60,130]]}""")]
    [InlineData("POLYGON ((16 60, 19 60, 19 63, 16 63, 16 60), (17 61, 17 62, 18 62, 18 61, 17 61))",
        """{"type":"Polygon","coordinates":[[[16,60],[19,60],[19,63],[16,63],[16,60]],[[17,61],[17,62],[18,62],[18,61],[17,61]]]}""")]
    [InlineData("MULTIPOINT Z ((18 59 120), (19 60 130))", """{"type":"MultiPoint","coordinates":[[18,59,120],[19,60,130]]}""")]
    [InlineData("MULTILINESTRING ((18 59, 19 60), (20 61, 21 62))", """{"type":"MultiLineString","coordinates":[[[18,59],[19,60]],[[20,61],[21,62]]]}""")]
    [InlineData("MULTIPOLYGON (((16 60, 19 60, 19 63, 16 60)))", """{"type":"MultiPolygon","coordinates":[[[[16,60],[19,60],[19,63],[16,60]]]]}""")]
    [InlineData("GEOMETRYCOLLECTION (POINT (18 59), LINESTRING (19 60, 20 61))",
        """{"type":"GeometryCollection","geometries":[{"type":"Point","coordinates":[18,59]},{"type":"LineString","coordinates":[[19,60],[20,61]]}]}""")]
    public void AdaptiveFilterArea_PreservesGeoJsonCoordinatesPolygonHolesAndAltitude(string wkt, string expectedJson)
    {
        var geometry = new WKTReader().Read(wkt);
        var area = AdaptiveSearchResultFactory.ToAreaWithGeometryDto((CreateArea(), geometry));

        using var expected = JsonDocument.Parse(expectedJson);
        using var actual = JsonDocument.Parse(JsonSerializer.Serialize(area.Geometry));
        Assert.True(JsonElement.DeepEquals(expected.RootElement, actual.RootElement),
            $"Expected: {expected.RootElement}; actual: {actual.RootElement}");
    }

    [Theory]
    [InlineData("POINT EMPTY")]
    [InlineData("LINESTRING EMPTY")]
    [InlineData("POLYGON EMPTY")]
    [InlineData("MULTIPOINT EMPTY")]
    [InlineData("MULTILINESTRING EMPTY")]
    [InlineData("MULTIPOLYGON EMPTY")]
    [InlineData("GEOMETRYCOLLECTION EMPTY")]
    public void EmptyGeometry_KeepsExistingGeoJsonFallbackBehavior(string wkt)
    {
        var geometry = new WKTReader().Read(wkt);
        var json = new GeoJsonWriter().Write(geometry);
        var existingError = Record.Exception(() => JsonSerializer.Deserialize<IGeometryObject>(json));

        if (existingError is not null)
        {
            Assert.Throws(existingError.GetType(), () => geometry.ToGeoJsonGeometry());
            return;
        }

        using var expected = JsonDocument.Parse(json);
        using var actual = JsonDocument.Parse(JsonSerializer.Serialize(geometry.ToGeoJsonGeometry()));
        Assert.True(JsonElement.DeepEquals(expected.RootElement, actual.RootElement));
    }

    [Fact]
    public void NullPointAndAdaptiveFilterAreaGeometry_RemainNull()
    {
        NetTopologySuite.Geometries.Point? point = null;
        Assert.Null(point.ToGeoJsonPoint());
        Assert.Null(AdaptiveSearchResultFactory.ToAreaWithGeometryDto((CreateArea(), null!)).Geometry);
        Assert.Null(AdaptiveSearchResultFactory.CreateObservationsResult<string>(null, 0, null, null).FilterAreas);
    }

    private static Area CreateArea() => new(AreaType.Municipality, "0180")
    {
        Name = "Test area",
        BoundingBox = new LatLonBoundingBoxDto
        {
            TopLeft = new LatLonCoordinateDto(63, 16),
            BottomRight = new LatLonCoordinateDto(60, 19)
        }
    };
}
