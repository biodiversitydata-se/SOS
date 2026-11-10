namespace SOS.Api.Contracts.Observations.Responses;

/// <summary>GeoJSON properties for a location or location cluster.</summary>
public sealed class AdaptiveSearchLocationFeaturePropertiesDto
{
    /// <summary>
    /// Identifies one location, coincident locations, or a meter/pixel cluster.
    /// Use this field to determine whether the feature is a cluster.
    /// </summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(LocationGroupingTypeJsonConverter))]
    public LocationGroupingType GroupingType { get; init; }

    /// <summary>True when Locations.Count exceeds one, including coincident locations.</summary>
    public bool HasMultipleLocations => Locations.Count > 1;

    /// <summary>Total matching observations, even when summaries use representative observations.</summary>
    public int ObservationCount { get; init; }

    /// <summary>
    /// True when ObservationSummary, Presentation.StatusCounts or Presentation.IsAllObservationsSensitive
    /// may be approximate. Individual values may still be exact; ObservationCount remains the total count.
    /// </summary>
    public bool IsSummaryApproximated { get; init; }

    /// <summary>
    /// True when Presentation.Status or Presentation.CutoffYearStatus may be approximate.
    /// Covers only these two fields; other presentation values are covered by IsSummaryApproximated.
    /// </summary>
    public bool IsPresentationStatusApproximated { get; init; }

    /// <summary>
    /// Metadata for meter and pixel clusters. Use GroupingType to identify the grouping.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public AdaptiveSearchLocationClusterDetailsDto? Cluster { get; init; }

    /// <summary>Map status, generalization information and status counts. The two approximation flags cover different fields.</summary>
    public AdaptiveSearchLocationPresentationDto Presentation { get; init; } = new();

    /// <summary>
    /// The original location(s) grouped into this feature.
    /// </summary>
    public AdaptiveSearchFeatureLocationsDto Locations { get; init; } = new();

    /// <summary>
    /// Taxa and observation-status counts for this feature. These may be approximate when
    /// IsSummaryApproximated is true.
    /// </summary>
    public AdaptiveSearchLocationObservationSummaryDto ObservationSummary { get; init; } = new();
}

/// <summary>Cluster-specific details for a clustered feature.</summary>
public sealed class AdaptiveSearchLocationClusterDetailsDto
{
    /// <summary>The clustering radius in metres that was used when this cluster was created, when known.</summary>
    public double? RadiusMeters { get; init; }

    /// <summary>Approximate radius in metres from the cluster point needed to enclose the member locations.</summary>
    public double? RadiusMetersMin { get; init; }
}

/// <summary>Rendering status and observation-status breakdown for a feature.</summary>
public sealed class AdaptiveSearchLocationPresentationDto
{
    /// <summary>The overall presentation status for this feature. Covered by IsPresentationStatusApproximated.</summary>
    public LocationPresentationStatusDto Status { get; init; }

    /// <summary>
    /// Whether to use the generalized polygon colors when rendering a polygon feature.
    /// True only for non-cluster Polygon or MultiPolygon features with at least one generalized location.
    /// False for points and clusters, even when their member locations are generalized.
    /// Computed from all represented location metadata, independently of the Locations.Items limit.
    /// Does not describe the returned geometry type; use the GeoJSON geometry.type for that.
    /// </summary>
    public bool Generalized { get; init; }

    /// <summary>
    /// Counts per presentation status. With representatives, these may not add up to ObservationCount.
    /// Covered by IsSummaryApproximated.
    /// </summary>
    public Dictionary<LocationPresentationStatusDto, int> StatusCounts { get; init; } = new();

    /// <summary>
    /// The cutoff-year rendering status, when a cutoff year was specified.
    /// Covered by IsPresentationStatusApproximated.
    /// </summary>
    public CutoffYearStatusDto? CutoffYearStatus { get; init; }

    /// <summary>
    /// Whether all observations are sensitive. A public observation guarantees false.
    /// With representatives, true may be approximate when both public and sensitive observations are allowed.
    /// Covered by IsSummaryApproximated.
    /// </summary>
    public bool? IsAllObservationsSensitive { get; init; }
}

/// <summary>Taxa and observation-status counts for a feature.</summary>
public sealed class AdaptiveSearchLocationObservationSummaryDto
{
    /// <summary>The number of taxa for this feature.</summary>
    public int TaxaCount { get; init; }

    /// <summary>The counts of observations in different status categories.</summary>
    public AdaptiveSearchLocationStatusCounts? StatusCounts { get; init; } = new(0, 0, 0, 0, 0);
}

/// <summary>The counts of observations in different status categories.</summary>
/// <param name="FoundCount">The number of observations where the organism is present.</param>
/// <param name="TreatedCount">The number of observations with an invasive-species treatment.</param>
/// <param name="DiffusedCount">The number of geographically generalized observations.</param>
/// <param name="RedlistedCount">The number of observations of redlisted taxa.</param>
/// <param name="InvasiveInSwedenCount">The number of observations of taxa considered invasive in Sweden.</param>
public sealed record AdaptiveSearchLocationStatusCounts(
    int FoundCount,
    int TreatedCount,
    int DiffusedCount,
    int RedlistedCount,
    int InvasiveInSwedenCount);

/// <summary>The original location(s) grouped into a feature.</summary>
public sealed class AdaptiveSearchFeatureLocationsDto
{
    /// <summary>The number of distinct original locations that are grouped into this feature.</summary>
    public int Count { get; init; }

    /// <summary>All non-empty original location ids, distinct and sorted without regard to case. Never truncated.</summary>
    public List<string> Ids { get; init; } = new();

    /// <summary>Maximum number of location details included per feature. Defaults to 5; change this constant and rebuild the API to test larger responses.</summary>
    public const int MaxLocationItems = 100;

    /// <summary>Up to <see cref="MaxLocationItems"/> original locations, in the same id order as Ids. Ids and Count describe all members.</summary>
    public List<AdaptiveSearchLocationDetailsDto> Items { get; init; } = new();

}

/// <summary>A single observation location.</summary>
public sealed class AdaptiveSearchLocationDetailsDto
{
    /// <summary>
    /// The horizontal distance (in meters) from the given coordinate describing the smallest circle
    /// containing the whole of the location.
    /// </summary>
    public int? CoordinateUncertaintyInMeters { get; init; }

    /// <summary>True when the location has been generalized (obfuscated) for one or more observations.</summary>
    public bool? IsGeneralized { get; init; }

    /// <summary>The geographic latitude (in decimal degrees, WGS84) of the location.</summary>
    public double? DecimalLatitude { get; init; }

    /// <summary>The geographic longitude (in decimal degrees, WGS84) of the location.</summary>
    public double? DecimalLongitude { get; init; }

    /// <summary>An identifier for the location.</summary>
    public string? LocationId { get; init; }

    /// <summary>An identifier for the distinct location, derived from its type and coordinates.</summary>
    public string? DistinctLocationId { get; init; }

    /// <summary>X coordinate in SWEREF99 TM.</summary>
    public double? Sweref99TmX { get; init; }

    /// <summary>Y coordinate in SWEREF99 TM.</summary>
    public double? Sweref99TmY { get; init; }

    /// <summary>The municipality name.</summary>
    public string? Municipality { get; init; }

    /// <summary>The locality name.</summary>
    public string? Locality { get; init; }

    /// <summary>The spatial representation used for the location.</summary>
    public LocationType Type { get; init; }
}

/// <summary>The presentation status of a location or cluster.</summary>
public enum LocationPresentationStatusDto
{
    /// <summary>At least one observation is present.</summary>
    Present = 0,

    /// <summary>All observations are present and all are sensitive.</summary>
    PresentSensitive = 1,

    /// <summary>No observations are present, but at least one observation is treated.</summary>
    Treated = 2,

    /// <summary>No observations are present, but at least one observation is not rediscovered.</summary>
    NotRecovered = 3,

    /// <summary>
    /// No observations are present and no observation is not rediscovered, but at least one observation is
    /// not present. This is also used when a cutoff year is used and all observations are not present.
    /// </summary>
    NotPresent = 4,

    /// <summary>At least one present observation before the cutoff year, none on or after it.</summary>
    DividedByYearUntil = 5,

    /// <summary>Multiple locations are clustered together because they are close to each other.</summary>
    Cluster = 6
}

/// <summary>The cutoff-year rendering status of a location or cluster.</summary>
public enum CutoffYearStatusDto
{
    /// <summary>At least one observation is present with an end year equal to or later than the cutoff year.</summary>
    After = 0,

    /// <summary>
    /// Not <see cref="After"/>, but there is at least one not-rediscovered observation with an end year
    /// equal to or later than the cutoff year.
    /// </summary>
    AfterNotRecovered = 2,

    /// <summary>
    /// Not <see cref="After"/> or <see cref="AfterNotRecovered"/>, but there is at least one present
    /// observation with an end year earlier than the cutoff year.
    /// </summary>
    Until = 3,

    /// <summary>
    /// Not <see cref="After"/>, <see cref="AfterNotRecovered"/> or <see cref="Until"/>, but there is at
    /// least one not-rediscovered observation with an end year earlier than the cutoff year.
    /// </summary>
    UntilNotRecovered = 4,

    /// <summary>All observations are not present.</summary>
    NotPresent = 5
}

/// <summary>How the geographic locations represented by a map feature were grouped.</summary>
[System.Text.Json.Serialization.JsonConverter(typeof(LocationGroupingTypeJsonConverter))]
public enum LocationGroupingType
{
    /// <summary>One original location at a geographic position.</summary>
    SingleLocation,

    /// <summary>Multiple original location ids sharing the same distinct geographic location id.</summary>
    CoLocatedLocations,

    /// <summary>At least two geographic units combined by adaptive clustering using a pixel radius.</summary>
    PixelCluster,

    /// <summary>Geographic locations combined using a fixed search radius in meters, before optional pixel clustering.</summary>
    MeterCluster
}

/// <summary>Serializes location grouping types as camelCase strings without changing other enum formats.</summary>
public sealed class LocationGroupingTypeJsonConverter : System.Text.Json.Serialization.JsonStringEnumConverter<LocationGroupingType>
{
    /// <summary>Creates a converter using camelCase names and rejecting integer enum values.</summary>
    public LocationGroupingTypeJsonConverter() : base(System.Text.Json.JsonNamingPolicy.CamelCase, allowIntegerValues: false) { }
}
