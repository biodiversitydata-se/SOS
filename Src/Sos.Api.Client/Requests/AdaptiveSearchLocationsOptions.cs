using SOS.Api.Contracts.Observations.Requests;

namespace SOS.Api.Client.Requests;
/// <summary>Options for adaptive observation-location search.</summary>
public sealed class AdaptiveSearchLocationsOptions : SosRoleRequestOptions
{
    /// <summary>The observation count above which grid aggregation is returned instead of locations.</summary>
    public int ObservationsLimit { get; set; } = 10000;

    /// <summary>The maximum grid zoom level used when the endpoint returns grid cells.</summary>
    public int MaxZoom { get; set; } = 21;

    /// <summary>The maximum number of grid cells used when the endpoint returns grid cells.</summary>
    public int MaxGridCells { get; set; } = 10000;

    /// <summary>
    /// Controls whether the response contains the input bounding box instead of a bounding box derived
    /// from the result.
    /// </summary>
    public bool ReturnInputBBox { get; set; }

    /// <summary>Controls whether only sensitive observations are searched.</summary>
    public bool SensitiveObservations { get; set; }

    /// <summary>Controls whether locations are clustered when the endpoint returns locations rather than grid cells.</summary>
    public bool Cluster { get; set; } = true;

    /// <summary>The clustering algorithm.</summary>
    public LocationClusteringAlgorithm ClusteringAlgorithm { get; set; } = LocationClusteringAlgorithm.Sweref99TmFastGreedy;

    /// <summary>
    /// The map resolution in ground metres per CSS pixel.
    /// </summary>
    public double? MetersPerPixel { get; set; }

    /// <summary>The cluster radius in pixels.</summary>
    public double ClusterRadiusPx { get; set; } = 40.0;

    /// <summary>The minimum number of distinct locations required for a result item to form a cluster.</summary>
    public int MinLocationsPerCluster { get; set; } = 2;

    /// <summary>
    /// The number of distinct locations that can be returned without clustering. Clustering starts above
    /// this value.
    /// </summary>
    public int MinLocationCountBeforeClustering { get; set; } = 100;

    /// <summary>
    /// The polygon rendering mode: centroid points, polygon geometries, or polygons only when warranted
    /// by the current viewport resolution.
    /// </summary>
    public CreatePolygonsMode CreatePolygons { get; set; } = CreatePolygonsMode.No;

    /// <summary>
    /// A WGS 84 bounding box formatted as
    /// <c>minLongitude,minLatitude,maxLongitude,maxLatitude</c>. It overrides the filter bounding box.
    /// </summary>
    public string? BoundingBoxQuery { get; set; }

    /// <summary>Controls whether areas from the search filter are included in the result.</summary>
    public bool IncludeFilterAreasInResult { get; set; }

    /// <summary>Controls whether server-side cached results may be used.</summary>
    public bool UseCache { get; set; } = true;

    /// <summary>Controls whether sample observations are created for map-symbol debugging.</summary>
    public bool CreateSampleObservations { get; set; }

    /// <summary>
    /// The estimated location-count threshold below which representative location aggregation is used.
    /// </summary>
    public int? LocationAggregationLimit { get; set; }

    /// <summary>The strategy used to choose among observations, representative locations, and grid aggregation.</summary>
    public AdaptiveSearchLocationsStrategy AdaptiveSearchStrategy { get; set; } = AdaptiveSearchLocationsStrategy.ObservationsThenGrid;

    /// <summary>An optional year before and after which observations are rendered differently.</summary>
    public int? CutoffYear { get; set; }

    /// <summary>Creates the endpoint query string.</summary>
    public string ToQueryString()
    {
        return new SosQueryBuilder()
            .Add("observationsLimit", ObservationsLimit)
            .Add("maxZoom", MaxZoom)
            .Add("maxGridCells", MaxGridCells)
            .Add("returnInputBBox", ReturnInputBBox)
            .Add("sensitiveObservations", SensitiveObservations)
            .Add("cluster", Cluster)
            .AddEnum("clusteringAlgorithm", ClusteringAlgorithm)
            .Add("metersPerPixel", MetersPerPixel)
            .Add("clusterRadiusPx", ClusterRadiusPx)
            .Add("minLocationsPerCluster", MinLocationsPerCluster)
            .Add("minLocationCountBeforeClustering", MinLocationCountBeforeClustering)
            .AddEnum("createPolygons", CreatePolygons)
            .Add("bbox", string.IsNullOrWhiteSpace(BoundingBoxQuery) ? null : BoundingBoxQuery)
            .Add("includeFilterAreasInResult", IncludeFilterAreasInResult)
            .Add("useCache", UseCache)
            .Add("createSampleObservations", CreateSampleObservations)
            .Add("locationAggregationLimit", LocationAggregationLimit)
            .AddEnum("adaptiveSearchStrategy", AdaptiveSearchStrategy)
            .Add("cutoffYear", CutoffYear)
            .ToString();
    }
}


