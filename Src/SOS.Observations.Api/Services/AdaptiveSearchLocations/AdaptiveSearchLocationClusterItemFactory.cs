using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Gis;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using System.Text.Json.Nodes;
using ProcessedLocationType = SOS.Lib.Enums.LocationType;

namespace SOS.Observations.Api.Services.AdaptiveSearchLocations;

internal interface IAdaptiveSearchLocationClusterItemFactory
{
    List<LocationClusterItem> CreateClusterItems(List<CachedLocationAggregate> locationAggregates, LatLonBoundingBox? clusteringBbox, LocationClusteringAlgorithm clusteringAlgorithm, double? metersPerPixel, double clusterRadiusPx, int minLocationsPerCluster, int minLocationCountBeforeClustering, Dictionary<string, Geometry>? polygonLocationGeometries, CreatePolygonsMode createPolygons);
    List<LocationClusterItem> CreateUnclusteredItems(List<CachedLocationAggregate> locationAggregates, Dictionary<string, Geometry>? polygonLocationGeometries, CreatePolygonsMode createPolygons, double? metersPerPixel);
    List<LocationClusterItem> CreateClusterItems(IEnumerable<JsonObject> records, LatLonBoundingBox? clusteringBbox, LocationClusteringAlgorithm clusteringAlgorithm, double? metersPerPixel, double clusterRadiusPx, int minLocationsPerCluster, int minLocationCountBeforeClustering, Dictionary<string, Geometry>? polygonLocationGeometries, CreatePolygonsMode createPolygons);
    List<LocationClusterItem> CreateUnclusteredItems(IEnumerable<JsonObject> records, Dictionary<string, Geometry>? polygonLocationGeometries, CreatePolygonsMode createPolygons, double? metersPerPixel);
}

internal sealed class AdaptiveSearchLocationClusterItemFactory : IAdaptiveSearchLocationClusterItemFactory
{
    private const int DefaultMaxLocationCountAfterClustering = 400;
    private const double MinimumMapPointRadiusPx = 5.0;
    private readonly IAdaptiveSearchLocationAggregator _aggregator;

    public AdaptiveSearchLocationClusterItemFactory(IAdaptiveSearchLocationAggregator aggregator)
    {
        _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
    }

    public List<LocationClusterItem> CreateClusterItems(
        List<CachedLocationAggregate> locationAggregates,
        LatLonBoundingBox? clusteringBbox,
        LocationClusteringAlgorithm clusteringAlgorithm,
        double? metersPerPixel,
        double clusterRadiusPx,
        int minLocationsPerCluster,
        int minLocationCountBeforeClustering,
        Dictionary<string, Geometry>? polygonLocationGeometries,
        CreatePolygonsMode createPolygons)
    {
        if (locationAggregates.Count == 0)
        {
            return new List<LocationClusterItem>();
        }

        var aggregatesByDistinctLocationId = locationAggregates.ToDictionary(
            location => location.DistinctLocationId,
            StringComparer.Ordinal);

        var locationClusterItems = clusteringBbox == null
            ? locationAggregates
                .Select(location => CreateLocationClusterItemFromCachedAggregates(new[] { location }, false))
                .ToList()
            : CreateClusteredLocationItemsFromCachedAggregates(
                locationAggregates,
                aggregatesByDistinctLocationId,
                clusteringBbox,
                clusteringAlgorithm,
                metersPerPixel,
                clusterRadiusPx,
                minLocationsPerCluster,
                minLocationCountBeforeClustering);

        ApplyFallbackClusterRadiusMeters(locationClusterItems, metersPerPixel, clusterRadiusPx);
        ApplyGeometries(locationClusterItems, polygonLocationGeometries, createPolygons, metersPerPixel);
        return locationClusterItems;
    }

    public List<LocationClusterItem> CreateUnclusteredItems(
        List<CachedLocationAggregate> locationAggregates,
        Dictionary<string, Geometry>? polygonLocationGeometries,
        CreatePolygonsMode createPolygons,
        double? metersPerPixel)
    {
        var locationClusterItems = locationAggregates
            .Select(location => CreateLocationClusterItemFromCachedAggregates(new[] { location }, false))
            .ToList();

        ApplyGeometries(locationClusterItems, polygonLocationGeometries, createPolygons, metersPerPixel);
        return locationClusterItems;
    }

    public List<LocationClusterItem> CreateClusterItems(
        IEnumerable<JsonObject> records,
        LatLonBoundingBox? clusteringBbox,
        LocationClusteringAlgorithm clusteringAlgorithm,
        double? metersPerPixel,
        double clusterRadiusPx,
        int minLocationsPerCluster,
        int minLocationCountBeforeClustering,
        Dictionary<string, Geometry>? polygonLocationGeometries,
        CreatePolygonsMode createPolygons)
    {
        var observations = records
            .ToObservationsArray()
            .Where(observation => observation.Location?.DecimalLatitude.HasValue == true &&
                                  observation.Location?.DecimalLongitude.HasValue == true)
            .ToList();
        if (observations.Count == 0)
        {
            return new List<LocationClusterItem>();
        }

        var clusterer = CreateLocationClusterer(clusteringAlgorithm, minLocationCountBeforeClustering, minLocationsPerCluster);
        var locationClusterItems = clusteringBbox == null
            ? _aggregator.GroupObservationsByLocation(observations)
                .Select(LocationClusterItem.FromSingleLocation)
                .ToList()
            : metersPerPixel.HasValue
                ? clusterer.ClusterLocations(observations, clusteringBbox, metersPerPixel.Value, clusterRadiusPx)
                : clusterer.ClusterLocations(observations, clusteringBbox);

        locationClusterItems = NormalizeNonClusteredLocationItems(locationClusterItems);
        ApplyFallbackClusterRadiusMeters(locationClusterItems, metersPerPixel, clusterRadiusPx);
        ApplyGeometries(locationClusterItems, polygonLocationGeometries, createPolygons, metersPerPixel);
        return locationClusterItems;
    }

    public List<LocationClusterItem> CreateUnclusteredItems(
        IEnumerable<JsonObject> records,
        Dictionary<string, Geometry>? polygonLocationGeometries,
        CreatePolygonsMode createPolygons,
        double? metersPerPixel)
    {
        var observations = records
            .ToObservationsArray()
            .Where(observation => observation.Location?.DecimalLatitude.HasValue == true &&
                                  observation.Location?.DecimalLongitude.HasValue == true)
            .ToList();

        var locationClusterItems = _aggregator.GroupObservationsByLocation(observations)
            .Select(LocationClusterItem.FromSingleLocation)
            .ToList();

        ApplyGeometries(locationClusterItems, polygonLocationGeometries, createPolygons, metersPerPixel);
        return locationClusterItems;
    }

    private List<LocationClusterItem> CreateClusteredLocationItemsFromCachedAggregates(
        List<CachedLocationAggregate> locationAggregates,
        Dictionary<string, CachedLocationAggregate> aggregatesByDistinctLocationId,
        LatLonBoundingBox clusteringBbox,
        LocationClusteringAlgorithm clusteringAlgorithm,
        double? metersPerPixel,
        double clusterRadiusPx,
        int minLocationsPerCluster,
        int minLocationCountBeforeClustering)
    {
        var representativeObservations = locationAggregates
            .Select(CreateRepresentativeObservation)
            .ToList();
        var clusterer = CreateLocationClusterer(clusteringAlgorithm, minLocationCountBeforeClustering, minLocationsPerCluster);
        var clusteredItems = metersPerPixel.HasValue
            ? clusterer.ClusterLocations(representativeObservations, clusteringBbox, metersPerPixel.Value, clusterRadiusPx)
            : clusterer.ClusterLocations(representativeObservations, clusteringBbox);

        var result = new List<LocationClusterItem>(clusteredItems.Count);
        foreach (var clusteredItem in clusteredItems)
        {
            var aggregates = GetCachedAggregatesForClusterItem(clusteredItem, aggregatesByDistinctLocationId);
            if (aggregates.Count == 0)
            {
                continue;
            }

            result.Add(CreateLocationClusterItemFromCachedAggregates(
                aggregates,
                clusteredItem.IsCluster,
                clusteredItem.ClusterRadiusMeters));
        }

        return result;
    }

    private static List<CachedLocationAggregate> GetCachedAggregatesForClusterItem(
        LocationClusterItem locationClusterItem,
        Dictionary<string, CachedLocationAggregate> aggregatesByDistinctLocationId)
    {
        var distinctLocationIds = locationClusterItem.Locations.Count > 0
            ? locationClusterItem.Locations.Select(location => location.DistinctLocationId)
            : locationClusterItem.Observations.Select(observation => observation.Location?.DistinctLocationId);

        return distinctLocationIds
            .Where(distinctLocationId => !string.IsNullOrWhiteSpace(distinctLocationId))
            .Distinct(StringComparer.Ordinal)
            .Select(distinctLocationId => aggregatesByDistinctLocationId.TryGetValue(distinctLocationId!, out var aggregate)
                ? aggregate
                : null)
            .Where(aggregate => aggregate != null)
            .Select(aggregate => aggregate!)
            .ToList();
    }

    private static Observation CreateRepresentativeObservation(CachedLocationAggregate locationAggregate)
    {
        return new Observation
        {
            Id = locationAggregate.DistinctLocationId,
            IsGeneralized = locationAggregate.ObservationSummary.DiffusedCount > 0,
            Sensitive = locationAggregate.ObservationSummary.IsAllObservationsSensitive,
            Location = locationAggregate.Location,
            Occurrence = new Occurrence
            {
                OccurrenceId = locationAggregate.DistinctLocationId,
                IsPositiveObservation = locationAggregate.ObservationSummary.FoundCount > 0
            },
            Taxon = locationAggregate.TaxonIds.Count > 0
                ? new Taxon { Id = locationAggregate.TaxonIds[0] }
                : null
        };
    }

    private static LocationClusterItem CreateLocationClusterItemFromCachedAggregates(
        IEnumerable<CachedLocationAggregate> aggregates,
        bool isCluster,
        double? clusterRadiusMeters = null)
    {
        var aggregateList = aggregates.ToList();
        var locations = aggregateList.Select(aggregate => aggregate.Location).ToList();
        var observationSummary = CombineLocationObservationSummaries(aggregateList);
        var locationIds = aggregateList
            .SelectMany(aggregate => aggregate.LocationIds)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var representativeObservations = aggregateList
            .Select(CreateRepresentativeObservation)
            .ToList();
        var taxaCount = aggregateList
            .SelectMany(aggregate => aggregate.TaxonIds)
            .Distinct()
            .Count();

        var locationClusterItem = new LocationClusterItem
        {
            IsCluster = isCluster,
            LocationCount = locations.Count,
            ObservationCount = observationSummary.ObservationCount,
            TaxaCount = taxaCount,
            Observations = representativeObservations,
            ObservationSummary = observationSummary,
            LocationIds = locationIds,
            Locations = locations,
            StatusCounts = observationSummary.ToStatusCounts(),
            ClusterRadiusMeters = isCluster ? clusterRadiusMeters : null
        };

        if (isCluster)
        {
            var centroidLatitude = locations.Average(location => location.DecimalLatitude!.Value);
            var centroidLongitude = locations.Average(location => location.DecimalLongitude!.Value);
            locationClusterItem.Location = new SOS.Lib.Models.Processed.Observation.Location
            {
                DecimalLatitude = centroidLatitude,
                DecimalLongitude = centroidLongitude
            };
            locationClusterItem.Geometry = new Point(centroidLongitude, centroidLatitude);
        }
        else
        {
            locationClusterItem.Location = locations[0];
            locationClusterItem.Geometry = CreatePointGeometry(locations[0]);
        }

        return locationClusterItem;
    }

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
                                            summary.FoundCount == summary.ObservationCount;
        summary.MinPresentEndYear = aggregateList.Select(aggregate => aggregate.ObservationSummary.MinPresentEndYear).Where(year => year.HasValue).Min();
        summary.MaxPresentEndYear = aggregateList.Select(aggregate => aggregate.ObservationSummary.MaxPresentEndYear).Where(year => year.HasValue).Max();
        summary.MinNotRecoveredEndYear = aggregateList.Select(aggregate => aggregate.ObservationSummary.MinNotRecoveredEndYear).Where(year => year.HasValue).Min();
        summary.MaxNotRecoveredEndYear = aggregateList.Select(aggregate => aggregate.ObservationSummary.MaxNotRecoveredEndYear).Where(year => year.HasValue).Max();
        summary.DebugObservations = summary.ObservationCount <= 5
            ? aggregateList.SelectMany(aggregate => aggregate.ObservationSummary.DebugObservations ?? new List<DebugObservation>()).ToList()
            : null;

        return summary;
    }

    private List<LocationClusterItem> NormalizeNonClusteredLocationItems(List<LocationClusterItem> locationClusterItems)
    {
        var result = new List<LocationClusterItem>(locationClusterItems.Count);
        var nonClusteredObservations = new List<Observation>();
        foreach (var item in locationClusterItems)
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
            result.Add(LocationClusterItem.FromSingleLocation(group));
        }

        return result;
    }

    private static ILocationClusterer CreateLocationClusterer(
        LocationClusteringAlgorithm clusteringAlgorithm,
        int minLocationCountBeforeClustering,
        int minLocationsPerCluster)
    {
        return clusteringAlgorithm switch
        {
            LocationClusteringAlgorithm.Sweref99TmFastGreedy => new Sweref99TmFastGreedyLocationClusterer(minLocationCountBeforeClustering, DefaultMaxLocationCountAfterClustering, minLocationsPerCluster),
            LocationClusteringAlgorithm.Sweref99TmGreedy => new Sweref99TmLocationClusterer(minLocationCountBeforeClustering, DefaultMaxLocationCountAfterClustering, minLocationsPerCluster),
            LocationClusteringAlgorithm.DbScan => new DbScanLocationClusterer(minLocationCountBeforeClustering, DefaultMaxLocationCountAfterClustering, minLocationsPerCluster),                       
            _ => new Sweref99TmFastGreedyLocationClusterer(minLocationCountBeforeClustering, DefaultMaxLocationCountAfterClustering, minLocationsPerCluster)
        };
    }

    private static void ApplyFallbackClusterRadiusMeters(List<LocationClusterItem> locationClusterItems, double? metersPerPixel, double clusterRadiusPx)
    {
        if (!metersPerPixel.HasValue || metersPerPixel.Value <= 0)
        {
            return;
        }

        var radiusMeters = metersPerPixel.Value * (clusterRadiusPx > 0 ? clusterRadiusPx : 40.0);
        foreach (var item in locationClusterItems)
        {
            if (item.IsCluster && !item.ClusterRadiusMeters.HasValue)
            {
                item.ClusterRadiusMeters = radiusMeters;
            }
        }
    }

    private static void ApplyGeometries(List<LocationClusterItem> locationClusterItems, Dictionary<string, Geometry>? polygonLocationGeometries, CreatePolygonsMode createPolygons, double? metersPerPixel)
    {
        var resolvedMetersPerPixel = createPolygons == CreatePolygonsMode.WhenNeeded ? metersPerPixel : null;

        foreach (var locationClusterItem in locationClusterItems)
        {
            if (locationClusterItem.IsCluster)
            {
                locationClusterItem.Geometry = CreatePointGeometry(locationClusterItem.Location);
                continue;
            }

            var location = locationClusterItem.Location;
            var pointGeometry = CreatePointGeometry(location);
            ClearActualGeometryMetadata(locationClusterItem);

            if (IsDiffusedLocationItem(locationClusterItem))
            {
                locationClusterItem.Geometry = CreateDiffusedLocationSquareGeometry(location) ?? pointGeometry;
                continue;
            }

            if (location?.Type == ProcessedLocationType.Polygon)
            {
                if (createPolygons != CreatePolygonsMode.No &&
                    !string.IsNullOrWhiteSpace(location.LocationId) &&
                    polygonLocationGeometries?.TryGetValue(location.LocationId, out var polygonGeometry) == true)
                {
                    if (createPolygons == CreatePolygonsMode.Yes)
                    {
                        locationClusterItem.Geometry = polygonGeometry;
                        continue;
                    }

                    var decision = GetPolygonGeometryDisplayDecision(polygonGeometry, resolvedMetersPerPixel);
                    if (decision.UsePolygon)
                    {
                        locationClusterItem.Geometry = polygonGeometry;
                        continue;
                    }

                    locationClusterItem.ActualGeometry = polygonGeometry;
                    locationClusterItem.ActualGeometryIsSimplifiedToPoint = true;
                    locationClusterItem.ActualGeometrySimplificationReason = decision.SimplificationReason;
                    locationClusterItem.ActualGeometryWidthPx = decision.WidthPx;
                    locationClusterItem.ActualGeometryHeightPx = decision.HeightPx;
                    locationClusterItem.ActualGeometryAreaPx = decision.AreaPx;
                }

                locationClusterItem.Geometry = pointGeometry;
                continue;
            }

            locationClusterItem.Geometry = pointGeometry;
        }
    }

    private static bool IsDiffusedLocationItem(LocationClusterItem locationClusterItem)
    {
        return locationClusterItem.ObservationSummary?.DiffusedCount > 0 ||
               locationClusterItem.Observations.Any(observation => observation.IsGeneralized);
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

    private static void ClearActualGeometryMetadata(LocationClusterItem locationClusterItem)
    {
        locationClusterItem.ActualGeometry = null;
        locationClusterItem.ActualGeometryIsSimplifiedToPoint = null;
        locationClusterItem.ActualGeometrySimplificationReason = null;
        locationClusterItem.ActualGeometryWidthPx = null;
        locationClusterItem.ActualGeometryHeightPx = null;
        locationClusterItem.ActualGeometryAreaPx = null;
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
