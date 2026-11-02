using FluentAssertions;
using Moq;
using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Observations.Responses;
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

public class AdaptiveSearchLocationsClusteringTests
{
    private static readonly LatLonBoundingBoxDto Bbox = LatLonBoundingBoxDto.Create(14, 60, 16, 58);

    [Fact]
    public async Task CanceledLocationSearchStopsBeforeFetchingObservations()
    {
        var fixture = new Fixture();
        using var cts = new CancellationTokenSource();
        var pending = new TaskCompletionSource<(long, LatLonBoundingBoxDto?, long)>(TaskCreationOptions.RunContinuationsAsynchronously);
        fixture.Manager.Setup(manager => manager.GetCountAndExtentAsync(It.IsAny<PreparedObservationFilter>(), It.IsAny<bool>()))
            .Returns<PreparedObservationFilter, bool>((filter, _) =>
            {
                Assert.Equal(cts.Token, filter.CancellationToken);
                return pending.Task.WaitAsync(filter.CancellationToken);
            });
        var task = fixture.Service.SearchAsync(new AdaptiveSearchLocationsRequest
        {
            Filter = new() { ProtectionFilter = ProtectionFilterDto.Public }, CancellationToken = cts.Token,
            AdaptiveSearchStrategy = AdaptiveSearchLocationsStrategy.ObservationsThenGrid,
            ObservationsLimit = 1000, InputBbox = Bbox, Bbox = Bbox
        });
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        fixture.Manager.Verify(manager => manager.GetChunkAsync(It.IsAny<PreparedObservationFilter>(),
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SearchExecutionOptions>()), Times.Never);
    }

    [Fact]
    public async Task PreCanceledLocationSearchDoesNotQueryTheManager()
    {
        var fixture = new Fixture();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => fixture.Service.SearchAsync(
            new AdaptiveSearchLocationsRequest { Filter = new(), CancellationToken = cts.Token }));
        Assert.Empty(fixture.Manager.Invocations);
    }

    [Fact]
    public void RequestClusteringDefaults_MatchSharedDefaults()
    {
        var request = new AdaptiveSearchLocationsRequest { Filter = new() };
        request.DisablePixelClusteringAtMetersPerPixel.Should().Be(AdaptiveSearchLocationsDefaults.DisablePixelClusteringAtMetersPerPixel);
        request.ProximityClusterRadiusPx.Should().Be(AdaptiveSearchLocationsDefaults.ProximityClusterRadiusPx);
        request.ClusterRadiusPx.Should().Be(AdaptiveSearchLocationsDefaults.ClusterRadiusPx);
        request.MinimumLocationCountBeforeClustering.Should().Be(AdaptiveSearchLocationsDefaults.MinLocationCountBeforeClustering);
    }

    [Theory]
    [InlineData(true, 1d, 2d, false)]
    [InlineData(true, 2d, 2d, false)]
    [InlineData(true, 2.01d, 2d, true)]
    [InlineData(true, 0.5d, 0d, true)]
    [InlineData(true, 3d, 4d, false)]
    [InlineData(true, 4d, 4d, false)]
    [InlineData(true, 4.01d, 4d, true)]
    [InlineData(true, null, 2d, true)]
    [InlineData(false, 3d, 2d, false)]
    [InlineData(false, 1d, 0d, false)]
    [InlineData(false, null, 0d, false)]
    public void PixelClusteringPolicy_RespectsSwitchResolutionAndCutoff(bool cluster, double? resolution, double cutoff, bool expected)
    {
        var request = Request(resolution, cutoff, cluster);
        AdaptiveSearchLocationsClusteringPolicy.ShouldClusterPixels(request).Should().Be(expected);
    }

    public static IEnumerable<object[]> SearchCases()
    {
        foreach (var strategy in new[] { AdaptiveSearchLocationsStrategy.ObservationsThenGrid, AdaptiveSearchLocationsStrategy.Auto })
        foreach (var (resolution, cutoff, clustered) in new (double?, double, bool)[]
                 { (1, 2, false), (2, 2, false), (2.01, 2, true), (1, 0, true), (3, 4, false), (null, 2, true) })
            yield return [strategy, resolution, cutoff, clustered];
    }

    [Theory]
    [MemberData(nameof(SearchCases))]
    public async Task Search_AppliesCutoffToFullAndRepresentativeResults(
        AdaptiveSearchLocationsStrategy strategy, double? resolution, double cutoff, bool clustered)
    {
        var fixture = new Fixture();
        var result = await fixture.Service.SearchAsync(Request(resolution, cutoff, strategy: strategy));

        result.ResultType.Should().Be("locations");
        result.LocationFeatures.Should().HaveCount(clustered ? 1 : 3);
        result.LocationFeatures.Should().OnlyContain(feature => feature.IsCluster == clustered);
        result.LocationFeatures.Sum(feature => feature.ObservationCount).Should().Be(3);
        result.LocationsBeforeClustering.Should().Be(3);
        result.LocationAggregationMode.Should().Be(strategy == AdaptiveSearchLocationsStrategy.Auto ? "representative" : null);
        fixture.Manager.Verify(manager => manager.GetCountAndExtentAsync(It.IsAny<PreparedObservationFilter>(), true), Times.Once);
    }

    [Fact]
    public async Task CachedLocations_ReevaluateResolutionAndSettingsWithoutFetchingObservationsAgain()
    {
        var fixture = new Fixture();
        var cases = new (double Resolution, double Cutoff, bool Cluster, int Count)[]
        {
            (3, 2, true, 1), (2, 2, true, 3), (1, 2, true, 3),
            (1, 0, true, 1), (3, 2, false, 3), (3, 2, true, 1)
        };
        for (var index = 0; index < cases.Length; index++)
        {
            var item = cases[index];
            var result = await fixture.Service.SearchAsync(Request(item.Resolution, item.Cutoff, item.Cluster));
            result.LocationFeatures.Should().HaveCount(item.Count);
            result.CacheStatus.Should().Be(index == 0 ? "MISS" : "HIT");
            result.ObservationsTotalCount.Should().Be(3);
        }
        fixture.Manager.Verify(manager => manager.GetCountAndExtentAsync(It.IsAny<PreparedObservationFilter>(), true), Times.Once);
        fixture.Manager.Verify(manager => manager.GetChunkAsync(It.IsAny<PreparedObservationFilter>(), 0, 1000, It.IsAny<SearchExecutionOptions>()), Times.Once);
    }

    [Theory]
    [InlineData(AdaptiveSearchLocationsStrategy.ObservationsThenGrid, true)]
    [InlineData(AdaptiveSearchLocationsStrategy.ObservationsThenGrid, false)]
    [InlineData(AdaptiveSearchLocationsStrategy.Auto, true)]
    [InlineData(AdaptiveSearchLocationsStrategy.Auto, false)]
    public async Task MeterClusteringContinuesWhenPixelClusteringIsDisabled(AdaptiveSearchLocationsStrategy strategy, bool cluster)
    {
        var fixture = new Fixture();
        for (var index = 0; index < 2; index++)
        {
            var result = await fixture.Service.SearchAsync(Request(1, cluster: cluster, meterRadius: 100, strategy: strategy));
            var feature = result.LocationFeatures.Should().ContainSingle().Subject;
            LocationFeatureResponseMapper.ToProperties(feature).GroupingType.Should().Be(LocationGroupingType.MeterCluster);
            feature.ObservationCount.Should().Be(3);
        }
    }

    [Theory]
    [InlineData(2, 1)]
    [InlineData(3, 3)]
    [InlineData(4, 3)]
    public async Task AboveResolutionCutoff_PreservesMinimumMapObjectThreshold(int minimumCount, int expectedCount)
    {
        var fixture = new Fixture();
        var result = await fixture.Service.SearchAsync(Request(3, minimumCount: minimumCount));
        result.LocationFeatures.Should().HaveCount(expectedCount);
    }

    [Fact]
    public async Task DefaultCutoff_PreventsAdaptiveTargetFromReactivatingPixelClustering()
    {
        var fixture = new Fixture(count: 401);
        var result = await fixture.Service.SearchAsync(Request(2));
        result.LocationFeatures.Should().HaveCount(401);
        result.LocationFeatures.Should().OnlyContain(feature => !feature.IsCluster);
    }

    [Fact]
    public async Task DisabledPixelClustering_StillGroupsCoincidentLocations()
    {
        var fixture = new Fixture(coincident: true);
        var result = await fixture.Service.SearchAsync(Request(1));
        result.LocationFeatures.Should().HaveCount(2);
        var coincident = result.LocationFeatures.Single(feature => feature.ObservationCount == 2);
        LocationFeatureResponseMapper.ToProperties(coincident).GroupingType.Should().Be(LocationGroupingType.CoLocatedLocations);
    }

    [Theory]
    [InlineData(2, false)]
    [InlineData(0, true)]
    public async Task SampleObservations_UseTheSamePixelClusteringCutoff(double cutoff, bool clustered)
    {
        var fixture = new Fixture();
        var request = Request(2, cutoff, samples: true);
        var result = await fixture.Service.SearchAsync(request);
        result.LocationFeatures.Should().OnlyContain(feature => feature.IsCluster == clustered);
        result.LocationFeatures.Should().HaveCount(clustered ? 1 : 3);
        result.CacheStatus.Should().Be("BYPASS");
    }

    [Fact]
    public async Task EmptyResult_RemainsEmptyWithDefaultCutoff()
    {
        var fixture = new Fixture(count: 0);
        var result = await fixture.Service.SearchAsync(Request(1));
        result.ResultType.Should().Be("locations");
        result.LocationFeatures.Should().BeEmpty();
    }

    [Fact]
    public async Task ResolutionCutoff_DoesNotOverrideGridStrategy()
    {
        var fixture = new Fixture(count: 1001);
        var result = await fixture.Service.SearchAsync(Request(1));
        result.ResultType.Should().Be("geogrid");
        fixture.Manager.Verify(manager => manager.GetChunkAsync(It.IsAny<PreparedObservationFilter>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SearchExecutionOptions>()), Times.Never);
    }

    [Theory]
    [InlineData(true, 1d, 2d)]
    [InlineData(true, null, 0d)]
    [InlineData(false, 1d, 0d)]
    public void ProximityGrouping_RequiresPixelSwitchAndResolution(bool cluster, double? resolution, double radius)
    {
        AdaptiveSearchLocationsClusteringPolicy.GetProximityClusterRadiusPx(Request(resolution, cluster: cluster, proximity: 2))
            .Should().Be(radius);
    }

    public static IEnumerable<object[]> ProximitySearchCases()
    {
        foreach (var strategy in new[] { AdaptiveSearchLocationsStrategy.ObservationsThenGrid, AdaptiveSearchLocationsStrategy.Auto })
        foreach (var (resolution, minimum, radius) in new[] { (1d, 0, 2d), (3d, 3, 6d), (3d, 4, 6d), (3d, 2, 1500d) })
            yield return [strategy, resolution, minimum, radius];
    }

    [Theory]
    [MemberData(nameof(ProximitySearchCases))]
    public async Task ProximityGrouping_PreservesMetadataAndSummariesAcrossStrategiesAndCacheHits(
        AdaptiveSearchLocationsStrategy strategy, double resolution, int minimum, double radius)
    {
        var fixture = new Fixture(spacing: 0.00002, observationsPerLocation: 2);
        for (var index = 0; index < 2; index++)
        {
            var result = await fixture.Service.SearchAsync(Request(resolution, strategy: strategy, minimumCount: minimum, proximity: 2));
            var feature = result.LocationFeatures.Should().ContainSingle().Subject;
            feature.GroupingType.Should().Be(LocationGroupingType.PixelCluster);
            var properties = LocationFeatureResponseMapper.ToProperties(feature);
            properties.Cluster!.RadiusMeters.Should().Be(radius);
            properties.ObservationCount.Should().Be(6);
            properties.ObservationSummary.TaxaCount.Should().Be(strategy == AdaptiveSearchLocationsStrategy.Auto ? 3 : 6);
            properties.Locations.Ids.Should().BeEquivalentTo("site-0", "site-1", "site-2");
            properties.IsPresentationStatusApproximated.Should().BeFalse();
            properties.IsSummaryApproximated.Should().Be(strategy == AdaptiveSearchLocationsStrategy.Auto);
            result.CacheStatus.Should().Be(strategy == AdaptiveSearchLocationsStrategy.Auto ? "BYPASS" : index == 0 ? "MISS" : "HIT");
        }
        fixture.Manager.Verify(manager => manager.GetCountAndExtentAsync(It.IsAny<PreparedObservationFilter>(), true),
            strategy == AdaptiveSearchLocationsStrategy.Auto ? Times.Exactly(2) : Times.Once());
    }

    [Fact]
    public async Task CachedProximityGrouping_ReevaluatesZoomRadiusAndMasterSwitch()
    {
        var fixture = new Fixture(spacing: 0.00002);
        var cases = new (double Resolution, double Proximity, bool Cluster, int Count)[]
        {
            (1, 2, true, 1), (0.1, 2, true, 3), (1, 0, true, 3),
            (1, 2, false, 3), (1, 2, true, 1)
        };
        for (var index = 0; index < cases.Length; index++)
        {
            var item = cases[index];
            var result = await fixture.Service.SearchAsync(Request(item.Resolution, cluster: item.Cluster, proximity: item.Proximity));
            result.LocationFeatures.Should().HaveCount(item.Count);
            result.CacheStatus.Should().Be(index == 0 ? "MISS" : "HIT");
        }
        fixture.Manager.Verify(manager => manager.GetChunkAsync(It.IsAny<PreparedObservationFilter>(), 0, 1000, It.IsAny<SearchExecutionOptions>()), Times.Once);
    }

    [Fact]
    public async Task ProximityGrouping_WithSamplesUsesRawObservationPath()
    {
        var result = await new Fixture().Service.SearchAsync(Request(2, samples: true, proximity: 500));
        var feature = result.LocationFeatures.Should().ContainSingle().Subject;
        feature.GroupingType.Should().Be(LocationGroupingType.PixelCluster);
        feature.ClusterRadiusMeters.Should().Be(1000);
        feature.ObservationCount.Should().Be(3);
        result.CacheStatus.Should().Be("BYPASS");
    }

    [Fact]
    public async Task ProximityGrouping_WithoutResolutionDoesNotBypassCountThreshold()
    {
        var result = await new Fixture(spacing: 0.00002).Service.SearchAsync(Request(null, minimumCount: 3, proximity: 2));
        result.LocationFeatures.Should().HaveCount(3).And.OnlyContain(feature => !feature.IsCluster);
    }

    private static AdaptiveSearchLocationsRequest Request(double? resolution, double cutoff = 2, bool cluster = true,
        double? meterRadius = null, AdaptiveSearchLocationsStrategy strategy = AdaptiveSearchLocationsStrategy.ObservationsThenGrid,
        int minimumCount = 0, bool samples = false, double proximity = 0) => new()
    {
        Filter = new SearchFilterInternalDto { ProtectionFilter = ProtectionFilterDto.Public },
        AdaptiveSearchStrategy = strategy, ObservationsLimit = 1000, MaxGridCells = 100, MaxZoom = 21,
        InputBbox = Bbox, Bbox = Bbox, Cluster = cluster, ClusterRadiusMeters = meterRadius,
        MetersPerPixel = resolution, DisablePixelClusteringAtMetersPerPixel = cutoff, ClusterRadiusPx = 500,
        ProximityClusterRadiusPx = proximity,
        MinLocationsPerCluster = 2, MinimumLocationCountBeforeClustering = minimumCount,
        UseCache = true, CreateSampleObservations = samples
    };

    private sealed class Fixture
    {
        public Mock<IObservationManager> Manager { get; } = new(MockBehavior.Strict);
        public AdaptiveSearchLocationsService Service { get; }

        public Fixture(int count = 3, bool coincident = false, double spacing = 0.0001, int observationsPerLocation = 1)
        {
            var records = Enumerable.Range(0, count * observationsPerLocation).Select(index =>
            {
                var locationIndex = index / observationsPerLocation;
                return new JsonObject
                {
                    ["location"] = new JsonObject
                    {
                        ["locationId"] = $"site-{locationIndex}", ["type"] = 0,
                        ["decimalLongitude"] = 15 + (coincident && locationIndex == 1 ? 0 : locationIndex) * spacing,
                        ["decimalLatitude"] = 59d
                    },
                    ["occurrence"] = new JsonObject { ["isPositiveObservation"] = true },
                    ["taxon"] = new JsonObject { ["id"] = index + 1 }
                };
            }).ToArray();
            Manager.Setup(manager => manager.PrepareObservationFilterAsync(null, null, It.IsAny<SearchFilter>()))
                .ReturnsAsync((int? role, string application, SearchFilter filter) => new PreparedObservationFilter(filter));
            Manager.Setup(manager => manager.GetCountAndExtentAsync(It.IsAny<PreparedObservationFilter>(), true))
                .ReturnsAsync(((long)records.Length, Bbox, (long)count));
            Manager.Setup(manager => manager.GetChunkAsync(It.IsAny<PreparedObservationFilter>(), 0, 1000, It.IsAny<SearchExecutionOptions>()))
                .ReturnsAsync(new PagedResult<JsonObject> { Records = records, TotalCount = records.Length });
            var locationGroups = records.GroupBy(record => record["location"]!["locationId"]!.ToString()).ToList();
            Manager.Setup(manager => manager.GetLocationRepresentativeChunkAsync(It.IsAny<PreparedObservationFilter>(), 1001, "location.locationId"))
                .ReturnsAsync(new LocationRepresentativeResult<JsonObject>
                {
                    Records = locationGroups.Select(group => group.First()).ToArray(),
                    ObservationCountByLocationId = locationGroups.ToDictionary(group => group.Key, group => group.LongCount())
                });
            Manager.Setup(manager => manager.GetGeogridTileAggregationAsync(It.IsAny<PreparedObservationFilter>(), It.IsAny<int>()))
                .ReturnsAsync(new GeoGridTileResult { GridCellTiles = [] });
            var aggregator = new AdaptiveSearchLocationAggregator();
            Service = new(Manager.Object, Mock.Of<ILocationGeometryLookupService>(), new MemoryCache(),
                aggregator, new AdaptiveSearchLocationFeatureFactory(aggregator));
        }
    }

    private sealed class MemoryCache : IAdaptiveSearchLocationsCache
    {
        private List<CachedLocationAggregate>? _locations;
        public string CreateCacheKey(int? roleId, string? application, bool sensitive, SearchFilterInternalDto filter) => "locations";
        public bool TryGet(string key, LatLonBoundingBoxDto? bbox, out List<CachedLocationAggregate> locations, bool requireExactBoundingBox = false)
        {
            locations = _locations ?? [];
            return _locations != null;
        }
        public void Set(string key, LatLonBoundingBoxDto? bbox, List<CachedLocationAggregate> locations) => _locations = locations;
        public LatLonBoundingBoxDto? CreateBoundingBox(IEnumerable<CachedLocationAggregate> locations) => Bbox;
    }
}
