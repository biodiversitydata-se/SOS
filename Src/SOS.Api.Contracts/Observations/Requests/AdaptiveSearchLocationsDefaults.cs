namespace SOS.Api.Contracts.Observations.Requests;

/// <summary>Clustering defaults shared by the endpoint, clients and map controls.</summary>
public static class AdaptiveSearchLocationsDefaults
{
    /// <summary>Fixed search radius in pixels for normal clustering when a pixel resolution is supplied.</summary>
    public const double ClusterRadiusPx = 20.0;

    /// <summary>Map-resolution limit (m/px) at or below which normal pixel clustering switches to detail clustering; 0 disables the zoom switch.</summary>
    public const double SwitchToDetailClusteringAtMetersPerPixel = 2.0;

    /// <summary>Fixed search radius in pixels for detail clustering when the zoom or count limit stops normal pixel clustering; 0 disables detail clustering.</summary>
    public const double DetailClusterRadiusPx = 10.0;

    /// <summary>Number of clusterable map items above which normal pixel clustering may start.</summary>
    public const int MinLocationCountBeforeClustering = 0;
}
