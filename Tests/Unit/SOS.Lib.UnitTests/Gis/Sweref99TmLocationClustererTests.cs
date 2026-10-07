using SOS.Api.Contracts.Common.Geo;
using FluentAssertions;
using SOS.Lib.Gis;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using Xunit;

namespace SOS.Lib.UnitTests.Gis;

public class Sweref99TmLocationClustererTests : LocationClustererAlgorithmTestBase
{
    protected override ILocationClusterer CreateClusterer(int minLocationCountBeforeClustering, int maxLocationCountAfterClustering)
    {
        return new Sweref99TmLocationClusterer(minLocationCountBeforeClustering, maxLocationCountAfterClustering);
    }

    protected override ILocationClusterer CreateClusterer(int minLocationCountBeforeClustering, int maxLocationCountAfterClustering, int minLocationsPerCluster)
    {
        return new Sweref99TmLocationClusterer(minLocationCountBeforeClustering, maxLocationCountAfterClustering, minLocationsPerCluster);
    }

    [Fact]
    public void MetersPerPixelOverloadClustersCloseLocationsUsingSweref99TmMeters()
    {
        // Arrange
        var observations = new List<Observation>
        {
            LocationClustererTestData.CreateObservation(new Location { LocationId = "sweref-location-0", DecimalLongitude = 18.0000, DecimalLatitude = 59.0000 }, "sweref-observation-0"),
            LocationClustererTestData.CreateObservation(new Location { LocationId = "sweref-location-1", DecimalLongitude = 18.0001, DecimalLatitude = 59.0000 }, "sweref-observation-1"),
            LocationClustererTestData.CreateObservation(new Location { LocationId = "sweref-location-2", DecimalLongitude = 18.0000, DecimalLatitude = 59.0001 }, "sweref-observation-2"),
            LocationClustererTestData.CreateObservation(new Location { LocationId = "sweref-location-3", DecimalLongitude = 18.0001, DecimalLatitude = 59.0001 }, "sweref-observation-3")
        };
        var boundingBox = LatLonBoundingBoxDto.Create(17.99, 59.01, 18.01, 58.99);

        // Act
        var result = new Sweref99TmLocationClusterer(3, 10)
            .ClusterLocations(observations, boundingBox, metersPerPixel: 2);

        // Assert
        result.Should().ContainSingle(item => item.IsCluster);
        result.Single(item => item.IsCluster).LocationCount.Should().Be(4);
    }

    [Fact]
    public void UsesExistingSweref99TmCoordinatesBeforeTransformingWgs84Coordinates()
    {
        // Arrange
        var observations = new List<Observation>
        {
            LocationClustererTestData.CreateObservation(new Location
            {
                LocationId = "sweref-property-location-0",
                DecimalLongitude = 18.00000,
                DecimalLatitude = 59.00000,
                Sweref99TmX = 500000,
                Sweref99TmY = 6500000
            }, "sweref-property-observation-0"),
            LocationClustererTestData.CreateObservation(new Location
            {
                LocationId = "sweref-property-location-1",
                DecimalLongitude = 18.00002,
                DecimalLatitude = 59.00000,
                Sweref99TmX = 700000,
                Sweref99TmY = 6500000
            }, "sweref-property-observation-1")
        };

        var boundingBox = LatLonBoundingBoxDto.Create(17.99, 59.01, 18.01, 58.99);

        // Act
        var result = new Sweref99TmLocationClusterer(1, 10)
            .ClusterLocations(observations, boundingBox, metersPerPixel: 10);

        // Assert
        result.Should().OnlyContain(item => !item.IsCluster);
        result.Should().HaveCount(2);
    }

    [Fact]
    public void ClustersWithCloseDisplayedCentroidsAreMerged()
    {
        // Arrange
        var observations = CreateCloseCentroidMergeObservations();
        var boundingBox = LatLonBoundingBoxDto.Create(17.99, 59.01, 18.01, 58.99);

        // Act
        var result = new Sweref99TmLocationClusterer(1, 10)
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
