using SOS.Api.Contracts.Common.Geo;
using NetTopologySuite.Geometries;
using SOS.Api.Contracts.Observations.Responses;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using System.Globalization;

namespace SOS.Lib.Gis;

/// <summary>Maps locations and clusters to GeoJSON feature response properties.</summary>
public static class LocationFeatureResponseMapper
{
    public static AdaptiveSearchLocationFeaturePropertiesDto ToProperties(
        LocationFeature locationFeature,
        string? locationId = null,
        int? cutoffYear = null)
    {
        var location = locationFeature.IsCluster
            ? null
            : ToLocationDetailsDto(locationFeature.Location, locationId,
                locationFeature.ObservationSummary?.DiffusedCount > 0 ||
                locationFeature.Observations.Any(observation => observation.IsGeneralized));

        var observationSummary = locationFeature.ObservationSummary;
        CutoffYearStatus? cutoffYearStatus = cutoffYear.HasValue && observationSummary != null
            ? CalculateCutoffYearStatus(observationSummary, cutoffYear.Value)
            : cutoffYear.HasValue
                ? CalculateCutoffYearStatus(locationFeature.Observations, cutoffYear.Value)
                : null;
        var isAllObservationsSensitive = observationSummary?.IsAllObservationsSensitive ??
                                         (locationFeature.Observations.Count > 0 &&
                                          locationFeature.Observations.All(observation => observation.Sensitive));
        var locationIds = locationFeature.LocationIds.Count > 0
            ? locationFeature.LocationIds
            : null;
        var presentationStatus = observationSummary != null
            ? CalculateLocationPresentationStatus(locationFeature.IsCluster, observationSummary, cutoffYear, isAllObservationsSensitive)
            : CalculateLocationPresentationStatus(locationFeature, cutoffYear, isAllObservationsSensitive);
        var presentationStatusCount = observationSummary != null
            ? CalculatePresentationStatusCount(observationSummary, cutoffYear)
            : CalculatePresentationStatusCount(locationFeature.Observations, cutoffYear);

        return new AdaptiveSearchLocationFeaturePropertiesDto
        {
            IsCluster = locationFeature.IsCluster,
            IsSummaryApproximated = locationFeature.IsSummaryApproximated == true,
            Cluster = locationFeature.IsCluster
                ? new AdaptiveSearchLocationClusterDetailsDto
                {
                    RadiusMeters = locationFeature.ClusterRadiusMeters,
                    RadiusMetersMin = CalculateClusterRadiusMetersMin(locationFeature)
                }
                : null,
            ObservationCount = locationFeature.ObservationCount,
            Presentation = new AdaptiveSearchLocationPresentationDto
            {
                Status = (LocationPresentationStatusDto)presentationStatus,
                StatusCounts = presentationStatusCount.ToDictionary(
                    entry => (LocationPresentationStatusDto)entry.Key,
                    entry => entry.Value),
                CutoffYearStatus = (CutoffYearStatusDto?)cutoffYearStatus,
                IsAllObservationsSensitive = isAllObservationsSensitive
            },
            Locations = new AdaptiveSearchFeatureLocationsDto
            {
                Count = locationFeature.LocationCount,
                Ids = locationIds ??
                      locationFeature.Observations
                          .Select(observation => GetLocationId(observation.Location))
                          .Where(locationId => !string.IsNullOrWhiteSpace(locationId))
                          .Distinct(StringComparer.OrdinalIgnoreCase)
                          .ToList(),
                Location = location
            },
            ObservationSummary = new AdaptiveSearchLocationObservationSummaryDto
            {
                TaxaCount = locationFeature.TaxaCount,
                StatusCounts = locationFeature.StatusCounts is { } counts
                    ? new AdaptiveSearchLocationStatusCounts(
                        counts.FoundCount, counts.TreatedCount, counts.DiffusedCount,
                        counts.RedlistedCount, counts.InvasiveInSwedenCount)
                    : null
            }
        };
    }

    private static double? CalculateClusterRadiusMetersMin(LocationFeature locationFeature)
    {
        if (!locationFeature.IsCluster || locationFeature.Locations.Count == 0)
        {
            return null;
        }

        var center = TryGetSweref99TmPoint(locationFeature.Location) ??
                     TryGetSweref99TmPoint(locationFeature.Geometry);
        if (center == null)
        {
            return null;
        }

        var maxRadiusMeters = 0.0;
        var hasLocations = false;
        foreach (var location in locationFeature.Locations
                     .GroupBy(location => location.DistinctLocationId)
                     .Select(group => group.First()))
        {
            var locationPoint = TryGetSweref99TmPoint(location);
            if (locationPoint == null)
            {
                continue;
            }

            var dx = locationPoint.Value.X - center.Value.X;
            var dy = locationPoint.Value.Y - center.Value.Y;
            var distanceMeters = Math.Sqrt(dx * dx + dy * dy);
            maxRadiusMeters = Math.Max(maxRadiusMeters, distanceMeters);
            hasLocations = true;
        }

        return hasLocations ? Math.Round(maxRadiusMeters, 3) : null;
    }

    private static string? GetLocationId(SOS.Lib.Models.Processed.Observation.Location? location)
    {
        if (location == null)
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(location.LocationId)
            ? null
            : location.LocationId;
    }

    private static Dictionary<LocationPresentationStatus, int> CalculatePresentationStatusCount(
        LocationObservationSummary observationSummary,
        int? cutoffYear)
    {
        if (observationSummary.PresentationStatusSummaries.Count == 0)
        {
            var fallbackStatus = CalculateLocationPresentationStatus(false, observationSummary, cutoffYear, observationSummary.IsAllObservationsSensitive);
            return observationSummary.ObservationCount > 0
                ? new Dictionary<LocationPresentationStatus, int> { [fallbackStatus] = observationSummary.ObservationCount }
                : new Dictionary<LocationPresentationStatus, int>();
        }

        var presentationStatusCount = new Dictionary<LocationPresentationStatus, int>();
        foreach (var summary in observationSummary.PresentationStatusSummaries)
        {
            AddPresentationStatusCount(
                presentationStatusCount,
                CalculateObservationPresentationStatus(summary, cutoffYear),
                summary.Count);
        }

        return SortPresentationStatusCount(presentationStatusCount);
    }

    private static Dictionary<LocationPresentationStatus, int> CalculatePresentationStatusCount(
        IReadOnlyCollection<SOS.Lib.Models.Processed.Observation.Observation> observations,
        int? cutoffYear)
    {
        var presentationStatusCount = new Dictionary<LocationPresentationStatus, int>();
        foreach (var observation in observations)
        {
            AddPresentationStatusCount(
                presentationStatusCount,
                CalculateObservationPresentationStatus(observation, cutoffYear),
                1);
        }

        return SortPresentationStatusCount(presentationStatusCount);
    }

    private static void AddPresentationStatusCount(
        Dictionary<LocationPresentationStatus, int> presentationStatusCount,
        LocationPresentationStatus presentationStatus,
        int count)
    {
        if (count <= 0)
        {
            return;
        }

        presentationStatusCount[presentationStatus] = presentationStatusCount.TryGetValue(presentationStatus, out var existingCount)
            ? existingCount + count
            : count;
    }

    private static Dictionary<LocationPresentationStatus, int> SortPresentationStatusCount(
        IReadOnlyDictionary<LocationPresentationStatus, int> presentationStatusCount)
    {
        var sortedPresentationStatusCount = new Dictionary<LocationPresentationStatus, int>();
        foreach (var item in presentationStatusCount
            .Where(item => item.Value > 0)
            .OrderBy(item => (int)item.Key))
        {
            sortedPresentationStatusCount[item.Key] = item.Value;
        }

        return sortedPresentationStatusCount;
    }

    private static LocationPresentationStatus CalculateObservationPresentationStatus(
        LocationObservationSummary.PresentationStatusSummary summary,
        int? cutoffYear)
    {
        if (cutoffYear.HasValue)
        {
            if (summary.IsPresent && summary.EndYear.HasValue && summary.EndYear.Value >= cutoffYear.Value)
            {
                return summary.IsSensitive
                    ? LocationPresentationStatus.PresentSensitive
                    : LocationPresentationStatus.Present;
            }

            if (summary.IsPresent && summary.EndYear.HasValue && summary.EndYear.Value < cutoffYear.Value)
            {
                return LocationPresentationStatus.DividedByYearUntil;
            }

            return summary.IsTreated
                ? LocationPresentationStatus.Treated
                : LocationPresentationStatus.NotPresent;
        }

        if (summary.IsPresent)
        {
            return summary.IsSensitive
                ? LocationPresentationStatus.PresentSensitive
                : LocationPresentationStatus.Present;
        }

        if (summary.IsTreated)
        {
            return LocationPresentationStatus.Treated;
        }

        return summary.IsNotRecovered
            ? LocationPresentationStatus.NotRecovered
            : LocationPresentationStatus.NotPresent;
    }

    private static LocationPresentationStatus CalculateObservationPresentationStatus(
        SOS.Lib.Models.Processed.Observation.Observation observation,
        int? cutoffYear)
    {
        if (cutoffYear.HasValue)
        {
            if (IsPresentObservation(observation) &&
                GetObservationEndYear(observation) is int endYear &&
                endYear >= cutoffYear.Value)
            {
                return observation.Sensitive
                    ? LocationPresentationStatus.PresentSensitive
                    : LocationPresentationStatus.Present;
            }

            if (IsPresentObservation(observation) &&
                GetObservationEndYear(observation) is int untilYear &&
                untilYear < cutoffYear.Value)
            {
                return LocationPresentationStatus.DividedByYearUntil;
            }

            return IsTreatedObservation(observation)
                ? LocationPresentationStatus.Treated
                : LocationPresentationStatus.NotPresent;
        }

        if (IsPresentObservation(observation))
        {
            return observation.Sensitive
                ? LocationPresentationStatus.PresentSensitive
                : LocationPresentationStatus.Present;
        }

        if (IsTreatedObservation(observation))
        {
            return LocationPresentationStatus.Treated;
        }

        return IsNotRecoveredObservation(observation)
            ? LocationPresentationStatus.NotRecovered
            : LocationPresentationStatus.NotPresent;
    }

    private static LocationPresentationStatus CalculateLocationPresentationStatus(
        bool isCluster,
        LocationObservationSummary observationSummary,
        int? cutoffYear,
        bool isAllObservationsSensitive)
    {
        if (isCluster)
        {
            return LocationPresentationStatus.Cluster;
        }

        if (cutoffYear.HasValue)
        {
            if (IsOnOrAfterCutoff(observationSummary.MaxPresentEndYear, cutoffYear.Value))
            {
                return observationSummary.AllObservationsArePresent && isAllObservationsSensitive
                    ? LocationPresentationStatus.PresentSensitive
                    : LocationPresentationStatus.Present;
            }

            return IsBeforeCutoff(observationSummary.MinPresentEndYear, cutoffYear.Value)
                ? LocationPresentationStatus.DividedByYearUntil
                : observationSummary.TreatedCount > 0
                    ? LocationPresentationStatus.Treated
                    : LocationPresentationStatus.NotPresent;
        }

        if (observationSummary.FoundCount > 0)
        {
            return observationSummary.AllObservationsArePresent && isAllObservationsSensitive
                ? LocationPresentationStatus.PresentSensitive
                : LocationPresentationStatus.Present;
        }

        if (observationSummary.TreatedCount > 0)
        {
            return LocationPresentationStatus.Treated;
        }

        if (observationSummary.NotRecoveredCount > 0)
        {
            return LocationPresentationStatus.NotRecovered;
        }

        return LocationPresentationStatus.NotPresent;
    }

    private static LocationPresentationStatus CalculateLocationPresentationStatus(
        LocationFeature locationFeature,
        int? cutoffYear,
        bool isAllObservationsSensitive)
    {
        if (locationFeature.IsCluster)
        {
            return LocationPresentationStatus.Cluster;
        }

        var observations = locationFeature.Observations;
        if (cutoffYear.HasValue)
        {
            if (observations.Any(observation => IsPresentObservation(observation) &&
                                                IsObservationOnOrAfterCutoffYear(observation, cutoffYear.Value)))
            {
                var allObservationsArePresent = observations.Count > 0 && observations.All(IsPresentObservation);
                return allObservationsArePresent && isAllObservationsSensitive
                    ? LocationPresentationStatus.PresentSensitive
                    : LocationPresentationStatus.Present;
            }

            return observations.Any(observation => IsPresentObservation(observation) &&
                                                   IsObservationBeforeCutoffYear(observation, cutoffYear.Value))
                ? LocationPresentationStatus.DividedByYearUntil
                : observations.Any(IsTreatedObservation)
                    ? LocationPresentationStatus.Treated
                    : LocationPresentationStatus.NotPresent;
        }

        var hasPresentObservation = observations.Any(IsPresentObservation);
        if (hasPresentObservation)
        {
            var allObservationsArePresent = observations.Count > 0 && observations.All(IsPresentObservation);
            return allObservationsArePresent && isAllObservationsSensitive
                ? LocationPresentationStatus.PresentSensitive
                : LocationPresentationStatus.Present;
        }

        if (observations.Any(IsTreatedObservation))
        {
            return LocationPresentationStatus.Treated;
        }

        if (observations.Any(IsNotRecoveredObservation))
        {
            return LocationPresentationStatus.NotRecovered;
        }

        return LocationPresentationStatus.NotPresent;
    }

    private static CutoffYearStatus CalculateCutoffYearStatus(IReadOnlyCollection<SOS.Lib.Models.Processed.Observation.Observation> observations, int cutoffYear)
    {
        if (observations.Any(observation => IsPresentObservation(observation) &&
                                            IsObservationOnOrAfterCutoffYear(observation, cutoffYear)))
        {
            return CutoffYearStatus.After;
        }

        if (observations.Any(observation => IsNotRecoveredObservation(observation) &&
                                            IsObservationOnOrAfterCutoffYear(observation, cutoffYear)))
        {
            return CutoffYearStatus.AfterNotRecovered;
        }

        if (observations.Any(observation => IsPresentObservation(observation) &&
                                            IsObservationBeforeCutoffYear(observation, cutoffYear)))
        {
            return CutoffYearStatus.Until;
        }

        if (observations.Any(observation => IsNotRecoveredObservation(observation) &&
                                            IsObservationBeforeCutoffYear(observation, cutoffYear)))
        {
            return CutoffYearStatus.UntilNotRecovered;
        }

        return CutoffYearStatus.NotPresent;
    }

    private static CutoffYearStatus CalculateCutoffYearStatus(LocationObservationSummary observationSummary, int cutoffYear)
    {
        if (IsOnOrAfterCutoff(observationSummary.MaxPresentEndYear, cutoffYear))
        {
            return CutoffYearStatus.After;
        }

        if (IsOnOrAfterCutoff(observationSummary.MaxNotRecoveredEndYear, cutoffYear))
        {
            return CutoffYearStatus.AfterNotRecovered;
        }

        if (IsBeforeCutoff(observationSummary.MinPresentEndYear, cutoffYear))
        {
            return CutoffYearStatus.Until;
        }

        if (IsBeforeCutoff(observationSummary.MinNotRecoveredEndYear, cutoffYear))
        {
            return CutoffYearStatus.UntilNotRecovered;
        }

        return CutoffYearStatus.NotPresent;
    }

    private static bool IsOnOrAfterCutoff(int? year, int cutoffYear)
    {
        return year.HasValue && year.Value >= cutoffYear;
    }

    private static bool IsBeforeCutoff(int? year, int cutoffYear)
    {
        return year.HasValue && year.Value < cutoffYear;
    }

    private static bool IsPresentObservation(SOS.Lib.Models.Processed.Observation.Observation observation)
    {
        return observation.Occurrence?.IsPositiveObservation == true;
    }

    private static bool IsNotRecoveredObservation(SOS.Lib.Models.Processed.Observation.Observation observation)
    {
        return observation.Occurrence?.IsNotRediscoveredObservation == true;
    }

    private static bool IsTreatedObservation(SOS.Lib.Models.Processed.Observation.Observation observation)
    {
        return observation.ArtportalenInternal?.InvasiveSpeciesTreatment?.Id >= 0;
    }

    private static int? GetObservationEndYear(SOS.Lib.Models.Processed.Observation.Observation observation)
    {
        return observation.Event?.EndYear ?? observation.Event?.StartYear;
    }

    private static bool IsObservationOnOrAfterCutoffYear(SOS.Lib.Models.Processed.Observation.Observation observation, int cutoffYear)
    {
        return GetObservationEndYear(observation) is int endYear && endYear >= cutoffYear;
    }

    private static bool IsObservationBeforeCutoffYear(SOS.Lib.Models.Processed.Observation.Observation observation, int cutoffYear)
    {
        return GetObservationEndYear(observation) is int endYear && endYear < cutoffYear;
    }

    private static MetricPoint? TryGetSweref99TmPoint(SOS.Lib.Models.Processed.Observation.Location? location)
    {
        if (location == null)
        {
            return null;
        }

        if (location.Sweref99TmX.HasValue &&
            location.Sweref99TmY.HasValue &&
            IsFinite(location.Sweref99TmX.Value) &&
            IsFinite(location.Sweref99TmY.Value))
        {
            return new MetricPoint(location.Sweref99TmX.Value, location.Sweref99TmY.Value);
        }

        if (location.DecimalLongitude.HasValue &&
            location.DecimalLatitude.HasValue &&
            IsFinite(location.DecimalLongitude.Value) &&
            IsFinite(location.DecimalLatitude.Value))
        {
            return TryTransformWgs84PointToSweref99Tm(location.DecimalLongitude.Value, location.DecimalLatitude.Value);
        }

        return null;
    }

    private static MetricPoint? TryGetSweref99TmPoint(Geometry? geometry)
    {
        if (geometry is Point point &&
            IsFinite(point.X) &&
            IsFinite(point.Y))
        {
            return TryTransformWgs84PointToSweref99Tm(point.X, point.Y);
        }

        return null;
    }

    private static MetricPoint? TryTransformWgs84PointToSweref99Tm(double longitude, double latitude)
    {
        try
        {
            var sweref99TmPoint = new Point(longitude, latitude)
            {
                SRID = CoordinateSys.WGS84.Srid()
            }.Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM);

            return sweref99TmPoint is Point point && IsFinite(point.X) && IsFinite(point.Y)
                ? new MetricPoint(point.X, point.Y)
                : null;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsFinite(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }

    private readonly struct MetricPoint
    {
        public MetricPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }

        public double Y { get; }
    }

    private static AdaptiveSearchLocationDetailsDto? ToLocationDetailsDto(
        SOS.Lib.Models.Processed.Observation.Location? location, string? locationId, bool isGeneralized)
    {
        if (location is null) return null;

        return new AdaptiveSearchLocationDetailsDto
        {
            CoordinateUncertaintyInMeters = location.CoordinateUncertaintyInMeters,
            IsGeneralized = isGeneralized,
            DecimalLatitude = location.DecimalLatitude,
            DecimalLongitude = location.DecimalLongitude,
            LocationId = !string.IsNullOrWhiteSpace(locationId) ? locationId : location.LocationId,
            DistinctLocationId = CreateResponseDistinctLocationId(location),
            Sweref99TmX = location.Sweref99TmX,
            Sweref99TmY = location.Sweref99TmY,
            Municipality = location.Municipality?.Name,
            Locality = location.Locality,
            Type = (SOS.Api.Contracts.Observations.Responses.LocationType)location.Type
        };
    }

    private static string CreateResponseDistinctLocationId(SOS.Lib.Models.Processed.Observation.Location location)
    {
        static string FormatCoordinate(double? coordinate) => coordinate.HasValue
            ? ((long)Math.Round(coordinate.Value * 100000d, MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture)
            : "x";

        var latitude = FormatCoordinate(location.DecimalLatitude);
        var longitude = FormatCoordinate(location.DecimalLongitude);
        var uncertainty = location.CoordinateUncertaintyInMeters.HasValue
            ? string.Create(CultureInfo.InvariantCulture, $"u{location.CoordinateUncertaintyInMeters.Value}")
            : "un";

        // Preserve the historic response ID, which differs from Location.DistinctLocationId.
        return string.Create(CultureInfo.InvariantCulture, $"loc_{(int)location.Type}_{latitude}_{longitude}_{uncertainty}");
    }
}
