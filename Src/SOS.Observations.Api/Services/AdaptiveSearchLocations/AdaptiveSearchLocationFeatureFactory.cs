﻿using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Gis;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using System.Text.Json.Nodes;
using ProcessedLocationType = SOS.Lib.Enums.LocationType;

namespace SOS.Observations.Api.Services.AdaptiveSearchLocations;

internal interface IAdaptiveSearchLocationFeatureFactory
{
    List<LocationFeature> CreateClusterItems(List<CachedLocationAggregate> locationAggregates, LatLonBoundingBox? clusteringBbox, LocationClusteringAlgorithm clusteringAlgorithm, double? metersPerPixel, double clusterRadiusPx, int minLocationsPerCluster, int minLocationCountBeforeClustering, Dictionary<string, Geometry>? polygonLocationGeometries, CreatePolygonsMode createPolygons);
    List<LocationFeature> CreateUnclusteredItems(List<CachedLocationAggregate> locationAggregates, Dictionary<string, Geometry>? polygonLocationGeometries, CreatePolygonsMode createPolygons, double? metersPerPixel);
    List<LocationFeature> CreateClusterItems(IEnumerable<JsonObject> records, LatLonBoundingBox? clusteringBbox, LocationClusteringAlgorithm clusteringAlgorithm, double? metersPerPixel, double clusterRadiusPx, int minLocationsPerCluster, int minLocationCountBeforeClustering, Dictionary<string, Geometry>? polygonLocationGeometries, CreatePolygonsMode createPolygons);
    List<LocationFeature> CreateUnclusteredItems(IEnumerable<JsonObject> records, Dictionary<string, Geometry>? polygonLocationGeometries, CreatePolygonsMode createPolygons, double? metersPerPixel);
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

    public List<LocationFeature> CreateClusterItems(
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
            return new List<LocationFeature>();
        }

        var aggregatesByDistinctLocationId = locationAggregates.ToDictionary(
            location => location.DistinctLocationId,
            StringComparer.Ordinal);

        var locationFeatures = clusteringBbox == null
            ? locationAggregates
                .Select(location => CreateLocationFeatureFromCachedAggregates(new[] { location }, false))
                .ToList()
            : CreateLocationFeaturesFromCachedAggregates(
                locationAggregates,
                aggregatesByDistinctLocationId,
                clusteringBbox,
                clusteringAlgorithm,
                metersPerPixel,
                clusterRadiusPx,
                minLocationsPerCluster,
                minLocationCountBeforeClustering);

        ApplyFallbackClusterRadiusMeters(locationFeatures, metersPerPixel, clusterRadiusPx);
        ApplyGeometries(locationFeatures, polygonLocationGeometries, createPolygons, metersPerPixel);
        return locationFeatures;
    }

    public List<LocationFeature> CreateUnclusteredItems(
        List<CachedLocationAggregate> locationAggregates,
        Dictionary<string, Geometry>? polygonLocationGeometries,
        CreatePolygonsMode createPolygons,
        double? metersPerPixel)
    {
        var locationFeatures = locationAggregates
            .Select(location => CreateLocationFeatureFromCachedAggregates(new[] { location }, false))
            .ToList();

        ApplyGeometries(locationFeatures, polygonLocationGeometries, createPolygons, metersPerPixel);
        return locationFeatures;
    }

    public List<LocationFeature> CreateClusterItems(
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
            .Where(HasUsableLocation)
            .ToList();
        if (observations.Count == 0)
        {
            return new List<LocationFeature>();
        }

        var clusterer = CreateLocationClusterer(clusteringAlgorithm, minLocationCountBeforeClustering, minLocationsPerCluster);
        var locationFeatures = clusteringBbox == null
            ? _aggregator.GroupObservationsByLocation(observations)
                .Select(LocationFeature.FromSingleLocation)
                .ToList()
            : metersPerPixel.HasValue
                ? clusterer.ClusterLocations(observations, clusteringBbox, metersPerPixel.Value, clusterRadiusPx)
                : clusterer.ClusterLocations(observations, clusteringBbox);

        locationFeatures = NormalizeNonClusteredLocationItems(locationFeatures);
        ApplyFallbackClusterRadiusMeters(locationFeatures, metersPerPixel, clusterRadiusPx);
        ApplyGeometries(locationFeatures, polygonLocationGeometries, createPolygons, metersPerPixel);
        return locationFeatures;
    }

    public List<LocationFeature> CreateUnclusteredItems(
        IEnumerable<JsonObject> records,
        Dictionary<string, Geometry>? polygonLocationGeometries,
        CreatePolygonsMode createPolygons,
        double? metersPerPixel)
    {
        var observations = records
            .ToObservationsArray()
            .Where(HasUsableLocation)
            .ToList();

        var locationFeatures = _aggregator.GroupObservationsByLocation(observations)
            .Select(LocationFeature.FromSingleLocation)
            .ToList();

        ApplyGeometries(locationFeatures, polygonLocationGeometries, createPolygons, metersPerPixel);
        return locationFeatures;
    }

    private List<LocationFeature> CreateLocationFeaturesFromCachedAggregates(
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

        var result = new List<LocationFeature>(clusteredItems.Count);
        foreach (var clusteredItem in clusteredItems)
        {
            var aggregates = GetCachedAggregatesForClusterItem(clusteredItem, aggregatesByDistinctLocationId);
            if (aggregates.Count == 0)
            {
                continue;
            }

            result.Add(CreateLocationFeatureFromCachedAggregates(
                aggregates,
                clusteredItem.IsCluster,
                clusteredItem.ClusterRadiusMeters));
        }

        return result;
    }

    private static List<CachedLocationAggregate> GetCachedAggregatesForClusterItem(
        LocationFeature locationFeature,
        Dictionary<string, CachedLocationAggregate> aggregatesByDistinctLocationId)
    {
        var distinctLocationIds = locationFeature.Locations.Count > 0
            ? locationFeature.Locations.Select(location => location.DistinctLocationId)
            : locationFeature.Observations.Select(observation => observation.Location?.DistinctLocationId);

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

    private static LocationFeature CreateLocationFeatureFromCachedAggregates(
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

        var locationFeature = new LocationFeature
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
            ClusterRadiusMeters = isCluster ? clusterRadiusMeters : null,
            IsSummaryApproximated = observationSummary.ObservationCount == 1
                ? null
                : aggregateList.Any(aggregate => aggregate.IsSummaryApproximated) ? true : null
        };

        if (isCluster)
        {
            var centroidLatitude = locations.Average(location => location.DecimalLatitude!.Value);
            var centroidLongitude = locations.Average(location => location.DecimalLongitude!.Value);
            locationFeature.Location = new SOS.Lib.Models.Processed.Observation.Location
            {
                DecimalLatitude = centroidLatitude,
                DecimalLongitude = centroidLongitude
            };
            locationFeature.Geometry = new Point(centroidLongitude, centroidLatitude);
        }
        else
        {
            locationFeature.Location = locations[0];
            locationFeature.Geometry = CreatePointGeometry(locations[0]);
        }

        return locationFeature;
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

    private static void ApplyGeometries(List<LocationFeature> locationFeatures, Dictionary<string, Geometry>? polygonLocationGeometries, CreatePolygonsMode createPolygons, double? metersPerPixel)
    {
        var resolvedMetersPerPixel = createPolygons == CreatePolygonsMode.WhenNeeded ? metersPerPixel : null;

        foreach (var feature in locationFeatures)
        {
            if (feature.IsCluster)
            {
                feature.Geometry = CreatePointGeometry(feature.Location);
                continue;
            }

            var location = feature.Location;
            var pointGeometry = CreatePointGeometry(location);

            if (IsDiffusedLocationItem(feature))
            {
                feature.Geometry = CreateDiffusedLocationSquareGeometry(location) ?? pointGeometry;
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
                        feature.Geometry = polygonGeometry;
                        continue;
                    }

                    var decision = GetPolygonGeometryDisplayDecision(polygonGeometry, resolvedMetersPerPixel);
                    if (decision.UsePolygon)
                    {
                        feature.Geometry = polygonGeometry;
                        continue;
                    }
                }

                feature.Geometry = pointGeometry;
                continue;
            }

            feature.Geometry = pointGeometry;
        }
    }

    private static bool IsDiffusedLocationItem(LocationFeature locationFeature)
    {
        return locationFeature.ObservationSummary?.DiffusedCount > 0 ||
               locationFeature.Observations.Any(observation => observation.IsGeneralized);
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
