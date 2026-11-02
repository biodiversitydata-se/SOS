namespace SOS.Observations.Api.Services.AdaptiveSearchLocations;

internal static class AdaptiveSearchLocationsClusteringPolicy
{
    public static double GetProximityClusterRadiusPx(AdaptiveSearchLocationsRequest request) =>
        request.Cluster && request.MetersPerPixel.HasValue ? request.ProximityClusterRadiusPx : 0;

    public static bool ShouldClusterPixels(AdaptiveSearchLocationsRequest request) =>
        request.Cluster &&
        (request.DisablePixelClusteringAtMetersPerPixel == 0 ||
         !request.MetersPerPixel.HasValue ||
         request.MetersPerPixel.Value > request.DisablePixelClusteringAtMetersPerPixel);
}
