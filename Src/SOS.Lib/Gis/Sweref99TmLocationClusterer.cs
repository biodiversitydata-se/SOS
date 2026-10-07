using SOS.Api.Contracts.Common.Geo;
using NetTopologySuite.Geometries;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Gis;

/// <summary>
/// Clusters <see cref="Observation"/> objects greedily after projecting their coordinates to SWEREF99 TM.
/// </summary>
public class Sweref99TmLocationClusterer : ILocationClusterer
{
    private const double DefaultClusterRadiusInPixels = 40.0;
    private const double DefaultFallbackClusterRadiusMeters = 1000.0;
    private const double MinimumClusterRadiusMeters = 1.0;
    private const int DefaultMinLocationsPerCluster = 2;
    private const int MaxNumberOfObservationsToCluster = 10000;

    private readonly int _minLocationCountBeforeClustering;
    private readonly int _maxLocationCountAfterClustering;
    private readonly int _minLocationsPerCluster;
    private int _idOfFirstObservationFailedToCluster = -1;

    /// <summary>
    /// Initializes a new instance of the <see cref="Sweref99TmLocationClusterer"/> class.
    /// </summary>
    public Sweref99TmLocationClusterer()
        : this(100, 400)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Sweref99TmLocationClusterer"/> class.
    /// </summary>
    /// <param name="minLocationCountBeforeClustering">
    /// The distinct-location threshold that controls when clustering starts.
    /// </param>
    /// <param name="maxLocationCountAfterClustering">
    /// The max number of result items after clustering.
    /// </param>
    public Sweref99TmLocationClusterer(int minLocationCountBeforeClustering, int maxLocationCountAfterClustering)
        : this(minLocationCountBeforeClustering, maxLocationCountAfterClustering, DefaultMinLocationsPerCluster)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Sweref99TmLocationClusterer"/> class.
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
    public Sweref99TmLocationClusterer(int minLocationCountBeforeClustering, int maxLocationCountAfterClustering, int minLocationsPerCluster)
    {
        _minLocationCountBeforeClustering = minLocationCountBeforeClustering;
        _maxLocationCountAfterClustering = maxLocationCountAfterClustering;
        _minLocationsPerCluster= Math.Max(2, minLocationsPerCluster);
    }

    /// <inheritdoc />
    public List<LocationFeature> ClusterLocations(List<Observation> observations, LatLonBoundingBoxDto boundingBox, int targetItemCount)
    {
        var clusterRadius = ComputeMinimumClusterRadius(boundingBox, targetItemCount);
        return ClusterLocations(observations, clusterRadius);
    }

    /// <inheritdoc />
    public List<LocationFeature> ClusterLocations(List<Observation> observations, LatLonBoundingBoxDto boundingBox)
    {
        return ClusterLocations(observations, boundingBox, _maxLocationCountAfterClustering);
    }

    /// <inheritdoc />
    public List<LocationFeature> ClusterLocations(List<Observation> observations, LatLonBoundingBoxDto boundingBox, double metersPerPixel, double clusterRadiusPx = DefaultClusterRadiusInPixels)
    {
        var clusterRadius = ComputeMinimumClusterRadius(boundingBox, metersPerPixel, clusterRadiusPx);
        return ClusterLocations(observations, clusterRadius);
    }

    private List<LocationFeature> ClusterLocations(List<Observation> observations, double minimumClusterRadiusMeters)
    {        
        var projectedObservationsToCluster = ProjectObservations(observations);
        var distinctLocationCount = GetDistinctLocationCount(projectedObservationsToCluster);
        var diagonal = ComputeObservationsDiagonal(projectedObservationsToCluster);
        var clusterRadius = Math.Max(diagonal / 100, minimumClusterRadiusMeters);
        var result = new List<LocationFeature>();

        if (distinctLocationCount > _minLocationCountBeforeClustering)
        {
            do
            {
                _idOfFirstObservationFailedToCluster = -1;
                result = MakeClusterItems(projectedObservationsToCluster, clusterRadius);
                clusterRadius *= 1.5;
            }
            while (result.Count > _maxLocationCountAfterClustering);
        }
        else
        {
            result = GroupObservationsByLocation(projectedObservationsToCluster)
                .Select(group => LocationFeature.FromSingleLocation(group.Select(item => item.Observation).ToList()))
                .ToList();
        }

        return result;
    }

    private static int GetDistinctLocationCount(IEnumerable<ProjectedObservation> observations)
    {
        return observations
            .Select(observation => observation.Observation.Location.DistinctLocationId)
            .Distinct()
            .Count();
    }

    private bool HasMinimumDistinctLocationCountForCluster(IEnumerable<ProjectedObservation> observations)
    {
        var distinctLocationIds = new HashSet<string>();

        foreach (var observation in observations)
        {
            distinctLocationIds.Add(observation.Observation.Location.DistinctLocationId);
            if (distinctLocationIds.Count >= _minLocationsPerCluster)
            {
                return true;
            }
        }

        return false;
    }

    private static double ComputeMinimumClusterRadius(LatLonBoundingBoxDto boundingBox, int targetItemCount)
    {
        if (targetItemCount <= 0)
        {
            return DefaultFallbackClusterRadiusMeters;
        }

        var boundsInMeters = GetSweref99TmBounds(boundingBox);
        if (boundsInMeters == null)
        {
            return DefaultFallbackClusterRadiusMeters;
        }

        var width = Math.Abs(boundsInMeters.Width);
        var height = Math.Abs(boundsInMeters.Height);

        if (width <= 0 || height <= 0)
        {
            return DefaultFallbackClusterRadiusMeters;
        }

        var sideLength = Math.Sqrt((width * height) / targetItemCount);
        return Math.Max(sideLength * 0.5, MinimumClusterRadiusMeters);
    }

    private static double ComputeMinimumClusterRadius(LatLonBoundingBoxDto boundingBox, double metersPerPixel, double clusterRadiusPx)
    {
        if (GetSweref99TmBounds(boundingBox) == null || metersPerPixel <= 0)
        {
            return DefaultFallbackClusterRadiusMeters;
        }

        return Math.Max(metersPerPixel * NormalizeClusterRadiusPx(clusterRadiusPx), MinimumClusterRadiusMeters);
    }

    private static double NormalizeClusterRadiusPx(double clusterRadiusPx)
    {
        return clusterRadiusPx > 0 ? clusterRadiusPx : DefaultClusterRadiusInPixels;
    }

    private static double ComputeObservationsDiagonal(List<ProjectedObservation> observations)
    {
        if (observations.Count == 0)
        {
            return 0.0;
        }

        var minX = observations[0].X;
        var maxX = minX;
        var minY = observations[0].Y;
        var maxY = minY;

        for (var index = 1; index < observations.Count; index++)
        {
            var observation = observations[index];

            if (observation.X < minX)
            {
                minX = observation.X;
            }

            if (observation.X > maxX)
            {
                maxX = observation.X;
            }

            if (observation.Y < minY)
            {
                minY = observation.Y;
            }

            if (observation.Y > maxY)
            {
                maxY = observation.Y;
            }
        }

        var xDelta = maxX - minX;
        var yDelta = maxY - minY;

        return Math.Sqrt(xDelta * xDelta + yDelta * yDelta);
    }

    private static Envelope? GetSweref99TmBounds(LatLonBoundingBoxDto boundingBox)
    {
        if (boundingBox?.TopLeft == null || boundingBox.BottomRight == null)
        {
            return null;
        }

        return boundingBox.GetPolygon(CoordinateSys.SWEREF99_TM).EnvelopeInternal;
    }

    private List<LocationFeature> MakeClusterItems(List<ProjectedObservation> observations, double clusterRadius)
    {
        var clusteredItems = new List<LocationFeature>();
        if (observations.Count == 0)
        {
            return clusteredItems;
        }

        var clusters = new List<List<ProjectedObservation>>();
        var remainingObservationsToBeClustered = observations.ToList();
        var clusteringFinished = false;
        var maxIterations = observations.Count;

        while (!clusteringFinished)
        {
            if (NoMoreClustersToBeMade(remainingObservationsToBeClustered))
            {
                clusteringFinished = true;
            }
            else
            {
                var cluster = MakeCluster(remainingObservationsToBeClustered[0], remainingObservationsToBeClustered, clusterRadius);
                if (HasMinimumDistinctLocationCountForCluster(cluster))
                {
                    if (_idOfFirstObservationFailedToCluster != -1 && ClusterContainsObservation(cluster, _idOfFirstObservationFailedToCluster))
                    {
                        _idOfFirstObservationFailedToCluster = -1;
                    }

                    remainingObservationsToBeClustered = RemoveClusteredObservations(cluster, remainingObservationsToBeClustered);
                    clusters.Add(cluster);
                }
                else
                {
                    PutFailedClusterObservationAtTheEnd(remainingObservationsToBeClustered);
                }
            }

            maxIterations--;
            if (maxIterations == 0)
            {
                clusteringFinished = true;
            }
        }

        clusters = MergeClustersWithCloseCentroids(clusters, clusterRadius);
        foreach (var cluster in clusters)
        {
            var clusterItem = LocationFeature.FromCluster(cluster.Select(item => item.Observation).ToList());
            clusterItem.ClusterRadiusMeters = clusterRadius;
            clusteredItems.Add(clusterItem);
        }

        clusteredItems.AddRange(GroupObservationsByLocation(remainingObservationsToBeClustered)
            .Select(group => LocationFeature.FromSingleLocation(group.Select(item => item.Observation).ToList())));
        return clusteredItems;
    }

    private static List<List<ProjectedObservation>> MergeClustersWithCloseCentroids(
        List<List<ProjectedObservation>> clusters,
        double clusterRadius)
    {
        if (clusters.Count < 2 || clusterRadius <= 0)
        {
            return clusters;
        }

        var clusterRadiusSquared = clusterRadius * clusterRadius;
        var merged = true;

        while (merged)
        {
            merged = false;

            for (var firstIndex = 0; firstIndex < clusters.Count; firstIndex++)
            {
                var firstCentroid = GetDistinctLocationCentroid(clusters[firstIndex]);

                for (var secondIndex = firstIndex + 1; secondIndex < clusters.Count; secondIndex++)
                {
                    var secondCentroid = GetDistinctLocationCentroid(clusters[secondIndex]);
                    var xDifference = secondCentroid.X - firstCentroid.X;
                    var yDifference = secondCentroid.Y - firstCentroid.Y;

                    if (xDifference * xDifference + yDifference * yDifference > clusterRadiusSquared)
                    {
                        continue;
                    }

                    clusters[firstIndex].AddRange(clusters[secondIndex]);
                    clusters.RemoveAt(secondIndex);
                    merged = true;
                    break;
                }

                if (merged)
                {
                    break;
                }
            }
        }

        return clusters;
    }

    private static (double X, double Y) GetDistinctLocationCentroid(List<ProjectedObservation> cluster)
    {
        var uniqueLocationIds = new HashSet<string>(StringComparer.Ordinal);
        var x = 0.0;
        var y = 0.0;
        var count = 0;

        foreach (var observation in cluster)
        {
            if (!uniqueLocationIds.Add(observation.Observation.Location.DistinctLocationId))
            {
                continue;
            }

            x += observation.X;
            y += observation.Y;
            count++;
        }

        return (x / count, y / count);
    }

    private static List<List<ProjectedObservation>> GroupObservationsByLocation(List<ProjectedObservation> observations)
    {
        return observations
            .GroupBy(observation => observation.Observation.Location.DistinctLocationId)
            .Select(group => group.ToList())
            .ToList();
    }

    private bool NoMoreClustersToBeMade(List<ProjectedObservation> remainingObservationsToCluster)
    {
        return NotEnoughRemainingObservationsToMakeCluster(remainingObservationsToCluster)
               || HaveTriedToClusterNextObservationBefore(remainingObservationsToCluster);
    }

    private bool NotEnoughRemainingObservationsToMakeCluster(List<ProjectedObservation> remainingObservationsToCluster)
    {
        return !HasMinimumDistinctLocationCountForCluster(remainingObservationsToCluster);
    }

    private bool HaveTriedToClusterNextObservationBefore(List<ProjectedObservation> remainingObservationsToCluster)
    {
        var nextObservationToCluster = remainingObservationsToCluster[0];
        return GetObservationId(nextObservationToCluster.Observation) == _idOfFirstObservationFailedToCluster;
    }

    private void PutFailedClusterObservationAtTheEnd(List<ProjectedObservation> remainingObservationsToBeClustered)
    {
        if (_idOfFirstObservationFailedToCluster == -1)
        {
            _idOfFirstObservationFailedToCluster = GetObservationId(remainingObservationsToBeClustered[0].Observation);
        }

        var failedObservation = remainingObservationsToBeClustered[0];
        remainingObservationsToBeClustered.RemoveAt(0);
        remainingObservationsToBeClustered.Add(failedObservation);
    }

    private static List<ProjectedObservation> MakeCluster(ProjectedObservation clusterObservation, List<ProjectedObservation> observationsToBeClustered, double clusterRadius)
    {
        var clusterRadiusSquared = clusterRadius * clusterRadius;
        var cluster = new List<ProjectedObservation>();

        foreach (var observation in observationsToBeClustered)
        {
            if (IsObservationWithinClusterRadius(clusterObservation.X, clusterObservation.Y, observation, clusterRadiusSquared))
            {
                cluster.Add(observation);
            }
        }

        return cluster;
    }

    private static bool IsObservationWithinClusterRadius(double clusterX, double clusterY, ProjectedObservation observation, double clusterRadiusSquared)
    {
        var xDifference = observation.X - clusterX;
        var yDifference = observation.Y - clusterY;
        return xDifference * xDifference + yDifference * yDifference < clusterRadiusSquared;
    }

    private static bool ClusterContainsObservation(List<ProjectedObservation> cluster, int observationId)
    {
        foreach (var observation in cluster)
        {
            if (GetObservationId(observation.Observation) == observationId)
            {
                return true;
            }
        }

        return false;
    }

    private static List<ProjectedObservation> RemoveClusteredObservations(List<ProjectedObservation> cluster, List<ProjectedObservation> remainingObservationsToBeClustered)
    {
        var clusteredObservationIds = new HashSet<int>(cluster.Count);
        foreach (var clusteredObservation in cluster)
        {
            clusteredObservationIds.Add(GetObservationId(clusteredObservation.Observation));
        }

        var remainingObservations = new List<ProjectedObservation>(remainingObservationsToBeClustered.Count - cluster.Count);
        foreach (var observation in remainingObservationsToBeClustered)
        {
            if (!clusteredObservationIds.Contains(GetObservationId(observation.Observation)))
            {
                remainingObservations.Add(observation);
            }
        }

        return remainingObservations;
    }

    private static List<ProjectedObservation> ProjectObservations(IEnumerable<Observation> observations)
    {
        var projectedObservations = new List<ProjectedObservation>();

        foreach (var observation in observations)
        {
            if (TryProjectObservation(observation, out var projectedObservation))
            {
                projectedObservations.Add(projectedObservation);
            }
        }

        return projectedObservations;
    }

    private static bool TryProjectObservation(Observation observation, out ProjectedObservation projectedObservation)
    {
        projectedObservation = default;

        var location = observation.Location;
        if (location?.DecimalLatitude.HasValue != true || location.DecimalLongitude.HasValue != true)
        {
            return false;
        }

        if (location.Sweref99TmX.HasValue &&
            location.Sweref99TmY.HasValue &&
            IsFinite(location.Sweref99TmX.Value) &&
            IsFinite(location.Sweref99TmY.Value))
        {
            projectedObservation = new ProjectedObservation(observation, location.Sweref99TmX.Value, location.Sweref99TmY.Value);
            return true;
        }

        var wgs84Point = new Point(location.DecimalLongitude.Value, location.DecimalLatitude.Value);
        var sweref99TmPoint = wgs84Point.Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM);
        if (sweref99TmPoint?.Coordinate == null ||
            !IsFinite(sweref99TmPoint.Coordinate.X) ||
            !IsFinite(sweref99TmPoint.Coordinate.Y))
        {
            return false;
        }

        projectedObservation = new ProjectedObservation(
            observation,
            sweref99TmPoint.Coordinate.X,
            sweref99TmPoint.Coordinate.Y);
        return true;
    }

    private static bool IsFinite(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }

    private static int GetObservationId(Observation observation)
    {
        return observation.Occurrence.OccurrenceId.GetHashCode();
    }

    private readonly record struct ProjectedObservation(Observation Observation, double X, double Y);
}
