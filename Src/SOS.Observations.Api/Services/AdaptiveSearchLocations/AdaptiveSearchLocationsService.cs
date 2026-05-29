using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Gis;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Search.Enums;
using SOS.Lib.Models.Search.Filters;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Services.AdaptiveSearch;
using SOS.Observations.Api.Services.Interfaces;
using SOS.Shared.Api.Extensions.Dto;
using System.Diagnostics;
using System.Text.Json.Nodes;
using ProcessedLocationType = SOS.Lib.Enums.LocationType;

namespace SOS.Observations.Api.Services.AdaptiveSearchLocations;

internal sealed class AdaptiveSearchLocationsService : IAdaptiveSearchLocationsService
{
    private const string DefaultSortBy = "";
    private const SearchSortOrder DefaultSortOrder = SearchSortOrder.Asc;
    private const string DefaultTranslationCultureCode = CultureCodeHelper.DefaultCultureCode;

    private readonly IObservationManager _observationManager;
    private readonly ILocationGeometryLookupService _locationGeometryLookupService;
    private readonly IAdaptiveSearchLocationsCache _cache;
    private readonly IAdaptiveSearchLocationAggregator _aggregator;
    private readonly IAdaptiveSearchLocationClusterItemFactory _clusterItemFactory;

    public AdaptiveSearchLocationsService(
        IObservationManager observationManager,
        ILocationGeometryLookupService locationGeometryLookupService,
        IAdaptiveSearchLocationsCache cache,
        IAdaptiveSearchLocationAggregator aggregator,
        IAdaptiveSearchLocationClusterItemFactory clusterItemFactory)
    {
        _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
        _locationGeometryLookupService = locationGeometryLookupService ?? throw new ArgumentNullException(nameof(locationGeometryLookupService));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
        _clusterItemFactory = clusterItemFactory ?? throw new ArgumentNullException(nameof(clusterItemFactory));
    }

    public async Task<AdaptiveSearchLocationsResult> SearchAsync(AdaptiveSearchLocationsRequest request)
    {
        var timings = new AdaptiveSearchLocationsTimings();
        var canUseLocationsCache = request.UseCache && !request.CreateSampleObservations;
        var locationsCacheKey = canUseLocationsCache
            ? _cache.CreateCacheKey(
                request.RoleId,
                request.AuthorizationApplicationIdentifier,
                request.SensitiveObservations,
                request.Filter)
            : null;

        var bbox = request.Bbox;
        var cacheStatus = canUseLocationsCache ? "MISS" : "BYPASS";
        var cacheLookupStopwatch = Stopwatch.StartNew();
        var cachedLocationAggregates = new List<CachedLocationAggregate>();
        var cacheHit = canUseLocationsCache &&
                       _cache.TryGet(locationsCacheKey!, request.InputBbox, out cachedLocationAggregates);
        timings.CacheLookup = cacheLookupStopwatch.Elapsed;

        if (cacheHit)
        {
            var cachedObservationCount = cachedLocationAggregates.Sum(location => location.ObservationSummary.ObservationCount);
            if (cachedObservationCount <= request.ObservationsLimit)
            {
                if (!request.ReturnInputBBox || bbox == null)
                {
                    bbox = _cache.CreateBoundingBox(cachedLocationAggregates);
                }

                var cachedClusteringBbox = request.InputBbox ?? bbox;
                Dictionary<string, Geometry>? cachedPolygonLocationGeometries = null;
                if (request.CreatePolygons != CreatePolygonsMode.No && cachedLocationAggregates.Count > 0)
                {
                    var polygonStopwatch = Stopwatch.StartNew();
                    cachedPolygonLocationGeometries = await GetPolygonLocationGeometriesAsync(cachedLocationAggregates);
                    timings.PolygonGeometries = polygonStopwatch.Elapsed;
                }

                var clusteringStopwatch = Stopwatch.StartNew();
                var cachedLocationClusterItems = request.Cluster
                    ? _clusterItemFactory.CreateClusterItems(
                        cachedLocationAggregates,
                        cachedClusteringBbox,
                        request.ClusteringAlgorithm,
                        request.MetersPerPixel,
                        request.ClusterRadiusPx,
                        request.MinLocationsPerCluster,
                        request.MinimumLocationCountBeforeClustering,
                        cachedPolygonLocationGeometries,
                        request.CreatePolygons)
                    : _clusterItemFactory.CreateUnclusteredItems(
                        cachedLocationAggregates,
                        cachedPolygonLocationGeometries,
                        request.CreatePolygons,
                        request.MetersPerPixel);
                timings.Clustering = clusteringStopwatch.Elapsed;

                return new AdaptiveSearchLocationsResult
                {
                    ResultType = "locations",
                    LocationClusterItems = cachedLocationClusterItems,
                    Bbox = bbox,
                    ObservationsTotalCount = cachedObservationCount,
                    FeatureCount = cachedLocationClusterItems.Count(item => item.Geometry != null),
                    LocationsBeforeClustering = cachedLocationAggregates.Count,
                    CacheStatus = "HIT",
                    Timings = timings
                };
            }
        }

        var searchFilter = request.Filter.ToSearchFilterInternal(
            request.UserId,
            DefaultTranslationCultureCode,
            DefaultSortBy,
            DefaultSortOrder);
        searchFilter.Output ??= new OutputFilter();
        searchFilter.Output.Fields = GetLocationClusterItemOutputFields();
        searchFilter.Output.ExcludeFields = new List<string>();

        var countAndExtentStopwatch = Stopwatch.StartNew();
        var countAndExtentResult = await _observationManager.GetCountAndExtentAsync(
            request.RoleId,
            request.AuthorizationApplicationIdentifier,
            searchFilter);
        timings.ElasticsearchCountAndExtent = countAndExtentStopwatch.Elapsed;

        if (!request.ReturnInputBBox || bbox == null)
        {
            bbox = countAndExtentResult.Extent;
        }

        var clusteringBbox = request.InputBbox ?? bbox;
        if (countAndExtentResult.Count > request.ObservationsLimit)
        {
            var zoom = GeoTileHelper.CalculateGeotileZoom(bbox, request.MaxGridCells, request.MaxZoom);
            var geogridStopwatch = Stopwatch.StartNew();
            var gridResult = await _observationManager.GetGeogridTileAggregationAsync(
                request.RoleId,
                request.AuthorizationApplicationIdentifier,
                searchFilter,
                zoom);
            timings.ElasticsearchGeogrid = geogridStopwatch.Elapsed;

            return new AdaptiveSearchLocationsResult
            {
                ResultType = "geogrid",
                GeoJson = gridResult.GetFeatureCollectionGeoJson(CoordinateSys.WGS84, bbox),
                Bbox = bbox,
                ObservationsTotalCount = countAndExtentResult.Count,
                FeatureCount = gridResult.GridCellTileCount,
                Zoom = zoom,
                CacheStatus = "BYPASS",
                Timings = timings
            };
        }

        var observationsStopwatch = Stopwatch.StartNew();
        var result = await _observationManager.GetChunkAsync(
            request.RoleId,
            request.AuthorizationApplicationIdentifier,
            searchFilter,
            0,
            request.ObservationsLimit);
        timings.ElasticsearchObservations = observationsStopwatch.Elapsed;
        if (result == null)
        {
            throw new Exception("Something went wrong when your query was executed. Make sure your filter is correct.");
        }

        var records = result.Records?.ToList() ?? new List<JsonObject>();
        if (request.CreateSampleObservations)
        {
            records = _aggregator.CreateSampleObservations(records);
        }

        var locationAggregates = request.CreateSampleObservations
            ? null
            : _aggregator.CreateAggregates(records);
        if (canUseLocationsCache && locationAggregates != null)
        {
            _cache.Set(locationsCacheKey!, request.InputBbox ?? bbox, locationAggregates);
        }

        Dictionary<string, Geometry>? polygonLocationGeometries = null;
        if (request.CreatePolygons != CreatePolygonsMode.No && (locationAggregates?.Count ?? records.Count) > 0)
        {
            var polygonStopwatch = Stopwatch.StartNew();
            polygonLocationGeometries = locationAggregates != null
                ? await GetPolygonLocationGeometriesAsync(locationAggregates)
                : await GetPolygonLocationGeometriesAsync(records);
            timings.PolygonGeometries = polygonStopwatch.Elapsed;
        }

        var locationProcessingStopwatch = Stopwatch.StartNew();
        var locationClusterItems = locationAggregates != null
            ? request.Cluster
                ? _clusterItemFactory.CreateClusterItems(
                    locationAggregates,
                    clusteringBbox,
                    request.ClusteringAlgorithm,
                    request.MetersPerPixel,
                    request.ClusterRadiusPx,
                    request.MinLocationsPerCluster,
                    request.MinimumLocationCountBeforeClustering,
                    polygonLocationGeometries,
                    request.CreatePolygons)
                : _clusterItemFactory.CreateUnclusteredItems(
                    locationAggregates,
                    polygonLocationGeometries,
                    request.CreatePolygons,
                    request.MetersPerPixel)
            : request.Cluster
                ? _clusterItemFactory.CreateClusterItems(
                    records,
                    clusteringBbox,
                    request.ClusteringAlgorithm,
                    request.MetersPerPixel,
                    request.ClusterRadiusPx,
                    request.MinLocationsPerCluster,
                    request.MinimumLocationCountBeforeClustering,
                    polygonLocationGeometries,
                    request.CreatePolygons)
                : _clusterItemFactory.CreateUnclusteredItems(
                    records,
                    polygonLocationGeometries,
                    request.CreatePolygons,
                    request.MetersPerPixel);
        timings.Clustering = locationProcessingStopwatch.Elapsed;

        return new AdaptiveSearchLocationsResult
        {
            ResultType = "locations",
            LocationClusterItems = locationClusterItems,
            Bbox = bbox,
            ObservationsTotalCount = countAndExtentResult.Count,
            FeatureCount = locationClusterItems.Count(item => item.Geometry != null),
            LocationsBeforeClustering = locationAggregates?.Count ?? _aggregator.CountLocations(records),
            CacheStatus = cacheStatus,
            Timings = timings
        };
    }

    public static ICollection<string> GetLocationClusterItemOutputFields()
    {
        return new List<string>
        {
            "isGeneralized",
            "sensitive",
            "artportalenInternal.invasiveSpeciesTreatment.id",
            "location.locationId",
            "location.type",
            "location.decimalLatitude",
            "location.decimalLongitude",
            "location.coordinateUncertaintyInMeters",
            "location.sweref99TmX",
            "location.sweref99TmY",
            "location.municipality",
            "location.locality",
            "occurrence.occurrenceId",
            "occurrence.isPositiveObservation",
            "occurrence.isNotRediscoveredObservation",
            "occurrence.isNeverFoundObservation",
            "event.startYear",
            "event.endYear",
            "taxon.id",
            "taxon.attributes.isRedlisted",
            "taxon.attributes.isInvasiveInSweden"
        };
    }

    private async Task<Dictionary<string, Geometry>?> GetPolygonLocationGeometriesAsync(IEnumerable<JsonObject> records)
    {
        return await _locationGeometryLookupService.GetPolygonLocationGeometriesAsync(records);
    }

    private async Task<Dictionary<string, Geometry>?> GetPolygonLocationGeometriesAsync(IEnumerable<CachedLocationAggregate> locations)
    {
        return await _locationGeometryLookupService.GetPolygonLocationGeometriesAsync(
            locations.Select(location => (
                (ProcessedLocationType?)location.Location.Type,
                location.LocationIds.Count > 0
                    ? location.LocationIds
                    : string.IsNullOrWhiteSpace(location.Location.LocationId)
                        ? Enumerable.Empty<string>()
                        : new[] { location.Location.LocationId })));
    }
}
