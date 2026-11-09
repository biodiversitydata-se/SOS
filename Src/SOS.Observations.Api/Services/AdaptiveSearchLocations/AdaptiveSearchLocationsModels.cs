using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Requests;
using NetTopologySuite.Features;
using SOS.Lib.Gis;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Search.Result;

namespace SOS.Observations.Api.Services.AdaptiveSearchLocations;

/// <summary>
/// Values of the X-Result-Type header.
/// </summary>
public static class AdaptiveSearchLocationsResultType
{
    public const string Locations = "locations";

    public const string Geogrid = "geogrid";
}

/// <summary>
/// Values of the X-Cache header.
/// </summary>
public static class AdaptiveSearchLocationsCacheStatus
{
    public const string Hit = "HIT";

    public const string Miss = "MISS";

    public const string Bypass = "BYPASS";
}

/// <summary>
/// Values of the X-Location-Aggregation-Mode header.
/// </summary>
public static class AdaptiveSearchLocationsAggregationMode
{
    public const string Representative = "representative";
}

public sealed class AdaptiveSearchLocationsRequest
{
    public CancellationToken CancellationToken { get; init; }
    public int? RoleId { get; init; }
    public string? AuthorizationApplicationIdentifier { get; init; }
    public required SearchFilterInternalDto Filter { get; init; }
    public int UserId { get; init; }

    /// <summary>
    /// The bounding box from the request, before it is clipped by filter initialization.
    /// </summary>
    public LatLonBoundingBoxDto? InputBbox { get; init; }

    public bool ReturnInputBBox { get; init; }
    public bool SensitiveObservations { get; init; }
    public int ObservationsLimit { get; init; }
    public int MaxZoom { get; init; }
    public int MaxGridCells { get; init; }
    public bool Cluster { get; init; }
    public double? ClusterRadiusMeters { get; init; }
    public double? MetersPerPixel { get; init; }
    /// <summary>Switches from normal pixel clustering to detail clustering at or below this ground resolution; 0 disables the zoom switch.</summary>
    public double SwitchToDetailClusteringAtMetersPerPixel { get; init; } = AdaptiveSearchLocationsDefaults.SwitchToDetailClusteringAtMetersPerPixel;
    /// <summary>Fixed pixel search radius when normal pixel clustering is skipped due to the zoom or count limit; 0 disables detail clustering.</summary>
    public double DetailClusterRadiusPx { get; init; } = AdaptiveSearchLocationsDefaults.DetailClusterRadiusPx;
    public double ClusterRadiusPx { get; init; } = AdaptiveSearchLocationsDefaults.ClusterRadiusPx;
    public int MinLocationsPerCluster { get; init; }
    public int MinimumLocationCountBeforeClustering { get; init; } = AdaptiveSearchLocationsDefaults.MinLocationCountBeforeClustering;
    public CreatePolygonsMode CreatePolygons { get; init; }
    public bool UseCache { get; init; }
    public bool CreateSampleObservations { get; init; }
    public int LocationAggregationLimit { get; init; } = 1000;
    public AdaptiveSearchLocationsStrategy AdaptiveSearchStrategy { get; init; } = AdaptiveSearchLocationsStrategy.Auto;
    public int? CutoffYear { get; init; }
    public bool IncludeFilterAreasInResult { get; init; }
    public bool BoundingBoxIsEmpty { get; init; }
}

public sealed class AdaptiveSearchLocationsResult
{
    /// <summary>
    /// One of <see cref="AdaptiveSearchLocationsResultType"/>.
    /// </summary>
    public required string ResultType { get; init; }
    public required AdaptiveSearchLocationsStrategy AdaptiveSearchStrategy { get; init; }

    /// <summary>
    /// Grid cells when <see cref="ResultType"/> is geogrid.
    /// </summary>
    public GeoGridTileResult? GridResult { get; init; }

    /// <summary>
    /// Location features when <see cref="ResultType"/> is locations.
    /// </summary>
    public IReadOnlyList<LocationFeature>? LocationFeatures { get; init; }
    public LatLonBoundingBoxDto? Bbox { get; init; }
    public long ObservationsTotalCount { get; init; }
    public long? EstimatedLocationCount { get; init; }
    public long FeatureCount { get; init; }
    public long? LocationsBeforeClustering { get; init; }
    public int? Zoom { get; init; }

    /// <summary>
    /// One of <see cref="AdaptiveSearchLocationsCacheStatus"/>.
    /// </summary>
    public required string CacheStatus { get; init; }

    /// <summary>
    /// One of <see cref="AdaptiveSearchLocationsAggregationMode"/>, or null when all observations were used.
    /// </summary>
    public string? LocationAggregationMode { get; init; }
    public bool LocationSummaryApproximated { get; init; }
    public bool LocationPresentationStatusApproximated { get; init; }
    public List<Feature>? GeographicAreas { get; init; }
    public AdaptiveSearchLocationsTimings Timings { get; init; } = new();
}

/// <summary>
/// Observations at one geographic position, summarized so that they can be cached and clustered
/// without keeping the individual observations.
/// </summary>
internal sealed class LocationAggregate
{
    /// <summary>
    /// Identifies the geographic position. Coincident location ids share the same value.
    /// </summary>
    public required string DistinctLocationId { get; init; }

    /// <summary>
    /// The representative location (lowest LocationId). The full model is kept because
    /// the clusterer and <see cref="LocationFeature"/> operate on it.
    /// </summary>
    public required SOS.Lib.Models.Processed.Observation.Location Location { get; init; }

    /// <summary>
    /// The distinct original locations at this position, ordered by LocationId.
    /// </summary>
    public required List<SOS.Lib.Models.Processed.Observation.Location> Locations { get; init; }

    public HashSet<string> GeneralizedLocationIds { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public List<int> TaxonIds { get; init; } = new();

    public required LocationObservationSummary ObservationSummary { get; init; }

    public bool IsSummaryApproximated { get; init; }

    public bool IsPresentationStatusApproximated { get; init; }

    public IEnumerable<string> LocationIds => Locations.Select(location => location.LocationId);
}

internal sealed record PolygonGeometryDisplayDecision(
    bool UsePolygon,
    double? WidthPx,
    double? HeightPx,
    double? AreaPx,
    string? SimplificationReason);

internal sealed class AdaptiveSearchLocationsCacheBucket
{
    public List<AdaptiveSearchLocationsCacheEntry> Entries { get; init; } = new();

    public int EstimatedSizeUnits => Math.Max(1, Entries.Sum(entry => entry.LocationCount));
}

internal sealed class AdaptiveSearchLocationsCacheEntry
{
    public LatLonBoundingBoxDto? BoundingBox { get; init; }
    public List<LocationAggregate> Locations { get; init; } = new();
    public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;

    public int LocationCount => Locations.Count;
}
