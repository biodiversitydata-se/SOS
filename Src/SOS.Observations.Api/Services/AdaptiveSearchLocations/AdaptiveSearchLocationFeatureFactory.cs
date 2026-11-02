using SOS.Api.Contracts.Common.Geo;
using NetTopologySuite.Geometries;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Extensions;
using SOS.Lib.Gis;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using System.Text.Json.Nodes;
using Location = SOS.Lib.Models.Processed.Observation.Location;
using ProcessedLocationType = SOS.Lib.Enums.LocationType;

namespace SOS.Observations.Api.Services.AdaptiveSearchLocations;

internal interface IAdaptiveSearchLocationFeatureFactory
{
    List<LocationFeature> CreateItems(List<CachedLocationAggregate> locations, LocationFeatureOptions options);
    List<LocationFeature> CreateItems(IEnumerable<JsonObject> records, LocationFeatureOptions options);
}

internal sealed class AdaptiveSearchLocationFeatureFactory : IAdaptiveSearchLocationFeatureFactory
{
    private const int DefaultMaxLocationCountAfterClustering = 400;
    private const double MinimumMapPointRadiusPx = 5.0;
    private readonly IAdaptiveSearchLocationAggregator _aggregator;

    public AdaptiveSearchLocationFeatureFactory(IAdaptiveSearchLocationAggregator aggregator)
    {
        _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
    }

    public List<LocationFeature> CreateItems(List<CachedLocationAggregate> locations, LocationFeatureOptions options) =>
        options.ShouldCluster ? CreateClusterItems(locations, options) : CreateUnclusteredItems(locations, options);

    public List<LocationFeature> CreateItems(IEnumerable<JsonObject> records, LocationFeatureOptions options) =>
        options.ShouldCluster ? CreateClusterItems(records, options) : CreateUnclusteredItems(records, options);

    private List<LocationFeature> CreateClusterItems(List<CachedLocationAggregate> locationAggregates, LocationFeatureOptions options)
    {
        if (locationAggregates.Count == 0)
        {
            return new List<LocationFeature>();
        }

        var preparedPolygonGeometries = PreparePolygonGeometries(
            locationAggregates, options.PolygonGeometries, options.CreatePolygons, options.MetersPerPixel);
        var polygonLocations = preparedPolygonGeometries.Count == 0 ? new List<CachedLocationAggregate>() : locationAggregates
            .Where(location => KeepOutsideClustering(preparedPolygonGeometries, location.DistinctLocationId))
            .ToList();
        var clusterableLocations = polygonLocations.Count == 0 ? locationAggregates : locationAggregates
            .Where(location => !KeepOutsideClustering(preparedPolygonGeometries, location.DistinctLocationId))
            .ToList();
        var aggregatesByDistinctLocationId = clusterableLocations.ToDictionary(
            location => location.DistinctLocationId,
            StringComparer.Ordinal);

        var locationFeatures = CreateLocationFeaturesFromCachedAggregates(
            clusterableLocations, aggregatesByDistinctLocationId, options);
        locationFeatures.AddRange(polygonLocations.Select(location =>
            CreateLocationFeatureFromCachedAggregates(new[] { location }, false)));

        ApplyFallbackClusterRadiusMeters(locationFeatures, options.MetersPerPixel, options.ClusterRadiusPx);
        ApplyGeometries(locationFeatures, options.PolygonGeometries, options.CreatePolygons, options.MetersPerPixel, preparedPolygonGeometries);
        return locationFeatures;
    }

    private List<LocationFeature> CreateUnclusteredItems(List<CachedLocationAggregate> locationAggregates, LocationFeatureOptions options)
    {
        var locationFeatures = locationAggregates
            .Select(location => CreateLocationFeatureFromCachedAggregates(new[] { location }, false))
            .ToList();

        ApplyGeometries(locationFeatures, options.PolygonGeometries, options.CreatePolygons, options.MetersPerPixel);
        return locationFeatures;
    }

    private List<LocationFeature> CreateClusterItems(IEnumerable<JsonObject> records, LocationFeatureOptions options)
    {
        var observations = records
            .ToObservationsArray()
            .Where(HasUsableLocation)
            .ToList();
        if (observations.Count == 0)
        {
            return new List<LocationFeature>();
        }

        var preparedPolygonGeometries = PreparePolygonGeometries(
            observations, options.PolygonGeometries, options.CreatePolygons, options.MetersPerPixel);
        var polygonObservations = preparedPolygonGeometries.Count == 0 ? new List<Observation>() : observations
            .Where(observation => KeepOutsideClustering(preparedPolygonGeometries, observation.Location.DistinctLocationId))
            .ToList();
        var clusterableObservations = polygonObservations.Count == 0 ? observations : observations
            .Where(observation => !KeepOutsideClustering(preparedPolygonGeometries, observation.Location.DistinctLocationId))
            .ToList();
        var clusterer = new Sweref99TmFastGreedyLocationClusterer(options.MinLocationCountBeforeClustering, DefaultMaxLocationCountAfterClustering, options.MinLocationsPerCluster);
        var locationFeatures = clusterer.ClusterLocations(clusterableObservations, options.BoundingBox, options.MetersPerPixel,
            options.ClusterRadiusPx, options.ClusterRadiusMeters, options.ClusterPixels, options.ProximityClusterRadiusPx);

        locationFeatures = NormalizeNonClusteredLocationItems(locationFeatures);
        locationFeatures.AddRange(_aggregator.GroupObservationsByLocation(polygonObservations)
            .Select(LocationFeature.FromSingleLocation));
        ApplyFallbackClusterRadiusMeters(locationFeatures, options.MetersPerPixel, options.ClusterRadiusPx);
        ApplyGeometries(locationFeatures, options.PolygonGeometries, options.CreatePolygons, options.MetersPerPixel, preparedPolygonGeometries);
        return locationFeatures;
    }

    private List<LocationFeature> CreateUnclusteredItems(IEnumerable<JsonObject> records, LocationFeatureOptions options)
    {
        var observations = records
            .ToObservationsArray()
            .Where(HasUsableLocation)
            .ToList();

        var locationFeatures = _aggregator.GroupObservationsByLocation(observations)
            .Select(LocationFeature.FromSingleLocation)
            .ToList();

        ApplyGeometries(locationFeatures, options.PolygonGeometries, options.CreatePolygons, options.MetersPerPixel);
        return locationFeatures;
    }

    private List<LocationFeature> CreateLocationFeaturesFromCachedAggregates(
        List<CachedLocationAggregate> locationAggregates,
        Dictionary<string, CachedLocationAggregate> aggregatesByDistinctLocationId,
        LocationFeatureOptions options)
    {
        var clusterer = new Sweref99TmFastGreedyLocationClusterer(options.MinLocationCountBeforeClustering,
            DefaultMaxLocationCountAfterClustering, options.MinLocationsPerCluster);
        var groups = clusterer.ClusterGroups(locationAggregates.Select(aggregate => aggregate.Location), options.Clustering);
        return groups.Select(group => CreateLocationFeatureFromCachedAggregates(
            group.DistinctLocationIds.Select(id => aggregatesByDistinctLocationId[id]),
            group.IsCluster, group.RadiusMeters, group.GroupingType, group.MetricCenter)).ToList();
    }

    private static LocationFeature CreateLocationFeatureFromCachedAggregates(
        IEnumerable<CachedLocationAggregate> aggregates,
        bool isCluster,
        double? clusterRadiusMeters = null, SOS.Api.Contracts.Observations.Responses.LocationGroupingType groupingType = SOS.Api.Contracts.Observations.Responses.LocationGroupingType.PixelCluster, Point? metricCenter = null)
    {
        var aggregateList = aggregates.ToList();
        var locations = aggregateList.Select(aggregate => aggregate.Location).ToList();
        var observationSummary = aggregateList.Count == 1
            ? CopySingleSummary(aggregateList[0].ObservationSummary)
            : CombineLocationObservationSummaries(aggregateList);
        var locationIds = aggregateList
            .SelectMany(aggregate => aggregate.LocationIds)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var taxaCount = aggregateList
            .SelectMany(aggregate => aggregate.TaxonIds)
            .Distinct()
            .Count();

        var locationFeature = new LocationFeature
        {
            IsCluster = isCluster,
            GroupingType = groupingType,
            OriginalLocations = aggregateList.SelectMany(a => a.Locations.Count > 0 ? a.Locations : new[] { a.Location }.ToList()).ToList(),
            GeneralizedLocationIds = aggregateList.SelectMany(a => a.GeneralizedLocationIds).ToHashSet(StringComparer.OrdinalIgnoreCase),
            LocationCount = locations.Count,
            ObservationCount = observationSummary.ObservationCount,
            TaxaCount = taxaCount,
            ObservationSummary = observationSummary,
            LocationIds = locationIds,
            Locations = locations,
            StatusCounts = observationSummary.ToStatusCounts(),
            ClusterRadiusMeters = isCluster ? clusterRadiusMeters : null,
            IsSummaryApproximated = observationSummary.ObservationCount == 1
                ? null
                : aggregateList.Any(aggregate => aggregate.IsSummaryApproximated) ? true : null,
            IsPresentationStatusApproximated = aggregateList.Any(aggregate => aggregate.IsPresentationStatusApproximated)
        };

        if (isCluster)
        {
            locationFeature.SetMetricCentroid(metricCenter);
        }
        else
        {
            locationFeature.Location = locations[0];
            locationFeature.Geometry = CreatePointGeometry(locations[0]);
        }

        return locationFeature;
    }

    // Copy mutable summary state so response processing cannot modify cached aggregates.
    private static LocationObservationSummary CopySingleSummary(LocationObservationSummary source) => new()
    {
        ObservationCount = source.ObservationCount,
        FoundCount = source.FoundCount,
        TreatedCount = source.TreatedCount,
        DiffusedCount = source.DiffusedCount,
        RedlistedCount = source.RedlistedCount,
        InvasiveInSwedenCount = source.InvasiveInSwedenCount,
        NotRecoveredCount = source.NotRecoveredCount,
        NeverFoundCount = source.NeverFoundCount,
        MinPresentEndYear = source.MinPresentEndYear,
        MaxPresentEndYear = source.MaxPresentEndYear,
        MinNotRecoveredEndYear = source.MinNotRecoveredEndYear,
        MaxNotRecoveredEndYear = source.MaxNotRecoveredEndYear,
        IsAllObservationsSensitive = source.ObservationCount > 0 && source.IsAllObservationsSensitive,
        AllObservationsArePresent = source.ObservationCount > 0 && source.AllObservationsArePresent,
        DebugObservations = source.ObservationCount <= 5 ? source.DebugObservations?.ToList() ?? new() : null,
        PresentationStatusSummaries = source.PresentationStatusSummaries.Select(summary => new LocationObservationSummary.PresentationStatusSummary
        {
            IsPresent = summary.IsPresent, IsSensitive = summary.IsSensitive, IsTreated = summary.IsTreated,
            IsNotRecovered = summary.IsNotRecovered, EndYear = summary.EndYear, Count = summary.Count
        }).ToList()
    };

    private static LocationObservationSummary CombineLocationObservationSummaries(IEnumerable<CachedLocationAggregate> aggregates)
    {
        var aggregateList = aggregates.ToList();
        var summary = new LocationObservationSummary
        {
            ObservationCount = aggregateList.Sum(aggregate => aggregate.ObservationSummary.ObservationCount),
            FoundCount = aggregateList.Sum(aggregate => aggregate.ObservationSummary.FoundCount),
            TreatedCount = aggregateList.Sum(aggregate => aggregate.ObservationSummary.TreatedCount),
            DiffusedCount = aggregateList.Sum(aggregate => aggregate.ObservationSummary.DiffusedCount),
            RedlistedCount = aggregateList.Sum(aggregate => aggregate.ObservationSummary.RedlistedCount),
            InvasiveInSwedenCount = aggregateList.Sum(aggregate => aggregate.ObservationSummary.InvasiveInSwedenCount),
            NotRecoveredCount = aggregateList.Sum(aggregate => aggregate.ObservationSummary.NotRecoveredCount),
            NeverFoundCount = aggregateList.Sum(aggregate => aggregate.ObservationSummary.NeverFoundCount)
        };

        summary.IsAllObservationsSensitive = summary.ObservationCount > 0 &&
                                             aggregateList.All(aggregate => aggregate.ObservationSummary.IsAllObservationsSensitive);
        summary.AllObservationsArePresent = summary.ObservationCount > 0 &&
                                            aggregateList.All(aggregate => aggregate.ObservationSummary.AllObservationsArePresent);
        summary.MinPresentEndYear = aggregateList.Select(aggregate => aggregate.ObservationSummary.MinPresentEndYear).Where(year => year.HasValue).Min();
        summary.MaxPresentEndYear = aggregateList.Select(aggregate => aggregate.ObservationSummary.MaxPresentEndYear).Where(year => year.HasValue).Max();
        summary.MinNotRecoveredEndYear = aggregateList.Select(aggregate => aggregate.ObservationSummary.MinNotRecoveredEndYear).Where(year => year.HasValue).Min();
        summary.MaxNotRecoveredEndYear = aggregateList.Select(aggregate => aggregate.ObservationSummary.MaxNotRecoveredEndYear).Where(year => year.HasValue).Max();
        summary.DebugObservations = summary.ObservationCount <= 5
            ? aggregateList.SelectMany(aggregate => aggregate.ObservationSummary.DebugObservations ?? new List<DebugObservation>()).ToList()
            : null;
        summary.PresentationStatusSummaries = CombinePresentationStatusSummaries(aggregateList);

        return summary;
    }

    private static List<LocationObservationSummary.PresentationStatusSummary> CombinePresentationStatusSummaries(
        IEnumerable<CachedLocationAggregate> aggregates)
    {
        return aggregates
            .SelectMany(aggregate => aggregate.ObservationSummary.PresentationStatusSummaries)
            .GroupBy(
                summary => new
                {
                    summary.IsPresent,
                    summary.IsSensitive,
                    summary.IsTreated,
                    summary.IsNotRecovered,
                    summary.EndYear
                })
            .Select(group => new LocationObservationSummary.PresentationStatusSummary
            {
                IsPresent = group.Key.IsPresent,
                IsSensitive = group.Key.IsSensitive,
                IsTreated = group.Key.IsTreated,
                IsNotRecovered = group.Key.IsNotRecovered,
                EndYear = group.Key.EndYear,
                Count = group.Sum(summary => summary.Count)
            })
            .ToList();
    }

    private List<LocationFeature> NormalizeNonClusteredLocationItems(List<LocationFeature> locationFeatures)
    {
        var result = new List<LocationFeature>(locationFeatures.Count);
        var nonClusteredObservations = new List<Observation>();
        foreach (var item in locationFeatures)
        {
            if (item.IsCluster)
            {
                result.Add(item);
                continue;
            }

            nonClusteredObservations.AddRange(item.Observations);
        }

        foreach (var group in _aggregator.GroupObservationsByLocation(nonClusteredObservations))
        {
            result.Add(LocationFeature.FromSingleLocation(group));
        }

        return result;
    }

    private static void ApplyFallbackClusterRadiusMeters(List<LocationFeature> locationFeatures, double? metersPerPixel, double clusterRadiusPx)
    {
        if (!metersPerPixel.HasValue || metersPerPixel.Value <= 0)
        {
            return;
        }

        var radiusMeters = metersPerPixel.Value * (clusterRadiusPx > 0 ? clusterRadiusPx : 40.0);
        foreach (var item in locationFeatures)
        {
            if (item.IsCluster && !item.ClusterRadiusMeters.HasValue)
            {
                item.ClusterRadiusMeters = radiusMeters;
            }
        }
    }

    private readonly record struct PreparedPolygonGeometry(Geometry? Geometry, bool KeepOutsideClustering);

    private static Dictionary<string, PreparedPolygonGeometry> PreparePolygonGeometries(
        IEnumerable<CachedLocationAggregate> locations, Dictionary<string, Geometry>? polygons,
        CreatePolygonsMode mode, double? metersPerPixel)
    {
        var prepared = new Dictionary<string, PreparedPolygonGeometry>(StringComparer.OrdinalIgnoreCase);
        if (mode == CreatePolygonsMode.No || polygons == null || polygons.Count == 0) return prepared;

        foreach (var location in locations)
        {
            if (location.Location.Type != ProcessedLocationType.Polygon) continue;
            PreparePolygonGeometry(prepared, location.DistinctLocationId,
                location.Locations.Count > 0 ? location.Locations : new[] { location.Location },
                location.GeneralizedLocationIds, polygons, mode, metersPerPixel);
        }
        return prepared;
    }

    private Dictionary<string, PreparedPolygonGeometry> PreparePolygonGeometries(
        IEnumerable<Observation> observations, Dictionary<string, Geometry>? polygons,
        CreatePolygonsMode mode, double? metersPerPixel)
    {
        var prepared = new Dictionary<string, PreparedPolygonGeometry>(StringComparer.OrdinalIgnoreCase);
        if (mode == CreatePolygonsMode.No || polygons == null || polygons.Count == 0) return prepared;

        foreach (var group in _aggregator.GroupObservationsByLocation(
                     observations.Where(observation => observation.Location.Type == ProcessedLocationType.Polygon)))
        {
            PreparePolygonGeometry(prepared, group[0].Location.DistinctLocationId,
                group.Select(observation => observation.Location),
                group.Where(observation => observation.IsGeneralized)
                    .Select(observation => observation.Location.LocationId).ToHashSet(StringComparer.OrdinalIgnoreCase),
                polygons, mode, metersPerPixel);
        }
        return prepared;
    }

    private static void PreparePolygonGeometry(Dictionary<string, PreparedPolygonGeometry> prepared, string distinctLocationId,
        IEnumerable<Location> locations, HashSet<string> generalizedIds, Dictionary<string, Geometry> polygons,
        CreatePolygonsMode mode, double? metersPerPixel)
    {
        var originals = locations.GroupBy(location => location.LocationId, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First()).ToList();
        if (!originals.Any(location => location.Type == ProcessedLocationType.Polygon &&
                                      !generalizedIds.Contains(location.LocationId) && polygons.ContainsKey(location.LocationId)))
        {
            return;
        }

        // Yes preserves the surface in the response even when it is small enough to render as a point.
        // Cache geometry and clustering eligibility separately to avoid projecting or measuring twice.
        var geometry = CreateSurfaceGeometry(originals, generalizedIds, polygons, mode, metersPerPixel);
        var keepOutsideClustering = geometry != null &&
            (mode != CreatePolygonsMode.Yes || metersPerPixel is not > 0 ||
             GetPolygonGeometryDisplayDecision(geometry, metersPerPixel).UsePolygon);
        prepared[distinctLocationId] = new PreparedPolygonGeometry(geometry, keepOutsideClustering);
    }

    private static bool KeepOutsideClustering(Dictionary<string, PreparedPolygonGeometry> prepared, string distinctLocationId) =>
        prepared.TryGetValue(distinctLocationId, out var polygon) && polygon.KeepOutsideClustering;

    private static void ApplyGeometries(List<LocationFeature> locationFeatures, Dictionary<string, Geometry>? polygonLocationGeometries,
        CreatePolygonsMode createPolygons, double? metersPerPixel, Dictionary<string, PreparedPolygonGeometry>? preparedPolygonGeometries = null)
    {
        foreach (var feature in locationFeatures)
        {
            if (feature.IsCluster)
            {
                feature.Geometry = CreatePointGeometry(feature.Location);
                continue;
            }

            if (preparedPolygonGeometries?.TryGetValue(feature.Location.DistinctLocationId, out var preparedGeometry) == true)
            {
                feature.Geometry = preparedGeometry.Geometry ?? CreatePointGeometry(feature.Location);
                continue;
            }

            var originals = (feature.OriginalLocations.Count > 0 ? feature.OriginalLocations
                : feature.Observations.Count > 0 ? feature.Observations.Select(o => o.Location).ToList() : feature.Locations)
                .GroupBy(l => l.LocationId, StringComparer.OrdinalIgnoreCase).Select(g => g.First()).ToList();
            var generalizedIds = feature.OriginalLocations.Count > 0 ? feature.GeneralizedLocationIds
                : feature.Observations.Where(o => o.IsGeneralized).Select(o => o.Location.LocationId)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            feature.Geometry = CreateSurfaceGeometry(originals, generalizedIds, polygonLocationGeometries, createPolygons, metersPerPixel)
                ?? CreatePointGeometry(feature.Location);
        }
    }

    private static Geometry? CreateSurfaceGeometry(IEnumerable<Location> originals, HashSet<string> generalizedIds,
        Dictionary<string, Geometry>? polygons, CreatePolygonsMode mode, double? metersPerPixel)
    {
        var surfaces = new List<Geometry>();
        var hasGeneralized = false;
        foreach (var location in originals)
        {
            if (generalizedIds.Contains(location.LocationId))
            {
                var square = CreateDiffusedLocationSquareGeometry(location);
                if (square != null) { surfaces.Add(square); hasGeneralized = true; }
            }
            else if (mode != CreatePolygonsMode.No && location.Type == ProcessedLocationType.Polygon &&
                     polygons?.TryGetValue(location.LocationId, out var polygon) == true)
                surfaces.Add(polygon);
        }
        if (surfaces.Count == 0) return null;

        var union = surfaces.Count == 1 ? surfaces[0] : NetTopologySuite.Operation.Union.UnaryUnionOp.Union(surfaces);
        return hasGeneralized || mode == CreatePolygonsMode.Yes ||
               mode == CreatePolygonsMode.WhenNeeded && GetPolygonGeometryDisplayDecision(union, metersPerPixel).UsePolygon
            ? union
            : null;
    }

    private static Geometry? CreateDiffusedLocationSquareGeometry(SOS.Lib.Models.Processed.Observation.Location? location)
    {
        if (location?.CoordinateUncertaintyInMeters is null or <= 0)
        {
            return null;
        }

        Point? centerPoint = null;
        if (location.Sweref99TmX.HasValue && location.Sweref99TmY.HasValue)
        {
            centerPoint = new Point(location.Sweref99TmX.Value, location.Sweref99TmY.Value)
            {
                SRID = CoordinateSys.SWEREF99_TM.Srid()
            };
        }
        else if (location.DecimalLongitude.HasValue && location.DecimalLatitude.HasValue)
        {
            centerPoint = new Point(location.DecimalLongitude.Value, location.DecimalLatitude.Value)
            {
                SRID = CoordinateSys.WGS84.Srid()
            }.Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM, usePointCache: false);
        }

        if (centerPoint == null)
        {
            return null;
        }

        var halfSideMeters = location.CoordinateUncertaintyInMeters.Value / 2d;
        var minX = centerPoint.X - halfSideMeters;
        var maxX = centerPoint.X + halfSideMeters;
        var minY = centerPoint.Y - halfSideMeters;
        var maxY = centerPoint.Y + halfSideMeters;
        var square = new Polygon(new LinearRing(new[]
        {
            new Coordinate(minX, minY),
            new Coordinate(maxX, minY),
            new Coordinate(maxX, maxY),
            new Coordinate(minX, maxY),
            new Coordinate(minX, minY)
        }))
        {
            SRID = CoordinateSys.SWEREF99_TM.Srid()
        };

        return square.Transform(CoordinateSys.SWEREF99_TM, CoordinateSys.WGS84, usePointCache: false);
    }

    private static bool HasUsableLocation(Observation observation)
    {
        return !string.IsNullOrWhiteSpace(observation.Location.LocationId);
    }

    private static Geometry? CreatePointGeometry(SOS.Lib.Models.Processed.Observation.Location? location)
    {
        if (location?.DecimalLatitude == null || location.DecimalLongitude == null)
        {
            return null;
        }

        return new Point(location.DecimalLongitude.Value, location.DecimalLatitude.Value)
        {
            SRID = CoordinateSys.WGS84.Srid()
        };
    }

    private static PolygonGeometryDisplayDecision GetPolygonGeometryDisplayDecision(Geometry polygonGeometry, double? metersPerPixel)
    {
        if (!metersPerPixel.HasValue || metersPerPixel.Value <= 0)
        {
            return new PolygonGeometryDisplayDecision(false, null, null, null, "PolygonSizeCouldNotBeCalculated");
        }

        var metricGeometry = polygonGeometry.Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM, usePointCache: false);
        var envelope = metricGeometry?.EnvelopeInternal;
        if (envelope == null)
        {
            return new PolygonGeometryDisplayDecision(false, null, null, null, "PolygonSizeCouldNotBeCalculated");
        }

        var widthPx = Math.Abs(envelope.Width) / metersPerPixel.Value;
        var heightPx = Math.Abs(envelope.Height) / metersPerPixel.Value;
        var areaPx = Math.Abs(metricGeometry.Area) / (metersPerPixel.Value * metersPerPixel.Value);
        var minimumPolygonDiameterPx = MinimumMapPointRadiusPx * 2.0;
        var minimumPolygonAreaPx = Math.PI * MinimumMapPointRadiusPx * MinimumMapPointRadiusPx;
        if (widthPx >= minimumPolygonDiameterPx &&
            heightPx >= minimumPolygonDiameterPx &&
            areaPx >= minimumPolygonAreaPx)
        {
            return new PolygonGeometryDisplayDecision(true, widthPx, heightPx, areaPx, null);
        }

        return new PolygonGeometryDisplayDecision(false, widthPx, heightPx, areaPx, "PolygonTooSmallForCurrentResolution");
    }
}
