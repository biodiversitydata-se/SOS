using FluentAssertions;
using NetTopologySuite.Geometries;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Process.UnitTests.TestHelpers.Factories;
using Xunit;
using ProcessedLocation = SOS.Lib.Models.Processed.Observation.Location;

namespace SOS.Process.UnitTests.Helpers;

public class AreaHelperUncertainAreaTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public async Task AddAreaDataToProcessedLocationShouldSetUncertainAreaTypesToNull()
    {
        var geometryFactory = new GeometryFactory();
        var geometry = geometryFactory.CreatePolygon([
            new Coordinate(0, 0),
            new Coordinate(0, 2),
            new Coordinate(2, 2),
            new Coordinate(2, 0),
            new Coordinate(0, 0)
        ]);
        var areas = new List<AreaWithGeometry>
        {
            new(AreaType.County, "county") { Name = "County", Geometry = geometry },
            new(AreaType.Municipality, "municipality") { Name = "Municipality", Geometry = geometry },
            new(AreaType.Province, "province") { Name = "Province", Geometry = geometry }
        };
        var areaRepository = ProcessedAreaRepositoryStubFactory.Create(areas);
        var areaHelper = new AreaHelper(new AreaConfiguration(), areaRepository.Object);
        await areaHelper.InitializeAsync();
        var location = new ProcessedLocation(LocationType.Point)
        {
            DecimalLatitude = 1,
            DecimalLongitude = 1
        };

        areaHelper.AddAreaDataToProcessedLocation(
            location,
            AreaType.Municipality,
            AreaType.Province);

        location.County.Should().NotBeNull();
        location.Municipality.Should().BeNull();
        location.Province.Should().BeNull();
        location.Attributes.ProvincePartIdByCoordinate.Should().BeNull();
        location.Attributes.CountyPartIdByCoordinate.Should().BeNull();
    }
}
