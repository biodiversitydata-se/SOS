namespace SOS.Observations.Api.Services.AdaptiveSearchLocations;

internal static class AdaptiveSearchLocationsClusteringPolicy
{
    public static double GetDetailClusterRadiusPx(AdaptiveSearchLocationsRequest request) =>
        request.Cluster && request.MetersPerPixel.HasValue ? request.DetailClusterRadiusPx : 0;

    public static bool ShouldClusterPixels(AdaptiveSearchLocationsRequest request) =>
        request.Cluster &&
        (request.SwitchToDetailClusteringAtMetersPerPixel == 0 ||
         !request.MetersPerPixel.HasValue ||
         request.MetersPerPixel.Value > request.SwitchToDetailClusteringAtMetersPerPixel);
}
