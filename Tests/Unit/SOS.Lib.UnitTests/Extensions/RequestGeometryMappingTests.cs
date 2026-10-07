using GeoJSON.Text.Geometry;
using Moq;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.Models.Search.Input;
using System.Text.Json;
using Xunit;

namespace SOS.Lib.UnitTests.Extensions;

public class RequestGeometryMappingTests
{
    [Fact]
    public void HasSearchGeometries_ChecksGeoJsonCollectionWithoutReadingCoordinates()
    {
        var position = new Mock<IPosition>();
        var point = new GeoJSON.Text.Geometry.Point(position.Object);
        position.Invocations.Clear();
        var filter = new GeographicsFilterDto { Geometries = [] };

        Assert.False(filter.HasSearchGeometries());
        filter.Geometries.Add(null!);
        Assert.False(filter.HasSearchGeometries());
        filter.Geometries.Add(point);
        Assert.True(filter.HasSearchGeometries());
        filter.Geometries.Clear();
        Assert.False(filter.HasSearchGeometries());
        filter.Geometries = null;
        Assert.False(filter.HasSearchGeometries());
        position.VerifyNoOtherCalls();
    }

    [Fact]
    public void HasSearchGeometries_UsesServerCollectionEvenWhenDtoCollectionIsPopulated()
    {
        var input = new GeographicsFilterInputModel
        {
            ServerGeometries = [],
            Geometries = [new GeoJSON.Text.Geometry.Point(new GeoJSON.Text.Geometry.Position(62, 16))]
        };
        GeographicsFilterDto filter = input;

        Assert.False(filter.HasSearchGeometries());
        input.ServerGeometries.Add(new GeometryFactory().CreatePoint(new Coordinate(16, 62)));
        Assert.True(filter.HasSearchGeometries());
        input.ServerGeometries = null;
        Assert.False(filter.HasSearchGeometries());
    }

    [Fact]
    public void HasSearchGeometries_AcceptsMissingFilter()
    {
        GeographicsFilterDto? filter = null;
        Assert.False(filter.HasSearchGeometries());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FromDto_PreservesNullAndEmptyGeometryCollections(bool emptyCollection)
    {
        var source = new GeographicsFilterDto { Geometries = emptyCollection ? [] : null };

        var input = GeographicsFilterInputModel.FromDto(source);

        Assert.Null(input.Geometries);
        if (emptyCollection) Assert.Empty(input.ServerGeometries!);
        else Assert.Null(input.ServerGeometries);
        Assert.Same(input, GeographicsFilterInputModel.FromDto(input));
    }

    [Theory]
    [InlineData("""{"type":"Point","coordinates":[18,59,120]}""", "POINT (18 59)")]
    [InlineData("""{"type":"LineString","coordinates":[[18,59,120],[19,60,130]]}""", "LINESTRING (18 59, 19 60)")]
    [InlineData("""{"type":"Polygon","coordinates":[[[16,60],[19,60],[19,63],[16,63],[16,60]],[[17,61],[17,62],[18,62],[18,61],[17,61]]]}""", "POLYGON ((16 60, 19 60, 19 63, 16 63, 16 60), (17 61, 17 62, 18 62, 18 61, 17 61))")]
    [InlineData("""{"type":"MultiPoint","coordinates":[[18,59,120],[19,60,130]]}""", "MULTIPOINT ((18 59), (19 60))")]
    [InlineData("""{"type":"MultiLineString","coordinates":[[[18,59],[19,60]],[[20,61],[21,62]]]}""", "MULTILINESTRING ((18 59, 19 60), (20 61, 21 62))")]
    [InlineData("""{"type":"MultiPolygon","coordinates":[[[[16,60,10],[19,60,20],[19,63,30],[16,60,10]]]]}""", "MULTIPOLYGON (((16 60, 19 60, 19 63, 16 60)))")]
    [InlineData("""{"type":"GeometryCollection","geometries":[{"type":"Point","coordinates":[18,59,120]},{"type":"LineString","coordinates":[[19,60],[20,61]]}]}""", "GEOMETRYCOLLECTION (POINT (18 59), LINESTRING (19 60, 20 61))")]
    [InlineData("""{"type":"LineString","coordinates":[]}""", "LINESTRING EMPTY")]
    [InlineData("""{"type":"MultiPoint","coordinates":[]}""", "MULTIPOINT EMPTY")]
    [InlineData("""{"type":"MultiLineString","coordinates":[]}""", "MULTILINESTRING EMPTY")]
    [InlineData("""{"type":"MultiPolygon","coordinates":[]}""", "MULTIPOLYGON EMPTY")]
    [InlineData("""{"type":"GeometryCollection","geometries":[]}""", "GEOMETRYCOLLECTION EMPTY")]
    public void GetSearchGeometries_PreservesTwoDimensionalCoordinatesRingsAndParts(string json, string expectedWkt)
    {
        var geoJson = JsonSerializer.Deserialize<IGeometryObject>(json)!;
        var expected = new WKTReader().Read(expectedWkt);
        var filter = new GeographicsFilterDto { Geometries = [geoJson] };

        var actual = Assert.Single(filter.GetSearchGeometries()!);

        Assert.Equal(4326, actual.SRID);
        Assert.Equal(expected.OgcGeometryType, actual.OgcGeometryType);
        Assert.True(expected.EqualsExact(actual), $"Expected: {expectedWkt}; actual: {actual.AsText()}");
        Assert.All(actual.Coordinates, coordinate => Assert.True(double.IsNaN(coordinate.Z), "Search geometries are two-dimensional."));
        Assert.Equal(expected.IsEmpty, actual.IsEmpty);
        Assert.False(filter.HasGeometryConversionError());
    }

    [Fact]
    public void GetSearchGeometries_ReflectsChangesToProgrammaticallyCreatedDtos()
    {
        var filter = new GeographicsFilterDto { Geometries = [] };
        Assert.Empty(filter.GetSearchGeometries()!);
        filter.Geometries.Add(new GeoJSON.Text.Geometry.Point(new GeoJSON.Text.Geometry.Position(59, 18)));
        Assert.Equal(18, Assert.Single(filter.GetSearchGeometries()!).Coordinate.X);
        filter.Geometries.Clear();
        Assert.Empty(filter.GetSearchGeometries()!);
        filter.Geometries = null;
        Assert.Null(filter.GetSearchGeometries());
    }

    [Fact]
    public void GetSearchGeometries_IgnoresNullEntriesWithoutDroppingSuppliedGeometries()
    {
        var filter = new GeographicsFilterDto
        {
            Geometries = [null!, new GeoJSON.Text.Geometry.Point(new GeoJSON.Text.Geometry.Position(59, 18)), null!]
        };

        var geometry = Assert.Single(filter.GetSearchGeometries()!);

        Assert.Equal(18, geometry.Coordinate.X);
        Assert.Equal(59, geometry.Coordinate.Y);
        Assert.False(filter.HasGeometryConversionError());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InvalidGeometry_CannotBeUsedForSearchOrExport(bool serverInput)
    {
        var source = new GeographicsFilterDto
        {
            Geometries =
            [
                new GeoJSON.Text.Geometry.Point(new GeoJSON.Text.Geometry.Position(62, 16)),
                JsonSerializer.Deserialize<IGeometryObject>(
                    """{"type":"Polygon","coordinates":[[[16,62],[17,63]]]}""")!
            ]
        };
        var input = GeographicsFilterInputModel.FromDto(source);
        GeographicsFilterDto filter = serverInput ? input : source;

        Assert.True(filter.HasGeometryConversionError());
        Assert.Throws<ArgumentException>(() => filter.GetSearchGeometries());
        var searchFilter = new SearchFilterDto { Geographics = filter };
        Assert.Throws<ArgumentException>(() => searchFilter.ToSearchFilter(0, ProtectionFilterDto.Public, "sv-SE"));
        Assert.Throws<ArgumentException>(() => input.ToSerializableGeographicsFilterDto());
    }

    [Fact]
    public void FromDto_DoesNotHideUnexpectedConversionFailures()
    {
        var position = new Mock<IPosition>();
        var point = new GeoJSON.Text.Geometry.Point(position.Object);
        var exception = new InvalidOperationException("Unexpected coordinate access failure.");
        position.SetupGet(value => value.Longitude).Throws(exception);
        var source = new GeographicsFilterDto { Geometries = [point] };

        Assert.Same(exception, Assert.Throws<InvalidOperationException>(
            () => GeographicsFilterInputModel.FromDto(source)));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void GetSearchGeometries_PreservesRejectionOfNonFiniteCoordinates(double altitude)
    {
        var filter = new GeographicsFilterDto
        {
            Geometries = [new GeoJSON.Text.Geometry.Point(new GeoJSON.Text.Geometry.Position(59, 18, altitude))]
        };

        Assert.True(filter.HasGeometryConversionError());
        var result = filter.TryGetSearchGeometries();
        Assert.True(result.IsFailure);
        Assert.Equal("Invalid geometry", result.Error);
    }
}
