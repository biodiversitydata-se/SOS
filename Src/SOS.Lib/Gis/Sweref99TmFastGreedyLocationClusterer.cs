using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Gis;

/// <summary>
/// Clusters observations greedily in SWEREF99 TM using a spatial grid index.
/// </summary>
public class Sweref99TmFastGreedyLocationClusterer : ILocationClusterer
{
    private const double DefaultClusterRadiusInPixels = 40.0;
    private const double DefaultFallbackClusterRadiusMeters = 1000.0;
    private const double MinimumClusterRadiusMeters = 1.0;
    private const int DefaultMinLocationsPerCluster = 2;
    private const int MaxNumberOfObservationsToCluster = 10000;
    private const int MaxClusterRadiusIterations = 40;

    private readonly int _minLocationCountBeforeClustering;
    private readonly int _maxLocationCountAfterClustering;
    private readonly int _minLocationsPerCluster;

    /// <summary>
    /// Initializes a new instance of the <see cref="Sweref99TmFastGreedyLocationClusterer"/> class.
    /// </summary>
    public Sweref99TmFastGreedyLocationClusterer()
        : this(100, 400)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Sweref99TmFastGreedyLocationClusterer"/> class.
    /// </summary>
    /// <param name="minLocationCountBeforeClustering">
    /// The distinct-location threshold that controls when clustering starts.
    /// </param>
    /// <param name="maxLocationCountAfterClustering">
    /// The max number of result items after clustering.
    /// </param>
    public Sweref99TmFastGreedyLocationClusterer(int minLocationCountBeforeClustering, int maxLocationCountAfterClustering)
        : this(minLocationCountBeforeClustering, maxLocationCountAfterClustering, DefaultMinLocationsPerCluster)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Sweref99TmFastGreedyLocationClusterer"/> class.
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
    public Sweref99TmFastGreedyLocationClusterer(int minLocationCountBeforeClustering, int maxLocationCountAfterClustering, int minLocationsPerCluster)
    {
        _minLocationCountBeforeClustering = minLocationCountBeforeClustering;
        _maxLocationCountAfterClustering = maxLocationCountAfterClustering;
        _minLocationsPerCluster= Math.Max(2, minLocationsPerCluster);
    }

    /// <inheritdoc />
    public List<LocationClusterItem> ClusterLocations(List<Observation> observations, LatLonBoundingBox boundingBox, int targetItemCount)
    {
        var clusterRadius = ComputeMinimumClusterRadius(boundingBox, targetItemCount);
        return ClusterLocations(observations, clusterRadius);
    }

    /// <inheritdoc />
    public List<LocationClusterItem> ClusterLocations(List<Observation> observations, LatLonBoundingBox boundingBox)
    {
        return ClusterLocations(observations, boundingBox, _maxLocationCountAfterClustering);
    }

    /// <inheritdoc />
    public List<LocationClusterItem> ClusterLocations(List<Observation> observations, LatLonBoundingBox boundingBox, double metersPerPixel, double clusterRadiusPx = DefaultClusterRadiusInPixels)
    {
        var clusterRadius = ComputeMinimumClusterRadius(boundingBox, metersPerPixel, clusterRadiusPx);
        return ClusterLocations(observations, clusterRadius);
    }

    private List<LocationClusterItem> ClusterLocations(List<Observation> observations, double minimumClusterRadiusMeters)
    {        
        var projectedLocations = ProjectLocations(observations);
        if (projectedLocations.Count == 0)
        {
            return new List<LocationClusterItem>();
        }

        if (projectedLocations.Count <= _minLocationCountBeforeClustering)
        {
            return CreateSingleLocationItems(projectedLocations);
        }

        var diagonal = ComputeLocationsDiagonal(projectedLocations);
        var clusterRadius = Math.Max(diagonal / 100, minimumClusterRadiusMeters);
        var result = new List<LocationClusterItem>();
        var iterationCount = 0;

        do
        {
            result = MakeClusterItems(projectedLocations, clusterRadius);
            clusterRadius *= 1.5;
            iterationCount++;
        }
        while (result.Count > _maxLocationCountAfterClustering && iterationCount < MaxClusterRadiusIterations);

        return result;
    }

    private List<LocationClusterItem> MakeClusterItems(List<ProjectedLocation> locations, double clusterRadius)
    {
        if (locations.Count == 0)
        {
            return new List<LocationClusterItem>();
        }

        if (clusterRadius <= 0)
        {
            return CreateSingleLocationItems(locations);
        }

        var buckets = BuildBuckets(locations, clusterRadius);
        var clustered = new bool[locations.Count];
        var processedAnchors = new bool[locations.Count];
        var clusters = new List<List<ProjectedLocation>>();
        var clusterRadiusSquared = clusterRadius * clusterRadius;

        for (var index = 0; index < locations.Count; index++)
        {
            if (clustered[index] || processedAnchors[index])
            {
                continue;
            }

            var anchor = locations[index];
            var candidateIndexes = GetCandidateIndexes(buckets, anchor.X, anchor.Y, clusterRadius);
            var cluster = new List<int>();

            foreach (var candidateIndex in candidateIndexes)
            {
                if (clustered[candidateIndex])
                {
                    continue;
                }

                var candidate = locations[candidateIndex];
                var xDifference = candidate.X - anchor.X;
                var yDifference = candidate.Y - anchor.Y;
                if (xDifference * xDifference + yDifference * yDifference < clusterRadiusSquared)
                {
                    cluster.Add(candidateIndex);
                }
            }

            if (cluster.Count >= _minLocationsPerCluster)
            {
                var clusterLocations = new List<ProjectedLocation>(cluster.Count);
                foreach (var clusterIndex in cluster)
                {
                    clustered[clusterIndex] = true;
                    clusterLocations.Add(locations[clusterIndex]);
                }

                clusters.Add(clusterLocations);
            }
            else
            {
                processedAnchors[index] = true;
            }
        }

        clusters = MergeClustersWithCloseCentroids(clusters, clusterRadius);

        var result = new List<LocationClusterItem>(clusters.Count);
        foreach (var cluster in clusters)
        {
            var clusterItem = LocationClusterItem.FromCluster(cluster.SelectMany(location => location.Observations).ToList());
            clusterItem.ClusterRadiusMeters = clusterRadius;
            result.Add(clusterItem);
        }

        var nonClusteredLocations = locations
            .Where((_, index) => !clustered[index])
            .ToList();
        result.AddRange(CreateSingleLocationItems(nonClusteredLocations));

        return result;
    }

    private static List<List<ProjectedLocation>> MergeClustersWithCloseCentroids(
        List<List<ProjectedLocation>> clusters,
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
                var firstCentroid = GetCentroid(clusters[firstIndex]);

                for (var secondIndex = firstIndex + 1; secondIndex < clusters.Count; secondIndex++)
                {
                    var secondCentroid = GetCentroid(clusters[secondIndex]);
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

    private static (double X, double Y) GetCentroid(List<ProjectedLocation> cluster)
    {
        var x = 0.0;
        var y = 0.0;

        foreach (var location in cluster)
        {
            x += location.X;
            y += location.Y;
        }

        return (x / cluster.Count, y / cluster.Count);
    }

    private static Dictionary<GridKey, List<int>> BuildBuckets(List<ProjectedLocation> locations, double cellSize)
    {
        var buckets = new Dictionary<GridKey, List<int>>();

        for (var index = 0; index < locations.Count; index++)
        {
            var location = locations[index];
            var key = GetGridKey(location.X, location.Y, cellSize);

            if (!buckets.TryGetValue(key, out var bucket))
            {
                bucket = new List<int>();
                buckets[key] = bucket;
            }

            bucket.Add(index);
        }

        return buckets;
    }

    private static IEnumerable<int> GetCandidateIndexes(
        Dictionary<GridKey, List<int>> buckets,
        double x,
        double y,
        double cellSize)
    {
        var anchorKey = GetGridKey(x, y, cellSize);

        for (var xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (var yOffset = -1; yOffset <= 1; yOffset++)
            {
                var key = new GridKey(anchorKey.X + xOffset, anchorKey.Y + yOffset);
                if (!buckets.TryGetValue(key, out var bucket))
                {
                    continue;
                }

                foreach (var index in bucket)
                {
                    yield return index;
                }
            }
        }
    }

    private static GridKey GetGridKey(double x, double y, double cellSize)
    {
        var normalizedCellSize = Math.Max(cellSize, MinimumClusterRadiusMeters);
        return new GridKey(
            (long)Math.Floor(x / normalizedCellSize),
            (long)Math.Floor(y / normalizedCellSize));
    }

    private static double ComputeMinimumClusterRadius(LatLonBoundingBox boundingBox, int targetItemCount)
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

    private static double ComputeMinimumClusterRadius(LatLonBoundingBox boundingBox, double metersPerPixel, double clusterRadiusPx)
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

    private static double ComputeLocationsDiagonal(List<ProjectedLocation> locations)
    {
        if (locations.Count == 0)
        {
            return 0.0;
        }

        var minX = locations[0].X;
        var maxX = minX;
        var minY = locations[0].Y;
        var maxY = minY;

        for (var index = 1; index < locations.Count; index++)
        {
            var location = locations[index];

            if (location.X < minX)
            {
                minX = location.X;
            }

            if (location.X > maxX)
            {
                maxX = location.X;
            }

            if (location.Y < minY)
            {
                minY = location.Y;
            }

            if (location.Y > maxY)
            {
                maxY = location.Y;
            }
        }

        var xDelta = maxX - minX;
        var yDelta = maxY - minY;

        return Math.Sqrt(xDelta * xDelta + yDelta * yDelta);
    }

    private static Envelope? GetSweref99TmBounds(LatLonBoundingBox boundingBox)
    {
        if (boundingBox?.TopLeft == null || boundingBox.BottomRight == null)
        {
            return null;
        }

        return boundingBox.GetPolygon(CoordinateSys.SWEREF99_TM).EnvelopeInternal;
    }

    private static List<LocationClusterItem> CreateSingleLocationItems(IEnumerable<ProjectedLocation> locations)
    {
        return locations
            .Select(location => LocationClusterItem.FromSingleLocation(location.Observations))
            .ToList();
    }

    private static List<ProjectedLocation> ProjectLocations(IEnumerable<Observation> observations)
    {
        var projectedLocationsById = new Dictionary<string, ProjectedLocation>(StringComparer.Ordinal);

        foreach (var observation in observations)
        {
            if (!TryProjectObservation(observation, out var x, out var y))
            {
                continue;
            }

            var locationId = observation.Location.DistinctLocationId;
            if (projectedLocationsById.TryGetValue(locationId, out var projectedLocation))
            {
                projectedLocation.Observations.Add(observation);
                continue;
            }

            projectedLocationsById.Add(
                locationId,
                new ProjectedLocation(locationId, x, y, new List<Observation> { observation }));
        }

        return projectedLocationsById.Values.ToList();
    }

    private static bool TryProjectObservation(Observation observation, out double x, out double y)
    {
        x = default;
        y = default;

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
            x = location.Sweref99TmX.Value;
            y = location.Sweref99TmY.Value;
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

        x = sweref99TmPoint.Coordinate.X;
        y = sweref99TmPoint.Coordinate.Y;
        return true;
    }

    private static bool IsFinite(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }

    private sealed record ProjectedLocation(string DistinctLocationId, double X, double Y, List<Observation> Observations);

    private readonly record struct GridKey(long X, long Y);
}
