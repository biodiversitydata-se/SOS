using NetTopologySuite.Geometries;
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

public class LocationClusterItemLocationDto
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
    public static LocationClusterItemLocationDto? FromLocation(SOS.Lib.Models.Processed.Observation.Location? location, string? locationId = null)
    {
        if (location == null)
        {
            return null;
        }

        return new LocationClusterItemLocationDto
        {
            CoordinateUncertaintyInMeters = location.CoordinateUncertaintyInMeters,
            DecimalLatitude = location.DecimalLatitude,
            DecimalLongitude = location.DecimalLongitude,
            LocationId = !string.IsNullOrWhiteSpace(locationId)
                ? locationId
                : !string.IsNullOrWhiteSpace(location.LocationId)
                    ? location.LocationId
                    : location.DistinctLocationId,
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

/// <summary>
/// Represents one item in a location clustering result.
/// </summary>
public class LocationClusterItemDto
{
    /// <summary>
    /// Creates a DTO from a location cluster item.
    /// </summary>
    /// <param name="locationClusterItem">
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
    public static LocationClusterItemDto FromLocationClusterItem(
        LocationClusterItem locationClusterItem,
        string? locationId = null,
        int? cutoffYear = null)
    {
        var location = !locationClusterItem.IsCluster
            ? LocationClusterItemLocationDto.FromLocation(locationClusterItem.Location, locationId)
            : null;
        if (location != null)
        {
            location.IsGeneralized = locationClusterItem.ObservationSummary?.DiffusedCount > 0 ||
                                     locationClusterItem.Observations.Any(observation => observation.IsGeneralized);
        }

        var observationSummary = locationClusterItem.ObservationSummary;
        CutoffStatus? cutoffYearStatus = cutoffYear.HasValue && observationSummary != null
            ? CalculateCutoffYearStatus(observationSummary, cutoffYear.Value)
            : cutoffYear.HasValue
                ? CalculateCutoffYearStatus(locationClusterItem.Observations, cutoffYear.Value)
                : null;
        var isAllObservationsSensitive = observationSummary?.IsAllObservationsSensitive ??
                                         (locationClusterItem.Observations.Count > 0 &&
                                          locationClusterItem.Observations.All(observation => observation.Sensitive));
        var debugObservations = observationSummary != null
            ? observationSummary.DebugObservations
            : CreateDebugObservations(locationClusterItem.Observations, locationClusterItem.ObservationCount);
        var locationIds = locationClusterItem.LocationIds.Count > 0
            ? locationClusterItem.LocationIds
            : null;

        return new LocationClusterItemDto
        {
            IsCluster = locationClusterItem.IsCluster,
            Location = location,
            Geometry = locationClusterItem.Geometry,
            DisplayStatus = observationSummary != null
                ? CalculateLocationDisplayStatus(locationClusterItem.IsCluster, observationSummary, cutoffYear, isAllObservationsSensitive)
                : CalculateLocationDisplayStatus(locationClusterItem, cutoffYear, isAllObservationsSensitive),
            DebugObservations = debugObservations,
            CutoffYearStatus = cutoffYearStatus,
            IsAllObservationsSensitive = isAllObservationsSensitive,
            ActualGeometry = locationClusterItem.ActualGeometry,
            ActualGeometryIsSimplifiedToPoint = locationClusterItem.ActualGeometryIsSimplifiedToPoint,
            ActualGeometrySimplificationReason = locationClusterItem.ActualGeometrySimplificationReason,
            ActualGeometryWidthPx = locationClusterItem.ActualGeometryWidthPx,
            ActualGeometryHeightPx = locationClusterItem.ActualGeometryHeightPx,
            ActualGeometryAreaPx = locationClusterItem.ActualGeometryAreaPx,
            ObservationCount = locationClusterItem.ObservationCount,
            TaxaCount = locationClusterItem.TaxaCount,
            LocationCount = locationClusterItem.LocationCount,
            ClusterRadiusMeters = locationClusterItem.IsCluster ? locationClusterItem.ClusterRadiusMeters : null,
            ClusterRadiusMetersMin = CalculateClusterRadiusMetersMin(locationClusterItem),
            LocationIds = locationIds?.Distinct(StringComparer.OrdinalIgnoreCase).ToList() ??
                          locationClusterItem.Observations
                              .Select(observation => GetLocationId(observation.Location))
                              .Where(locationId => !string.IsNullOrWhiteSpace(locationId))
                              .Distinct(StringComparer.OrdinalIgnoreCase)
                              .ToList(),
            DistinctLocationIds = locationClusterItem.Locations
                .Select(location => location.DistinctLocationId)
                .Distinct(StringComparer.Ordinal)
                .ToList(),
            StatusCounts = locationClusterItem.StatusCounts
        };
    }

    /// <summary>
    /// Indicates whether this result represents a cluster of multiple locations (true) or a single location (false).
    /// When true, the Location and Geometry properties represent the centroid of the cluster, computed as the average
    /// of member latitudes and longitudes. The Locations property contains the list of original location objects that
    /// are grouped into the cluster. When false, the Location and Geometry properties represent the single location,
    /// and the Locations property is null.
    /// </summary>
    public bool IsCluster { get; set; }

    /// <summary>
    /// If IsCluster is false, this property represents the single location associated with the observation(s). For
    /// clusters, this property represents the centroid location of the cluster.
    /// </summary>
    public LocationClusterItemLocationDto? Location { get; set; }

    /// <summary>
    /// The geometry of the location or cluster. For a single location, this is a Point geometry or Polygon geometry.
    /// Clustered locations will use the centroid point as the geometry for the cluster, which is computed as the
    /// average of member latitudes and longitudes.
    /// </summary>
    [JsonIgnore]
    public Geometry? Geometry { get; set; }    

    public LocationDisplayStatus DisplayStatus { get; set; }

    public List<DebugObservation>? DebugObservations { get; set; }

    public CutoffStatus? CutoffYearStatus { get; set; } = null;

    /// <summary>
    /// True, if all observations are sensitive.
    /// </summary>
    public bool? IsAllObservationsSensitive { get; set; }

    /// <summary>
    /// The real location geometry when the feature geometry has been simplified to a point.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Geometry? ActualGeometry { get; set; }

    /// <summary>
    /// Indicates that the real location geometry was simplified to a point in the GeoJSON feature geometry.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ActualGeometryIsSimplifiedToPoint { get; set; }

    /// <summary>
    /// Describes why the real location geometry was simplified.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ActualGeometrySimplificationReason { get; set; }

    /// <summary>
    /// The approximate rendered width of the real location geometry in pixels.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? ActualGeometryWidthPx { get; set; }

    /// <summary>
    /// The approximate rendered height of the real location geometry in pixels.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? ActualGeometryHeightPx { get; set; }

    /// <summary>
    /// The approximate rendered area of the real location geometry in square pixels.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? ActualGeometryAreaPx { get; set; }

    /// <summary>
    /// The number of observations for this item.
    /// </summary>
    public int ObservationCount { get; set; }

    /// <summary>
    /// The number of taxa for this item.
    /// </summary>
    public int TaxaCount { get; set; }

    /// <summary>
    /// Indicates the number of distinct original locations that are
    /// grouped into this cluster. Always 1 when IsCluster is false.    
    /// </summary>
    public int LocationCount { get; set; } = 1;

    /// <summary>
    /// The clustering radius in metres that was used when this cluster was created, when known.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? ClusterRadiusMeters { get; set; }

    /// <summary>
    /// Approximate radius in metres from the cluster point needed to enclose the member locations.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? ClusterRadiusMetersMin { get; set; }


    public List<string> LocationIds { get; set; } = new();

    public List<string> DistinctLocationIds { get; set; } = new();

    ///// <summary>
    ///// The locations represented by this item.
    ///// </summary>
    //public List<LocationClusterItemLocationDto> Locations { get; set; } = new();

    ///// <summary>
    ///// Gets the centroid latitude for this item.
    ///// </summary>
    //public double CentroidLatitude => Locations.Average(l => l.DecimalLatitude!.Value);

    ///// <summary>
    ///// Gets the centroid longitude for this item.
    ///// </summary>
    //public double CentroidLongitude => Locations.Average(l => l.DecimalLongitude!.Value);

    /// <summary>
    /// The counts of observations in different status categories (found, not found, treated, not treated, diffused, not
    /// diffused).
    /// </summary>
    public ObservationStatusCounts StatusCounts { get; set; }

    private static double? CalculateClusterRadiusMetersMin(LocationClusterItem locationClusterItem)
    {
        if (!locationClusterItem.IsCluster || locationClusterItem.Locations.Count == 0)
        {
            return null;
        }

        var center = TryGetSweref99TmPoint(locationClusterItem.Location) ??
                     TryGetSweref99TmPoint(locationClusterItem.Geometry);
        if (center == null)
        {
            return null;
        }

        var maxRadiusMeters = 0.0;
        var hasLocations = false;
        foreach (var location in locationClusterItem.Locations
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

        return !string.IsNullOrWhiteSpace(location.LocationId)
            ? location.LocationId
            : location.DistinctLocationId;
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

    private static LocationDisplayStatus CalculateLocationDisplayStatus(
        bool isCluster,
        LocationObservationSummary observationSummary,
        int? cutoffYear,
        bool isAllObservationsSensitive)
    {
        if (isCluster)
        {
            return LocationDisplayStatus.Cluster;
        }

        if (cutoffYear.HasValue)
        {
            if (IsOnOrAfterCutoff(observationSummary.MaxPresentEndYear, cutoffYear.Value))
            {
                return observationSummary.AllObservationsArePresent && isAllObservationsSensitive
                    ? LocationDisplayStatus.PresentSensitive
                    : LocationDisplayStatus.Present;
            }

            return IsBeforeCutoff(observationSummary.MinPresentEndYear, cutoffYear.Value)
                ? LocationDisplayStatus.DividedByYearUntil
                : observationSummary.TreatedCount > 0
                    ? LocationDisplayStatus.Treated
                    : LocationDisplayStatus.NotPresent;
        }

        if (observationSummary.FoundCount > 0)
        {
            return observationSummary.AllObservationsArePresent && isAllObservationsSensitive
                ? LocationDisplayStatus.PresentSensitive
                : LocationDisplayStatus.Present;
        }

        if (observationSummary.TreatedCount > 0)
        {
            return LocationDisplayStatus.Treated;
        }

        if (observationSummary.NotRecoveredCount > 0)
        {
            return LocationDisplayStatus.NotRecovered;
        }

        return LocationDisplayStatus.NotPresent;
    }

    private static LocationDisplayStatus CalculateLocationDisplayStatus(
        LocationClusterItem locationClusterItem,
        int? cutoffYear,
        bool isAllObservationsSensitive)
    {
        if (locationClusterItem.IsCluster)
        {
            return LocationDisplayStatus.Cluster;
        }

        var observations = locationClusterItem.Observations;
        if (cutoffYear.HasValue)
        {
            if (observations.Any(observation => IsPresentObservation(observation) &&
                                                IsObservationOnOrAfterCutoffYear(observation, cutoffYear.Value)))
            {
                var allObservationsArePresent = observations.Count > 0 && observations.All(IsPresentObservation);
                return allObservationsArePresent && isAllObservationsSensitive
                    ? LocationDisplayStatus.PresentSensitive
                    : LocationDisplayStatus.Present;
            }

            return observations.Any(observation => IsPresentObservation(observation) &&
                                                   IsObservationBeforeCutoffYear(observation, cutoffYear.Value))
                ? LocationDisplayStatus.DividedByYearUntil
                : observations.Any(IsTreatedObservation)
                    ? LocationDisplayStatus.Treated
                    : LocationDisplayStatus.NotPresent;
        }

        var hasPresentObservation = observations.Any(IsPresentObservation);
        if (hasPresentObservation)
        {
            var allObservationsArePresent = observations.Count > 0 && observations.All(IsPresentObservation);
            return allObservationsArePresent && isAllObservationsSensitive
                ? LocationDisplayStatus.PresentSensitive
                : LocationDisplayStatus.Present;
        }

        if (observations.Any(IsTreatedObservation))
        {
            return LocationDisplayStatus.Treated;
        }

        if (observations.Any(IsNotRecoveredObservation))
        {
            return LocationDisplayStatus.NotRecovered;
        }

        return LocationDisplayStatus.NotPresent;
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
   

    private static ObservationStatusCounts CreateStatusCounts(List<Observation> observations)
    {
        int foundCount = 0;
        var diffusedCount = 0;
        var treatedCount = 0;
        var redlistedCount = 0;
        var invasiveInSwedenCount = 0;

        foreach (var observation in observations)
        {
            if (observation.Occurrence?.IsPositiveObservation == true)
            {
                foundCount++;
            }            

            if (observation.IsGeneralized)
            {
                diffusedCount++;
            }

            if ((observation.ArtportalenInternal?.InvasiveSpeciesTreatment?.Id ?? -1) >= 0)
            {
                treatedCount++;
            }

            if (observation.Taxon?.Attributes?.IsRedlisted == true)
            {
                redlistedCount++;
            }

            if (observation.Taxon?.Attributes?.IsInvasiveInSweden == true)
            {
                invasiveInSwedenCount++;
            }
        }

        return new ObservationStatusCounts(
            foundCount,
            treatedCount,
            diffusedCount,
            redlistedCount,
            invasiveInSwedenCount);
    }
}
