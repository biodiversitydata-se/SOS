using FluentAssertions;
using Moq;
using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Gis;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Observations.Api.Managers;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Services.AdaptiveSearch;
using SOS.Observations.Api.Services.AdaptiveSearchLocations;
using System.Text.Json.Nodes;
using Xunit;

namespace SOS.Observations.Api.UnitTests.Services;

public class AdaptiveSearchLocationsAutoTests
{
    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 2)]
    [InlineData(false, 3)]
    [InlineData(true, 3)]
    public async Task Auto_PreservesCompletePresentation_ForLocationsCoincidenceAndClustering(bool sensitive, int clusterMode)
    {
        var records = new[]
        {
            Record("a", 15, sensitive, 1, 2023), Record("a", 15, sensitive, 2, 2025),
            Record("b", 15, sensitive, 3, 2024), Record("b", 15, sensitive, 4, 2025),
            Record("c", 15.0001, sensitive, 5, 2025), Record("c", 15.0001, sensitive, 6, 2025)
        };
        var manager = Manager(records);
        var service = Service(manager);
        var filter = new SearchFilterInternalDto { ProtectionFilter = sensitive ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public };
        AdaptiveSearchLocationsRequest Request(AdaptiveSearchLocationsStrategy strategy) => new()
        {
            Filter = filter, AdaptiveSearchStrategy = strategy, ObservationsLimit = 100,
            Cluster = clusterMode is 2 or 3, ClusterRadiusMeters = clusterMode is 1 or 3 ? 100 : null,
            MetersPerPixel = 1, DisablePixelClusteringAtMetersPerPixel = 0, ClusterRadiusPx = 40, MinLocationsPerCluster = 2,
            MinimumLocationCountBeforeClustering = 0
        };

        var auto = await service.SearchAsync(Request(AdaptiveSearchLocationsStrategy.Auto));
        var full = await service.SearchAsync(Request(AdaptiveSearchLocationsStrategy.ObservationsThenGrid));

        auto.LocationAggregationMode.Should().Be("representative");
        auto.AdaptiveSearchStrategy.Should().Be(AdaptiveSearchLocationsStrategy.LocationsThenGrid);
        auto.CacheStatus.Should().Be("BYPASS");
        auto.LocationSummaryApproximated.Should().BeTrue();
        auto.LocationPresentationStatusApproximated.Should().BeFalse();
        auto.LocationFeatures.Should().HaveCount(full.LocationFeatures.Count);
        foreach (var feature in auto.LocationFeatures)
        {
            var actual = LocationFeatureResponseMapper.ToProperties(feature);
            var expected = full.LocationFeatures.Select(f => LocationFeatureResponseMapper.ToProperties(f))
                .Single(f => f.Locations.Ids.SequenceEqual(actual.Locations.Ids));
            actual.Presentation.Status.Should().Be(expected.Presentation.Status);
            actual.Presentation.CutoffYearStatus.Should().Be(expected.Presentation.CutoffYearStatus);
            actual.ObservationCount.Should().Be(expected.ObservationCount);
            actual.IsPresentationStatusApproximated.Should().BeFalse();
            actual.IsSummaryApproximated.Should().BeTrue();
            actual.ObservationSummary.TaxaCount.Should().BeLessThan(expected.ObservationSummary.TaxaCount);
        }
        manager.Verify(m => m.GetLocationRepresentativeChunkAsync(It.IsAny<PreparedObservationFilter>(), 1001, "location.locationId"), Times.Once);
        manager.Verify(m => m.GetChunkAsync(It.IsAny<PreparedObservationFilter>(), 0, 100, It.IsAny<SearchExecutionOptions>()), Times.Once);
    }

    [Theory]
    [InlineData(2024, false, false)]
    [InlineData(null, true, false)]
    [InlineData(null, false, true)]
    public async Task Auto_FallsBackToAllObservations_ForCutoffMixedProtectionAndSamples(int? cutoff, bool mixed, bool samples)
    {
        var records = new[] { Record("a", 15, false, 1, 2023), Record("a", 15, mixed, 2, 2024), Record("a", 15, mixed, 3, 2025) };
        var manager = Manager(records);
        var result = await Service(manager).SearchAsync(new AdaptiveSearchLocationsRequest
        {
            Filter = new SearchFilterInternalDto { ProtectionFilter = mixed ? ProtectionFilterDto.BothPublicAndSensitive : ProtectionFilterDto.Public },
            ObservationsLimit = 100, CutoffYear = cutoff, CreateSampleObservations = samples
        });
        result.LocationAggregationMode.Should().BeNull();
        result.AdaptiveSearchStrategy.Should().Be(AdaptiveSearchLocationsStrategy.ObservationsThenGrid);
        result.LocationSummaryApproximated.Should().BeFalse();
        result.LocationPresentationStatusApproximated.Should().BeFalse();
        manager.Verify(m => m.GetChunkAsync(It.IsAny<PreparedObservationFilter>(), 0, 100, It.IsAny<SearchExecutionOptions>()), Times.Once);
        manager.Verify(m => m.GetLocationRepresentativeChunkAsync(It.IsAny<PreparedObservationFilter>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        if (cutoff.HasValue)
        {
            var properties = LocationFeatureResponseMapper.ToProperties(result.LocationFeatures.Single(), cutoff);
            properties.Presentation.Status.ToString().Should().Be("Present");
            properties.Presentation.CutoffYearStatus.ToString().Should().Be("After");
        }
    }

    [Theory]
    [InlineData(1, 1, false)]
    [InlineData(1, 2, true)]
    [InlineData(1, 3, true)]
    [InlineData(2, 2, true)]
    [InlineData(3, 2, true)]
    public async Task Auto_EnforcesEstimatedAndActualStrictLocationLimits(long estimate, int count, bool grid)
    {
        // All source locations coincide: the raw count must still be checked before merging.
        var records = Enumerable.Range(0, count).Select(i => Record(i.ToString(), 15, false, 1, 2025)).ToArray();
        var manager = Manager(records, estimate);
        var result = await Service(manager).SearchAsync(new AdaptiveSearchLocationsRequest
        {
            Filter = new SearchFilterInternalDto(), LocationAggregationLimit = 2, MaxGridCells = 100, MaxZoom = 20
        });
        result.ResultType.Should().Be(grid ? "geogrid" : "locations");
        result.AdaptiveSearchStrategy.Should().Be(AdaptiveSearchLocationsStrategy.LocationsThenGrid);
        result.LocationPresentationStatusApproximated.Should().BeFalse();
        manager.Verify(m => m.GetLocationRepresentativeChunkAsync(It.IsAny<PreparedObservationFilter>(), 3, "location.locationId"),
            estimate < 2 ? Times.Once() : Times.Never());
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    public async Task ExplicitRepresentativeStrategy_ReportsApproximationWithCutoffUnlessComplete(int count, bool approximated)
    {
        var manager = Manager(Enumerable.Range(0, count).Select(i => Record("a", 15, false, 1, 2023 + i * 2)).ToArray());
        var result = await Service(manager).SearchAsync(new AdaptiveSearchLocationsRequest
        {
            Filter = new SearchFilterInternalDto(), AdaptiveSearchStrategy = AdaptiveSearchLocationsStrategy.LocationsThenGrid, CutoffYear = 2024
        });
        result.LocationPresentationStatusApproximated.Should().Be(approximated);
        result.AdaptiveSearchStrategy.Should().Be(AdaptiveSearchLocationsStrategy.LocationsThenGrid);
        result.LocationFeatures.Single().IsPresentationStatusApproximated.Should().Be(approximated);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Auto_EmptyResultHasNoApproximation_AndReportsResolvedStrategy(bool emptyBoundingBox)
    {
        var result = await Service(Manager([])).SearchAsync(new AdaptiveSearchLocationsRequest { Filter = new(), BoundingBoxIsEmpty = emptyBoundingBox });
        result.LocationFeatures.Should().BeEmpty();
        result.AdaptiveSearchStrategy.Should().Be(AdaptiveSearchLocationsStrategy.LocationsThenGrid);
        result.LocationSummaryApproximated.Should().BeFalse();
        result.LocationPresentationStatusApproximated.Should().BeFalse();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExplicitRepresentatives_CombinePresentationApproximationFromAnyClusterMember(bool cluster)
    {
        var manager = Manager([Record("a", 15, false, 1, 2023), Record("b", 15.0001, false, 1, 2023), Record("b", 15.0001, false, 2, 2025)]);
        var result = await Service(manager).SearchAsync(new AdaptiveSearchLocationsRequest
        {
            Filter = new(), AdaptiveSearchStrategy = AdaptiveSearchLocationsStrategy.LocationsThenGrid,
            CutoffYear = 2024, Cluster = cluster, MetersPerPixel = 1, DisablePixelClusteringAtMetersPerPixel = 0, ClusterRadiusPx = 40,
            MinLocationsPerCluster = 2, MinimumLocationCountBeforeClustering = 0
        });
        result.LocationPresentationStatusApproximated.Should().BeTrue();
        if (cluster)
            result.LocationFeatures.Should().ContainSingle().Which.IsPresentationStatusApproximated.Should().BeTrue();
        else
            result.LocationFeatures.Select(f => f.IsPresentationStatusApproximated).Should().BeEquivalentTo([false, true]);
    }

    private static JsonObject Record(string locationId, double longitude, bool sensitive, int taxon, int year) => new()
    {
        ["sensitive"] = sensitive,
        ["location"] = new JsonObject { ["locationId"] = locationId, ["type"] = 0, ["decimalLongitude"] = longitude, ["decimalLatitude"] = 59d },
        ["occurrence"] = new JsonObject { ["isPositiveObservation"] = true },
        ["taxon"] = new JsonObject { ["id"] = taxon },
        ["event"] = new JsonObject { ["endYear"] = year }
    };

    private static Mock<IObservationManager> Manager(JsonObject[] records, long? estimatedCount = null)
    {
        var manager = new Mock<IObservationManager>(MockBehavior.Strict);
        manager.Setup(m => m.PrepareObservationFilterAsync(null, null, It.IsAny<SearchFilter>()))
            .ReturnsAsync((int? role, string application, SearchFilter filter) => new PreparedObservationFilter(filter));
        manager.Setup(m => m.GetCountAndExtentAsync(It.IsAny<PreparedObservationFilter>(), true))
            .ReturnsAsync(((long)records.Length, LatLonBoundingBoxDto.Create(14, 60, 16, 58), estimatedCount ?? records.Select(r => r["location"]!["locationId"]!.ToString()).Distinct().LongCount()));
        manager.Setup(m => m.GetChunkAsync(It.IsAny<PreparedObservationFilter>(), 0, It.IsAny<int>(), It.IsAny<SearchExecutionOptions>()))
            .ReturnsAsync(new PagedResult<JsonObject> { Records = records, TotalCount = records.Length });
        var groups = records.GroupBy(r => r["location"]!["locationId"]!.ToString()).ToList();
        manager.Setup(m => m.GetLocationRepresentativeChunkAsync(It.IsAny<PreparedObservationFilter>(), It.IsAny<int>(), "location.locationId"))
            .ReturnsAsync(new LocationRepresentativeResult<JsonObject>
            {
                Records = groups.Select(g => g.First()).ToArray(),
                ObservationCountByLocationId = groups.ToDictionary(g => g.Key, g => g.LongCount())
            });
        manager.Setup(m => m.GetGeogridTileAggregationAsync(It.IsAny<PreparedObservationFilter>(), It.IsAny<int>()))
            .ReturnsAsync(new GeoGridTileResult { GridCellTiles = Array.Empty<GridCellTile>() });
        return manager;
    }

    private static AdaptiveSearchLocationsService Service(Mock<IObservationManager> manager)
    {
        var aggregator = new AdaptiveSearchLocationAggregator();
        return new(manager.Object, Mock.Of<ILocationGeometryLookupService>(), new UnusedCache(),
            aggregator, new AdaptiveSearchLocationFeatureFactory(aggregator));
    }

    private sealed class UnusedCache : IAdaptiveSearchLocationsCache
    {
        public string CreateCacheKey(int? roleId, string? application, bool sensitive, SearchFilterInternalDto filter) => throw new InvalidOperationException("Cache must be bypassed.");
        public bool TryGet(string key, LatLonBoundingBoxDto? bbox, out List<CachedLocationAggregate> locations, bool requireExactBoundingBox = false) => throw new InvalidOperationException("Cache must be bypassed.");
        public void Set(string key, LatLonBoundingBoxDto? bbox, List<CachedLocationAggregate> locations) => throw new InvalidOperationException("Cache must be bypassed.");
        public LatLonBoundingBoxDto? CreateBoundingBox(IEnumerable<CachedLocationAggregate> locations) => null;
    }
}
