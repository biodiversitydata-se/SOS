using NetTopologySuite.Geometries;
using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Gis;

namespace SOS.Observations.Api.Services.AdaptiveSearchLocations;

internal sealed record LocationFeatureOptions
{
    public LatLonBoundingBoxDto? BoundingBox { get; init; }
    public double? ClusterRadiusMeters { get; init; }
    public bool ClusterPixels { get; init; }
    public double? MetersPerPixel { get; init; }
    public double ClusterRadiusPx { get; init; } = AdaptiveSearchLocationsDefaults.ClusterRadiusPx;
    public int MinLocationsPerCluster { get; init; } = 2;
    public int MinLocationCountBeforeClustering { get; init; }
    public Dictionary<string, Geometry>? PolygonGeometries { get; init; }
    public CreatePolygonsMode CreatePolygons { get; init; }
    public double ProximityClusterRadiusPx { get; init; }
    public bool ShouldCluster => ClusterPixels || ProximityClusterRadiusPx > 0 || ClusterRadiusMeters.HasValue;

    public LocationClusteringOptions Clustering => new()
    {
        BoundingBox = BoundingBox, ClusterRadiusMeters = ClusterRadiusMeters, ClusterPixels = ClusterPixels,
        MetersPerPixel = MetersPerPixel, ClusterRadiusPx = ClusterRadiusPx, ProximityClusterRadiusPx = ProximityClusterRadiusPx
    };
}
