using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Gis;
using SOS.Lib.Models.Gis;
using SOS.Shared.Api.Dtos.Filter;
using System.Text.Json.Nodes;

namespace SOS.Observations.Api.Services.AdaptiveSearchLocations;

public sealed class AdaptiveSearchLocationsRequest
{
    public int? RoleId { get; init; }
    public string? AuthorizationApplicationIdentifier { get; init; }
    public required SearchFilterInternalDto Filter { get; init; }
    public int UserId { get; init; }
    public LatLonBoundingBox? InputBbox { get; init; }
    public LatLonBoundingBox? Bbox { get; init; }
    public bool ReturnInputBBox { get; init; }
    public bool SensitiveObservations { get; init; }
    public int ObservationsLimit { get; init; }
    public int MaxZoom { get; init; }
    public int MaxGridCells { get; init; }
    public bool Cluster { get; init; }
    public LocationClusteringAlgorithm ClusteringAlgorithm { get; init; }
    public double? MetersPerPixel { get; init; }
    public double ClusterRadiusPx { get; init; }
    public int MinLocationsPerCluster{ get; init; }
    public int MinimumLocationCountBeforeClustering { get; init; }
    public CreatePolygonsMode CreatePolygons { get; init; }
    public bool UseCache { get; init; }
    public bool CreateSampleObservations { get; init; }
}

public sealed class AdaptiveSearchLocationsResult
{
    public required string ResultType { get; init; }
    public string? GeoJson { get; init; }
    public IReadOnlyList<LocationClusterItem>? LocationClusterItems { get; init; }
    public LatLonBoundingBox? Bbox { get; init; }
    public long ObservationsTotalCount { get; init; }
    public long FeatureCount { get; init; }
    public long? LocationsBeforeClustering { get; init; }
    public int? Zoom { get; init; }
    public required string CacheStatus { get; init; }
    public AdaptiveSearchLocationsTimings Timings { get; init; } = new();
}

internal sealed class CachedLocationAggregate
{
    public required string DistinctLocationId { get; init; }
    public required SOS.Lib.Models.Processed.Observation.Location Location { get; init; } // todo - går det ha en mindre modell med enbart det som är viktigt för endpointen?
    public List<string> LocationIds { get; init; } = new();
    public List<int> TaxonIds { get; init; } = new();
    public required LocationObservationSummary ObservationSummary { get; init; }
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
    public LatLonBoundingBox? BoundingBox { get; init; }
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
