﻿using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Swagger;
using System.Globalization;
using System.Text.Json.Serialization;
using CutoffStatus = SOS.Lib.Gis.CutoffYearStatus;
using ProcessedLocationType = SOS.Lib.Enums.LocationType;
using static SOS.Lib.Swagger.UseSchema;

namespace SOS.Lib.Gis;

/// <summary>
/// Represents one item in a location clustering result.
/// </summary>
public class LocationFeatureDto
{
    /// <summary>
    /// Indicates whether this result represents a cluster of multiple locations (true) or a single location (false).
    /// When true, the Location and Geometry properties represent the centroid of the cluster, computed as the average
    /// of member latitudes and longitudes. The Locations property contains the list of original location objects that
    /// are grouped into the cluster. When false, the Location and Geometry properties represent the single location,
    /// and the Locations property is null.
    /// </summary>
    public bool IsCluster { get; set; }

    /// <summary>
    /// The number of observations for this location.
    /// </summary>
    public int ObservationCount { get; set; }

    /// <summary>
    /// True when this non-clustered location was created from a representative observation rather than all
    /// observations for the location.
    /// </summary>    
    public bool? IsSummaryApproximated { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ClusterDetailsDto? Cluster { get; set; }

    public PresentationDto Presentation { get; set; } = new();

    public LocationsDto Locations { get; set; } = new();

    public ObservationSummaryDto ObservationSummary { get; set; } = new();

    /// <summary>
    /// The geometry of the location or cluster. For a single location, this is a Point geometry or Polygon geometry.
    /// Clustered locations will use the centroid point as the geometry for the cluster, which is computed as the
    /// average of member latitudes and longitudes.
    /// </summary>
    [JsonIgnore]
    public Geometry? Geometry { get; set; }

    [JsonIgnore]
    public List<DebugObservation>? DebugObservations { get; set; }

    /// <summary>
    /// Creates a DTO from a location cluster item.
    /// </summary>
    /// <param name="locationFeature">
    /// The source location cluster item.
    /// </param>
    /// <param name="locationId">
    /// Optional location id override for the representative location.
    /// </param>
    /// <param name="cutoffYear">
    /// Optional cutoff year used to calculate cutoff-year rendering status.
    /// </param>
    /// <returns>
    /// A location cluster item DTO without observation payload.
    /// </returns>
    public static LocationFeatureDto FromLocationFeature(
        LocationFeature locationFeature,
        string? locationId = null,
        int? cutoffYear = null)
    {
        var location = !locationFeature.IsCluster
            ? LocationDto.FromLocation(locationFeature.Location, locationId)
            : null;
        if (location != null)
        {
            location.IsGeneralized = locationFeature.ObservationSummary?.DiffusedCount > 0 ||
                                     locationFeature.Observations.Any(observation => observation.IsGeneralized);
        }

        var observationSummary = locationFeature.ObservationSummary;
        CutoffStatus? cutoffYearStatus = cutoffYear.HasValue && observationSummary != null
            ? CalculateCutoffYearStatus(observationSummary, cutoffYear.Value)
            : cutoffYear.HasValue
                ? CalculateCutoffYearStatus(locationFeature.Observations, cutoffYear.Value)
                : null;
        var isAllObservationsSensitive = observationSummary?.IsAllObservationsSensitive ??
                                         (locationFeature.Observations.Count > 0 &&
                                          locationFeature.Observations.All(observation => observation.Sensitive));
        var debugObservations = observationSummary != null
            ? observationSummary.DebugObservations
            : CreateDebugObservations(locationFeature.Observations, locationFeature.ObservationCount);
        var locationIds = locationFeature.LocationIds.Count > 0
            ? locationFeature.LocationIds
            : null;
        var presentationStatus = observationSummary != null
            ? CalculateLocationPresentationStatus(locationFeature.IsCluster, observationSummary, cutoffYear, isAllObservationsSensitive)
            : CalculateLocationPresentationStatus(locationFeature, cutoffYear, isAllObservationsSensitive);
        var presentationStatusCount = observationSummary != null
            ? CalculatePresentationStatusCount(observationSummary, cutoffYear)
            : CalculatePresentationStatusCount(locationFeature.Observations, cutoffYear);
        var distinctLocationIds = locationFeature.Locations
            .Select(location => location.DistinctLocationId)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return new LocationFeatureDto
        {
            IsCluster = locationFeature.IsCluster,
            Geometry = locationFeature.Geometry,
            DebugObservations = debugObservations,
            IsSummaryApproximated = !locationFeature.IsCluster &&
                                    locationFeature.IsSummaryApproximated == true
                ? true
                : null,
            Cluster = locationFeature.IsCluster
                ? new ClusterDetailsDto
                {
                    RadiusMeters = locationFeature.ClusterRadiusMeters,
                    RadiusMetersMin = CalculateClusterRadiusMetersMin(locationFeature)
                }
                : null,
            ObservationCount = locationFeature.ObservationCount,
            Presentation = new PresentationDto
            {
                Status = presentationStatus,
                StatusCounts = presentationStatusCount,
                CutoffYearStatus = cutoffYearStatus,
                IsAllObservationsSensitive = isAllObservationsSensitive
            },
            Locations = new LocationsDto
            {
                Count = locationFeature.LocationCount,
                Ids = locationIds?.Distinct(StringComparer.OrdinalIgnoreCase).ToList() ??
                      locationFeature.Observations
                          .Select(observation => GetLocationId(observation.Location))
                          .Where(locationId => !string.IsNullOrWhiteSpace(locationId))
                          .Distinct(StringComparer.OrdinalIgnoreCase)
                          .ToList(),
                //DistinctIds = distinctLocationIds,
                Location = location
            },
            ObservationSummary = new ObservationSummaryDto
            {                
                TaxaCount = locationFeature.TaxaCount,
                StatusCounts = locationFeature.StatusCounts
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

    private static List<DebugObservation>? CreateDebugObservations(
        IReadOnlyCollection<Observation> observations,
        int observationCount)
    {
        if (observationCount > 5)
        {
            return null;
        }

        return observations
            .Select(observation => new DebugObservation
            {
                //OccurrenceId = observation.Occurrence?.OccurrenceId ?? observation.Id,
                EndYear = GetObservationEndYear(observation),
                Diffused = observation.IsGeneralized,
                Sensitive = observation.Sensitive,
                Treatment = observation.ArtportalenInternal?.InvasiveSpeciesTreatment?.Id,
                Positive = observation.Occurrence?.IsPositiveObservation,
                NotRedisc = observation.Occurrence?.IsNotRediscoveredObservation,
                NeverFound = observation.Occurrence?.IsNeverFoundObservation
            })
            .ToList();
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
        IReadOnlyCollection<Observation> observations,
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
        Observation observation,
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

    private static CutoffStatus CalculateCutoffYearStatus(IReadOnlyCollection<Observation> observations, int cutoffYear)
    {
        if (observations.Any(observation => IsPresentObservation(observation) &&
                                            IsObservationOnOrAfterCutoffYear(observation, cutoffYear)))
        {
            return CutoffStatus.After;
        }

        if (observations.Any(observation => IsNotRecoveredObservation(observation) &&
                                            IsObservationOnOrAfterCutoffYear(observation, cutoffYear)))
        {
            return CutoffStatus.AfterNotRecovered;
        }

        if (observations.Any(observation => IsPresentObservation(observation) &&
                                            IsObservationBeforeCutoffYear(observation, cutoffYear)))
        {
            return CutoffStatus.Until;
        }

        if (observations.Any(observation => IsNotRecoveredObservation(observation) &&
                                            IsObservationBeforeCutoffYear(observation, cutoffYear)))
        {
            return CutoffStatus.UntilNotRecovered;
        }

        return CutoffStatus.NotPresent;
    }

    private static CutoffStatus CalculateCutoffYearStatus(LocationObservationSummary observationSummary, int cutoffYear)
    {
        if (IsOnOrAfterCutoff(observationSummary.MaxPresentEndYear, cutoffYear))
        {
            return CutoffStatus.After;
        }

        if (IsOnOrAfterCutoff(observationSummary.MaxNotRecoveredEndYear, cutoffYear))
        {
            return CutoffStatus.AfterNotRecovered;
        }

        if (IsBeforeCutoff(observationSummary.MinPresentEndYear, cutoffYear))
        {
            return CutoffStatus.Until;
        }

        if (IsBeforeCutoff(observationSummary.MinNotRecoveredEndYear, cutoffYear))
        {
            return CutoffStatus.UntilNotRecovered;
        }

        return CutoffStatus.NotPresent;
    }

    private static bool IsOnOrAfterCutoff(int? year, int cutoffYear)
    {
        return year.HasValue && year.Value >= cutoffYear;
    }

    private static bool IsBeforeCutoff(int? year, int cutoffYear)
    {
        return year.HasValue && year.Value < cutoffYear;
    }

    private static bool IsPresentObservation(Observation observation)
    {
        return observation.Occurrence?.IsPositiveObservation == true;
    }

    private static bool IsNotRecoveredObservation(Observation observation)
    {
        return observation.Occurrence?.IsNotRediscoveredObservation == true;
    }

    private static bool IsTreatedObservation(Observation observation)
    {
        return observation.ArtportalenInternal?.InvasiveSpeciesTreatment?.Id >= 0;
    }

    private static int? GetObservationEndYear(Observation observation)
    {
        return observation.Event?.EndYear ?? observation.Event?.StartYear;
    }

    private static bool IsObservationOnOrAfterCutoffYear(Observation observation, int cutoffYear)
    {
        return GetObservationEndYear(observation) is int endYear && endYear >= cutoffYear;
    }

    private static bool IsObservationBeforeCutoffYear(Observation observation, int cutoffYear)
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
   

    public class LocationsDto
    {
        /// <summary>
        /// Indicates the number of distinct original locations that are grouped into this item.
        /// </summary>
        public int Count { get; set; }

        public List<string> Ids { get; set; } = new();

        //public List<string> DistinctIds { get; set; } = new();

        /// <summary>
        /// If IsCluster is false, this property represents the single location associated with the observation(s).
        /// </summary>
        public LocationDto? Location { get; set; }
    }

    public class LocationDto
    {
        /// <summary>
        /// Creates a DTO from a processed observation location.
        /// </summary>
        /// <param name="location">
        /// The source location.
        /// </param>
        /// <param name="locationId">
        /// Optional location id override.
        /// </param>
        /// <returns>
        /// A location DTO.
        /// </returns>
        public static LocationDto? FromLocation(SOS.Lib.Models.Processed.Observation.Location? location, string? locationId = null)
        {
            if (location == null)
            {
                return null;
            }

            return new LocationDto
            {
                CoordinateUncertaintyInMeters = location.CoordinateUncertaintyInMeters,
                DecimalLatitude = location.DecimalLatitude,
                DecimalLongitude = location.DecimalLongitude,
                LocationId = !string.IsNullOrWhiteSpace(locationId)
                    ? locationId
                    : location.LocationId,
                Sweref99TmX = location.Sweref99TmX,
                Sweref99TmY = location.Sweref99TmY,
                Municipality = location.Municipality?.Name,
                Locality = location.Locality,
                Type = location.Type
            };
        }

        /// <summary>
        ///     The horizontal distance (in meters) from the given
        ///     CoordinateX and CoordinateY describing the
        ///     smallest circle containing the whole of the Location.
        ///     Leave the value empty if the uncertainty is unknown, cannot
        ///     be estimated, or is not applicable (because there are
        ///     no coordinates). Zero is not a valid value for this term.
        /// </summary>
        public int? CoordinateUncertaintyInMeters { get; set; }

        public bool? IsGeneralized { get; set; }

        /// <summary>
        ///     The geographic latitude (in decimal degrees, using
        ///     the spatial reference system given in geodeticDatum)
        ///     of the geographic center of a Location. Positive values
        ///     are north of the Equator, negative values are south of it.
        ///     Legal values lie between -90 and 90, inclusive.
        /// </summary>
        public double? DecimalLatitude { get; set; }

        /// <summary>
        ///     The geographic longitude (in decimal degrees, using
        ///     the spatial reference system given in geodeticDatum)
        ///     of the geographic center of a Location. Positive
        ///     values are east of the Greenwich Meridian, negative
        ///     values are west of it. Legal values lie between -180
        ///     and 180, inclusive.
        /// </summary>
        public double? DecimalLongitude { get; set; }

        /// <summary>
        ///     An identifier for the set of location information
        ///     (data associated with dcterms:Location).
        ///     May be a global unique identifier or an identifier
        ///     specific to the data set.
        /// </summary>
        public string LocationId { get; set; }

        /// <summary>
        /// An identifier for the distinct location, based on the combination of location type, decimalLatitude,
        /// decimalLongitude and coordinateUncertaintyInMeters.
        /// </summary>
        //[JsonIgnore]
        public string DistinctLocationId => CreateDistinctLocationId(
            Type,
            DecimalLatitude,
            DecimalLongitude,
            CoordinateUncertaintyInMeters);

        private static string CreateDistinctLocationId(
            ProcessedLocationType type,
            double? decimalLatitude,
            double? decimalLongitude,
            int? coordinateUncertaintyInMeters)
        {
            var latitude = FormatCoordinate(decimalLatitude);
            var longitude = FormatCoordinate(decimalLongitude);
            var uncertainty = coordinateUncertaintyInMeters.HasValue
                ? string.Create(CultureInfo.InvariantCulture, $"u{coordinateUncertaintyInMeters.Value}")
                : "un";

            return string.Create(CultureInfo.InvariantCulture, $"loc_{(int)type}_{latitude}_{longitude}_{uncertainty}");
        }

        private static string FormatCoordinate(double? coordinate)
        {
            if (!coordinate.HasValue)
            {
                return "x";
            }

            var scaledCoordinate = (long)Math.Round(coordinate.Value * 100000d, MidpointRounding.AwayFromZero);
            return scaledCoordinate.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// X coordinate in SWEREF99 TM.
        /// </summary>
        public double? Sweref99TmX { get; set; }

        /// <summary>
        /// Y coordinate in SWEREF99 TM.
        /// </summary>
        public double? Sweref99TmY { get; set; }

        public string? Municipality { get; set; }

        public string? Locality { get; set; }

        /// <summary>
        /// Location type. 1=Point, 2=Polygon.
        /// </summary>        
        [UseSchema(SchemaDataType.Integer, "Location type. 1=Point, 2=Polygon.")]
        public ProcessedLocationType Type { get; set; }
    }

    public class PresentationDto
    {
        public LocationPresentationStatus Status { get; set; }

        public Dictionary<LocationPresentationStatus, int> StatusCounts { get; set; } = new();

        public CutoffStatus? CutoffYearStatus { get; set; } = null;

        /// <summary>
        /// True, if all observations are sensitive.
        /// </summary>
        public bool? IsAllObservationsSensitive { get; set; }
    }

    public class ObservationSummaryDto
    {
        /// <summary>
        /// The number of taxa for this item.
        /// </summary>
        public int TaxaCount { get; set; }

        /// <summary>
        /// The counts of observations in different status categories.
        /// </summary>
        public ObservationStatusCounts StatusCounts { get; set; } = new(0, 0, 0, 0, 0);
    }

    public class ClusterDetailsDto
    {
        /// <summary>
        /// The clustering radius in metres that was used when this cluster was created, when known.
        /// </summary>
        public double? RadiusMeters { get; set; }

        /// <summary>
        /// Approximate radius in metres from the cluster point needed to enclose the member locations.
        /// </summary>
        public double? RadiusMetersMin { get; set; }
    }
}
