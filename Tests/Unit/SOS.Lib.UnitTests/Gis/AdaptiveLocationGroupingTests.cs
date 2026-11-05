using FluentAssertions;
using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Responses;
using SOS.Lib.Extensions;
using SOS.Lib.Gis;
using SOS.Lib.Models.Gis;
using System.Text.Json;
using Xunit;
using Observation = SOS.Lib.Models.Processed.Observation.Observation;
using Location = SOS.Lib.Models.Processed.Observation.Location;
using Point = NetTopologySuite.Geometries.Point;

namespace SOS.Lib.UnitTests.Gis;

public class AdaptiveLocationGroupingTests
{
    private static readonly LatLonBoundingBoxDto Box = LatLonBoundingBoxDto.Create(14, 60, 16, 58);

    [Fact]
    public void GeographicIdentityIgnoresAccuracyButHarvestIdentityPreservesIt()
    {
        var a = At("a", 0).Location;
        var b = At("b", 0).Location;
        a.CoordinateUncertaintyInMeters = 10;
        b.CoordinateUncertaintyInMeters = 200;
        a.DistinctLocationId.Should().Be(b.DistinctLocationId);
        a.DistinctLocationId.Should().NotEndWith("_a10");
        a.CreateFallbackLocationId().Should().Be(a.DistinctLocationId + "_a10");
        b.CreateFallbackLocationId().Should().Be(b.DistinctLocationId + "_a200");
    }

    public static IEnumerable<object[]> DetailCounts() => new[]
    {
        1, 5, 6, 100,
        AdaptiveSearchFeatureLocationsDto.MaxLocationItems,
        AdaptiveSearchFeatureLocationsDto.MaxLocationItems + 1
    }.Distinct().Select(count => new object[] { count });

    [Theory]
    [MemberData(nameof(DetailCounts))]
    public void ItemsAreLimitedButCountsAndIdsIncludeEveryOriginalLocation(int count)
    {
        var observations = Enumerable.Range(0, count).Select(i => At($"site-{i:D3}", 0, i + 1)).Reverse().ToList();
        var feature = new Sweref99TmFastGreedyLocationClusterer().ClusterLocations(observations, null, null, 40, null, false).Single();
        var dto = LocationFeatureResponseMapper.ToProperties(feature);
        dto.GroupingType.Should().Be(count == 1 ? LocationGroupingType.SingleLocation : LocationGroupingType.CoLocatedLocations);
        dto.Locations.Count.Should().Be(count);
        dto.HasMultipleLocations.Should().Be(count > 1);
        dto.ObservationCount.Should().Be(count);
        dto.Locations.Ids.Should().HaveCount(count).And.BeInAscendingOrder();
        dto.Locations.Items.Should().HaveCount(Math.Min(AdaptiveSearchFeatureLocationsDto.MaxLocationItems, count));
        dto.Locations.Items.Select(l => l.LocationId).Should().Equal(dto.Locations.Ids.Take(AdaptiveSearchFeatureLocationsDto.MaxLocationItems));
        dto.Locations.Items[0].Locality.Should().Be("Name site-000");
        dto.Locations.Items[0].CoordinateUncertaintyInMeters.Should().Be(1);
        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web)
        { Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } });
        json.Should().Contain(count == 1 ? "singleLocation" : "coLocatedLocations").And.NotContain("isCluster");
        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("hasMultipleLocations").GetBoolean().Should().Be(count > 1);
    }

    [Theory]
    [InlineData(99, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void FixedMeterRadiusIncludesBoundaryAndIgnoresActivationThreshold(double distance, bool clustered)
    {
        var results = new Sweref99TmFastGreedyLocationClusterer(1000, 1).ClusterLocations(
            new() { At("a", 0), At("b", distance), At("far", 100000) }, null, null, 40, 100, false);
        results.Any(f => f.IsCluster).Should().Be(clustered);
        if (clustered) results.Where(f => f.IsCluster).Should().OnlyContain(f => f.GroupingType == LocationGroupingType.MeterCluster && f.ClusterRadiusMeters == 100);
        results.Sum(f => f.ObservationCount).Should().Be(3);
    }

    [Fact]
    public void PixelStagePreservesOrCombinesWholeMeterGroups()
    {
        var observations = new List<Observation> { At("a", 0), At("b", 50), At("c", 200), At("d", 250) };
        var clusterer = new Sweref99TmFastGreedyLocationClusterer(0, 400);
        var meters = clusterer.ClusterLocations(observations, Box, 1, 1, 100, true);
        meters.Should().HaveCount(2).And.OnlyContain(f => f.GroupingType == LocationGroupingType.MeterCluster);
        var pixels = clusterer.ClusterLocations(observations, Box, 1, 300, 100, true).Single();
        pixels.GroupingType.Should().Be(LocationGroupingType.PixelCluster);
        LocationFeatureResponseMapper.ToProperties(pixels).HasMultipleLocations.Should().BeTrue();
        pixels.ObservationCount.Should().Be(4);
        pixels.Location!.Sweref99TmX.Should().BeApproximately(500125, 0.000001);
        LocationFeatureResponseMapper.ToProperties(pixels).Cluster!.RadiusMetersMin.Should().BeApproximately(125, 0.001);
        var reversed = clusterer.ClusterLocations(observations.AsEnumerable().Reverse().ToList(), Box, 1, 300, 100, true).Single();
        LocationFeatureResponseMapper.ToProperties(reversed).Locations.Ids.Should().Equal(LocationFeatureResponseMapper.ToProperties(pixels).Locations.Ids);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RemainingPointWithinFinalCentroidRadiusIsIncluded(bool clusterPixels)
    {
        var positions = new[] { 0d }.Concat(Enumerable.Range(31, 11).Select(x => (double)x)).Append(500);
        var observations = positions.Select((x, i) => At($"site-{i:D2}", x)).ToList();
        var clusterer = new Sweref99TmFastGreedyLocationClusterer(0, 400);

        var result = clusterer.ClusterLocations(observations, Box, 1, 40, clusterPixels ? null : 40, clusterPixels);

        result.Should().HaveCount(2);
        var cluster = result.Single(f => f.IsCluster);
        cluster.GroupingType.Should().Be(clusterPixels ? LocationGroupingType.PixelCluster : LocationGroupingType.MeterCluster);
        cluster.ObservationCount.Should().Be(12);
        cluster.Location!.Sweref99TmX.Should().BeApproximately(500033, 0.000001);
        cluster.ClusterRadiusMeters.Should().Be(40);
        result.Single(f => !f.IsCluster).Location!.LocationId.Should().Be("site-12");
        result.SelectMany(f => f.Observations).Should().BeEquivalentTo(observations);
        var reversed = clusterer.ClusterLocations(observations.AsEnumerable().Reverse().ToList(), Box, 1, 40, clusterPixels ? null : 40, clusterPixels);
        JsonSerializer.Serialize(reversed.Select(f => LocationFeatureResponseMapper.ToProperties(f)))
            .Should().Be(JsonSerializer.Serialize(result.Select(f => LocationFeatureResponseMapper.ToProperties(f))));
    }

    [Fact]
    public void RemainingMeterGroupIsAbsorbedWholeWithItsGeographicWeight()
    {
        var observations = new double[] { 0, 30, 34, 38, 43, 44 }.Select((x, i) => At($"site-{i}", x)).ToList();
        var clusterer = new Sweref99TmFastGreedyLocationClusterer(0, 400);
        var meters = clusterer.ClusterLocations(observations, null, null, 40, 2, false);
        meters.Single(f => f.IsCluster).LocationCount.Should().Be(2);

        var result = clusterer.ClusterLocations(observations, Box, 1, 40, 2, true).Single();

        result.GroupingType.Should().Be(LocationGroupingType.PixelCluster);
        result.LocationCount.Should().Be(6);
        result.Observations.Should().BeEquivalentTo(observations);
        result.Location!.Sweref99TmX.Should().BeApproximately(500031.5, 0.000001);
        LocationFeatureResponseMapper.ToProperties(result).Cluster!.RadiusMetersMin.Should().Be(31.5);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RemotePointDoesNotIncreaseExplicitPixelSearchRadius(bool useTwoStageOverload)
    {
        var observations = new List<Observation> { At("a", 0), At("b", 30), At("c", 100), At("remote", 100000) };
        var clusterer = new Sweref99TmFastGreedyLocationClusterer(0, 400);

        var result = useTwoStageOverload
            ? clusterer.ClusterLocations(observations, Box, 1, 40, null, true)
            : clusterer.ClusterLocations(observations, Box, 1, 40);

        result.Should().HaveCount(3);
        var cluster = result.Single(f => f.IsCluster);
        cluster.ClusterRadiusMeters.Should().Be(40);
        cluster.Observations.Select(o => o.Location.LocationId).Should().BeEquivalentTo(new[] { "a", "b" });
    }

    [Fact]
    public void ExplicitPixelRadiusCanBeSmallerThanOneMeter()
    {
        var observations = new List<Observation> { At("a", 0), At("b", 0.75) };

        var result = new Sweref99TmFastGreedyLocationClusterer(0, 400)
            .ClusterLocations(observations, Box, 0.01, 40, null, true);

        result.Should().HaveCount(2).And.OnlyContain(f => !f.IsCluster);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PixelRadiusRemainsFixedWhenResultExceedsTargetCount(bool useTwoStageOverload)
    {
        var observations = new List<Observation> { At("a", 0), At("b", 10), At("c", 1000), At("d", 1010) };
        var clusterer = new Sweref99TmFastGreedyLocationClusterer(0, 1);

        var result = useTwoStageOverload
            ? clusterer.ClusterLocations(observations, Box, 1, 40, null, true)
            : clusterer.ClusterLocations(observations, Box, 1, 40);

        result.Should().HaveCount(2).And.OnlyContain(f => f.IsCluster && f.ClusterRadiusMeters == 40);
        result.SelectMany(f => f.Observations).Should().BeEquivalentTo(observations);
    }

    [Theory]
    [InlineData(1, 500)]
    [InlineData(0.1, 1000)]
    public void PixelOverloadKeepsFixedRadiusAndCanSeparateMoreThan400Groups(double resolution, int expectedCount)
    {
        var observations = MakeSeparatedPairs();
        var result = new Sweref99TmFastGreedyLocationClusterer(0, 400)
            .ClusterLocations(observations, Box, resolution, 20);

        result.Should().HaveCount(expectedCount);
        result.Should().OnlyContain(f => !f.IsCluster || f.ClusterRadiusMeters == resolution * 20);
        result.Count(f => f.IsCluster).Should().Be(resolution == 1 ? 500 : 0);
        result.SelectMany(f => f.Observations).Select(o => o.Occurrence.OccurrenceId)
            .OrderBy(id => id, StringComparer.Ordinal).Should().Equal(observations.Select(o => o.Occurrence.OccurrenceId)
                .OrderBy(id => id, StringComparer.Ordinal));
    }

    [Theory]
    [InlineData(1, 500)]
    [InlineData(0.1, 1000)]
    public void ClusterGroupsKeepsFixedRadiusAndAllMembersAbove400Groups(double resolution, int expectedCount)
    {
        var locations = MakeSeparatedPairs().Select(o => o.Location).ToList();
        var result = new Sweref99TmFastGreedyLocationClusterer(0, 400).ClusterGroups(locations,
            new LocationClusteringOptions { BoundingBox = Box, MetersPerPixel = resolution, ClusterRadiusPx = 20, ClusterPixels = true });

        result.Should().HaveCount(expectedCount);
        result.Should().OnlyContain(g => !g.IsCluster || g.RadiusMeters == resolution * 20);
        result.Count(g => g.IsCluster).Should().Be(resolution == 1 ? 500 : 0);
        result.SelectMany(g => g.DistinctLocationIds).Should().BeEquivalentTo(locations.Select(l => l.DistinctLocationId));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WithoutPixelResolutionRadiusStillGrowsToReachTarget(bool useTwoStageOverload)
    {
        var observations = new List<Observation> { At("a", 0), At("b", 1000), At("c", 2000) };
        var clusterer = new Sweref99TmFastGreedyLocationClusterer(0, 1);
        var box = LatLonBoundingBoxDto.Create(15, 59.0001, 15.0001, 59);

        var result = useTwoStageOverload
            ? clusterer.ClusterLocations(observations, box, null, 40, null, true)
            : clusterer.ClusterLocations(observations, box);

        var cluster = result.Should().ContainSingle().Subject;
        cluster.ClusterRadiusMeters.Should().BeGreaterThan(40);
        cluster.Observations.Should().BeEquivalentTo(observations);
    }

    private static List<Observation> MakeSeparatedPairs() => Enumerable.Range(0, 1000)
        .Select(i => At($"pair-{i:D4}", i / 2 * 100 + i % 2 * 10)).ToList();

    [Fact]
    public void PixelThresholdCountsMeterUnitsAndCentroidWeightsUnderlyingGroups()
    {
        var observations = new List<Observation> { At("a", 0), At("b", 10), At("c", 20), At("d", 200) };
        var preserved = new Sweref99TmFastGreedyLocationClusterer(2, 400)
            .ClusterLocations(observations, Box, 1, 300, 25, true);
        preserved.Should().HaveCount(2);
        preserved.Single(f => f.IsCluster).GroupingType.Should().Be(LocationGroupingType.MeterCluster);

        var merged = new Sweref99TmFastGreedyLocationClusterer(0, 400, 3)
            .ClusterLocations(observations, Box, 1, 300, 25, true).Single();
        merged.GroupingType.Should().Be(LocationGroupingType.PixelCluster);
        merged.Location!.Sweref99TmX.Should().BeApproximately(500057.5, 0.000001);
        LocationFeatureResponseMapper.ToProperties(merged).Cluster!.RadiusMetersMin.Should().Be(142.5);
    }

    [Fact]
    public void GeographicGroupingIsDeterministicAcrossInputPermutations()
    {
        var observations = new double[] { 0, 80, 150, 230, 500, 550, 1000 }
            .Select((x, i) => At($"site-{i}", x)).ToList();
        var clusterer = new Sweref99TmFastGreedyLocationClusterer(0, 400);
        string Grouping(List<Observation> input) => JsonSerializer.Serialize(clusterer
            .ClusterLocations(input, null, null, 40, 100, false)
            .Select(f => LocationFeatureResponseMapper.ToProperties(f)));
        var expected = Grouping(observations);
        var random = new Random(42);
        for (var iteration = 0; iteration < 10; iteration++)
            Grouping(observations.OrderBy(_ => random.Next()).ToList()).Should().Be(expected);
    }

    [Fact]
    public void EnclosureCanExceedSearchRadiusAndUsesAllPositions()
    {
        var observations = new double[] { 0, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 110, 200 }.Select((x, i) => At(i.ToString(), x)).ToList();
        var cluster = new Sweref99TmFastGreedyLocationClusterer(0, 400)
            .ClusterLocations(observations, null, null, 40, 100, false).Single();
        var properties = LocationFeatureResponseMapper.ToProperties(cluster);
        properties.Cluster!.RadiusMeters.Should().Be(100);
        properties.Cluster.RadiusMetersMin.Should().BeGreaterThan(100);
    }

    public static Observation At(string id, double x, int accuracy = 10)
    {
        var point = (Point)new Point(500000 + x, 6500000).Transform(CoordinateSys.SWEREF99_TM, CoordinateSys.WGS84, usePointCache: false);
        return LocationClustererTestData.CreateObservation(new Location
        {
            LocationId = id, Locality = "Name " + id,
            Type = SOS.Lib.Enums.LocationType.Point,
            DecimalLongitude = point.X, DecimalLatitude = point.Y,
            Sweref99TmX = 500000 + x, Sweref99TmY = 6500000,
            CoordinateUncertaintyInMeters = accuracy
        }, "obs-" + id);
    }
}
