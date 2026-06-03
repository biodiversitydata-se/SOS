using FluentAssertions;
using NetTopologySuite.Geometries;
using NSubstitute;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Shared;
using SOS.Lib.Services.Interfaces;
using Xunit;

namespace SOS.Lib.UnitTests.Managers;

public class FilterManagerTests
{
    [Fact]
    public async Task GetAreaFiltersAsAreaTuplesAsync_ReturnsEmptyList_WhenGeometriesAreMissing()
    {
        // Arrange
        var areaCache = CreateAreaCache();
        areaCache.GetGeometriesAsync(Arg.Any<IEnumerable<(AreaType areaType, string featureId)>>())
            .Returns(Task.FromResult<IDictionary<(AreaType areaType, string featureId), Geometry>>(null!));

        var filterManager = CreateFilterManager(areaCache);

        // Act
        var result = await filterManager.GetAreaFiltersAsAreaTuplesAsync([CreateAreaFilter()]);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAreaFiltersAsAreaTuplesAsync_ReturnsEmptyList_WhenAreasAreMissing()
    {
        // Arrange
        var areaCache = CreateAreaCache();
        areaCache.GetGeometriesAsync(Arg.Any<IEnumerable<(AreaType areaType, string featureId)>>())
            .Returns(Task.FromResult<IDictionary<(AreaType areaType, string featureId), Geometry>>(
                new Dictionary<(AreaType areaType, string featureId), Geometry>
                {
                    [(AreaType.County, "1")] = new Point(new Coordinate(1, 2))
                }));
        areaCache.GetAreasAsync(Arg.Any<IEnumerable<(AreaType areaType, string featureId)>>())
            .Returns(Task.FromResult<IEnumerable<Area>>(null!));

        var filterManager = CreateFilterManager(areaCache);

        // Act
        var result = await filterManager.GetAreaFiltersAsAreaTuplesAsync([CreateAreaFilter()]);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    private static FilterManager CreateFilterManager(IAreaCache areaCache)
    {
        return new FilterManager(
            Substitute.For<ITaxonManager>(),
            Substitute.For<IUserService>(),
            areaCache,
            Substitute.For<IDataProviderCache>());
    }

    private static IAreaCache CreateAreaCache()
    {
        return Substitute.For<IAreaCache>();
    }

    private static AreaFilter CreateAreaFilter()
    {
        return new AreaFilter
        {
            AreaType = AreaType.County,
            FeatureId = "1"
        };
    }
}
