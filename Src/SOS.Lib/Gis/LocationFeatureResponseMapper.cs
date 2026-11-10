using SOS.Api.Contracts.Common.Geo;
using NetTopologySuite.Geometries;
using SOS.Api.Contracts.Observations.Responses;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;

namespace SOS.Lib.Gis;

/// <summary>Maps locations and clusters to GeoJSON feature response properties.</summary>
public static class LocationFeatureResponseMapper
{
    public static AdaptiveSearchLocationFeaturePropertiesDto ToProperties(
        LocationFeature locationFeature,
        int? cutoffYear = null)
    {
        var originalLocations = (locationFeature.OriginalLocations.Count > 0
            ? locationFeature.OriginalLocations
            : locationFeature.Observations.Count > 0
                ? locationFeature.Observations.Select(o => o.Location)
                : locationFeature.Locations)
            .Where(l => !string.IsNullOrWhiteSpace(l?.LocationId))
            .OrderBy(l => l.LocationId, StringComparer.Ordinal)
            .GroupBy(l => l.LocationId, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First()).OrderBy(l => l.LocationId, StringComparer.OrdinalIgnoreCase).ToList();
        var observationSummary = locationFeature.ObservationSummary;
        CutoffYearStatus? cutoffYearStatus = cutoffYear.HasValue && observationSummary != null
            ? CalculateCutoffYearStatus(observationSummary, cutoffYear.Value)
            : cutoffYear.HasValue
                ? CalculateCutoffYearStatus(locationFeature.Observations, cutoffYear.Value)
                : null;
        var isAllObservationsSensitive = observationSummary?.IsAllObservationsSensitive ??
                                         (locationFeature.Observations.Count > 0 &&
                                          locationFeature.Observations.All(observation => observation.Sensitive));
        var locationIds = (locationFeature.LocationIds.Count > 0
            ? locationFeature.LocationIds : originalLocations.Select(l => l.LocationId))
            .Where(id => !string.IsNullOrWhiteSpace(id)).OrderBy(id => id, StringComparer.Ordinal)
            .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(id => id, StringComparer.OrdinalIgnoreCase).ToList();
        var presentationStatus = observationSummary != null
            ? CalculateLocationPresentationStatus(locationFeature.IsCluster, observationSummary, cutoffYear, isAllObservationsSensitive)
            : CalculateLocationPresentationStatus(locationFeature, cutoffYear, isAllObservationsSensitive);
        var presentationStatusCount = observationSummary != null
            ? CalculatePresentationStatusCount(observationSummary, cutoffYear)
            : CalculatePresentationStatusCount(locationFeature.Observations, cutoffYear);

        return new AdaptiveSearchLocationFeaturePropertiesDto
        {
            GroupingType = locationFeature.IsCluster ? locationFeature.GroupingType
                : locationIds.Count > 1 ? LocationGroupingType.CoLocatedLocations : LocationGroupingType.SingleLocation,
            IsSummaryApproximated = locationFeature.IsSummaryApproximated == true,
            IsPresentationStatusApproximated = locationFeature.IsPresentationStatusApproximated,
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
                Status = presentationStatus,
                Generalized = !locationFeature.IsCluster &&
                    locationFeature.Geometry is Polygon or MultiPolygon &&
                    (locationFeature.GeneralizedLocationIds.Count > 0 ||
                     locationFeature.Observations.Any(observation => observation.IsGeneralized)),
                StatusCounts = presentationStatusCount,
                CutoffYearStatus = (CutoffYearStatusDto?)cutoffYearStatus,
                IsAllObservationsSensitive = isAllObservationsSensitive
            },
            Locations = new AdaptiveSearchFeatureLocationsDto
            {
                Count = locationIds.Count,
                Ids = locationIds,
                Items = originalLocations.Take(AdaptiveSearchFeatureLocationsDto.MaxLocationItems)
                    .Select(l => ToLocationDetailsDto(l, l.LocationId,
                        locationFeature.GeneralizedLocationIds.Contains(l.LocationId) ||
                        locationFeature.OriginalLocations.Count == 0 && locationFeature.Observations.Any(o => o.IsGeneralized && string.Equals(o.Location?.LocationId, l.LocationId, StringComparison.OrdinalIgnoreCase)))!).ToList()

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

    private static Dictionary<LocationPresentationStatusDto, int> CalculatePresentationStatusCount(
        LocationObservationSummary observationSummary,
        int? cutoffYear)
    {
        if (observationSummary.PresentationStatusSummaries.Count == 0)
        {
            var fallbackStatus = CalculateLocationPresentationStatus(false, observationSummary, cutoffYear, observationSummary.IsAllObservationsSensitive);
            return observationSummary.ObservationCount > 0
                ? new Dictionary<LocationPresentationStatusDto, int> { [fallbackStatus] = observationSummary.ObservationCount }
                : new Dictionary<LocationPresentationStatusDto, int>();
        }

        var presentationStatusCount = new Dictionary<LocationPresentationStatusDto, int>();
        foreach (var summary in observationSummary.PresentationStatusSummaries)
        {
            AddPresentationStatusCount(
                presentationStatusCount,
                CalculateObservationPresentationStatus(summary, cutoffYear),
                summary.Count);
        }

        return SortPresentationStatusCount(presentationStatusCount);
    }

    private static Dictionary<LocationPresentationStatusDto, int> CalculatePresentationStatusCount(
        IReadOnlyCollection<SOS.Lib.Models.Processed.Observation.Observation> observations,
        int? cutoffYear)
    {
        var presentationStatusCount = new Dictionary<LocationPresentationStatusDto, int>();
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
        Dictionary<LocationPresentationStatusDto, int> presentationStatusCount,
        LocationPresentationStatusDto presentationStatus,
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

    private static Dictionary<LocationPresentationStatusDto, int> SortPresentationStatusCount(
        IReadOnlyDictionary<LocationPresentationStatusDto, int> presentationStatusCount)
    {
        var sortedPresentationStatusCount = new Dictionary<LocationPresentationStatusDto, int>();
        foreach (var item in presentationStatusCount
            .Where(item => item.Value > 0)
            .OrderBy(item => (int)item.Key))
        {
            sortedPresentationStatusCount[item.Key] = item.Value;
        }

        return sortedPresentationStatusCount;
    }

    private static LocationPresentationStatusDto CalculateObservationPresentationStatus(
        LocationObservationSummary.PresentationStatusSummary summary,
        int? cutoffYear)
    {
        if (cutoffYear.HasValue)
        {
            if (summary.IsPresent && summary.EndYear.HasValue && summary.EndYear.Value >= cutoffYear.Value)
            {
                return summary.IsSensitive
                    ? LocationPresentationStatusDto.PresentSensitive
                    : LocationPresentationStatusDto.Present;
            }

            if (summary.IsPresent && summary.EndYear.HasValue && summary.EndYear.Value < cutoffYear.Value)
            {
                return LocationPresentationStatusDto.DividedByYearUntil;
            }

            return summary.IsTreated
                ? LocationPresentationStatusDto.Treated
                : LocationPresentationStatusDto.NotPresent;
        }

        if (summary.IsPresent)
        {
            return summary.IsSensitive
                ? LocationPresentationStatusDto.PresentSensitive
                : LocationPresentationStatusDto.Present;
        }

        if (summary.IsTreated)
        {
            return LocationPresentationStatusDto.Treated;
        }

        return summary.IsNotRecovered
            ? LocationPresentationStatusDto.NotRecovered
            : LocationPresentationStatusDto.NotPresent;
    }

    private static LocationPresentationStatusDto CalculateObservationPresentationStatus(
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
                    ? LocationPresentationStatusDto.PresentSensitive
                    : LocationPresentationStatusDto.Present;
            }

            if (IsPresentObservation(observation) &&
                GetObservationEndYear(observation) is int untilYear &&
                untilYear < cutoffYear.Value)
            {
                return LocationPresentationStatusDto.DividedByYearUntil;
            }

            return IsTreatedObservation(observation)
                ? LocationPresentationStatusDto.Treated
                : LocationPresentationStatusDto.NotPresent;
        }

        if (IsPresentObservation(observation))
        {
            return observation.Sensitive
                ? LocationPresentationStatusDto.PresentSensitive
                : LocationPresentationStatusDto.Present;
        }

        if (IsTreatedObservation(observation))
        {
            return LocationPresentationStatusDto.Treated;
        }

        return IsNotRecoveredObservation(observation)
            ? LocationPresentationStatusDto.NotRecovered
            : LocationPresentationStatusDto.NotPresent;
    }

    private static LocationPresentationStatusDto CalculateLocationPresentationStatus(
        bool isCluster,
        LocationObservationSummary observationSummary,
        int? cutoffYear,
        bool isAllObservationsSensitive)
    {
        if (isCluster)
        {
            return LocationPresentationStatusDto.Cluster;
        }

        if (cutoffYear.HasValue)
        {
            if (IsOnOrAfterCutoff(observationSummary.MaxPresentEndYear, cutoffYear.Value))
            {
                return observationSummary.AllObservationsArePresent && isAllObservationsSensitive
                    ? LocationPresentationStatusDto.PresentSensitive
                    : LocationPresentationStatusDto.Present;
            }

            return IsBeforeCutoff(observationSummary.MinPresentEndYear, cutoffYear.Value)
                ? LocationPresentationStatusDto.DividedByYearUntil
                : observationSummary.TreatedCount > 0
                    ? LocationPresentationStatusDto.Treated
                    : LocationPresentationStatusDto.NotPresent;
        }

        if (observationSummary.FoundCount > 0)
        {
            return observationSummary.AllObservationsArePresent && isAllObservationsSensitive
                ? LocationPresentationStatusDto.PresentSensitive
                : LocationPresentationStatusDto.Present;
        }

        if (observationSummary.TreatedCount > 0)
        {
            return LocationPresentationStatusDto.Treated;
        }

        if (observationSummary.NotRecoveredCount > 0)
        {
            return LocationPresentationStatusDto.NotRecovered;
        }

        return LocationPresentationStatusDto.NotPresent;
    }

    private static LocationPresentationStatusDto CalculateLocationPresentationStatus(
        LocationFeature locationFeature,
        int? cutoffYear,
        bool isAllObservationsSensitive)
    {
        if (locationFeature.IsCluster)
        {
            return LocationPresentationStatusDto.Cluster;
        }

        var observations = locationFeature.Observations;
        if (cutoffYear.HasValue)
        {
            if (observations.Any(observation => IsPresentObservation(observation) &&
                                                IsObservationOnOrAfterCutoffYear(observation, cutoffYear.Value)))
            {
                var allObservationsArePresent = observations.Count > 0 && observations.All(IsPresentObservation);
                return allObservationsArePresent && isAllObservationsSensitive
                    ? LocationPresentationStatusDto.PresentSensitive
                    : LocationPresentationStatusDto.Present;
            }

            return observations.Any(observation => IsPresentObservation(observation) &&
                                                   IsObservationBeforeCutoffYear(observation, cutoffYear.Value))
                ? LocationPresentationStatusDto.DividedByYearUntil
                : observations.Any(IsTreatedObservation)
                    ? LocationPresentationStatusDto.Treated
                    : LocationPresentationStatusDto.NotPresent;
        }

        var hasPresentObservation = observations.Any(IsPresentObservation);
        if (hasPresentObservation)
        {
            var allObservationsArePresent = observations.Count > 0 && observations.All(IsPresentObservation);
            return allObservationsArePresent && isAllObservationsSensitive
                ? LocationPresentationStatusDto.PresentSensitive
                : LocationPresentationStatusDto.Present;
        }

        if (observations.Any(IsTreatedObservation))
        {
            return LocationPresentationStatusDto.Treated;
        }

        if (observations.Any(IsNotRecoveredObservation))
        {
            return LocationPresentationStatusDto.NotRecovered;
        }

        return LocationPresentationStatusDto.NotPresent;
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
            DistinctLocationId = location.DistinctLocationId,
            Sweref99TmX = location.Sweref99TmX,
            Sweref99TmY = location.Sweref99TmY,
            Municipality = location.Municipality?.Name,
            Locality = location.Locality,
            Type = (SOS.Api.Contracts.Observations.Responses.LocationType)location.Type
        };
    }

}
