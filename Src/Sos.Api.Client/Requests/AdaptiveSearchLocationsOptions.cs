using SOS.Api.Contracts.Observations.Requests;

namespace SOS.Api.Client.Requests;
/// <summary>Options for adaptive observation-location search.</summary>
public sealed class AdaptiveSearchLocationsOptions : SosRoleRequestOptions
{
    /// <summary>Maximum observation count for fetching all observations instead of using representatives or grid cells.</summary>
    public int ObservationsLimit { get; set; } = 10000;

    /// <summary>The maximum grid zoom level used when the endpoint returns grid cells.</summary>
    public int MaxZoom { get; set; } = 21;

    /// <summary>The maximum number of grid cells used when the endpoint returns grid cells.</summary>
    public int MaxGridCells { get; set; } = 10000;

    /// <summary>Return the input bounding box instead of the result's extent.</summary>
    public bool ReturnInputBBox { get; set; }

    /// <summary>Used when the filter's ProtectionFilter is unset: true selects sensitive observations, false selects public observations.</summary>
    public bool SensitiveObservations { get; set; }

    /// <summary>Enable normal or detail pixel clustering.</summary>
    public bool Cluster { get; set; } = true;

    /// <summary>Optional fixed search radius in metres, applied before pixel clustering even when Cluster is false.</summary>
    public double? ClusterRadiusMeters { get; set; }

    /// <summary>
    /// Map resolution in ground metres per CSS pixel. Without it, normal clustering uses the bounding box
    /// and result extent and may grow its radius toward 400 map items; the zoom switch and detail clustering are unused.
    /// </summary>
    public double? MetersPerPixel { get; set; }

    /// <summary>
    /// Switches from normal pixel clustering to detail clustering when MetersPerPixel is at or below this limit (m/px).
    /// DetailClusterRadiusPx controls the fixed search radius. 0 disables the zoom switch; ignored without MetersPerPixel.
    /// </summary>
    public double SwitchToDetailClusteringAtMetersPerPixel { get; set; } = AdaptiveSearchLocationsDefaults.SwitchToDetailClusteringAtMetersPerPixel;

    /// <summary>
    /// Fixed search radius in pixels for detail clustering when the zoom or count limit stops normal pixel clustering.
    /// Requires Cluster and MetersPerPixel. Range 0–500; 0 disables detail clustering.
    /// </summary>
    public double DetailClusterRadiusPx { get; set; } = AdaptiveSearchLocationsDefaults.DetailClusterRadiusPx;

    /// <summary>Fixed search radius in pixels for normal clustering when MetersPerPixel is supplied. The result may exceed 400 map items. This is a search radius, not a maximum cluster extent.</summary>
    public double ClusterRadiusPx { get; set; } = AdaptiveSearchLocationsDefaults.ClusterRadiusPx;

    /// <summary>Minimum number of geographic groups in a cluster. Coincident location ids count as one group.</summary>
    public int MinLocationsPerCluster { get; set; } = 2;

    /// <summary>
    /// Normal pixel clustering starts above this number of clusterable map items.
    /// Detail clustering may still run at or below the limit.
    /// </summary>
    public int MinLocationCountBeforeClustering { get; set; } = AdaptiveSearchLocationsDefaults.MinLocationCountBeforeClustering;

    /// <summary>How polygon locations are returned. Defaults to No; WhenNeeded requires MetersPerPixel.</summary>
    public CreatePolygonsMode CreatePolygons { get; set; } = CreatePolygonsMode.No;

    /// <summary>
    /// A WGS 84 bounding box formatted as
    /// <c>minLongitude,minLatitude,maxLongitude,maxLatitude</c>. It overrides the filter bounding box.
    /// </summary>
    public string? BoundingBoxQuery { get; set; }

    /// <summary>Controls whether filter geometries are embedded as features. The GeoJSON body's filterAreaReferences are included independently of this option.</summary>
    public bool IncludeFilterAreasInResult { get; set; }

    /// <summary>Controls whether server-side cached results may be used.</summary>
    public bool UseCache { get; set; } = true;

    /// <summary>Controls whether sample observations are created for map-symbol debugging.</summary>
    public bool CreateSampleObservations { get; set; }

    /// <summary>
    /// The location-count threshold below which representative location aggregation is used. Defaults to 1000; range 1–10000.
    /// </summary>
    public int LocationAggregationLimit { get; set; } = 1000;

    /// <summary>The strategy used to choose among observations, representative locations, and grid aggregation.</summary>
    public AdaptiveSearchLocationsStrategy AdaptiveSearchStrategy { get; set; } = AdaptiveSearchLocationsStrategy.Auto;

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
            .Add("clusterRadiusMeters", ClusterRadiusMeters)
            .Add("metersPerPixel", MetersPerPixel)
            .Add("switchToDetailClusteringAtMetersPerPixel", SwitchToDetailClusteringAtMetersPerPixel)
            .Add("detailClusterRadiusPx", DetailClusterRadiusPx)
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


