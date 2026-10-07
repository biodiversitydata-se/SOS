using SOS.Api.Contracts.Common.Geo;
using FluentAssertions;
using SOS.Lib.Gis;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using Xunit;

namespace SOS.Lib.UnitTests.Gis;

public class Sweref99TmFastGreedyLocationClustererTests : LocationClustererAlgorithmTestBase
{
    protected override ILocationClusterer CreateClusterer(int minLocationCountBeforeClustering, int maxLocationCountAfterClustering)
    {
        return new Sweref99TmFastGreedyLocationClusterer(minLocationCountBeforeClustering, maxLocationCountAfterClustering);
    }

    protected override ILocationClusterer CreateClusterer(int minLocationCountBeforeClustering, int maxLocationCountAfterClustering, int minLocationsPerCluster)
    {
        return new Sweref99TmFastGreedyLocationClusterer(minLocationCountBeforeClustering, maxLocationCountAfterClustering, minLocationsPerCluster);
    }

    [Fact]
    public void ClustersWithCloseDisplayedCentroidsAreMerged()
    {
        // Arrange
        var observations = CreateCloseCentroidMergeObservations();
        var boundingBox = LatLonBoundingBoxDto.Create(17.99, 59.01, 18.01, 58.99);

        // Act
        var result = new Sweref99TmFastGreedyLocationClusterer(1, 10)
            .ClusterLocations(observations, boundingBox, metersPerPixel: 10, clusterRadiusPx: 10);

        // Assert
        result.Should().ContainSingle();
        result.Single().IsCluster.Should().BeTrue();
        result.Single().LocationCount.Should().Be(4);
    }

    private static List<Observation> CreateCloseCentroidMergeObservations()
    {
        return new List<Observation>
        {
            CreateObservation("centroid-location-0", "centroid-observation-0", 18.0000, 0),
            CreateObservation("centroid-location-1", "centroid-observation-1", 18.0001, 90),
            CreateObservation("centroid-location-2", "centroid-observation-2", 18.0002, 101),
            CreateObservation("centroid-location-3", "centroid-observation-3", 18.0003, 102)
        };
    }

    [Fact]
    public void Sweref99TmCoordinatesAreUsedForClustering()
    {
        // Arrange - two locations close in WGS84 but far apart in Sweref99Tm X should NOT cluster
        var observations = new List<Observation>
        {
            CreateObservation("sweref-location-0", "sweref-observation-0", 18.0, 500000),
            CreateObservation("sweref-location-1", "sweref-observation-1", 18.001, 510000)
        };
        var boundingBox = LatLonBoundingBoxDto.Create(17.99, 59.01, 18.01, 58.99);

        // Act
        var result = new Sweref99TmFastGreedyLocationClusterer(1, 10, 2)
            .ClusterLocations(observations, boundingBox, metersPerPixel: 1, clusterRadiusPx: 100);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(item => item.IsCluster == false);
    }

    [Fact]
    public void LargeClusterRadiusCausesMoreAggressiveClustering()
    {
        // Arrange
        var observations = new List<Observation>
        {
            CreateObservation("radius-location-0", "radius-observation-0", 18.000, 500000),
            CreateObservation("radius-location-1", "radius-observation-1", 18.001, 500100),
            CreateObservation("radius-location-2", "radius-observation-2", 18.002, 500200),
            CreateObservation("radius-location-3", "radius-observation-3", 18.003, 500300)
        };
        var boundingBox = LatLonBoundingBoxDto.Create(17.99, 59.01, 18.01, 58.99);

        // Act
        var aggressivelyClustered = new Sweref99TmFastGreedyLocationClusterer(1, 10)
            .ClusterLocations(observations, boundingBox, metersPerPixel: 1, clusterRadiusPx: 1000);
        var lightlyClustered = new Sweref99TmFastGreedyLocationClusterer(1, 10)
            .ClusterLocations(observations, boundingBox, metersPerPixel: 1, clusterRadiusPx: 1);

        // Assert
        aggressivelyClustered.Should().ContainSingle(item => item.IsCluster);
        aggressivelyClustered.Single(item => item.IsCluster).LocationCount.Should().Be(4);
        lightlyClustered.Should().OnlyContain(item => item.IsCluster == false);
        lightlyClustered.Should().HaveCount(4);
    }

    [Fact]
    public void TaxaCountIsAggregatedCorrectlyInCluster()
    {
        // Arrange - use Make16ObservationsCloseToEachOtherAndOneRemote which has 3 distinct taxa
        var observations = LocationClustererTestData.Make16ObservationsCloseToEachOtherAndOneRemote();
        var boundingBox = LatLonBoundingBoxDto.Create(5.0, 75.0, 35.0, 50.0);

        // Act
        var result = new Sweref99TmFastGreedyLocationClusterer(5, 10)
            .ClusterLocations(observations, boundingBox, metersPerPixel: 25.0);

        // Assert
        var cluster = result.Single(item => item.IsCluster);
        cluster.TaxaCount.Should().Be(3);
    }

    [Fact]
    public void MultipleObservationsAtSameSweref99TmLocationAreNotReturnedAsCluster()
    {
        // Arrange
        var observations = Enumerable.Range(0, 10)
            .Select(index => LocationClustererTestData.CreateObservation(
                new Location
                {
                    LocationId = "same-sweref-location",
                    DecimalLongitude = 18.0,
                    DecimalLatitude = 59.0,
                    Sweref99TmX = 500000,
                    Sweref99TmY = 6500000
                },
                $"same-sweref-observation-{index}"))
            .ToList();
        var boundingBox = LatLonBoundingBoxDto.Create(17.99, 59.01, 18.01, 58.99);

        // Act
        var result = new Sweref99TmFastGreedyLocationClusterer(1, 10)
            .ClusterLocations(observations, boundingBox, metersPerPixel: 10);

        // Assert
        result.Should().ContainSingle();
        result.Single().IsCluster.Should().BeFalse();
        result.Single().ObservationCount.Should().Be(10);
    }

    [Fact]
    public void AllObservationsAreAccountedForAfterClustering()
    {
        // Arrange
        var observations = LocationClustererTestData.MakeRandomObservations(300);
        var boundingBox = LatLonBoundingBoxDto.Create(5.0, 75.0, 35.0, 50.0);

        // Act
        var result = new Sweref99TmFastGreedyLocationClusterer(50, 100)
            .ClusterLocations(observations, boundingBox, metersPerPixel: 25.0);

        // Assert
        var allAccountedFor = result.SelectMany(item => item.Observations).ToList();
        var actualIds = allAccountedFor.Select(o => o.Occurrence.OccurrenceId).OrderBy(id => id).ToList();
        var expectedIds = observations.Select(o => o.Occurrence.OccurrenceId).OrderBy(id => id).ToList();
        allAccountedFor.Should().HaveCount(observations.Count);
        actualIds.Should().Equal(expectedIds);
    }

    private static Observation CreateObservation(string locationId, string observationId, double longitude, double sweref99TmX)
    {
        return LocationClustererTestData.CreateObservation(
            new Location
            {
                LocationId = locationId,
                DecimalLongitude = longitude,
                DecimalLatitude = 59.0,
                Sweref99TmX = sweref99TmX,
                Sweref99TmY = 6500000
            },
            observationId);
    }
}
