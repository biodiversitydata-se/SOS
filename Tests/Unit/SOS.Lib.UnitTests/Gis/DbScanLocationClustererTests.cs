using FluentAssertions;
using SOS.Lib.Gis;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using Xunit;

namespace SOS.Lib.UnitTests.Gis;

public class DbScanLocationClustererTests : LocationClustererAlgorithmTestBase
{
    protected override ILocationClusterer CreateClusterer(int minLocationCountBeforeClustering, int maxLocationCountAfterClustering)
    {
        return new DbScanLocationClusterer(minLocationCountBeforeClustering, maxLocationCountAfterClustering);
    }

    protected override ILocationClusterer CreateClusterer(int minLocationCountBeforeClustering, int maxLocationCountAfterClustering, int minLocationsPerCluster)
    {
        return new DbScanLocationClusterer(minLocationCountBeforeClustering, maxLocationCountAfterClustering, minLocationsPerCluster);
    }

    [Fact]
    public void Sweref99TmCoordinatesArePreferredOverWgs84Coordinates()
    {
        // Arrange
        var observations = new List<Observation>
        {
            CreateObservation("sweref-location-0", "sweref-observation-0", 18.0, 59.0, 10, 500000, 6500000),
            CreateObservation("sweref-location-1", "sweref-observation-1", 18.0, 59.0, 20, 501000, 6500000)
        };
        var boundingBox = LatLonBoundingBox.Create(17.99, 59.01, 18.01, 58.99);

        // Act
        var result = new DbScanLocationClusterer(1, 10, 2)
            .ClusterLocations(observations, boundingBox, metersPerPixel: 1, clusterRadiusPx: 100);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(item => item.IsCluster == false);
    }

    [Fact]
    public void BoundedDbScanDoesNotCreateOneLargeClusterFromAChainedLine()
    {
        // Arrange
        var observations = Enumerable.Range(0, 5)
            .Select(index => CreateObservation(
                $"chain-location-{index}",
                $"chain-observation-{index}",
                18.0 + index * 0.0001,
                59.0,
                25 + index,
                index * 90,
                6500000))
            .ToList();
        var boundingBox = LatLonBoundingBox.Create(17.99, 59.01, 18.01, 58.99);

        // Act
        var result = new DbScanLocationClusterer(1, 10, 2)
            .ClusterLocations(observations, boundingBox, metersPerPixel: 1, clusterRadiusPx: 100);

        // Assert
        result.Should().NotContain(item => item.IsCluster && item.LocationCount == 5);
        result.Where(item => item.IsCluster).Should().OnlyContain(item => item.LocationCount <= 3);
        result.SelectMany(item => item.Observations).Should().HaveCount(5);
    }

    [Fact]
    public void MaxLocationCountAfterClusteringCanIncreaseTheRadiusThroughIterations()
    {
        // Arrange
        var observations = Enumerable.Range(0, 10)
            .Select(index => CreateObservation(
                $"iteration-location-{index}",
                $"iteration-observation-{index}",
                18.0 + index * 0.0001,
                59.0,
                25 + index,
                index * 10,
                6500000))
            .ToList();
        var boundingBox = LatLonBoundingBox.Create(17.99, 59.01, 18.01, 58.99);

        // Act
        var result = new DbScanLocationClusterer(1, 2, 2)
            .ClusterLocations(observations, boundingBox, metersPerPixel: 1, clusterRadiusPx: 1);

        // Assert
        result.Count.Should().BeLessOrEqualTo(2);
        result.SelectMany(item => item.Observations).Should().HaveCount(10);
    }

    [Fact]
    public void TaxaCountIsAggregatedCorrectlyInCluster()
    {
        // Arrange - Make16ObservationsCloseToEachOtherAndOneRemote uses 3 distinct taxa
        var observations = LocationClustererTestData.Make16ObservationsCloseToEachOtherAndOneRemote();
        var boundingBox = LatLonBoundingBox.Create(5.0, 75.0, 35.0, 50.0);

        // Act
        var result = new DbScanLocationClusterer(5, 10)
            .ClusterLocations(observations, boundingBox, metersPerPixel: 25.0);

        // Assert
        var cluster = result.Single(item => item.IsCluster);
        cluster.TaxaCount.Should().Be(3);
    }

    [Fact]
    public void ClusterRadiusPxControlsWhetherNearbyLocationsCluster()
    {
        // Arrange
        var observations = new List<Observation>
        {
            CreateObservation("cr-location-0", "cr-observation-0", 18.000, 59.0, 10, 500000, 6500000),
            CreateObservation("cr-location-1", "cr-observation-1", 18.001, 59.0, 10, 500100, 6500000),
            CreateObservation("cr-location-2", "cr-observation-2", 18.002, 59.0, 10, 500200, 6500000),
            CreateObservation("cr-location-3", "cr-observation-3", 18.003, 59.0, 10, 500300, 6500000)
        };
        var boundingBox = LatLonBoundingBox.Create(17.99, 59.01, 18.01, 58.99);

        // Act
        var aggressivelyClustered = new DbScanLocationClusterer(1, 10)
            .ClusterLocations(observations, boundingBox, metersPerPixel: 1, clusterRadiusPx: 1000);
        var lightlyClustered = new DbScanLocationClusterer(1, 10)
            .ClusterLocations(observations, boundingBox, metersPerPixel: 1, clusterRadiusPx: 1);

        // Assert
        aggressivelyClustered.Should().ContainSingle(item => item.IsCluster);
        aggressivelyClustered.Single(item => item.IsCluster).LocationCount.Should().Be(4);
        lightlyClustered.Should().OnlyContain(item => item.IsCluster == false);
        lightlyClustered.Should().HaveCount(4);
    }

    [Fact]
    public void AllObservationsAreAccountedForAfterClustering()
    {
        // Arrange
        var observations = LocationClustererTestData.MakeRandomObservations(300);
        var boundingBox = LatLonBoundingBox.Create(5.0, 75.0, 35.0, 50.0);

        // Act
        var result = new DbScanLocationClusterer(50, 100)
            .ClusterLocations(observations, boundingBox, metersPerPixel: 25.0);

        // Assert
        var allAccountedFor = result.SelectMany(item => item.Observations).ToList();
        var actualIds = allAccountedFor.Select(o => o.Occurrence.OccurrenceId).OrderBy(id => id).ToList();
        var expectedIds = observations.Select(o => o.Occurrence.OccurrenceId).OrderBy(id => id).ToList();
        allAccountedFor.Should().HaveCount(observations.Count);
        actualIds.Should().Equal(expectedIds);
    }    

    [Fact]
    public void SingleRemoteObservationRemainsNonClustered()
    {
        // Arrange
        var observations = LocationClustererTestData.Make16ObservationsCloseToEachOtherAndOneRemote();
        var boundingBox = LatLonBoundingBox.Create(5.0, 75.0, 35.0, 50.0);

        // Act
        var result = new DbScanLocationClusterer(5, 10)
            .ClusterLocations(observations, boundingBox, metersPerPixel: 25.0);

        // Assert
        var singleItem = result.Single(item => item.IsCluster == false);
        singleItem.Location.Should().NotBeNull();
        singleItem.Location!.DecimalLongitude.Should().Be(30.0);
        singleItem.Location.DecimalLatitude.Should().Be(70.0);
        singleItem.ObservationCount.Should().Be(1);
    }

    [Fact]
    public void MultipleObservationsAtSameLocationAreNotReturnedAsCluster()
    {
        // Arrange
        var observations = Enumerable.Range(0, 10)
            .Select(index => CreateObservation(
                "same-location",
                $"same-observation-{index}",
                18.0, 59.0, 10, 500000, 6500000))
            .ToList();
        var boundingBox = LatLonBoundingBox.Create(17.99, 59.01, 18.01, 58.99);

        // Act
        var result = new DbScanLocationClusterer(1, 10)
            .ClusterLocations(observations, boundingBox, metersPerPixel: 10);

        // Assert
        result.Should().ContainSingle();
        result.Single().IsCluster.Should().BeFalse();
        result.Single().LocationCount.Should().Be(1);
        result.Single().ObservationCount.Should().Be(10);
    }

    private static Observation CreateObservation(
        string locationId,
        string observationId,
        double longitude,
        double latitude,
        int coordinateUncertaintyInMeters,
        double sweref99TmX,
        double sweref99TmY)
    {
        return LocationClustererTestData.CreateObservation(
            new Location
            {
                LocationId = locationId,
                DecimalLongitude = longitude,
                DecimalLatitude = latitude,
                CoordinateUncertaintyInMeters = coordinateUncertaintyInMeters,
                Sweref99TmX = sweref99TmX,
                Sweref99TmY = sweref99TmY
            },
            observationId);
    }
}
