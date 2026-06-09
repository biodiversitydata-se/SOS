using FluentAssertions;
using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Gis;
using SOS.Lib.Models.Processed.Observation;
using Xunit;
using ObservationLocation = SOS.Lib.Models.Processed.Observation.Location;

namespace SOS.Lib.UnitTests.Gis;

public class LocationClusterItemDtoTests
{
    [Fact]
    public void FromLocationClusterItem_WhenCutoffYearIsMissing_LeavesCutoffYearStatusEmpty()
    {
        // Arrange
        var item = LocationClusterItem.FromSingleLocation([
            CreateObservation(isPresent: true, isSensitive: false, endYear: 2024)
        ]);

        // Act
        var dto = LocationClusterItemDto.FromLocationClusterItem(item);

        // Assert
        dto.DisplayStatus.Should().Be(LocationDisplayStatus.Present);
        dto.CutoffYearStatus.Should().BeNull();
        dto.IsAllObservationsSensitive.Should().BeFalse();
    }

    [Fact]
    public void FromLocationClusterItem_WhenAllPresentObservationsAreSensitive_SetsPresentSensitive()
    {
        // Arrange
        var item = LocationClusterItem.FromSingleLocation([
            CreateObservation(isPresent: true, isSensitive: true, endYear: 2025),
            CreateObservation(isPresent: true, isSensitive: true, endYear: 2024)
        ]);

        // Act
        var dto = LocationClusterItemDto.FromLocationClusterItem(item, cutoffYear: 2024);

        // Assert
        dto.DisplayStatus.Should().Be(LocationDisplayStatus.PresentSensitive);
        dto.CutoffYearStatus.Should().Be(CutoffYearStatus.After);
        dto.IsAllObservationsSensitive.Should().BeTrue();
    }

    [Fact]
    public void FromLocationClusterItem_WhenOnlyPresentObservationIsBeforeCutoffYear_SetsDividedByYearUntil()
    {
        // Arrange
        var item = LocationClusterItem.FromSingleLocation([
            CreateObservation(isPresent: true, isSensitive: false, endYear: 2020)
        ]);

        // Act
        var dto = LocationClusterItemDto.FromLocationClusterItem(item, cutoffYear: 2024);

        // Assert
        dto.DisplayStatus.Should().Be(LocationDisplayStatus.DividedByYearUntil);
        dto.CutoffYearStatus.Should().Be(CutoffYearStatus.Until);
    }

    [Fact]
    public void FromLocationClusterItem_WhenCutoffYearIsUsedAndAllObservationsAreNotPresent_SetsNotPresent()
    {
        // Arrange
        var item = LocationClusterItem.FromSingleLocation([
            CreateObservation(isPresent: false, isSensitive: false, endYear: 2025, isNotRecovered: true)
        ]);

        // Act
        var dto = LocationClusterItemDto.FromLocationClusterItem(item, cutoffYear: 2024);

        // Assert
        dto.DisplayStatus.Should().Be(LocationDisplayStatus.NotPresent);
        dto.CutoffYearStatus.Should().Be(CutoffYearStatus.AfterNotRecovered);
    }

    [Fact]
    public void FromLocationClusterItem_WhenNoObservationIsPresentButOneIsTreated_SetsTreated()
    {
        // Arrange
        var item = LocationClusterItem.FromSingleLocation([
            CreateObservation(isPresent: false, isSensitive: false, endYear: 2025, isNotRecovered: true, treatmentId: 0)
        ]);

        // Act
        var dto = LocationClusterItemDto.FromLocationClusterItem(item);

        // Assert
        dto.DisplayStatus.Should().Be(LocationDisplayStatus.Treated);
    }

    [Fact]
    public void FromLocationClusterItem_WhenObservationsHaveMixedStatuses_FillsDisplayStatusCountAndOrder()
    {
        // Arrange
        var item = LocationClusterItem.FromSingleLocation([
            CreateObservation(isPresent: true, isSensitive: false, endYear: 2025),
            CreateObservation(isPresent: true, isSensitive: true, endYear: 2025),
            CreateObservation(isPresent: false, isSensitive: false, endYear: 2025, treatmentId: 0),
            CreateObservation(isPresent: false, isSensitive: false, endYear: 2025, isNotRecovered: true),
            CreateObservation(isPresent: false, isSensitive: false, endYear: 2025)
        ]);

        // Act
        var dto = LocationClusterItemDto.FromLocationClusterItem(item);

        // Assert
        dto.DisplayStatusCount.Should().BeEquivalentTo(new Dictionary<LocationDisplayStatus, int>
        {
            [LocationDisplayStatus.Present] = 1,
            [LocationDisplayStatus.PresentSensitive] = 1,
            [LocationDisplayStatus.Treated] = 1,
            [LocationDisplayStatus.NotRecovered] = 1,
            [LocationDisplayStatus.NotPresent] = 1
        });
        dto.DisplayStatusCount.Keys.Should().Equal(
            LocationDisplayStatus.Present,
            LocationDisplayStatus.PresentSensitive,
            LocationDisplayStatus.Treated,
            LocationDisplayStatus.NotRecovered,
            LocationDisplayStatus.NotPresent);
    }

    [Fact]
    public void FromLocationClusterItem_WhenCutoffYearIsUsed_FillsDisplayStatusCountWithCutoffStatusRules()
    {
        // Arrange
        var item = LocationClusterItem.FromSingleLocation([
            CreateObservation(isPresent: true, isSensitive: false, endYear: 2025),
            CreateObservation(isPresent: true, isSensitive: true, endYear: 2025),
            CreateObservation(isPresent: true, isSensitive: false, endYear: 2020),
            CreateObservation(isPresent: false, isSensitive: false, endYear: 2025, treatmentId: 0),
            CreateObservation(isPresent: false, isSensitive: false, endYear: 2025, isNotRecovered: true)
        ]);

        // Act
        var dto = LocationClusterItemDto.FromLocationClusterItem(item, cutoffYear: 2024);

        // Assert
        dto.DisplayStatusCount.Should().BeEquivalentTo(new Dictionary<LocationDisplayStatus, int>
        {
            [LocationDisplayStatus.Present] = 1,
            [LocationDisplayStatus.PresentSensitive] = 1,
            [LocationDisplayStatus.DividedByYearUntil] = 1,
            [LocationDisplayStatus.Treated] = 1,
            [LocationDisplayStatus.NotPresent] = 1
        });
        dto.DisplayStatusCount.Keys.Should().Equal(
            LocationDisplayStatus.Present,
            LocationDisplayStatus.PresentSensitive,
            LocationDisplayStatus.Treated,
            LocationDisplayStatus.NotPresent,
            LocationDisplayStatus.DividedByYearUntil);
    }

    [Fact]
    public void FromLocationClusterItem_WhenCutoffYearIsUsedAndNoObservationIsPresentButOneIsTreated_SetsTreated()
    {
        // Arrange
        var item = LocationClusterItem.FromSingleLocation([
            CreateObservation(isPresent: false, isSensitive: false, endYear: 2025, isNotRecovered: true, treatmentId: 0)
        ]);

        // Act
        var dto = LocationClusterItemDto.FromLocationClusterItem(item, cutoffYear: 2024);

        // Assert
        dto.DisplayStatus.Should().Be(LocationDisplayStatus.Treated);
        dto.CutoffYearStatus.Should().Be(CutoffYearStatus.AfterNotRecovered);
    }

    [Fact]
    public void FromLocationClusterItem_WhenObservationSummaryExists_FillsDisplayStatusCountFromSummary()
    {
        // Arrange
        var location = CreateLocation("cached-location", 18.0, 59.0);
        var item = new LocationClusterItem
        {
            IsCluster = false,
            Location = location,
            Geometry = new Point(18.0, 59.0),
            LocationCount = 1,
            ObservationCount = 4,
            Locations = [location],
            ObservationSummary = new LocationObservationSummary
            {
                ObservationCount = 4,
                FoundCount = 2,
                TreatedCount = 1,
                DisplayStatusSummaries =
                [
                    new LocationObservationDisplayStatusSummary
                    {
                        IsPresent = true,
                        IsSensitive = false,
                        EndYear = 2025,
                        Count = 1
                    },
                    new LocationObservationDisplayStatusSummary
                    {
                        IsPresent = true,
                        IsSensitive = true,
                        EndYear = 2020,
                        Count = 1
                    },
                    new LocationObservationDisplayStatusSummary
                    {
                        IsTreated = true,
                        EndYear = 2025,
                        Count = 1
                    },
                    new LocationObservationDisplayStatusSummary
                    {
                        IsNotRecovered = true,
                        EndYear = 2025,
                        Count = 1
                    }
                ]
            }
        };

        // Act
        var dto = LocationClusterItemDto.FromLocationClusterItem(item, cutoffYear: 2024);

        // Assert
        dto.DisplayStatusCount.Should().BeEquivalentTo(new Dictionary<LocationDisplayStatus, int>
        {
            [LocationDisplayStatus.Present] = 1,
            [LocationDisplayStatus.DividedByYearUntil] = 1,
            [LocationDisplayStatus.Treated] = 1,
            [LocationDisplayStatus.NotPresent] = 1
        });
    }

    [Fact]
    public void FromLocationClusterItem_WhenItemIsCluster_SetsClusterLocationType()
    {
        // Arrange
        var observations = new List<Observation>
        {
            CreateObservation(isPresent: true, isSensitive: false, endYear: 2025),
            CreateObservation(isPresent: false, isSensitive: false, endYear: 2020)
        };
        var item = new LocationClusterItem
        {
            IsCluster = true,
            Location = CreateLocation("cluster", 18.1, 59.1),
            Geometry = new Point(18.1, 59.1),
            Observations = observations,
            Locations = observations.Select(observation => observation.Location).ToList()
        };

        // Act
        var dto = LocationClusterItemDto.FromLocationClusterItem(item, cutoffYear: 2024);

        // Assert
        dto.DisplayStatus.Should().Be(LocationDisplayStatus.Cluster);
        dto.CutoffYearStatus.Should().Be(CutoffYearStatus.After);
        dto.IsAllObservationsSensitive.Should().BeFalse();
    }

    [Fact]
    public void FromLocationClusterItem_WhenObservationCountIsFiveOrLess_FillsDebugLocations()
    {
        // Arrange
        var item = LocationClusterItem.FromSingleLocation([
            CreateObservation(
                isPresent: false,
                isSensitive: true,
                endYear: 2026,
                isNotRecovered: true,
                occurrenceId: "occurrence-1",
                isGeneralized: true,
                treatmentId: 17)
        ]);

        // Act
        var dto = LocationClusterItemDto.FromLocationClusterItem(item);

        // Assert
        dto.DebugObservations.Should().ContainSingle();
        var debugLocation = dto.DebugObservations![0];
        //debugLocation.OccurrenceId.Should().Be("occurrence-1");
        debugLocation.EndYear.Should().Be(2026);
        debugLocation.Diffused.Should().BeTrue();
        debugLocation.Sensitive.Should().BeTrue();
        debugLocation.Treatment.Should().Be(17);
        debugLocation.Positive.Should().BeFalse();
        debugLocation.NotRedisc.Should().BeTrue();
        debugLocation.NeverFound.Should().BeFalse();
    }

    [Fact]
    public void FromLocationClusterItem_WhenObservationCountIsSixOrMore_LeavesDebugLocationsNull()
    {
        // Arrange
        var item = LocationClusterItem.FromSingleLocation(
            Enumerable.Range(0, 6)
                .Select(index => CreateObservation(isPresent: true, isSensitive: false, endYear: 2024, occurrenceId: $"occurrence-{index}"))
                .ToList());

        // Act
        var dto = LocationClusterItemDto.FromLocationClusterItem(item);

        // Assert
        dto.DebugObservations.Should().BeNull();
    }

    private static Observation CreateObservation(
        bool isPresent,
        bool isSensitive,
        int endYear,
        bool isNotRecovered = false,
        string? occurrenceId = null,
        bool isGeneralized = false,
        int? treatmentId = null)
    {
        occurrenceId ??= Guid.NewGuid().ToString("N");

        return new Observation
        {
            Id = occurrenceId,
            IsGeneralized = isGeneralized,
            Sensitive = isSensitive,
            Location = CreateLocation(Guid.NewGuid().ToString("N"), 18.0, 59.0),
            Occurrence = new Occurrence
            {
                OccurrenceId = occurrenceId,
                IsPositiveObservation = isPresent,
                IsNotRediscoveredObservation = isNotRecovered,
                IsNeverFoundObservation = !isPresent && !isNotRecovered
            },
            Event = new Event
            {
                StartYear = endYear,
                EndYear = endYear
            },
            ArtportalenInternal = treatmentId.HasValue
                ? new ArtportalenInternal
                {
                    InvasiveSpeciesTreatment = new VocabularyValue
                    {
                        Id = treatmentId.Value
                    }
                }
                : null
        };
    }

    private static ObservationLocation CreateLocation(string locationId, double longitude, double latitude)
    {
        return new ObservationLocation(LocationType.Point)
        {
            LocationId = locationId,
            DecimalLongitude = longitude,
            DecimalLatitude = latitude
        };
    }
}
