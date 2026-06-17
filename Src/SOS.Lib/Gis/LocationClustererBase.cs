using SOS.Lib.Enums;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Gis;

/// <summary>
/// Base implementation for observation location clusterers that share filtering,
/// threshold handling and radius derivation, but differ in the clustering algorithm.
/// </summary>
public abstract class LocationClustererBase : ILocationClusterer
{
    private const double DefaultClusterRadiusInPixels = 40.0;
    private const int DefaultMinLocationsPerCluster = 2;
    private const int MaxNumberOfObservationsToCluster = 10000;
    private const int MaxClusterRadiusIterations = 40;

    private readonly int _minLocationCountBeforeClustering;
    private readonly int _maxLocationCountAfterClustering;
    private readonly int _minLocationsPerCluster;

    /// <summary>
    /// The default minimum number of distinct locations needed before a result item is rendered as a cluster.
    /// </summary>
    protected const int MinLocationsPerCluster = DefaultMinLocationsPerCluster;

    /// <summary>
    /// Gets the configured minimum number of distinct locations needed before a result item is rendered as a cluster.
    /// </summary>
    protected int MinimalClusterLocationCount => _minLocationsPerCluster;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocationClustererBase"/> class.
    /// </summary>
    protected LocationClustererBase()
        : this(100, 400)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocationClustererBase"/> class.
    /// </summary>
    /// <param name="minLocationCountBeforeClustering">
    /// The distinct-location threshold that controls when clustering starts.
    /// </param>
    /// <param name="maxLocationCountAfterClustering">
    /// The max number of result items after clustering.
    /// </param>
    protected LocationClustererBase(int minLocationCountBeforeClustering, int maxLocationCountAfterClustering)
        : this(minLocationCountBeforeClustering, maxLocationCountAfterClustering, DefaultMinLocationsPerCluster)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocationClustererBase"/> class.
    /// </summary>
    /// <param name="minLocationCountBeforeClustering">
    /// The distinct-location threshold that controls when clustering starts.
    /// </param>
    /// <param name="maxLocationCountAfterClustering">
    /// The max number of result items after clustering.
    /// </param>
    /// <param name="minLocationsPerCluster">
    /// The minimum number of distinct locations required for a cluster item.
    /// </param>
    protected LocationClustererBase(int minLocationCountBeforeClustering, int maxLocationCountAfterClustering, int minLocationsPerCluster)
    {
        _minLocationCountBeforeClustering = minLocationCountBeforeClustering;
        _maxLocationCountAfterClustering = maxLocationCountAfterClustering;
        _minLocationsPerCluster= Math.Max(2, minLocationsPerCluster);
    }

    /// <inheritdoc />
    public List<LocationFeature> ClusterLocations(List<Observation> observations, LatLonBoundingBox boundingBox, int targetItemCount)
    {
        var clusterRadius = ComputeMinimumClusterRadius(boundingBox, targetItemCount);
        return ClusterLocations(observations, clusterRadius);
    }

    /// <inheritdoc />
    public List<LocationFeature> ClusterLocations(List<Observation> observations, LatLonBoundingBox boundingBox)
    {
        return ClusterLocations(observations, boundingBox, _maxLocationCountAfterClustering);
    }

    /// <inheritdoc />
    public List<LocationFeature> ClusterLocations(List<Observation> observations, LatLonBoundingBox boundingBox, double metersPerPixel, double clusterRadiusPx = DefaultClusterRadiusInPixels)
    {
        var clusterRadius = ComputeMinimumClusterRadius(boundingBox, metersPerPixel, clusterRadiusPx);
        return ClusterLocations(observations, clusterRadius);
    }

    /// <summary>
    /// Creates cluster items for observations that have already been filtered and selected for clustering.
    /// </summary>
    /// <param name="observations">
    /// Observations with valid coordinates.
    /// </param>
    /// <param name="clusterRadius">
    /// The current cluster radius in WGS84 degrees.
    /// </param>
    /// <returns>
    /// A flat list of clustered and non-clustered location items.
    /// </returns>
    protected abstract List<LocationFeature> MakeClusterItems(List<Observation> observations, double clusterRadius);

    /// <summary>
    /// Groups remaining observations by distinct location and creates non-clustered location items.
    /// </summary>
    /// <param name="observations">
    /// Observations to group.
    /// </param>
    /// <returns>
    /// Non-clustered location items.
    /// </returns>
    protected static IEnumerable<LocationFeature> CreateSingleLocationItems(IEnumerable<Observation> observations)
    {
        return GroupObservationsByLocation(observations)
            .Select(LocationFeature.FromSingleLocation);
    }

    /// <summary>
    /// Gets the number of distinct locations represented by the supplied observations.
    /// </summary>
    /// <param name="observations">
    /// Observations to inspect.
    /// </param>
    /// <returns>
    /// Number of distinct locations.
    /// </returns>
    protected static int GetClusterLocationCount(IEnumerable<Observation> observations)
    {
        return observations
            .Select(observation => observation.Location.DistinctLocationId)
            .Distinct()
            .Count();
    }

    /// <summary>
    /// Converts a radius in WGS84 degrees to an approximate radius in metres at the observations' mean latitude.
    /// </summary>
    /// <param name="degrees">
    /// Radius in WGS84 degrees.
    /// </param>
    /// <param name="observations">
    /// Observations used to derive mean latitude.
    /// </param>
    /// <returns>
    /// Approximate radius in metres.
    /// </returns>
    protected static double DegreesToMeters(double degrees, IReadOnlyCollection<Observation> observations)
    {
        if (degrees <= 0 || observations.Count == 0)
        {
            return 0.0;
        }

        var meanLatitude = observations.Average(o => o.Location.DecimalLatitude!.Value);
        const double metersPerLatitudeDegree = 110540.0;
        var metersPerLongitudeDegree = 111320.0 * Math.Cos(meanLatitude * Math.PI / 180.0);
        var metersPerDegree = Math.Max(metersPerLatitudeDegree, Math.Abs(metersPerLongitudeDegree));

        return degrees * metersPerDegree;
    }

    private List<LocationFeature> ClusterLocations(List<Observation> observations, double minimumClusterRadius)
    {        
        var distinctLocationCount = GetDistinctLocationCount(observations);

        if (distinctLocationCount <= _minLocationCountBeforeClustering)
        {
            return CreateSingleLocationItems(observations).ToList();
        }

        var diagonal = ComputeObservationsDiagonal(observations);
        var clusterRadius = Math.Max(diagonal / 100, minimumClusterRadius);
        var result = new List<LocationFeature>();
        var iterationCount = 0;

        do
        {
            result = MakeClusterItems(observations, clusterRadius);
            clusterRadius *= 1.5;
            iterationCount++;
        }
        while (result.Count > _maxLocationCountAfterClustering && iterationCount < MaxClusterRadiusIterations);

        return result;
    }

    private static int GetDistinctLocationCount(List<Observation> observations)
    {
        return observations
            .Select(observation => observation.Location.DistinctLocationId)
            .Distinct()
            .Count();
    }

    private static double ComputeMinimumClusterRadius(LatLonBoundingBox boundingBox, int targetItemCount)
    {
        if (boundingBox?.TopLeft == null || boundingBox.BottomRight == null || targetItemCount <= 0)
        {
            return 0.01;
        }

        var width = Math.Abs(boundingBox.BottomRight.Longitude - boundingBox.TopLeft.Longitude);
        var height = Math.Abs(boundingBox.TopLeft.Latitude - boundingBox.BottomRight.Latitude);

        if (width <= 0 || height <= 0)
        {
            return 0.01;
        }

        var sideLength = Math.Sqrt((width * height) / targetItemCount);
        return Math.Max(sideLength * 0.5, 0.00001);
    }

    private static double ComputeMinimumClusterRadius(LatLonBoundingBox boundingBox, double metersPerPixel, double clusterRadiusPx)
    {
        if (boundingBox?.TopLeft == null || boundingBox.BottomRight == null || metersPerPixel <= 0)
        {
            return 0.01;
        }

        var widthDegrees = Math.Abs(boundingBox.BottomRight.Longitude - boundingBox.TopLeft.Longitude);
        var heightDegrees = Math.Abs(boundingBox.TopLeft.Latitude - boundingBox.BottomRight.Latitude);
        var boundsInMeters = boundingBox.GetPolygon(CoordinateSys.WebMercator).EnvelopeInternal;
        var widthMeters = Math.Abs(boundsInMeters.Width);
        var heightMeters = Math.Abs(boundsInMeters.Height);

        if (widthDegrees <= 0 || heightDegrees <= 0 || widthMeters <= 0 || heightMeters <= 0)
        {
            return 0.01;
        }

        var degreesPerMeter = Math.Max(widthDegrees / widthMeters, heightDegrees / heightMeters);
        var clusterRadiusMeters = metersPerPixel * NormalizeClusterRadiusPx(clusterRadiusPx);
        return Math.Max(clusterRadiusMeters * degreesPerMeter, 0.00001);
    }

    private static double NormalizeClusterRadiusPx(double clusterRadiusPx)
    {
        return clusterRadiusPx > 0 ? clusterRadiusPx : DefaultClusterRadiusInPixels;
    }

    private static double ComputeObservationsDiagonal(List<Observation> observations)
    {
        if (observations.Count == 0)
        {
            return 0.0;
        }

        var firstLocation = observations[0].Location;
        var minLongitude = firstLocation.DecimalLongitude!.Value;
        var maxLongitude = minLongitude;
        var minLatitude = firstLocation.DecimalLatitude!.Value;
        var maxLatitude = minLatitude;

        for (var index = 1; index < observations.Count; index++)
        {
            var location = observations[index].Location;
            var longitude = location.DecimalLongitude!.Value;
            var latitude = location.DecimalLatitude!.Value;

            if (longitude < minLongitude)
            {
                minLongitude = longitude;
            }

            if (longitude > maxLongitude)
            {
                maxLongitude = longitude;
            }

            if (latitude < minLatitude)
            {
                minLatitude = latitude;
            }

            if (latitude > maxLatitude)
            {
                maxLatitude = latitude;
            }
        }

        var longitudeDelta = maxLongitude - minLongitude;
        var latitudeDelta = maxLatitude - minLatitude;

        return Math.Sqrt(longitudeDelta * longitudeDelta + latitudeDelta * latitudeDelta);
    }

    private static List<List<Observation>> GroupObservationsByLocation(IEnumerable<Observation> observations)
    {
        return observations
            .GroupBy(observation => observation.Location.DistinctLocationId)
            .Select(group => group.ToList())
            .ToList();
    }
}
