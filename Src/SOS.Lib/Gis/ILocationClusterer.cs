using SOS.Api.Contracts.Common.Geo;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Gis;

/// <summary>
/// Clusters observations into map-ready location items.
/// </summary>
public interface ILocationClusterer
{
    /// <summary>
    /// Cluster all observations using a bounding box and target item count to derive the minimum cluster radius.
    /// </summary>
    /// <param name="observations">
    /// The observations to cluster.
    /// </param>
    /// <param name="boundingBox">
    /// The current bounding box.
    /// </param>
    /// <param name="targetItemCount">
    /// Target number of items to fit within the bounding box.
    /// </param>
    /// <returns>
    /// A flat list of clustered and non-clustered location items.
    /// </returns>
    List<LocationFeature> ClusterLocations(List<Observation> observations, LatLonBoundingBoxDto boundingBox, int targetItemCount);

    /// <summary>
    /// Cluster all observations using the configured default target item count for the bounding box.
    /// </summary>
    /// <param name="observations">
    /// The observations to cluster.
    /// </param>
    /// <param name="boundingBox">
    /// The current bounding box.
    /// </param>
    /// <returns>
    /// A flat list of clustered and non-clustered location items.
    /// </returns>
    List<LocationFeature> ClusterLocations(List<Observation> observations, LatLonBoundingBoxDto boundingBox);

    /// <summary>
    /// Cluster all observations using a viewport resolution in metres per pixel.
    /// </summary>
    /// <param name="observations">
    /// The observations to cluster.
    /// </param>
    /// <param name="boundingBox">
    /// The current bounding box.
    /// </param>
    /// <param name="metersPerPixel">
    /// The current viewport resolution in metres per pixel.
    /// </param>
    /// <param name="clusterRadiusPx">
    /// The screen-space cluster radius in pixels.
    /// </param>
    /// <returns>
    /// A flat list of clustered and non-clustered location items.
    /// </returns>
    List<LocationFeature> ClusterLocations(List<Observation> observations, LatLonBoundingBoxDto boundingBox, double metersPerPixel, double clusterRadiusPx = 40.0);
}
