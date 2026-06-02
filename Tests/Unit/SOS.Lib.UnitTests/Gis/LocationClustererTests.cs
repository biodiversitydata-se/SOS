//using FluentAssertions;
//using SOS.Lib.Gis;
//using SOS.Lib.Models.Gis;
//using SOS.Lib.Models.Processed.Observation;
//using Xunit;

//namespace SOS.Lib.UnitTests.Gis;

//public class LocationClustererTests
//{
//    private static readonly LatLonBoundingBox DefaultBoundingBox = LatLonBoundingBox.Create(5.0, 75.0, 35.0, 50.0);
//    private const double DefaultMetersPerPixel = 25.0;

//    [Fact]
//    public void WhenWeClusterZeroObservationsWeGetNoClusterItems()
//    {
//        // Arrange
//        var observations = new List<Observation>();

//        // Act
//        var result = new LocationClusterer(50, 400).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

//        // Assert
//        result.Should().BeEmpty();
//    }

//    /// <summary>
//    /// Observations without valid location coordinates should be silently ignored.
//    /// </summary>
//    [Fact]
//    public void ObservationsWithMissingCoordinatesAreIgnored()
//    {
//        // Arrange
//        var observations = new List<Observation>
//        {
//            CreateObservation(new Location { DecimalLatitude = null, DecimalLongitude = null }),
//            CreateObservation(new Location { DecimalLatitude = 59.0, DecimalLongitude = null }),
//            CreateObservation(new Location { DecimalLatitude = null, DecimalLongitude = 18.0 })
//        };

//        // Act
//        var result = new LocationClusterer(50, 400).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

//        // Assert
//        result.Should().BeEmpty();
//    }

//    /// <summary>
//    /// When locations are too far apart no clusters should be formed.
//    /// </summary>
//    [Fact]
//    public void WhenLocationsAreTooFarApartNoClusteringOccurs()
//    {
//        // Arrange
//        var observations = Make361ObservationsFarApart();

//        // Act
//        var result = new LocationClusterer(100, 400).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

//        // Assert
//        result.Should().OnlyContain(item => item.IsCluster == false);
//        result.Should().HaveCount(361);
//    }

//    /// <summary>
//    /// Verify that the number of map elements stays within the configured max.
//    /// </summary>
//    [Fact]
//    public void NumberOfMapElementsIsWithinBounds()
//    {
//        // Arrange
//        var observations = MakeRandomObservations(1000);

//        // Act
//        var result = new LocationClusterer(50, 100).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

//        // Assert
//        result.Count.Should().BeLessOrEqualTo(100);
//    }

//    /// <summary>
//    /// Verify that the cluster threshold is based on distinct locations rather than raw observation count.
//    /// </summary>
//    [Fact]
//    public void DoNotClusterWhenObservationCountIsHighButDistinctLocationCountIsLow()
//    {
//        // Arrange
//        var observations = MakeObservationsOnFewDistinctLocations(20, 10);

//        // Act
//        var result = new LocationClusterer(50, 100).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

//        // Assert
//        result.Should().OnlyContain(item => item.IsCluster == false);
//        result.Should().HaveCount(20);
//        result.Should().OnlyContain(item => item.LocationCount == 1);
//        result.Sum(item => item.ObservationCount).Should().Be(200);
//    }

//    /// <summary>
//    /// Verify that roughly half of the observations end up in clusters and half remain non-clustered.
//    /// </summary>
//    [Fact]
//    public void ApproximatelyHalfOfObservationsAreClustered()
//    {
//        // Arrange
//        var observations = MakeObservationsWithBalancedClusterDistribution();

//        // Act
//        var result = new LocationClusterer(30, 150).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

//        // Assert
//        var clusteredObservationCount = result
//            .Where(item => item.IsCluster)
//            .Sum(item => item.Observations.Count);

//        var nonClusteredObservationCount = result
//            .Where(item => item.IsCluster == false)
//            .Sum(item => item.Observations.Count);

//        var clusteredLocationCounts = result
//            .Where(item => item.IsCluster)
//            .Select(item => item.LocationCount)
//            .OrderBy(count => count)
//            .ToList();

//        var nonClusteredDistinctLocationCount = result
//            .Where(item => item.IsCluster == false)
//            .Select(item => item.Location!.LocationId)
//            .Distinct()
//            .Count();

//        var nonClusteredObservationCounts = result
//            .Where(item => item.IsCluster == false)
//            .Select(item => item.ObservationCount)
//            .OrderBy(count => count)
//            .ToList();

//        var allItems = result.ToList();

//        clusteredObservationCount.Should().Be(48);
//        nonClusteredObservationCount.Should().Be(48);
//        clusteredLocationCounts.Should().Equal(2, 3, 4, 5, 6);
//        nonClusteredDistinctLocationCount.Should().Be(result.Count(item => item.IsCluster == false));
//        nonClusteredObservationCounts.Should().Contain(2);
//        nonClusteredObservationCounts.Should().Contain(3);
//        allItems.Should().OnlyContain(item => item.ObservationCount >= item.LocationCount);
//    }

//    /// <summary>
//    /// The union of all observations in the result should equal the original input.
//    /// </summary>
//    [Fact]
//    public void ClusteredAndNonClusteredObservationsMatchOriginalInput()
//    {
//        // Arrange
//        var observations = MakeRandomObservations(1000);

//        // Act
//        var result = new LocationClusterer(50, 100).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

//        // Assert
//        var allAccountedFor = result
//            .SelectMany(item => item.Observations)
//            .ToList();
//        var actualOccurrenceIds = allAccountedFor
//            .Select(observation => observation.Occurrence.OccurrenceId)
//            .OrderBy(occurrenceId => occurrenceId)
//            .ToList();
//        var expectedOccurrenceIds = observations
//            .Select(observation => observation.Occurrence.OccurrenceId)
//            .OrderBy(occurrenceId => occurrenceId)
//            .ToList();

//        allAccountedFor.Should().HaveCount(observations.Count);
//        actualOccurrenceIds.Should().Equal(expectedOccurrenceIds);
//    }

//    /// <summary>
//    /// Verify that 16 close observations are clustered into one cluster and the remote location stays separate.
//    /// </summary>
//    [Fact]
//    public void ClusteringWorksWhenASingleLocationIsNotClustered()
//    {
//        // Arrange
//        var observations = Make16ObservationsCloseToEachOtherAndOneRemote();

//        // Act
//        var result = new LocationClusterer(5, 10).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

//        // Assert
//        var singleItem = result.Single(item => item.IsCluster == false);

//        result.Should().ContainSingle(item => item.IsCluster);
//        result.Single(item => item.IsCluster).Locations.Should().HaveCount(16);
//        result.Should().ContainSingle(item => item.IsCluster == false);
//        singleItem.Location.Should().NotBeNull();
//        singleItem.ObservationCount.Should().Be(1);
//        singleItem.LocationCount.Should().Be(1);
//        singleItem.TaxaCount.Should().Be(1);        
//        singleItem.Location!.DecimalLongitude.Should().Be(30.0);
//        singleItem.Location.DecimalLatitude.Should().Be(70.0);
//    }

//    [Fact]
//    public void MultipleObservationsAtOneLocationAreNotReturnedAsACluster()
//    {
//        // Arrange
//        var observations = Enumerable.Range(0, 10)
//            .Select(index => CreateObservation(
//                new Location
//                {
//                    LocationId = "same-location",
//                    DecimalLongitude = 18.12345,
//                    DecimalLatitude = 59.12345
//                },
//                $"same-location-observation-{index}"))
//            .ToList();

//        // Act
//        var result = new LocationClusterer(1, 10).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

//        // Assert
//        result.Should().ContainSingle();
//        result.Single().IsCluster.Should().BeFalse();
//        result.Single().LocationCount.Should().Be(1);
//        result.Single().ObservationCount.Should().Be(10);
//    }

//    [Fact]
//    public void DifferentLocationIdsAtSameCoordinatesAndUncertaintyAreTreatedAsOneLocation()
//    {
//        // Arrange
//        var observations = new List<Observation>
//        {
//            CreateObservation(
//                new Location
//                {
//                    LocationId = "same-place-location-0",
//                    DecimalLongitude = 18.12345,
//                    DecimalLatitude = 59.12345,
//                    CoordinateUncertaintyInMeters = 25
//                },
//                "same-place-observation-0"),
//            CreateObservation(
//                new Location
//                {
//                    LocationId = "same-place-location-1",
//                    DecimalLongitude = 18.12345,
//                    DecimalLatitude = 59.12345,
//                    CoordinateUncertaintyInMeters = 25
//                },
//                "same-place-observation-1")
//        };

//        // Act
//        var result = new LocationClusterer(1, 10, 2).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

//        // Assert
//        result.Should().ContainSingle();
//        result.Single().IsCluster.Should().BeFalse();
//        result.Single().LocationCount.Should().Be(1);
//        result.Single().ObservationCount.Should().Be(2);
//    }

//    [Fact]
//    public void SameLocationIdAtDifferentDistinctLocationsCanBecomeACluster()
//    {
//        // Arrange
//        var observations = new List<Observation>
//        {
//            CreateObservation(
//                new Location
//                {
//                    LocationId = "shared-location-id",
//                    DecimalLongitude = 18.12345,
//                    DecimalLatitude = 59.12345,
//                    CoordinateUncertaintyInMeters = 25
//                },
//                "shared-id-observation-0"),
//            CreateObservation(
//                new Location
//                {
//                    LocationId = "shared-location-id",
//                    DecimalLongitude = 18.12365,
//                    DecimalLatitude = 59.12365,
//                    CoordinateUncertaintyInMeters = 25
//                },
//                "shared-id-observation-1")
//        };

//        // Act
//        var result = new LocationClusterer(1, 10, 2).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

//        // Assert
//        result.Should().ContainSingle();
//        result.Single().IsCluster.Should().BeTrue();
//        result.Single().LocationCount.Should().Be(2);
//        result.Single().ObservationCount.Should().Be(2);
//    }

//    [Fact]
//    public void CustomMinimumNumberOfLocationsInOneClusterIsHonored()
//    {
//        // Arrange
//        var observations = new List<Observation>();
//        observations.AddRange(CreateObservations(new Location
//        {
//            LocationId = "custom-min-location-0",
//            DecimalLongitude = 18.0000,
//            DecimalLatitude = 59.0000
//        }, 2));
//        observations.AddRange(CreateObservations(new Location
//        {
//            LocationId = "custom-min-location-1",
//            DecimalLongitude = 18.0002,
//            DecimalLatitude = 59.0002
//        }, 2));
//        observations.AddRange(CreateObservations(new Location
//        {
//            LocationId = "custom-min-location-2",
//            DecimalLongitude = 18.0004,
//            DecimalLatitude = 59.0004
//        }, 2));

//        // Act
//        var result = new LocationClusterer(1, 10, 3)
//            .ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

//        // Assert
//        result.Should().ContainSingle();
//        result.Single().IsCluster.Should().BeTrue();
//        result.Single().LocationCount.Should().Be(3);
//        result.Single().ObservationCount.Should().Be(6);
//    }

//    /// <summary>
//    /// Centroid latitude and longitude of a cluster should be the average of the member coordinates.
//    /// </summary>
//    [Fact]
//    public void CentroidOfClusterIsAverageOfMemberCoordinates()
//    {
//        // Arrange
//        var observations = Make16ObservationsCloseToEachOtherAndOneRemote();

//        // Act
//        var result = new LocationClusterer(5, 10).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

//        // Assert
//        var cluster = result.Single(item => item.IsCluster);
//        var expectedLat = cluster.Locations.Average(l => l.DecimalLatitude!.Value);
//        var expectedLon = cluster.Locations.Average(l => l.DecimalLongitude!.Value);
//        cluster.CentroidLatitude.Should().BeApproximately(expectedLat, precision: 1e-9);
//        cluster.CentroidLongitude.Should().BeApproximately(expectedLon, precision: 1e-9);
//    }

//    /// <summary>
//    /// Verify that taxa and status counts are aggregated from the observations in a clustered item.
//    /// </summary>
//    [Fact]
//    public void ClusteredItemContainsTaxaAndStatusCounts()
//    {
//        // Arrange
//        var observations = Make16ObservationsCloseToEachOtherAndOneRemote();

//        // Act
//        var result = new LocationClusterer(5, 10).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);

//        // Assert
//        var cluster = result.Single(item => item.IsCluster);
//        cluster.TaxaCount.Should().Be(3);
//    }

//    /// <summary>
//    /// Verify that random down-sampling keeps the count within the expected range.
//    /// </summary>
//    [Fact]
//    public void CanFilterObservationsToCluster()
//    {
//        // Arrange
//        var observations = MakeRandomObservations(10000);

//        // Act
//        var filtered = new FilterObservationsToCluster(1000).Filter(observations);

//        // Assert
//        filtered.Count.Should().BeGreaterThan(900).And.BeLessOrEqualTo(1100);
//    }

//    /// <summary>
//    /// No filtering should occur when the number of observations is within the limit.
//    /// </summary>
//    [Fact]
//    public void DontFilterObservationsWhenBelowLimit()
//    {
//        // Arrange
//        var observations = MakeRandomObservations(1000);

//        // Act
//        var filtered = new FilterObservationsToCluster(1000).Filter(observations);

//        // Assert
//        var actualOccurrenceIds = filtered
//           .Select(observation => observation.Occurrence.OccurrenceId)
//           .OrderBy(occurrenceId => occurrenceId)
//           .ToList();
//        var expectedOccurrenceIds = observations
//            .Select(observation => observation.Occurrence.OccurrenceId)
//            .OrderBy(occurrenceId => occurrenceId)
//            .ToList();

//        actualOccurrenceIds.Should().Equal(expectedOccurrenceIds);
//    }

//    /// <summary>
//    /// Clustering 10 000 random observations should complete in under 20 seconds.
//    /// </summary>
//    [Fact]
//    public void ShallTakeLessThan20SecondsToCluster10000Locations()
//    {
//        // Arrange
//        var observations = MakeRandomObservations(10000);

//        // Act
//        var start = DateTime.Now;
//        new LocationClusterer(50, 400).ClusterLocations(observations, DefaultBoundingBox, metersPerPixel: DefaultMetersPerPixel);
//        var elapsed = DateTime.Now - start;

//        // Assert
//        elapsed.TotalSeconds.Should().BeLessThan(20);
//    }

//    [Fact]
//    public void BoundingBoxOverloadClustersCloseLocations()
//    {
//        // Arrange
//        var observations = Make16ObservationsCloseToEachOtherAndOneRemote();
//        var boundingBox = LatLonBoundingBox.Create(18.0, 70.0, 30.0, 59.0);

//        // Act
//        var result = new LocationClusterer(5, 10).ClusterLocations(observations, boundingBox, targetItemCount: 10);

//        // Assert
//        result.Should().ContainSingle(item => item.IsCluster);
//        result.Single(item => item.IsCluster).Locations.Should().HaveCount(16);
//        result.Should().ContainSingle(item => item.IsCluster == false);
//        result.Single(item => item.IsCluster == false).Location!.LocationId.Should().Be("remote-location-0");
//    }

//    [Fact]
//    public void BoundingBoxOverloadUsesConfiguredMaxItemCountAsDefaultTargetItemCount()
//    {
//        // Arrange
//        var observations = Make16ObservationsCloseToEachOtherAndOneRemote();
//        var boundingBox = LatLonBoundingBox.Create(18.0, 70.0, 30.0, 59.0);
//        var clusterer = new LocationClusterer(5, 10);

//        // Act
//        var resultUsingDefaultTarget = clusterer.ClusterLocations(observations, boundingBox);
//        var resultUsingExplicitTarget = clusterer.ClusterLocations(observations, boundingBox, targetItemCount: 10);

//        // Assert
//        resultUsingDefaultTarget.Select(item => (item.IsCluster, item.ObservationCount, item.LocationCount))
//            .Should().Equal(resultUsingExplicitTarget.Select(item => (item.IsCluster, item.ObservationCount, item.LocationCount)));
//    }

//    [Fact]
//    public void BoundingBoxOverloadTargetItemCountAffectsClusteringDensity()
//    {
//        // Arrange
//        var observations = new List<Observation>
//        {
//            CreateObservation(new Location { LocationId = "bbox-location-0", DecimalLongitude = 18.000, DecimalLatitude = 59.000 }, "bbox-observation-0"),
//            CreateObservation(new Location { LocationId = "bbox-location-1", DecimalLongitude = 18.005, DecimalLatitude = 59.000 }, "bbox-observation-1"),
//            CreateObservation(new Location { LocationId = "bbox-location-2", DecimalLongitude = 18.000, DecimalLatitude = 59.005 }, "bbox-observation-2"),
//            CreateObservation(new Location { LocationId = "bbox-location-3", DecimalLongitude = 18.005, DecimalLatitude = 59.005 }, "bbox-observation-3")
//        };
//        var boundingBox = LatLonBoundingBox.Create(17.99, 59.01, 18.01, 58.99);
//        var clusterer = new LocationClusterer(3, 10);

//        // Act
//        var aggressivelyClustered = clusterer.ClusterLocations(observations, boundingBox, targetItemCount: 1);
//        var lightlyClustered = clusterer.ClusterLocations(observations, boundingBox, targetItemCount: 10000);

//        // Assert
//        aggressivelyClustered.Should().ContainSingle(item => item.IsCluster);
//        aggressivelyClustered.Single(item => item.IsCluster).ObservationCount.Should().Be(4);
//        lightlyClustered.Should().OnlyContain(item => item.IsCluster == false);
//        lightlyClustered.Should().HaveCount(4);
//    }

//    [Fact]
//    public void MetersPerPixelOverloadClustersCloseLocations()
//    {
//        // Arrange
//        var observations = Make16ObservationsCloseToEachOtherAndOneRemote();
//        var boundingBox = LatLonBoundingBox.Create(18.0, 70.0, 30.0, 59.0);

//        // Act
//        var result = new LocationClusterer(5, 10).ClusterLocations(observations, boundingBox, metersPerPixel: 25);

//        // Assert
//        result.Should().ContainSingle(item => item.IsCluster);
//        result.Single(item => item.IsCluster).Locations.Should().HaveCount(16);
//        result.Should().ContainSingle(item => item.IsCluster == false);
//    }

//    [Fact]
//    public void MetersPerPixelOverloadTargetResolutionAffectsClusteringDensity()
//    {
//        // Arrange
//        var observations = new List<Observation>
//        {
//            CreateObservation(new Location { LocationId = "mpp-location-0", DecimalLongitude = 18.000, DecimalLatitude = 59.000 }, "mpp-observation-0"),
//            CreateObservation(new Location { LocationId = "mpp-location-1", DecimalLongitude = 18.005, DecimalLatitude = 59.000 }, "mpp-observation-1"),
//            CreateObservation(new Location { LocationId = "mpp-location-2", DecimalLongitude = 18.000, DecimalLatitude = 59.005 }, "mpp-observation-2"),
//            CreateObservation(new Location { LocationId = "mpp-location-3", DecimalLongitude = 18.005, DecimalLatitude = 59.005 }, "mpp-observation-3")
//        };
//        var boundingBox = LatLonBoundingBox.Create(17.99, 59.01, 18.01, 58.99);
//        var clusterer = new LocationClusterer(3, 10);

//        // Act
//        var aggressivelyClustered = clusterer.ClusterLocations(observations, boundingBox, metersPerPixel: 1000);
//        var lightlyClustered = clusterer.ClusterLocations(observations, boundingBox, metersPerPixel: 1);

//        // Assert
//        aggressivelyClustered.Should().ContainSingle(item => item.IsCluster);
//        aggressivelyClustered.Single(item => item.IsCluster).ObservationCount.Should().Be(4);
//        lightlyClustered.Should().OnlyContain(item => item.IsCluster == false);
//        lightlyClustered.Should().HaveCount(4);
//    }

//    private static List<Observation> Make361ObservationsFarApart()
//    {
//        var observations = new List<Observation>();
//        for (var i = 1; i <= 19; i++)
//        {
//            for (var j = 1; j <= 19; j++)
//            {
//                observations.Add(CreateObservation(new Location
//                {
//                    DecimalLongitude = 10.0 + i * 0.5,
//                    DecimalLatitude = 55.0 + j * 0.5
//                }));
//            }
//        }
//        return observations;
//    }

//    private static List<Observation> Make16ObservationsCloseToEachOtherAndOneRemote()
//    {
//        var observations = new List<Observation>();
//        var locationIndex = 0;
//        for (var i = 1; i <= 4; i++)
//        {
//            for (var j = 1; j <= 4; j++)
//            {
//                var location = new Location
//                {
//                    LocationId = $"close-location-{locationIndex++}",
//                    DecimalLongitude = 18.0 + i * 0.0001,
//                    DecimalLatitude = 59.0 + j * 0.0001
//                };

//                var closeLocationIndex = locationIndex - 1;
//                observations.Add(CreateObservation(
//                    location,
//                    $"close-observation-{closeLocationIndex}",
//                    closeLocationIndex % 3 + 1,
//                    closeLocationIndex % 2 == 0,
//                    closeLocationIndex % 3 == 0));
//            }
//        }

//        observations.Add(CreateObservation(
//            new Location
//            {
//                LocationId = "remote-location-0",
//                DecimalLongitude = 30.0,
//                DecimalLatitude = 70.0
//            },
//            "remote-observation-0",
//            99,
//            true,
//            false));

//        return observations;
//    }

//    private static List<Observation> MakeObservationsOnFewDistinctLocations(int locationCount, int observationsPerLocation)
//    {
//        var observations = new List<Observation>();

//        for (var index = 0; index < locationCount; index++)
//        {
//            var location = new Location
//            {
//                LocationId = $"few-location-{index}",
//                DecimalLongitude = 10.0 + index,
//                DecimalLatitude = 55.0 + index
//            };

//            observations.AddRange(CreateObservations(location, observationsPerLocation));
//        }

//        return observations;
//    }

//    private static List<Observation> MakeObservationsWithBalancedClusterDistribution()
//    {
//        var observations = new List<Observation>();

//        AddClusterGroup(observations, 10.00, 55.00, 0.04, 2, new[] { 2, 2 });
//        AddClusterGroup(observations, 11.00, 56.00, 0.04, 3, new[] { 2, 2, 2 });
//        AddClusterGroup(observations, 12.00, 57.00, 0.04, 4, new[] { 2, 3, 2, 3 });
//        AddClusterGroup(observations, 13.00, 58.00, 0.04, 5, new[] { 2, 2, 3, 2, 3 });
//        AddClusterGroup(observations, 14.00, 59.00, 0.04, 6, new[] { 3, 2, 3, 2, 3, 3 });

//        AddNonClusteredLocation(observations, 30.0, 70.0, 2, 0);
//        AddNonClusteredLocation(observations, 31.0, 70.0, 2, 1);
//        AddNonClusteredLocation(observations, 32.0, 70.0, 2, 2);
//        AddNonClusteredLocation(observations, 33.0, 70.0, 2, 3);
//        AddNonClusteredLocation(observations, 34.0, 70.0, 2, 4);
//        AddNonClusteredLocation(observations, 35.0, 70.0, 2, 5);
//        AddNonClusteredLocation(observations, 36.0, 70.0, 2, 6);
//        AddNonClusteredLocation(observations, 37.0, 70.0, 2, 7);
//        AddNonClusteredLocation(observations, 38.0, 70.0, 2, 8);
//        AddNonClusteredLocation(observations, 39.0, 70.0, 2, 9);
//        AddNonClusteredLocation(observations, 30.0, 72.0, 3, 10);
//        AddNonClusteredLocation(observations, 31.0, 72.0, 3, 11);
//        AddNonClusteredLocation(observations, 32.0, 72.0, 3, 12);
//        AddNonClusteredLocation(observations, 33.0, 72.0, 3, 13);
//        AddNonClusteredLocation(observations, 34.0, 72.0, 3, 14);
//        AddNonClusteredLocation(observations, 35.0, 72.0, 3, 15);
//        AddNonClusteredLocation(observations, 36.0, 72.0, 3, 16);
//        AddNonClusteredLocation(observations, 37.0, 72.0, 3, 17);
//        AddNonClusteredLocation(observations, 38.0, 72.0, 2, 18);
//        AddNonClusteredLocation(observations, 39.0, 72.0, 2, 19);

//        return observations;
//    }

//    private static void AddClusterGroup(List<Observation> observations, double baseLongitude, double baseLatitude, double offsetStep, int locationCount, IReadOnlyList<int> observationsPerLocation)
//    {
//        for (var index = 0; index < locationCount; index++)
//        {
//            var location = new Location
//            {
//                LocationId = $"cluster-location-{baseLongitude:F2}-{baseLatitude:F2}-{index}",
//                DecimalLongitude = baseLongitude + index * offsetStep,
//                DecimalLatitude = baseLatitude + index * offsetStep
//            };

//            observations.AddRange(CreateObservations(location, observationsPerLocation[index]));
//        }
//    }

//    private static void AddNonClusteredLocation(List<Observation> observations, double longitude, double latitude, int observationCount, int index)
//    {
//        var location = new Location
//        {
//            LocationId = $"single-location-{index}",
//            DecimalLongitude = longitude,
//            DecimalLatitude = latitude
//        };

//        observations.AddRange(CreateObservations(location, observationCount));
//    }

//    private static List<Observation> MakeRandomObservations(int count)
//    {
//        var rand = new Random();
//        var observations = new List<Observation>(count);
//        var locationIndex = 0;

//        while (observations.Count < count)
//        {
//            var location = new Location
//            {
//                DecimalLongitude = 10.0 + rand.NextDouble() * 20.0,
//                DecimalLatitude = 55.0 + rand.NextDouble() * 10.0,
//                LocationId = $"location-{locationIndex++}"
//            };

//            var observationsToCreate = Math.Min(2, count - observations.Count);
//            observations.AddRange(CreateObservations(location, observationsToCreate));
//        }

//        return observations;
//    }

//    private static Observation CreateObservation(Location location, string? observationId = null, int taxonId = 1, bool isPositiveObservation = true, bool isGeneralized = false)
//    {
//        return new Observation
//        {
//            Id = observationId ?? Guid.NewGuid().ToString(),
//            IsGeneralized = isGeneralized,
//            Location = location,
//            Occurrence = new Occurrence
//            {
//                OccurrenceId = observationId ?? Guid.NewGuid().ToString(),
//                IsPositiveObservation = isPositiveObservation
//            },
//            Taxon = new Taxon { Id = taxonId }
//        };
//    }

//    private static IEnumerable<Observation> CreateObservations(Location location, int count)
//    {
//        return Enumerable.Range(0, count)
//            .Select(_ => CreateObservation(location));
//    }
//}
