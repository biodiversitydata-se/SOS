using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Requests;
using NetTopologySuite.Features;
using SOS.Lib.Gis;
using SOS.Lib.Models.Gis;

namespace SOS.Observations.Api.Services.AdaptiveSearchLocations;

public sealed class AdaptiveSearchLocationsRequest
{
    public CancellationToken CancellationToken { get; init; }
    public int? RoleId { get; init; }
    public string? AuthorizationApplicationIdentifier { get; init; }
    public required SearchFilterInternalDto Filter { get; init; }
    public int UserId { get; init; }
    public LatLonBoundingBoxDto? InputBbox { get; init; }
    public LatLonBoundingBoxDto? Bbox { get; init; }
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
    public int MinLocationsPerCluster{ get; init; }
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
    public required string ResultType { get; init; }
    public required AdaptiveSearchLocationsStrategy AdaptiveSearchStrategy { get; init; }
    public string? GeoJson { get; init; }
    public IReadOnlyList<LocationFeature>? LocationFeatures { get; init; }
    public LatLonBoundingBoxDto? Bbox { get; init; }
    public long ObservationsTotalCount { get; init; }
    public long? EstimatedLocationCount { get; init; }
    public long FeatureCount { get; init; }
    public long? LocationsBeforeClustering { get; init; }
    public int? Zoom { get; init; }
    public required string CacheStatus { get; init; }
    public string? LocationAggregationMode { get; init; }
    public bool LocationSummaryApproximated { get; init; }
    public bool LocationPresentationStatusApproximated { get; init; }
    public List<Feature>? GeographicAreas { get; init; }
    public AdaptiveSearchLocationsTimings Timings { get; init; } = new();
}

internal sealed class CachedLocationAggregate
{
    public required string DistinctLocationId { get; init; }
    public required SOS.Lib.Models.Processed.Observation.Location Location { get; init; } // todo - går det ha en mindre modell med enbart det som är viktigt för endpointen?
    public List<string> LocationIds { get; init; } = new();
    public List<SOS.Lib.Models.Processed.Observation.Location> Locations { get; init; } = new();
    public HashSet<string> GeneralizedLocationIds { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public List<int> TaxonIds { get; init; } = new();
    public required LocationObservationSummary ObservationSummary { get; init; }
    public bool IsSummaryApproximated { get; init; }
    public bool IsPresentationStatusApproximated { get; init; }
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
    public List<CachedLocationAggregate> Locations { get; init; } = new();
    public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;

    public int LocationCount => Locations.Count;
}

internal enum SampleObservationType
{
    Present,
    PresentSensitive,
    Treated,
    NotRecovered,
    NotPresent
}

internal sealed record SampleLocationPriorityCase(
    string LocationName,
    IReadOnlyList<SampleObservationType> ObservationTypes,
    int CoordinateUncertaintyInMeters);
