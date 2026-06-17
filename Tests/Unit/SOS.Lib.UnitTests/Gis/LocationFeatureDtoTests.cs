using FluentAssertions;
using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Gis;
using SOS.Lib.Models.Processed.Observation;
using Xunit;
using ObservationLocation = SOS.Lib.Models.Processed.Observation.Location;

namespace SOS.Lib.UnitTests.Gis;

public class LocationFeatureDtoTests
{
    [Fact]
    public void FromLocationFeature_WhenCutoffYearIsMissing_LeavesCutoffYearStatusEmpty()
    {
        // Arrange
        var item = LocationFeature.FromSingleLocation([
            CreateObservation(isPresent: true, isSensitive: false, endYear: 2024)
        ]);

        // Act
        var dto = LocationFeatureDto.FromLocationFeature(item);

        // Assert
        dto.Presentation.Status.Should().Be(LocationPresentationStatus.Present);
        dto.Presentation.CutoffYearStatus.Should().BeNull();
        dto.Presentation.IsAllObservationsSensitive.Should().BeFalse();
    }

    [Fact]
    public void FromLocationFeature_WhenAllPresentObservationsAreSensitive_SetsPresentSensitive()
    {
        // Arrange
        var item = LocationFeature.FromSingleLocation([
            CreateObservation(isPresent: true, isSensitive: true, endYear: 2025),
            CreateObservation(isPresent: true, isSensitive: true, endYear: 2024)
        ]);

        // Act
        var dto = LocationFeatureDto.FromLocationFeature(item, cutoffYear: 2024);

        // Assert
        dto.Presentation.Status.Should().Be(LocationPresentationStatus.PresentSensitive);
        dto.Presentation.CutoffYearStatus.Should().Be(CutoffYearStatus.After);
        dto.Presentation.IsAllObservationsSensitive.Should().BeTrue();
    }

    [Fact]
    public void FromLocationFeature_WhenOnlyPresentObservationIsBeforeCutoffYear_SetsDividedByYearUntil()
    {
        // Arrange
        var item = LocationFeature.FromSingleLocation([
            CreateObservation(isPresent: true, isSensitive: false, endYear: 2020)
        ]);

        // Act
        var dto = LocationFeatureDto.FromLocationFeature(item, cutoffYear: 2024);

        // Assert
        dto.Presentation.Status.Should().Be(LocationPresentationStatus.DividedByYearUntil);
        dto.Presentation.CutoffYearStatus.Should().Be(CutoffYearStatus.Until);
    }

    [Fact]
    public void FromLocationFeature_WhenCutoffYearIsUsedAndAllObservationsAreNotPresent_SetsNotPresent()
    {
        // Arrange
        var item = LocationFeature.FromSingleLocation([
            CreateObservation(isPresent: false, isSensitive: false, endYear: 2025, isNotRecovered: true)
        ]);

        // Act
        var dto = LocationFeatureDto.FromLocationFeature(item, cutoffYear: 2024);

        // Assert
        dto.Presentation.Status.Should().Be(LocationPresentationStatus.NotPresent);
        dto.Presentation.CutoffYearStatus.Should().Be(CutoffYearStatus.AfterNotRecovered);
    }

    [Fact]
    public void FromLocationFeature_WhenNoObservationIsPresentButOneIsTreated_SetsTreated()
    {
        // Arrange
        var item = LocationFeature.FromSingleLocation([
            CreateObservation(isPresent: false, isSensitive: false, endYear: 2025, isNotRecovered: true, treatmentId: 0)
        ]);

        // Act
        var dto = LocationFeatureDto.FromLocationFeature(item);

        // Assert
        dto.Presentation.Status.Should().Be(LocationPresentationStatus.Treated);
    }

    [Fact]
    public void FromLocationFeature_WhenObservationsHaveMixedStatuses_FillsPresentationStatusCountAndOrder()
    {
        // Arrange
        var item = LocationFeature.FromSingleLocation([
            CreateObservation(isPresent: true, isSensitive: false, endYear: 2025),
            CreateObservation(isPresent: true, isSensitive: true, endYear: 2025),
            CreateObservation(isPresent: false, isSensitive: false, endYear: 2025, treatmentId: 0),
            CreateObservation(isPresent: false, isSensitive: false, endYear: 2025, isNotRecovered: true),
            CreateObservation(isPresent: false, isSensitive: false, endYear: 2025)
        ]);

        // Act
        var dto = LocationFeatureDto.FromLocationFeature(item);

        // Assert
        dto.Presentation.StatusCounts.Should().BeEquivalentTo(new Dictionary<LocationPresentationStatus, int>
        {
            [LocationPresentationStatus.Present] = 1,
            [LocationPresentationStatus.PresentSensitive] = 1,
            [LocationPresentationStatus.Treated] = 1,
            [LocationPresentationStatus.NotRecovered] = 1,
            [LocationPresentationStatus.NotPresent] = 1
        });
        dto.Presentation.StatusCounts.Keys.Should().Equal(
            LocationPresentationStatus.Present,
            LocationPresentationStatus.PresentSensitive,
            LocationPresentationStatus.Treated,
            LocationPresentationStatus.NotRecovered,
            LocationPresentationStatus.NotPresent);
    }

    [Fact]
    public void FromLocationFeature_WhenCutoffYearIsUsed_FillsPresentationStatusCountWithCutoffStatusRules()
    {
        // Arrange
        var item = LocationFeature.FromSingleLocation([
            CreateObservation(isPresent: true, isSensitive: false, endYear: 2025),
            CreateObservation(isPresent: true, isSensitive: true, endYear: 2025),
            CreateObservation(isPresent: true, isSensitive: false, endYear: 2020),
            CreateObservation(isPresent: false, isSensitive: false, endYear: 2025, treatmentId: 0),
            CreateObservation(isPresent: false, isSensitive: false, endYear: 2025, isNotRecovered: true)
        ]);

        // Act
        var dto = LocationFeatureDto.FromLocationFeature(item, cutoffYear: 2024);

        // Assert
        dto.Presentation.StatusCounts.Should().BeEquivalentTo(new Dictionary<LocationPresentationStatus, int>
        {
            [LocationPresentationStatus.Present] = 1,
            [LocationPresentationStatus.PresentSensitive] = 1,
            [LocationPresentationStatus.DividedByYearUntil] = 1,
            [LocationPresentationStatus.Treated] = 1,
            [LocationPresentationStatus.NotPresent] = 1
        });
        dto.Presentation.StatusCounts.Keys.Should().Equal(
            LocationPresentationStatus.Present,
            LocationPresentationStatus.PresentSensitive,
            LocationPresentationStatus.Treated,
            LocationPresentationStatus.NotPresent,
            LocationPresentationStatus.DividedByYearUntil);
    }

    [Fact]
    public void FromLocationFeature_WhenCutoffYearIsUsedAndNoObservationIsPresentButOneIsTreated_SetsTreated()
    {
        // Arrange
        var item = LocationFeature.FromSingleLocation([
            CreateObservation(isPresent: false, isSensitive: false, endYear: 2025, isNotRecovered: true, treatmentId: 0)
        ]);

        // Act
        var dto = LocationFeatureDto.FromLocationFeature(item, cutoffYear: 2024);

        // Assert
        dto.Presentation.Status.Should().Be(LocationPresentationStatus.Treated);
        dto.Presentation.CutoffYearStatus.Should().Be(CutoffYearStatus.AfterNotRecovered);
    }

    [Fact]
    public void FromLocationFeature_WhenObservationSummaryExists_FillsPresentationStatusCountFromSummary()
    {
        // Arrange
        var location = CreateLocation("cached-location", 18.0, 59.0);
        var item = new LocationFeature
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
                PresentationStatusSummaries =
                [
                    new LocationObservationSummary.PresentationStatusSummary
                    {
                        IsPresent = true,
                        IsSensitive = false,
                        EndYear = 2025,
                        Count = 1
                    },
                    new LocationObservationSummary.PresentationStatusSummary
                    {
                        IsPresent = true,
                        IsSensitive = true,
                        EndYear = 2020,
                        Count = 1
                    },
                    new LocationObservationSummary.PresentationStatusSummary
                    {
                        IsTreated = true,
                        EndYear = 2025,
                        Count = 1
                    },
                    new LocationObservationSummary.PresentationStatusSummary
                    {
                        IsNotRecovered = true,
                        EndYear = 2025,
                        Count = 1
                    }
                ]
            }
        };

        // Act
        var dto = LocationFeatureDto.FromLocationFeature(item, cutoffYear: 2024);

        // Assert
        dto.Presentation.StatusCounts.Should().BeEquivalentTo(new Dictionary<LocationPresentationStatus, int>
        {
            [LocationPresentationStatus.Present] = 1,
            [LocationPresentationStatus.DividedByYearUntil] = 1,
            [LocationPresentationStatus.Treated] = 1,
            [LocationPresentationStatus.NotPresent] = 1
        });
    }

    [Fact]
    public void FromLocationFeature_WhenItemIsCluster_SetsClusterLocationType()
    {
        // Arrange
        var observations = new List<Observation>
        {
            CreateObservation(isPresent: true, isSensitive: false, endYear: 2025),
            CreateObservation(isPresent: false, isSensitive: false, endYear: 2020)
        };
        var item = new LocationFeature
        {
            IsCluster = true,
            Location = CreateLocation("cluster", 18.1, 59.1),
            Geometry = new Point(18.1, 59.1),
            Observations = observations,
            Locations = observations.Select(observation => observation.Location).ToList()
        };

        // Act
        var dto = LocationFeatureDto.FromLocationFeature(item, cutoffYear: 2024);

        // Assert
        dto.Presentation.Status.Should().Be(LocationPresentationStatus.Cluster);
        dto.Presentation.CutoffYearStatus.Should().Be(CutoffYearStatus.After);
        dto.Presentation.IsAllObservationsSensitive.Should().BeFalse();
    }

    [Fact]
    public void FromLocationFeature_WhenObservationCountIsFiveOrLess_FillsDebugLocations()
    {
        // Arrange
        var item = LocationFeature.FromSingleLocation([
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
        var dto = LocationFeatureDto.FromLocationFeature(item);

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
    public void FromLocationFeature_WhenObservationCountIsSixOrMore_LeavesDebugLocationsNull()
    {
        // Arrange
        var item = LocationFeature.FromSingleLocation(
            Enumerable.Range(0, 6)
                .Select(index => CreateObservation(isPresent: true, isSensitive: false, endYear: 2024, occurrenceId: $"occurrence-{index}"))
                .ToList());

        // Act
        var dto = LocationFeatureDto.FromLocationFeature(item);

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
