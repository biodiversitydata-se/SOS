using SOS.Api.Contracts.Common.Geo;
using FluentAssertions;
using SOS.Lib.Gis;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using Xunit;

namespace SOS.Lib.UnitTests.Gis;

public abstract class LocationClustererAlgorithmTestBase
{
    private static readonly LatLonBoundingBoxDto DefaultBoundingBox = LatLonBoundingBoxDto.Create(5.0, 75.0, 35.0, 50.0);
    private const double DefaultMetersPerPixel = 25.0;

    protected abstract ILocationClusterer CreateClusterer(int minLocationCountBeforeClustering, int maxLocationCountAfterClustering);

    protected virtual ILocationClusterer CreateClusterer(int minLocationCountBeforeClustering, int maxLocationCountAfterClustering, int minLocationsPerCluster)
    {
        return CreateClusterer(minLocationCountBeforeClustering, maxLocationCountAfterClustering);
    }

    [Fact]
    public void WhenWeClusterZeroObservationsWeGetNoClusterItems()
    {
        // Arrange
        var observations = new List<Observation>();

        // Act
        var result = CreateClusterer(50, 400).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ObservationsWithMissingCoordinatesAreIgnored()
    {
        // Arrange
        var observations = new List<Observation>
        {
            LocationClustererTestData.CreateObservation(new Location { DecimalLatitude = null, DecimalLongitude = null }),
            LocationClustererTestData.CreateObservation(new Location { DecimalLatitude = 59.0, DecimalLongitude = null }),
            LocationClustererTestData.CreateObservation(new Location { DecimalLatitude = null, DecimalLongitude = 18.0 })
        };

        // Act
        var result = CreateClusterer(50, 400).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void WhenLocationsAreTooFarApartNoClusteringOccurs()
    {
        // Arrange
        var observations = LocationClustererTestData.Make361ObservationsFarApart();

        // Act
        var result = CreateClusterer(100, 400).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

        // Assert
        result.Should().OnlyContain(item => item.IsCluster == false);
        result.Should().HaveCount(361);
    }

    [Fact]
    public void NumberOfMapElementsIsWithinBounds()
    {
        // Arrange
        var observations = LocationClustererTestData.MakeRandomObservations(300);

        // Act
        var result = CreateClusterer(50, 100).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

        // Assert
        result.Count.Should().BeLessOrEqualTo(100);
    }

    [Fact]
    public void ClusteredAndNonClusteredObservationsMatchOriginalInput()
    {
        // Arrange
        var observations = LocationClustererTestData.MakeRandomObservations(300);

        // Act
        var result = CreateClusterer(50, 100).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

        // Assert
        var allAccountedFor = result
            .SelectMany(item => item.Observations)
            .ToList();
        var actualOccurrenceIds = allAccountedFor
            .Select(observation => observation.Occurrence.OccurrenceId)
            .OrderBy(occurrenceId => occurrenceId)
            .ToList();
        var expectedOccurrenceIds = observations
            .Select(observation => observation.Occurrence.OccurrenceId)
            .OrderBy(occurrenceId => occurrenceId)
            .ToList();

        allAccountedFor.Should().HaveCount(observations.Count);
        actualOccurrenceIds.Should().Equal(expectedOccurrenceIds);
    }

    [Fact]
    public void ClusteringWorksWhenASingleLocationIsNotClustered()
    {
        // Arrange
        var observations = LocationClustererTestData.Make16ObservationsCloseToEachOtherAndOneRemote();

        // Act
        var result = CreateClusterer(5, 10).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

        // Assert
        var singleItem = result.Single(item => item.IsCluster == false);

        result.Should().ContainSingle(item => item.IsCluster);
        result.Single(item => item.IsCluster).Locations.Should().HaveCount(16);
        result.Should().ContainSingle(item => item.IsCluster == false);
        singleItem.Location.Should().NotBeNull();
        singleItem.ObservationCount.Should().Be(1);
        singleItem.LocationCount.Should().Be(1);
        singleItem.TaxaCount.Should().Be(1);
        singleItem.Location!.DecimalLongitude.Should().Be(30.0);
        singleItem.Location.DecimalLatitude.Should().Be(70.0);
    }

    [Fact]
    public void CentroidOfClusterIsAverageOfMemberCoordinates()
    {
        // Arrange
        var observations = LocationClustererTestData.Make16ObservationsCloseToEachOtherAndOneRemote();

        // Act
        var result = CreateClusterer(5, 10).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

        // Assert
        var cluster = result.Single(item => item.IsCluster);
        var expectedLat = cluster.Locations.Average(l => l.DecimalLatitude!.Value);
        var expectedLon = cluster.Locations.Average(l => l.DecimalLongitude!.Value);
        cluster.CentroidLatitude.Should().BeApproximately(expectedLat, precision: 1e-9);
        cluster.CentroidLongitude.Should().BeApproximately(expectedLon, precision: 1e-9);
    }

    [Fact]
    public void MultipleObservationsAtOneLocationAreNotReturnedAsACluster()
    {
        // Arrange
        var observations = Enumerable.Range(0, 10)
            .Select(index => LocationClustererTestData.CreateObservation(
                new Location
                {
                    LocationId = "same-location",
                    DecimalLatitude = 59.12345,
                    DecimalLongitude = 18.12345
                },
                $"same-location-observation-{index}"))
            .ToList();

        // Act
        var result = CreateClusterer(1, 10).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

        // Assert
        result.Should().ContainSingle();
        result.Single().IsCluster.Should().BeFalse();
        result.Single().LocationCount.Should().Be(1);
        result.Single().ObservationCount.Should().Be(10);
    }

    [Fact]
    public void DifferentLocationIdsAtSameCoordinatesAndUncertaintyAreTreatedAsOneLocation()
    {
        // Arrange
        var observations = new List<Observation>
        {
            LocationClustererTestData.CreateObservation(
                new Location
                {
                    LocationId = "same-place-location-0",
                    DecimalLongitude = 18.12345,
                    DecimalLatitude = 59.12345,
                    CoordinateUncertaintyInMeters = 25
                },
                "same-place-observation-0"),
            LocationClustererTestData.CreateObservation(
                new Location
                {
                    LocationId = "same-place-location-1",
                    DecimalLongitude = 18.12345,
                    DecimalLatitude = 59.12345,
                    CoordinateUncertaintyInMeters = 25
                },
                "same-place-observation-1")
        };

        // Act
        var result = CreateClusterer(1, 10, 2).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

        // Assert
        result.Should().ContainSingle();
        result.Single().IsCluster.Should().BeFalse();
        result.Single().LocationCount.Should().Be(1);
        result.Single().ObservationCount.Should().Be(2);
    }

    [Fact]
    public void SameLocationIdAtDifferentDistinctLocationsCanBecomeACluster()
    {
        // Arrange
        var observations = new List<Observation>
        {
            LocationClustererTestData.CreateObservation(
                new Location
                {
                    LocationId = "shared-location-id",
                    DecimalLongitude = 18.12345,
                    DecimalLatitude = 59.12345,
                    CoordinateUncertaintyInMeters = 25
                },
                "shared-id-observation-0"),
            LocationClustererTestData.CreateObservation(
                new Location
                {
                    LocationId = "shared-location-id",
                    DecimalLongitude = 18.12365,
                    DecimalLatitude = 59.12365,
                    CoordinateUncertaintyInMeters = 25
                },
                "shared-id-observation-1")
        };

        // Act
        var result = CreateClusterer(1, 10, 2).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

        // Assert
        result.Should().ContainSingle();
        result.Single().IsCluster.Should().BeTrue();
        result.Single().LocationCount.Should().Be(2);
        result.Single().ObservationCount.Should().Be(2);
    }

    [Fact]
    public void CustomMinimumNumberOfLocationsInOneClusterIsHonored()
    {
        // Arrange
        var observations = new List<Observation>();
        observations.AddRange(LocationClustererTestData.CreateObservations(new Location
        {
            LocationId = "custom-min-location-0",
            DecimalLongitude = 18.0000,
            DecimalLatitude = 59.0000
        }, 2));
        observations.AddRange(LocationClustererTestData.CreateObservations(new Location
        {
            LocationId = "custom-min-location-1",
            DecimalLongitude = 18.0002,
            DecimalLatitude = 59.0002
        }, 2));
        observations.AddRange(LocationClustererTestData.CreateObservations(new Location
        {
            LocationId = "custom-min-location-2",
            DecimalLongitude = 18.0004,
            DecimalLatitude = 59.0004
        }, 2));

        // Act
        var result = CreateClusterer(1, 10, 3)
            .ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

        // Assert
        result.Should().ContainSingle();
        result.Single().IsCluster.Should().BeTrue();
        result.Single().LocationCount.Should().Be(3);
        result.Single().ObservationCount.Should().Be(6);
    }

    [Fact]
    public void ClusterRadiusPxControlsClusteringDensityWhenMetersPerPixelIsSpecified()
    {
        // Arrange
        var observations = new List<Observation>
        {
            LocationClustererTestData.CreateObservation(new Location { LocationId = "radius-location-0", DecimalLongitude = 18.000, DecimalLatitude = 59.000 }, "radius-observation-0"),
            LocationClustererTestData.CreateObservation(new Location { LocationId = "radius-location-1", DecimalLongitude = 18.005, DecimalLatitude = 59.000 }, "radius-observation-1"),
            LocationClustererTestData.CreateObservation(new Location { LocationId = "radius-location-2", DecimalLongitude = 18.000, DecimalLatitude = 59.005 }, "radius-observation-2"),
            LocationClustererTestData.CreateObservation(new Location { LocationId = "radius-location-3", DecimalLongitude = 18.005, DecimalLatitude = 59.005 }, "radius-observation-3")
        };
        var boundingBox = LatLonBoundingBoxDto.Create(17.99, 59.01, 18.01, 58.99);
        var clusterer = CreateClusterer(1, 10);

        // Act
        var aggressivelyClustered = clusterer.ClusterLocations(observations, boundingBox, metersPerPixel: 10, clusterRadiusPx: 500);
        var lightlyClustered = clusterer.ClusterLocations(observations, boundingBox, metersPerPixel: 10, clusterRadiusPx: 1);

        // Assert
        aggressivelyClustered.Should().ContainSingle(item => item.IsCluster);
        aggressivelyClustered.Single(item => item.IsCluster).LocationCount.Should().Be(4);
        lightlyClustered.Should().OnlyContain(item => item.IsCluster == false);
        lightlyClustered.Should().HaveCount(4);
    }
}
