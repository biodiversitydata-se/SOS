using FluentAssertions;
using Moq;
using NetTopologySuite.Geometries;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.Models.Search.Input;
using SOS.Shared.Api.Utilities.Objects;
using System.Text.Json;
using Xunit;

namespace SOS.Observations.Api.UnitTests.Utilities;

public class SearchFilterUtilityTests
{
    [Fact]
    public async Task InitializeSearchFilterAsync_KeepsServerGeometryWithoutCreatingContractGeometry()
    {
        var utility = CreateUtility();
        var polygon = CreateRectangle(15, 60, 16, 61);
        var input = new GeographicsFilterInputModel { ServerGeometries = [polygon] };
        var filter = new SearchFilterDto
        {
            Geographics = input
        };

        var prepared = await utility.InitializeSearchFilterAsync(filter);

        prepared.Should().BeSameAs(filter);
        prepared.Geographics.Should().BeSameAs(input);
        input.Geometries.Should().BeNull();
        input.ServerGeometries.Should().ContainSingle()
            .Which.IsValid.Should().BeTrue();
        prepared.ToSearchFilter(0, ProtectionFilterDto.Public, "sv-SE")
            .Location.Geometries.Geometries.Should().BeSameAs(input.ServerGeometries);
        prepared.Geographics!.BoundingBox.Should().NotBeNull();
    }

    [Theory]
    [InlineData("""{"type":"Point","coordinates":[16,62]}""")]
    [InlineData("""{"type":"Polygon","coordinates":[[[16,60],[19,60],[19,63],[16,63],[16,60]],[[17,61],[17,62],[18,62],[18,61],[17,61]]]}""")]
    [InlineData("""{"type":"MultiPolygon","coordinates":[[[[16,60],[19,60],[19,63],[16,60]]]]}""")]
    public async Task InitializeSearchFilterAsync_KeepsConvertedGeometriesThroughSearchAndExport(string geometryJson)
    {
        var utility = CreateUtility();
        var geoJson = JsonSerializer.Deserialize<GeoJSON.Text.Geometry.IGeometryObject>(geometryJson)!;
        var source = new GeographicsFilterDto
        {
            Geometries = [geoJson],
            MaxDistanceFromPoint = 150,
            ConsiderDisturbanceRadius = true,
            ConsiderObservationAccuracy = true,
            ConsiderAuthorizationBuffer = false,
            IncludeObservationsOutsideSweden = true,
            LocationIds = ["location-1"],
            LocationNameFilter = "test*",
            MaxAccuracy = 500
        };
        var filter = new SearchFilterDto { Geographics = source };

        var prepared = await utility.InitializeSearchFilterAsync(filter);

        prepared.Should().BeSameAs(filter);
        var input = prepared.Geographics.Should().BeOfType<GeographicsFilterInputModel>().Subject;
        input.Geometries.Should().BeNull();
        input.ServerGeometries.Should().ContainSingle().Which.IsValid.Should().BeTrue();
        input.GetSearchGeometries().Should().BeSameAs(input.ServerGeometries);
        input.GetSearchGeometries().Should().BeSameAs(input.ServerGeometries);
        input.Should().BeEquivalentTo(source, options => options.Excluding(value => value.Geometries)
            .Excluding(value => value.BoundingBox));
        input.BoundingBox.Should().NotBeNull();
        source.Geometries.Should().ContainSingle().Which.Should().BeSameAs(geoJson);
        prepared.ToSearchFilter(0, ProtectionFilterDto.Public, "sv-SE")
            .Location.Geometries.Geometries.Should().BeSameAs(input.ServerGeometries);

        prepared.MaterializeGeoJsonGeometriesForSerialization();
        prepared.Geographics.Should().BeOfType<GeographicsFilterDto>();
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var restored = JsonSerializer.Deserialize<SearchFilterDto>(JsonSerializer.Serialize(prepared, options), options)!;
        restored.Geographics!.GetSearchGeometries().Should().ContainSingle().Which.AsText()
            .Should().Be(input.ServerGeometries!.Single().AsText());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InitializeSearchFilterAsync_PreservesMissingAndEmptyGeometries(bool emptyCollection)
    {
        var filter = new SearchFilterDto
        {
            Geographics = new GeographicsFilterDto { Geometries = emptyCollection ? [] : null }
        };

        var prepared = await CreateUtility().InitializeSearchFilterAsync(filter);

        var input = prepared.Geographics.Should().BeOfType<GeographicsFilterInputModel>().Subject;
        input.Geometries.Should().BeNull();
        if (emptyCollection) input.ServerGeometries.Should().BeEmpty();
        else input.ServerGeometries.Should().BeNull();
        input.HasSearchGeometries().Should().BeFalse();
    }

    [Fact]
    public async Task InitializeSearchFilterAsync_CreatesEmptyServerInputWhenGeographicFilterIsMissing()
    {
        var prepared = await CreateUtility().InitializeSearchFilterAsync(new SearchFilterDto());

        var input = prepared.Geographics.Should().BeOfType<GeographicsFilterInputModel>().Subject;
        input.ServerGeometries.Should().BeNull();
        input.Geometries.Should().BeNull();
        input.BoundingBox.Should().NotBeNull();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InitializeSearchFilterAsync_PreservesInvalidPolygonForValidation(bool serverInput)
    {
        var utility = CreateUtility();
        const string invalidPolygon = """
            {"type":"Polygon","coordinates":[[[16,62],[17,63]]]}
            """;
        var contract = new GeographicsFilterDto
        {
            Geometries = [JsonSerializer.Deserialize<GeoJSON.Text.Geometry.IGeometryObject>(invalidPolygon)!]
        };
        var filter = new SearchFilterDto
        {
            Geographics = serverInput ? GeographicsFilterInputModel.FromDto(contract) : contract
        };

        var prepared = await utility.InitializeSearchFilterAsync(filter);

        prepared.Geographics.HasGeometryConversionError().Should().BeTrue();
        prepared.Geographics.BoundingBox.Should().BeNull();
        prepared.Geographics.HasSearchGeometries().Should().BeTrue();
        var mapToSearch = () => prepared.ToSearchFilter(0, ProtectionFilterDto.Public, "sv-SE");
        mapToSearch.Should().Throw<ArgumentException>().WithMessage("Invalid geometry*");
    }

    private static SearchFilterUtility CreateUtility()
    {
        var cache = new Mock<IAreaCache>();
        cache.Setup(areaCache => areaCache.GetGeometryAsync(AreaType.EconomicZoneOfSweden, "1"))
            .ReturnsAsync(CreateRectangle(10, 50, 20, 70));
        return new SearchFilterUtility(new AreaConfiguration(), cache.Object);
    }

    private static Polygon CreateRectangle(double minX, double minY, double maxX, double maxY) =>
        new GeometryFactory().CreatePolygon([
            new Coordinate(minX, minY),
            new Coordinate(maxX, minY),
            new Coordinate(maxX, maxY),
            new Coordinate(minX, maxY),
            new Coordinate(minX, minY)
        ]);
}
