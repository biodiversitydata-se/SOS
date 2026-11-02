namespace SOS.Api.Contracts.Observations.Requests;

/// <summary>Clustering defaults shared by the endpoint, clients and map controls.</summary>
public static class AdaptiveSearchLocationsDefaults
{
    /// <summary>Initial search radius in pixels for normal clustering.</summary>
    public const double ClusterRadiusPx = 20.0;

    /// <summary>Map-resolution limit (m/px) at or below which normal pixel clustering stops.</summary>
    public const double DisablePixelClusteringAtMetersPerPixel = 2.0;

    /// <summary>Fixed search radius in pixels when the zoom or count limit stops normal clustering.</summary>
    public const double ProximityClusterRadiusPx = 10.0;

    /// <summary>Number of clusterable map items above which normal pixel clustering may start.</summary>
    public const int MinLocationCountBeforeClustering = 0;
}
