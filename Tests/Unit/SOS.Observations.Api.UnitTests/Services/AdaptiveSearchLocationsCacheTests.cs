using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Common.Geo;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NetTopologySuite.IO;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Search.Input;
using SOS.Lib.Gis;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Configuration;
using SOS.Observations.Api.Controllers;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Managers;
using SOS.Observations.Api.Services.AdaptiveSearch;
using SOS.Observations.Api.Services.AdaptiveSearchLocations;
using System.Text.Json.Nodes;
using Xunit;
using ZiggyCreatures.Caching.Fusion;

namespace SOS.Observations.Api.UnitTests.Services;

public class AdaptiveSearchLocationsCacheTests
{
    [Fact]
    public void DifferentPolygonsWithSameBoundingBoxCannotReuseCachedLocations()
    {
        using var fixture = new CacheFixture();
        var input = new GeographicsFilterInputModel
        {
            BoundingBox = new LatLonBoundingBoxDto
            {
                TopLeft = new LatLonCoordinateDto { Longitude = 0, Latitude = 1 },
                BottomRight = new LatLonCoordinateDto { Longitude = 1, Latitude = 0 }
            },
            ServerGeometries = [new WKTReader().Read("POLYGON ((0 0, 1 0, 1 1, 0 1, 0 0))")]
        };
        var filter = new SearchFilterInternalDto { Geographics = input, ProtectionFilter = ProtectionFilterDto.Public };
        var firstKey = fixture.Cache.CreateCacheKey(null, null, false, filter);
        fixture.Cache.Set(firstKey, Box(0), new() { Aggregate("inside", 0.5, 2) });
        fixture.Cache.TryGet(firstKey, Box(0), out _).Should().BeTrue();

        input.ServerGeometries = [new WKTReader().Read(
            "POLYGON ((0 0, 1 0, 1 1, 0 1, 0 0), (0.2 0.2, 0.2 0.8, 0.8 0.8, 0.8 0.2, 0.2 0.2))")];
        var secondKey = fixture.Cache.CreateCacheKey(null, null, false, filter);

        secondKey.Should().NotBe(firstKey);
        fixture.Cache.TryGet(secondKey, Box(0), out _).Should().BeFalse();
        input.Geometries.Should().BeNull();
    }

    [Fact]
    public void CacheKeyIgnoresBoundingBoxWithoutDroppingOrModifyingServerGeometry()
    {
        using var fixture = new CacheFixture();
        var geometry = new WKTReader().Read("POINT (0.5 0.5)");
        var input = new GeographicsFilterInputModel { ServerGeometries = [geometry] };
        var filter = new SearchFilterInternalDto { Geographics = input };
        var key = fixture.Cache.CreateCacheKey(null, null, false, filter);
        input.BoundingBox = new LatLonBoundingBoxDto
        {
            TopLeft = new LatLonCoordinateDto { Longitude = 0, Latitude = 1 },
            BottomRight = new LatLonCoordinateDto { Longitude = 1, Latitude = 0 }
        };

        fixture.Cache.CreateCacheKey(null, null, false, filter).Should().Be(key);
        input.ServerGeometries.Should().ContainSingle().Which.Should().BeSameAs(geometry);
        input.BoundingBox.Should().NotBeNull();
        input.Geometries.Should().BeNull();
        key.Should().StartWith("adaptive-search-locations:v5:");
    }

    [Theory]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(false, false, true)]
    public async Task Search_RequestsExtentOnlyWhenNeeded(bool returnInputBBox, bool hasInputBBox, bool includeExtent)
    {
        var manager = new Mock<IObservationManager>(MockBehavior.Strict);
        var inputBbox = hasInputBBox ? Box(0) : null;
        var extent = includeExtent ? Box(1) : null;
        manager.Setup(m => m.PrepareObservationFilterAsync(null, null, It.IsAny<SearchFilter>()))
            .ReturnsAsync((int? roleId, string application, SearchFilter filter) => new PreparedObservationFilter(filter));
        manager.Setup(m => m.GetCountAndExtentAsync(It.IsAny<PreparedObservationFilter>(), includeExtent))
            .ReturnsAsync((0L, extent, 0L));

        var result = await CreateService(manager.Object, new CacheSpy()).SearchAsync(new AdaptiveSearchLocationsRequest
        {
            Filter = new SearchFilterInternalDto { ProtectionFilter = ProtectionFilterDto.Public },
            ReturnInputBBox = returnInputBBox,
            InputBbox = inputBbox,
            Bbox = inputBbox,
            UseCache = false
        });

        result.Bbox.Should().BeSameAs(includeExtent ? extent : inputBbox);
        manager.VerifyAll();
    }

    [Theory]
    [InlineData(ProtectionFilterDto.Sensitive, false, false)]
    [InlineData(ProtectionFilterDto.BothPublicAndSensitive, false, false)]
    [InlineData(ProtectionFilterDto.Public, true, false)]
    [InlineData(ProtectionFilterDto.Public, false, true)]
    [InlineData(null, false, false)]
    public async Task Search_BypassesCacheForProtectedOrUserDependentFilters(
        ProtectionFilterDto? protection, bool observedByMe, bool reportedByMe)
    {
        var cache = new CacheSpy();
        var manager = new Mock<IObservationManager>(MockBehavior.Strict);
        manager.Setup(m => m.PrepareObservationFilterAsync(null, null, It.IsAny<SearchFilter>()))
            .ReturnsAsync((int? roleId, string application, SearchFilter filter) => new PreparedObservationFilter(filter));
        manager.Setup(m => m.GetCountAndExtentAsync(It.IsAny<PreparedObservationFilter>(), true))
            .ReturnsAsync((1L, (LatLonBoundingBoxDto?)null, 1L));
        manager.Setup(m => m.GetChunkAsync(It.IsAny<PreparedObservationFilter>(), 0, 100,
                It.Is<SearchExecutionOptions>(o => o.KnownTotalCount == 1L)))
            .ReturnsAsync(new PagedResult<JsonObject>
            {
                TotalCount = 1,
                Records = new[] { JsonNode.Parse("""
                    {"location":{"locationId":"1","type":0,"decimalLatitude":58,"decimalLongitude":13},
                     "occurrence":{"isPositiveObservation":true}}
                    """)!.AsObject() }
            });
        var service = CreateService(manager.Object, cache);

        var result = await service.SearchAsync(new AdaptiveSearchLocationsRequest
        {
            UserId = 123,
            Filter = new SearchFilterInternalDto
            {
                ProtectionFilter = protection, ObservedByMe = observedByMe, ReportedByMe = reportedByMe
            },
            UseCache = true,
            ObservationsLimit = 100
        });

        result.CacheStatus.Should().Be("BYPASS");
        result.ObservationsTotalCount.Should().Be(1);
        cache.Calls.Should().Be(0, "neither reads nor writes should use the shared cache");
        manager.VerifyAll();
    }

    [Fact]
    public async Task Search_PublicUserIndependentFilterCanStillHitCache()
    {
        var cache = new CacheSpy();
        var manager = new Mock<IObservationManager>(MockBehavior.Strict);
        var result = await CreateService(manager.Object, cache).SearchAsync(new AdaptiveSearchLocationsRequest
        {
            UserId = 456,
            Filter = new SearchFilterInternalDto { ProtectionFilter = ProtectionFilterDto.Public },
            UseCache = true,
            ObservationsLimit = 100
        });

        result.CacheStatus.Should().Be("HIT");
        cache.Calls.Should().Be(2);
        manager.VerifyNoOtherCalls();
    }

    [Fact]
    public void NewWriteDoesNotExtendOldEntryLifetimeOrUseExpiredCoverage()
    {
        using var fixture = new CacheFixture();
        var first = Box(0);
        var second = Box(1);
        fixture.Cache.Set("key", first, new());
        fixture.Clock.Advance(TimeSpan.FromMinutes(9));
        fixture.Cache.Set("key", second, new());
        fixture.Cache.TryGet("key", first, out _).Should().BeTrue();

        fixture.Clock.Advance(TimeSpan.FromMinutes(1));
        fixture.Cache.TryGet("key", first, out _).Should().BeFalse();
        fixture.Cache.TryGet("key", LatLonBoundingBoxDto.Create(0, 1, 2, 0), out _).Should().BeFalse();
        fixture.Cache.TryGet("key", second, out _).Should().BeTrue();

        fixture.Cache.Set("key", Box(2), new());
        fixture.RawCache.TryGet<AdaptiveSearchLocationsCacheBucket>("key", token: TestContext.Current.CancellationToken).Value.Entries.Should().HaveCount(2);
    }

    [Fact]
    public void WritesPublishNewSnapshotsAndDoNotKeepCallerOwnedListsOrBounds()
    {
        using var fixture = new CacheFixture();
        var bbox = Box(0);
        var locations = new List<CachedLocationAggregate>();
        fixture.Cache.Set("key", bbox, locations);
        var previous = fixture.RawCache.TryGet<AdaptiveSearchLocationsCacheBucket>("key", token: TestContext.Current.CancellationToken).Value;
        fixture.Cache.Set("key", Box(1), new());

        previous.Entries.Should().HaveCount(1);
        previous.Entries[0].Locations.Should().NotBeSameAs(locations);
        previous.Entries[0].BoundingBox.Should().NotBeSameAs(bbox);
        fixture.RawCache.TryGet<AdaptiveSearchLocationsCacheBucket>("key", token: TestContext.Current.CancellationToken).Value.Entries.Should().HaveCount(2);
    }

    [Fact]
    public async Task ConcurrentScopedWritersPreserveAllEntriesAndAllowReads()
    {
        using var fixture = new CacheFixture();
        using var start = new ManualResetEventSlim(false);
        var writers = Enumerable.Range(0, 8).Select(index => Task.Run(() =>
        {
            var cache = fixture.CreateCache();
            start.Wait();
            for (var iteration = 0; iteration < 30; iteration++)
            {
                cache.Set("key", Box(index), new());
                cache.TryGet("key", Box(index), out _).Should().BeTrue();
            }
        })).ToArray();
        start.Set();
        await Task.WhenAll(writers);

        for (var index = 0; index < 8; index++)
            fixture.Cache.TryGet("key", Box(index), out _).Should().BeTrue();
        fixture.RawCache.TryGet<AdaptiveSearchLocationsCacheBucket>("key", token: TestContext.Current.CancellationToken).Value.Entries.Should().HaveCount(8);
    }

    private static LatLonBoundingBoxDto Box(int index) => LatLonBoundingBoxDto.Create(index, 1, index + 1, 0);

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task Search_SelectsExactBboxReuseForAccuracyAndDisturbanceFilters(bool accuracy, bool disturbance)
    {
        var cache = new CacheSpy();
        var manager = new Mock<IObservationManager>(MockBehavior.Strict);
        await CreateService(manager.Object, cache).SearchAsync(new AdaptiveSearchLocationsRequest
        {
            Filter = new SearchFilterInternalDto
            {
                ProtectionFilter = ProtectionFilterDto.Public,
                Geographics = new GeographicsFilterDto
                {
                    ConsiderObservationAccuracy = accuracy, ConsiderDisturbanceRadius = disturbance
                }
            },
            UseCache = true, ObservationsLimit = 100
        });
        cache.RequiredExactBoundingBox.Should().Be(accuracy || disturbance);
        manager.VerifyNoOtherCalls();
    }

    [Fact]
    public void ExactBboxHitKeepsObservationWhoseBufferCenterIsOutside()
    {
        using var fixture = new CacheFixture();
        var outside = Aggregate("outside", -0.1, 3);
        fixture.Cache.Set("key", Box(0), new() { outside });

        fixture.Cache.TryGet("key", Box(0), out var exact, requireExactBoundingBox: true).Should().BeTrue();
        exact.Should().ContainSingle().Which.ObservationSummary.ObservationCount.Should().Be(3);
        fixture.Cache.TryGet("key", LatLonBoundingBoxDto.Create(0.1, 0.9, 0.9, 0.1), out _,
            requireExactBoundingBox: true).Should().BeFalse();
        fixture.Cache.TryGet("key", LatLonBoundingBoxDto.Create(0, 1, 2, 0), out _,
            requireExactBoundingBox: true).Should().BeFalse();
    }

    [Fact]
    public void OrdinaryPointSearchCanReuseOneContainingEntryAndFilterLocations()
    {
        using var fixture = new CacheFixture();
        fixture.Cache.Set("key", LatLonBoundingBoxDto.Create(0, 1, 2, 0),
            new() { Aggregate("inside", 0.5, 2), Aggregate("outside", 1.5, 4) });

        fixture.Cache.TryGet("key", Box(0), out var locations).Should().BeTrue();
        locations.Should().ContainSingle().Which.Location.LocationId.Should().Be("inside");
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void AdjacentOrOverlappingEntriesAreNotMerged(double secondLeft)
    {
        using var fixture = new CacheFixture();
        // Different globally unique LocationIds can share a rounded DistinctLocationId.
        var left = Aggregate("left-location", 0.999999, 2);
        var right = Aggregate("right-location", 1.000001, 5);
        left.DistinctLocationId.Should().Be(right.DistinctLocationId);
        fixture.Cache.Set("key", Box(0), new() { left });
        fixture.Cache.Set("key", LatLonBoundingBoxDto.Create(secondLeft, 1, 2, 0), new() { right });

        fixture.Cache.TryGet("key", LatLonBoundingBoxDto.Create(0, 1, 2, 0), out var locations).Should().BeFalse();
        locations.Should().BeEmpty();
        fixture.Cache.TryGet("key", Box(0), out var exact).Should().BeTrue();
        exact.Single().ObservationSummary.ObservationCount.Should().Be(2);
    }

    [Fact]
    public void SummaryCombiningMultipleLocationIdsIsOnlyReusedForExactBbox()
    {
        using var fixture = new CacheFixture();
        var summary = Aggregate("left", 0.499999, 7);
        summary.LocationIds.Add("right");
        fixture.Cache.Set("key", Box(0), new() { summary });

        fixture.Cache.TryGet("key", LatLonBoundingBoxDto.Create(0, 1, 0.5, 0), out _).Should().BeFalse();
        fixture.Cache.TryGet("key", Box(0), out var exact).Should().BeTrue();
        exact.Single().ObservationSummary.ObservationCount.Should().Be(7);
    }

    private static CachedLocationAggregate Aggregate(string id, double longitude, int count)
    {
        var location = new Location
        {
            LocationId = id, DecimalLongitude = longitude, DecimalLatitude = 0.5
        };
        return new CachedLocationAggregate
        {
            DistinctLocationId = location.DistinctLocationId, Location = location,
            LocationIds = new() { id }, ObservationSummary = new LocationObservationSummary { ObservationCount = count }
        };
    }

    private static AdaptiveSearchLocationsService CreateService(IObservationManager manager, IAdaptiveSearchLocationsCache cache)
    {
        var aggregator = new AdaptiveSearchLocationAggregator();
        return new AdaptiveSearchLocationsService(manager, Mock.Of<ILocationGeometryLookupService>(), cache,
            aggregator, new AdaptiveSearchLocationFeatureFactory(aggregator));
    }

    private sealed class CacheSpy : IAdaptiveSearchLocationsCache
    {
        public int Calls { get; private set; }
        public bool RequiredExactBoundingBox { get; private set; }
        public string CreateCacheKey(int? roleId, string? application, bool sensitive, SearchFilterInternalDto filter)
        { Calls++; return "shared"; }
        public bool TryGet(string key, LatLonBoundingBoxDto? bbox, out List<CachedLocationAggregate> locations, bool requireExactBoundingBox = false)
        { Calls++; RequiredExactBoundingBox = requireExactBoundingBox; locations = new(); return true; }
        public void Set(string key, LatLonBoundingBoxDto? bbox, List<CachedLocationAggregate> locations) => Calls++;
        public LatLonBoundingBoxDto? CreateBoundingBox(IEnumerable<CachedLocationAggregate> locations) => null;
    }

    private sealed class TestClock : TimeProvider
    {
        private DateTimeOffset _now = new(2026, 9, 6, 12, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => _now;
        public void Advance(TimeSpan duration) => _now += duration;
    }

    private sealed class CacheFixture : IDisposable
    {
        private readonly ServiceProvider _services;
        private readonly IFusionCacheProvider _provider;
        private readonly AdaptiveSearchLocationsCacheMonitor _monitor;
        public TestClock Clock { get; } = new();
        public AdaptiveSearchLocationsCache Cache { get; }
        public IFusionCache RawCache => _provider.GetCache(ObservationsController.AdaptiveSearchLocationsCacheName);

        public CacheFixture()
        {
            var services = new ServiceCollection();
            services.AddFusionCache(ObservationsController.AdaptiveSearchLocationsCacheName);
            _services = services.BuildServiceProvider();
            _provider = _services.GetRequiredService<IFusionCacheProvider>();
            _monitor = new AdaptiveSearchLocationsCacheMonitor(_provider,
                NullLogger<AdaptiveSearchLocationsCacheMonitor>.Instance, new ObservationApiConfiguration());
            Cache = CreateCache();
        }

        public AdaptiveSearchLocationsCache CreateCache() => new(_provider, _monitor, Clock);
        public void Dispose() => _services.Dispose();
    }
}
