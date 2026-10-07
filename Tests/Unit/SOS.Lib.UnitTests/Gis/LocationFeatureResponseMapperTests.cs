using FluentAssertions;
using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Gis;
using SOS.Lib.Models.Processed.Observation;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using ObservationLocation = SOS.Lib.Models.Processed.Observation.Location;

namespace SOS.Lib.UnitTests.Gis;

public class LocationFeatureResponseMapperTests
{
    [Fact]
    public void FromLocationFeature_WhenCutoffYearIsMissing_LeavesCutoffYearStatusEmpty()
    {
        // Arrange
        var item = LocationFeature.FromSingleLocation([
            CreateObservation(isPresent: true, isSensitive: false, endYear: 2024)
        ]);

        // Act
        var dto = LocationFeatureResponseMapper.ToProperties(item);

        // Assert
        dto.Presentation.Status.Should().Be(SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.Present);
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
        var dto = LocationFeatureResponseMapper.ToProperties(item, cutoffYear: 2024);

        // Assert
        dto.Presentation.Status.Should().Be(SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.PresentSensitive);
        dto.Presentation.CutoffYearStatus.Should().Be(SOS.Api.Contracts.Observations.Responses.CutoffYearStatusDto.After);
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
        var dto = LocationFeatureResponseMapper.ToProperties(item, cutoffYear: 2024);

        // Assert
        dto.Presentation.Status.Should().Be(SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.DividedByYearUntil);
        dto.Presentation.CutoffYearStatus.Should().Be(SOS.Api.Contracts.Observations.Responses.CutoffYearStatusDto.Until);
    }

    [Fact]
    public void FromLocationFeature_WhenCutoffYearIsUsedAndAllObservationsAreNotPresent_SetsNotPresent()
    {
        // Arrange
        var item = LocationFeature.FromSingleLocation([
            CreateObservation(isPresent: false, isSensitive: false, endYear: 2025, isNotRecovered: true)
        ]);

        // Act
        var dto = LocationFeatureResponseMapper.ToProperties(item, cutoffYear: 2024);

        // Assert
        dto.Presentation.Status.Should().Be(SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.NotPresent);
        dto.Presentation.CutoffYearStatus.Should().Be(SOS.Api.Contracts.Observations.Responses.CutoffYearStatusDto.AfterNotRecovered);
    }

    [Fact]
    public void FromLocationFeature_WhenNoObservationIsPresentButOneIsTreated_SetsTreated()
    {
        // Arrange
        var item = LocationFeature.FromSingleLocation([
            CreateObservation(isPresent: false, isSensitive: false, endYear: 2025, isNotRecovered: true, treatmentId: 0)
        ]);

        // Act
        var dto = LocationFeatureResponseMapper.ToProperties(item);

        // Assert
        dto.Presentation.Status.Should().Be(SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.Treated);
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
        var dto = LocationFeatureResponseMapper.ToProperties(item);

        // Assert
        dto.Presentation.StatusCounts.Should().BeEquivalentTo(new Dictionary<SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto, int>
        {
            [SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.Present] = 1,
            [SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.PresentSensitive] = 1,
            [SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.Treated] = 1,
            [SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.NotRecovered] = 1,
            [SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.NotPresent] = 1
        });
        dto.Presentation.StatusCounts.Keys.Should().Equal(
            SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.Present,
            SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.PresentSensitive,
            SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.Treated,
            SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.NotRecovered,
            SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.NotPresent);
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
        var dto = LocationFeatureResponseMapper.ToProperties(item, cutoffYear: 2024);

        // Assert
        dto.Presentation.StatusCounts.Should().BeEquivalentTo(new Dictionary<SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto, int>
        {
            [SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.Present] = 1,
            [SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.PresentSensitive] = 1,
            [SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.DividedByYearUntil] = 1,
            [SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.Treated] = 1,
            [SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.NotPresent] = 1
        });
        dto.Presentation.StatusCounts.Keys.Should().Equal(
            SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.Present,
            SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.PresentSensitive,
            SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.Treated,
            SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.NotPresent,
            SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.DividedByYearUntil);
    }

    [Fact]
    public void FromLocationFeature_WhenCutoffYearIsUsedAndNoObservationIsPresentButOneIsTreated_SetsTreated()
    {
        // Arrange
        var item = LocationFeature.FromSingleLocation([
            CreateObservation(isPresent: false, isSensitive: false, endYear: 2025, isNotRecovered: true, treatmentId: 0)
        ]);

        // Act
        var dto = LocationFeatureResponseMapper.ToProperties(item, cutoffYear: 2024);

        // Assert
        dto.Presentation.Status.Should().Be(SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.Treated);
        dto.Presentation.CutoffYearStatus.Should().Be(SOS.Api.Contracts.Observations.Responses.CutoffYearStatusDto.AfterNotRecovered);
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
        var dto = LocationFeatureResponseMapper.ToProperties(item, cutoffYear: 2024);

        // Assert
        dto.Presentation.StatusCounts.Should().BeEquivalentTo(new Dictionary<SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto, int>
        {
            [SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.Present] = 1,
            [SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.DividedByYearUntil] = 1,
            [SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.Treated] = 1,
            [SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.NotPresent] = 1
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
        var dto = LocationFeatureResponseMapper.ToProperties(item, cutoffYear: 2024);

        // Assert
        dto.Presentation.Status.Should().Be(SOS.Api.Contracts.Observations.Responses.LocationPresentationStatusDto.Cluster);
        dto.Presentation.CutoffYearStatus.Should().Be(SOS.Api.Contracts.Observations.Responses.CutoffYearStatusDto.After);
        dto.Presentation.IsAllObservationsSensitive.Should().BeFalse();
    }

    [Fact]
    public void ToProperties_PreservesDistinctLocationIdAndOmitsServerOnlyFields()
    {
        var observation = CreateObservation(isPresent: true, isSensitive: false, endYear: 2025);
        observation.Location = CreateLocation("site-1", 18.12345, 59.12345);
        var item = LocationFeature.FromSingleLocation([observation]);

        var properties = LocationFeatureResponseMapper.ToProperties(item, "site-1");
        properties.Locations.Location!.DistinctLocationId.Should().Be("loc_1_5912345_1812345_un");

        var json = JsonSerializer.Serialize(properties, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        json.Should().NotContain("debugObservations").And.NotContain("\"geometry\"");
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
