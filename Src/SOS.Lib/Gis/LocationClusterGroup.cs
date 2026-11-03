using NetTopologySuite.Geometries;
using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Responses;

namespace SOS.Lib.Gis;

/// <summary>Spatial membership and metric centre, without observation summaries or display features.</summary>
public sealed record LocationClusterGroup(
    IReadOnlyList<string> DistinctLocationIds, Point MetricCenter, bool IsCluster,
    LocationGroupingType GroupingType, double? RadiusMeters);

/// <summary>Settings for the meter stage and the mutually exclusive pixel stages.</summary>
public sealed record LocationClusteringOptions
{
    public LatLonBoundingBoxDto? BoundingBox { get; init; }
    public double? MetersPerPixel { get; init; }
    public double ClusterRadiusPx { get; init; }
    public double? ClusterRadiusMeters { get; init; }
    public bool ClusterPixels { get; init; }
    public double DetailClusterRadiusPx { get; init; }
}
