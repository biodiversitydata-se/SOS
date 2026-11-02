using FluentAssertions;
using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Responses;
using SOS.Lib.Gis;
using Observation = SOS.Lib.Models.Processed.Observation.Observation;
using System.Text.Json;
using Xunit;
using static SOS.Lib.UnitTests.Gis.AdaptiveLocationGroupingTests;

namespace SOS.Lib.UnitTests.Gis;

public class ProximityLocationGroupingTests
{
    private static readonly LatLonBoundingBoxDto Box = LatLonBoundingBoxDto.Create(14, 60, 16, 58);

    [Theory]
    [InlineData(false, 3, true)]
    [InlineData(false, 4, true)]
    [InlineData(false, 5, false)]
    [InlineData(true, 3, true)]
    [InlineData(true, 4, true)]
    [InlineData(true, 5, false)]
    public void FixedPixelRadius_IncludesBoundaryAndWorksBelowActivationThreshold(bool adaptivePixels, double distance, bool clustered)
    {
        var result = new Sweref99TmFastGreedyLocationClusterer(100, 400)
            .ClusterLocations([At("a", 0), At("b", distance)], Box, 2, 40, null, adaptivePixels, 2);
        result.Should().HaveCount(clustered ? 1 : 2);
        if (clustered)
        {
            var feature = result.Single();
            feature.GroupingType.Should().Be(LocationGroupingType.PixelCluster);
            LocationFeatureResponseMapper.ToProperties(feature).Cluster!.RadiusMeters.Should().Be(4);
        }
    }

    [Theory]
    [InlineData(2, 1, 40)]
    [InlineData(3, 2, 2)]
    [InlineData(4, 2, 2)]
    public void ActivationThreshold_SelectsExactlyOnePixelStage(int threshold, int count, double radius)
    {
        var result = new Sweref99TmFastGreedyLocationClusterer(threshold, 400)
            .ClusterLocations([At("a", 0), At("b", 1), At("c", 20)], Box, 1, 40, null, true, 2);
        result.Should().HaveCount(count);
        result.Single(feature => feature.IsCluster).ClusterRadiusMeters.Should().Be(radius);
    }

    [Fact]
    public void ProximityStage_DoesNotRunAfterAdaptiveStage()
    {
        var result = new Sweref99TmFastGreedyLocationClusterer(0, 400)
            .ClusterLocations([At("a", 0), At("b", 4), At("c", 20)], Box, 1, 1, null, true, 100);
        result.Should().HaveCount(3).And.OnlyContain(feature => !feature.IsCluster);
    }

    [Theory]
    [InlineData(null, 2)]
    [InlineData(1d, 0)]
    public void MissingResolutionOrZeroRadius_DisablesProximityGrouping(double? resolution, double radius)
    {
        var result = new Sweref99TmFastGreedyLocationClusterer(100, 400)
            .ClusterLocations([At("a", 0), At("b", 1)], Box, resolution, 40, null, false, radius);
        result.Should().HaveCount(2).And.OnlyContain(feature => !feature.IsCluster);
    }

    [Theory]
    [InlineData(0.1, 2)]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    public void ZoomingIn_CanSeparateNearbyLocations(double resolution, int count)
    {
        var result = new Sweref99TmFastGreedyLocationClusterer(100, 400)
            .ClusterLocations([At("a", 0), At("b", 1)], Box, resolution, 40, null, false, 2);
        result.Should().HaveCount(count);
    }

    [Fact]
    public void ExplicitResolution_AllowsProximityGroupingWithoutBoundingBox()
    {
        var result = new Sweref99TmFastGreedyLocationClusterer(100, 400)
            .ClusterLocations([At("a", 0), At("b", 1)], null, 1, 40, null, false, 2);
        result.Should().ContainSingle().Which.ClusterRadiusMeters.Should().Be(2);
    }

    [Fact]
    public void MinimumLocationsPerCluster_StillApplies()
    {
        var result = new Sweref99TmFastGreedyLocationClusterer(100, 400, 3)
            .ClusterLocations([At("a", 0), At("b", 1)], Box, 1, 40, null, false, 2);
        result.Should().HaveCount(2).And.OnlyContain(feature => !feature.IsCluster);
    }

    [Theory]
    [InlineData(2, 3)]
    [InlineData(10, 2)]
    public void MeterGroups_ArePreservedOrCombinedWhole(double proximityRadius, int count)
    {
        var observations = new List<Observation> { At("a", 0), At("b", 1), At("c", 5), At("d", 6), At("far", 100) };
        var result = new Sweref99TmFastGreedyLocationClusterer(100, 400)
            .ClusterLocations(observations, Box, 1, 40, 1, false, proximityRadius);
        result.Should().HaveCount(count);
        result.Sum(feature => feature.ObservationCount).Should().Be(5);
        result.Where(feature => feature.IsCluster).Should().OnlyContain(feature =>
            feature.GroupingType == (proximityRadius == 2 ? LocationGroupingType.MeterCluster : LocationGroupingType.PixelCluster));
        result.Where(feature => feature.IsCluster).SelectMany(feature => feature.Observations)
            .Should().BeEquivalentTo(observations.Take(4));
    }

    [Fact]
    public void ActivationThreshold_CountsMeterGroupsBeforeSelectingProximityStage()
    {
        var result = new Sweref99TmFastGreedyLocationClusterer(2, 400)
            .ClusterLocations([At("a", 0), At("b", 1), At("c", 5), At("d", 6)], Box, 1, 100, 1, true, 10);
        var cluster = result.Should().ContainSingle().Subject;
        cluster.GroupingType.Should().Be(LocationGroupingType.PixelCluster);
        cluster.ClusterRadiusMeters.Should().Be(10);
    }

    [Fact]
    public void Grouping_IsDeterministicAndPreservesCoincidentLocations()
    {
        var observations = new List<Observation> { At("a", 0), At("b", 0), At("c", 1), At("far", 10) };
        var clusterer = new Sweref99TmFastGreedyLocationClusterer(100, 400);
        string Grouping(List<Observation> items) => JsonSerializer.Serialize(clusterer
            .ClusterLocations(items, Box, 1, 40, null, false, 2)
            .Select(feature => LocationFeatureResponseMapper.ToProperties(feature)));
        Grouping(observations.AsEnumerable().Reverse().ToList()).Should().Be(Grouping(observations));
        var cluster = clusterer.ClusterLocations(observations, Box, 1, 40, null, false, 2).Single(feature => feature.IsCluster);
        var properties = LocationFeatureResponseMapper.ToProperties(cluster);
        properties.ObservationCount.Should().Be(3);
        properties.Locations.Ids.Should().BeEquivalentTo("a", "b", "c");
    }

    [Fact]
    public void ResultAboveTargetItemCount_KeepsFixedRadius()
    {
        // 500 separate pairs exceed the adaptive target of 400 map objects.
        var observations = Enumerable.Range(0, 1000)
            .Select(index => At($"site-{index:D5}", index / 2 * 20 + index % 2)).ToList();
        var clusterer = new Sweref99TmFastGreedyLocationClusterer(100, 400);
        var result = clusterer.ClusterLocations(observations, Box, 1, 40, null, false, 2);

        result.Should().HaveCount(500).And.OnlyContain(feature => feature.IsCluster && feature.ClusterRadiusMeters == 2);
        result.Sum(feature => feature.ObservationCount).Should().Be(1000);
    }
}
