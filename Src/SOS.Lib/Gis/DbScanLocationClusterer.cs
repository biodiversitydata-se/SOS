using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Gis;

/// <summary>
/// Clusters distinct locations with a map-bounded DBSCAN variant in SWEREF99 TM.
/// </summary>
public class DbScanLocationClusterer : ILocationClusterer
{
    private const double DefaultClusterRadiusInPixels = 40.0;
    private const double DefaultFallbackClusterRadiusMeters = 1000.0;
    private const double MinimumClusterRadiusMeters = 1.0;
    private const int DefaultMinLocationsPerCluster = 2;
    private const int MaxClusterRadiusIterations = 40;

    private readonly int _minLocationCountBeforeClustering;
    private readonly int _maxLocationCountAfterClustering;
    private readonly int _minLocationsPerCluster;
    private readonly int _minPoints;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbScanLocationClusterer"/> class.
    /// </summary>
    public DbScanLocationClusterer()
        : this(100, 400)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DbScanLocationClusterer"/> class.
    /// </summary>
    /// <param name="minLocationCountBeforeClustering">
    /// The distinct-location threshold that controls when clustering starts.
    /// </param>
    /// <param name="maxLocationCountAfterClustering">
    /// The max number of result items after clustering.
    /// </param>
    public DbScanLocationClusterer(int minLocationCountBeforeClustering, int maxLocationCountAfterClustering)
        : this(minLocationCountBeforeClustering, maxLocationCountAfterClustering, DefaultMinLocationsPerCluster)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DbScanLocationClusterer"/> class.
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
    public DbScanLocationClusterer(int minLocationCountBeforeClustering, int maxLocationCountAfterClustering, int minLocationsPerCluster)
        : this(minLocationCountBeforeClustering, maxLocationCountAfterClustering, Math.Max(DefaultMinLocationsPerCluster, minLocationsPerCluster), minLocationsPerCluster)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DbScanLocationClusterer"/> class.
    /// </summary>
    /// <param name="minLocationCountBeforeClustering">
    /// The distinct-location threshold that controls when clustering starts.
    /// </param>
    /// <param name="maxLocationCountAfterClustering">
    /// The max number of result items after clustering.
    /// </param>
    /// <param name="minPoints">
    /// The minimum number of neighbouring locations, including the location itself, needed for a DBSCAN core point.
    /// </param>
    /// <param name="minLocationsPerCluster">
    /// The minimum number of distinct locations required for a cluster item.
    /// </param>
    public DbScanLocationClusterer(int minLocationCountBeforeClustering, int maxLocationCountAfterClustering, int minPoints, int minLocationsPerCluster)
    {
        _minLocationCountBeforeClustering = minLocationCountBeforeClustering;
        _maxLocationCountAfterClustering = maxLocationCountAfterClustering;
        _minLocationsPerCluster = Math.Max(DefaultMinLocationsPerCluster, minLocationsPerCluster);
        _minPoints = Math.Max(DefaultMinLocationsPerCluster, Math.Max(minPoints, _minLocationsPerCluster));
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
        var labels = new int[locations.Count];
        Array.Fill(labels, -2);
        var clusterId = 0;
        var clusters = new Dictionary<int, List<int>>();

        for (var index = 0; index < locations.Count; index++)
        {
            if (labels[index] != -2)
            {
                continue;
            }

            var neighbours = GetNeighbourIndexes(locations, buckets, index, clusterRadius).ToList();
            if (neighbours.Count + 1 < _minPoints)
            {
                labels[index] = -1;
                continue;
            }

            var cluster = ExpandCluster(locations, buckets, labels, index, neighbours, clusterId, clusterRadius);
            if (cluster.Count >= _minLocationsPerCluster)
            {
                clusters[clusterId] = cluster;
                clusterId++;
                continue;
            }

            foreach (var clusterIndex in cluster)
            {
                labels[clusterIndex] = -1;
            }
        }

        var result = new List<LocationClusterItem>(clusters.Count);
        foreach (var cluster in clusters.Values)
        {
            var clusterItem = LocationClusterItem.FromCluster(cluster
                .SelectMany(index => locations[index].Observations)
                .ToList());
            clusterItem.ClusterRadiusMeters = clusterRadius;
            result.Add(clusterItem);
        }

        var nonClusteredLocations = locations
            .Where((_, index) => labels[index] < 0)
            .ToList();
        result.AddRange(CreateSingleLocationItems(nonClusteredLocations));

        return result;
    }

    private List<int> ExpandCluster(
        List<ProjectedLocation> locations,
        Dictionary<GridKey, List<int>> buckets,
        int[] labels,
        int seedIndex,
        List<int> seedNeighbours,
        int clusterId,
        double clusterRadius)
    {
        var cluster = new List<int> { seedIndex };
        labels[seedIndex] = clusterId;
        var queue = new Queue<int>(seedNeighbours);

        while (queue.Count > 0)
        {
            var candidateIndex = queue.Dequeue();
            if (labels[candidateIndex] >= 0)
            {
                continue;
            }

            if (!IsWithinClusterBounds(locations, seedIndex, cluster, candidateIndex, clusterRadius))
            {
                continue;
            }

            if (labels[candidateIndex] == -1)
            {
                labels[candidateIndex] = clusterId;
                cluster.Add(candidateIndex);
                continue;
            }

            labels[candidateIndex] = clusterId;
            cluster.Add(candidateIndex);

            var candidateNeighbours = GetNeighbourIndexes(locations, buckets, candidateIndex, clusterRadius).ToList();
            if (candidateNeighbours.Count + 1 < _minPoints)
            {
                continue;
            }

            foreach (var neighbourIndex in candidateNeighbours)
            {
                if (labels[neighbourIndex] < 0)
                {
                    queue.Enqueue(neighbourIndex);
                }
            }
        }

        return cluster;
    }

    private static bool IsWithinClusterBounds(
        List<ProjectedLocation> locations,
        int seedIndex,
        List<int> cluster,
        int candidateIndex,
        double clusterRadius)
    {
        var candidate = locations[candidateIndex];
        var seed = locations[seedIndex];
        if (SquaredDistance(candidate, seed) <= clusterRadius * clusterRadius)
        {
            return true;
        }

        var centroid = GetCentroid(locations, cluster);
        var xDifference = candidate.X - centroid.X;
        var yDifference = candidate.Y - centroid.Y;
        return xDifference * xDifference + yDifference * yDifference <= clusterRadius * clusterRadius;
    }

    private static (double X, double Y) GetCentroid(List<ProjectedLocation> locations, List<int> cluster)
    {
        var x = 0.0;
        var y = 0.0;

        foreach (var index in cluster)
        {
            x += locations[index].X;
            y += locations[index].Y;
        }

        return (x / cluster.Count, y / cluster.Count);
    }

    private static IEnumerable<int> GetNeighbourIndexes(
        List<ProjectedLocation> locations,
        Dictionary<GridKey, List<int>> buckets,
        int locationIndex,
        double clusterRadius)
    {
        var location = locations[locationIndex];
        var clusterRadiusSquared = clusterRadius * clusterRadius;
        foreach (var candidateIndex in GetCandidateIndexes(buckets, location.X, location.Y, clusterRadius))
        {
            if (candidateIndex == locationIndex)
            {
                continue;
            }

            if (SquaredDistance(location, locations[candidateIndex]) <= clusterRadiusSquared)
            {
                yield return candidateIndex;
            }
        }
    }

    private static double SquaredDistance(ProjectedLocation first, ProjectedLocation second)
    {
        var xDifference = second.X - first.X;
        var yDifference = second.Y - first.Y;
        return xDifference * xDifference + yDifference * yDifference;
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
            minX = Math.Min(minX, location.X);
            maxX = Math.Max(maxX, location.X);
            minY = Math.Min(minY, location.Y);
            maxY = Math.Max(maxY, location.Y);
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