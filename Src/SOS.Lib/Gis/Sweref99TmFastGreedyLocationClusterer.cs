using LocationGroupingType = SOS.Api.Contracts.Observations.Responses.LocationGroupingType;
using SOS.Api.Contracts.Common.Geo;
using NetTopologySuite.Geometries;
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
    /// The target number of result items for bbox-based clustering without a pixel resolution.
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
    /// The target number of result items for bbox-based clustering without a pixel resolution.
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
        return ClusterLocations(observations, clusterRadius, useAdaptiveRadius: false);
    }

    private List<LocationFeature> ClusterLocations(List<Observation> observations, double minimumClusterRadiusMeters, bool useAdaptiveRadius = true)
    {        
        return CreateFeatures(ClusterProjectedLocations(ProjectLocations(observations.Select(o => o.Location)), minimumClusterRadiusMeters, useAdaptiveRadius), observations);
    }

    /// <summary>
    /// Meter groups are indivisible inputs to either normal pixel clustering or detail clustering.
    /// With a pixel resolution, both stages use a fixed search radius regardless of the result count.
    /// </summary>
    public List<LocationFeature> ClusterLocations(List<Observation> observations, LatLonBoundingBoxDto? boundingBox,
        double? metersPerPixel, double clusterRadiusPx, double? clusterRadiusMeters, bool clusterPixels,
        double detailClusterRadiusPx = 0)
    {
        return CreateFeatures(ClusterGroups(observations.Select(o => o.Location), new LocationClusteringOptions
        {
            BoundingBox = boundingBox, MetersPerPixel = metersPerPixel, ClusterRadiusPx = clusterRadiusPx,
            ClusterRadiusMeters = clusterRadiusMeters, ClusterPixels = clusterPixels,
            DetailClusterRadiusPx = detailClusterRadiusPx
        }), observations);
    }

    /// <summary>Clusters locations without constructing observations or presentation features.</summary>
    public List<LocationClusterGroup> ClusterGroups(IEnumerable<SOS.Lib.Models.Processed.Observation.Location> locations,
        LocationClusteringOptions options)
    {
        var boundingBox = options.BoundingBox;
        var metersPerPixel = options.MetersPerPixel;
        var clusterRadiusPx = options.ClusterRadiusPx;
        var clusterRadiusMeters = options.ClusterRadiusMeters;
        var clusterPixels = options.ClusterPixels;
        var detailClusterRadiusPx = options.DetailClusterRadiusPx;
        if (!double.IsFinite(detailClusterRadiusPx) || detailClusterRadiusPx < 0)
            throw new ArgumentOutOfRangeException(nameof(detailClusterRadiusPx));

        var units = ProjectLocations(locations);
        if (clusterRadiusMeters.HasValue)
        {
            if (!double.IsFinite(clusterRadiusMeters.Value) || clusterRadiusMeters.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(clusterRadiusMeters));
            var meterGroups = MakeClusterItems(units, clusterRadiusMeters.Value, LocationGroupingType.MeterCluster);
            units = meterGroups.Select(group => new ProjectedLocation(
                group.DistinctLocationIds.OrderBy(id => id, StringComparer.Ordinal).First(),
                group.MetricCenter.X, group.MetricCenter.Y, group.DistinctLocationIds.ToList(), group))
                .OrderBy(u => u.DistinctLocationId, StringComparer.Ordinal).ToList();
        }
        if (clusterPixels && boundingBox != null && units.Count > _minLocationCountBeforeClustering)
        {
            var radius = metersPerPixel.HasValue
                ? ComputeMinimumClusterRadius(boundingBox, metersPerPixel.Value, clusterRadiusPx)
                : ComputeMinimumClusterRadius(boundingBox, _maxLocationCountAfterClustering);
            return ClusterProjectedLocations(units, radius, useAdaptiveRadius: !metersPerPixel.HasValue);
        }

        // Detail clustering uses one fixed-radius pass, independently of the bbox-based target count.
        if (detailClusterRadiusPx > 0 && metersPerPixel is > 0 && double.IsFinite(metersPerPixel.Value))
            return MakeClusterItems(units, metersPerPixel.Value * detailClusterRadiusPx);

        return CreateSingleLocationItems(units);
    }

    private List<LocationClusterGroup> ClusterProjectedLocations(List<ProjectedLocation> projectedLocations, double minimumClusterRadiusMeters, bool useAdaptiveRadius)
    {
        if (projectedLocations.Count == 0)
        {
            return new List<LocationClusterGroup>();
        }

        if (projectedLocations.Count <= _minLocationCountBeforeClustering)
        {
            return CreateSingleLocationItems(projectedLocations);
        }

        // An explicit pixel radius controls screen-space grouping, not the number of map items.
        if (!useAdaptiveRadius)
        {
            return MakeClusterItems(projectedLocations, minimumClusterRadiusMeters);
        }

        var clusterRadius = Math.Max(ComputeLocationsDiagonal(projectedLocations) / 100, minimumClusterRadiusMeters);
        var result = new List<LocationClusterGroup>();
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

    private List<LocationClusterGroup> MakeClusterItems(List<ProjectedLocation> locations, double clusterRadius, LocationGroupingType groupingType = LocationGroupingType.PixelCluster)
    {
        if (locations.Count == 0)
        {
            return new List<LocationClusterGroup>();
        }

        if (clusterRadius <= 0)
        {
            return CreateSingleLocationItems(locations);
        }

        var buckets = BuildBuckets(locations, clusterRadius);
        var clustered = new bool[locations.Count];
        var processedAnchors = new bool[locations.Count];
        var clusters = new List<SpatialCluster>();
        var clusterRadiusSquared = clusterRadius * clusterRadius;

        for (var index = 0; index < locations.Count; index++)
        {
            if (clustered[index] || processedAnchors[index])
            {
                continue;
            }

            var anchor = locations[index];
            var candidateIndexes = GetCandidateIndexes(buckets, anchor.X, anchor.Y, clusterRadius).OrderBy(i => i);
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
                if (xDifference * xDifference + yDifference * yDifference <= clusterRadiusSquared)
                {
                    cluster.Add(candidateIndex);
                }
            }

            if (cluster.Count >= 2 && cluster.Sum(i => locations[i].Weight) >= _minLocationsPerCluster)
            {
                var clusterLocations = new SpatialCluster();
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

        clusters = MergeClustersAndNearbyLocations(clusters, locations, buckets, clustered, clusterRadius);

        var result = new List<LocationClusterGroup>(clusters.Count);
        foreach (var cluster in clusters)
        {
            var centroid = GetCentroid(cluster);
            result.Add(new LocationClusterGroup(
                cluster.SelectMany(location => location.MemberIds).ToList(),
                new Point(centroid.X, centroid.Y), true, groupingType, clusterRadius));
        }

        var nonClusteredLocations = locations
            .Where((_, index) => !clustered[index])
            .ToList();
        result.AddRange(CreateSingleLocationItems(nonClusteredLocations));

        return result;
    }

    private static List<SpatialCluster> MergeClustersAndNearbyLocations(
        List<SpatialCluster> clusters,
        List<ProjectedLocation> locations,
        Dictionary<GridKey, List<int>> buckets,
        bool[] clustered,
        double clusterRadius)
    {
        var clusterRadiusSquared = clusterRadius * clusterRadius;
        var unassignedCount = clustered.Count(isClustered => !isClustered);
        bool addedLocations;
        do
        {
            clusters = MergeClustersWithCloseCentroids(clusters, clusterRadius);
            if (unassignedCount == 0) break;
            addedLocations = false;
            foreach (var cluster in clusters)
            {
                if (unassignedCount == 0) break;
                var centroid = GetCentroid(cluster);
                // Reuse the input grid. A projected location may be an indivisible meter group.
                var candidates = GetCandidateIndexes(buckets, centroid.X, centroid.Y, clusterRadius).OrderBy(i => i);
                foreach (var index in candidates)
                {
                    if (clustered[index]) continue;
                    var location = locations[index];
                    var dx = location.X - centroid.X;
                    var dy = location.Y - centroid.Y;
                    if (dx * dx + dy * dy > clusterRadiusSquared) continue;

                    cluster.Add(location);
                    clustered[index] = true;
                    unassignedCount--;
                    addedLocations = true;
                    centroid = GetCentroid(cluster);
                }
            }
            // New centroids can reach other clusters or previously unvisited grid cells.
            // Each repeat absorbs at least one input unit, so the loop always terminates.
        } while (addedLocations);

        return clusters;
    }

    private static List<SpatialCluster> MergeClustersWithCloseCentroids(
        List<SpatialCluster> clusters,
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

    private static (double X, double Y) GetCentroid(SpatialCluster cluster) =>
        (cluster.SumX / cluster.Weight, cluster.SumY / cluster.Weight);

    private sealed class SpatialCluster : List<ProjectedLocation>
    {
        public double SumX { get; private set; }
        public double SumY { get; private set; }
        public int Weight { get; private set; }
        public new void Add(ProjectedLocation location)
        {
            base.Add(location);
            SumX += location.X * location.Weight;
            SumY += location.Y * location.Weight;
            Weight += location.Weight;
        }
        public new void AddRange(IEnumerable<ProjectedLocation> locations)
        {
            foreach (var location in locations) Add(location);
        }
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

        return metersPerPixel * NormalizeClusterRadiusPx(clusterRadiusPx);
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

    private static Envelope? GetSweref99TmBounds(LatLonBoundingBoxDto boundingBox)
    {
        if (boundingBox?.TopLeft == null || boundingBox.BottomRight == null)
        {
            return null;
        }

        return boundingBox.GetPolygon(CoordinateSys.SWEREF99_TM).EnvelopeInternal;
    }

    private static List<LocationClusterGroup> CreateSingleLocationItems(IEnumerable<ProjectedLocation> locations)
    {
        return locations.Select(location => location.Source ?? new LocationClusterGroup(
            location.MemberIds, new Point(location.X, location.Y), false, LocationGroupingType.SingleLocation, null)).ToList();
    }

    private static List<LocationFeature> CreateFeatures(IEnumerable<LocationClusterGroup> groups, IEnumerable<Observation> observations)
    {
        var observationsByLocation = observations.OrderBy(o => o.Location?.LocationId, StringComparer.Ordinal)
            .Where(o => o.Location != null)
            .GroupBy(o => o.Location.DistinctLocationId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
        return groups.Select(group =>
        {
            var members = group.DistinctLocationIds.SelectMany(id => observationsByLocation[id]).ToList();
            var feature = group.IsCluster
                ? LocationFeature.FromCluster(members, group.MetricCenter)
                : LocationFeature.FromSingleLocation(members);
            if (group.IsCluster)
            {
                feature.ClusterRadiusMeters = group.RadiusMeters;
                feature.GroupingType = group.GroupingType;
            }
            return feature;
        }).ToList();
    }

    private static List<ProjectedLocation> ProjectLocations(IEnumerable<SOS.Lib.Models.Processed.Observation.Location> locations)
    {
        var projectedLocationsById = new Dictionary<string, ProjectedLocation>(StringComparer.Ordinal);
        foreach (var location in locations.OrderBy(l => l?.LocationId, StringComparer.Ordinal))
        {
            if (!TryProjectLocation(location, out var x, out var y)) continue;
            var id = location.DistinctLocationId;
            projectedLocationsById.TryAdd(id, new ProjectedLocation(id, x, y, new List<string> { id }));
        }
        return projectedLocationsById.Values.OrderBy(l => l.DistinctLocationId, StringComparer.Ordinal).ToList();
    }

    private static bool TryProjectLocation(SOS.Lib.Models.Processed.Observation.Location location, out double x, out double y)
    {
        x = default;
        y = default;
        if (location?.DecimalLatitude.HasValue != true || !location.DecimalLongitude.HasValue) return false;
        if (location.Sweref99TmX.HasValue && location.Sweref99TmY.HasValue &&
            IsFinite(location.Sweref99TmX.Value) && IsFinite(location.Sweref99TmY.Value))
        {
            x = location.Sweref99TmX.Value;
            y = location.Sweref99TmY.Value;
            return true;
        }
        var point = new Point(location.DecimalLongitude.Value, location.DecimalLatitude.Value)
            .Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM);
        if (point?.Coordinate == null || !IsFinite(point.Coordinate.X) || !IsFinite(point.Coordinate.Y)) return false;
        x = point.Coordinate.X;
        y = point.Coordinate.Y;
        return true;
    }

    private static bool IsFinite(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }

    private sealed record ProjectedLocation(string DistinctLocationId, double X, double Y, List<string> MemberIds, LocationClusterGroup? Source = null)
    {
        public int Weight => Source?.DistinctLocationIds.Count ?? 1;
    }

    private readonly record struct GridKey(long X, long Y);
}
