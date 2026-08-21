namespace SOS.Api.Contracts.Observations.Responses;

/// <summary>
/// The "properties" payload of one feature (a location or a cluster of locations) in an adaptive search
/// locations GeoJSON response.
/// </summary>
public sealed class AdaptiveSearchLocationFeaturePropertiesDto
{
    /// <summary>
    /// Indicates whether this feature represents a cluster of multiple locations (true) or a single
    /// location (false).
    /// </summary>
    public bool IsCluster { get; init; }

    /// <summary>
    /// The number of observations for this feature.
    /// </summary>
    public int ObservationCount { get; init; }

    /// <summary>
    /// True when this location or, for a cluster, any of its member locations was created from a
    /// representative observation rather than all observations for the location.
    /// </summary>
    public bool IsSummaryApproximated { get; init; }

    /// <summary>
    /// Cluster-specific details. Only present when <see cref="IsCluster"/> is true.
    /// </summary>
    public AdaptiveSearchLocationClusterDetailsDto? Cluster { get; init; }

    /// <summary>
    /// Rendering status and observation-status breakdown for this feature.
    /// </summary>
    public AdaptiveSearchLocationPresentationDto Presentation { get; init; } = new();

    /// <summary>
    /// The original location(s) grouped into this feature.
    /// </summary>
    public AdaptiveSearchFeatureLocationsDto Locations { get; init; } = new();

    /// <summary>
    /// Taxa and observation-status counts for this feature.
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
    /// <summary>The overall presentation status for this feature.</summary>
    public LocationPresentationStatus Status { get; init; }

    /// <summary>The number of observations in each presentation status.</summary>
    public Dictionary<LocationPresentationStatus, int> StatusCounts { get; init; } = new();

    /// <summary>The cutoff-year rendering status, when a cutoff year was specified.</summary>
    public CutoffYearStatus? CutoffYearStatus { get; init; }

    /// <summary>True when all observations for this feature are sensitive.</summary>
    public bool? IsAllObservationsSensitive { get; init; }
}

/// <summary>Taxa and observation-status counts for a feature.</summary>
public sealed class AdaptiveSearchLocationObservationSummaryDto
{
    /// <summary>The number of taxa for this feature.</summary>
    public int TaxaCount { get; init; }

    /// <summary>The counts of observations in different status categories.</summary>
    public AdaptiveSearchLocationStatusCounts StatusCounts { get; init; } = new(0, 0, 0, 0, 0);
}

/// <summary>The counts of observations in different status categories.</summary>
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

    /// <summary>The identifiers of the original locations that are grouped into this feature.</summary>
    public List<string> Ids { get; init; } = new();

    /// <summary>
    /// If <see cref="AdaptiveSearchLocationFeaturePropertiesDto.IsCluster"/> is false, this represents the
    /// single location associated with the observation(s).
    /// </summary>
    public AdaptiveSearchLocationDetailsDto? Location { get; init; }
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
public enum LocationPresentationStatus
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

    /// <summary>
    /// When a cutoff year is used and there are no present observations with an end year equal to or later
    /// than the cutoff year, but there is at least one present observation with an end year before the
    /// cutoff year.
    /// </summary>
    DividedByYearUntil = 5,

    /// <summary>Multiple locations are clustered together because they are close to each other.</summary>
    Cluster = 6
}

/// <summary>The cutoff-year rendering status of a location or cluster.</summary>
public enum CutoffYearStatus
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
