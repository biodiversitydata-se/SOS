using NetTopologySuite.Geometries;
using NetTopologySuite.Features;
using SOS.Lib.Enums;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.Gis;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Search.Filters;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Services.AdaptiveSearch;
using SOS.Observations.Api.Services.Interfaces;
using System.Diagnostics;
using System.Text.Json.Nodes;
using ProcessedLocationType = SOS.Lib.Enums.LocationType;

namespace SOS.Observations.Api.Services.AdaptiveSearchLocations;

internal sealed class AdaptiveSearchLocationsService : IAdaptiveSearchLocationsService
{
    private const string DefaultSortBy = "";
    private const SearchSortOrder DefaultSortOrder = SearchSortOrder.Asc;
    private const string DefaultTranslationCultureCode = CultureCodeHelper.DefaultCultureCode;
    private const string LocationCollapseField = "location.locationId";

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
        if (request.BoundingBoxIsEmpty)
        {
            var geographicAreas = await GetGeoJsonGeographicAreasAsync(request);
            return new AdaptiveSearchLocationsResult
            {
                ResultType = "locations",
                LocationClusterItems = Array.Empty<LocationClusterItem>(),
                Bbox = request.ReturnInputBBox ? request.InputBbox : null,
                ObservationsTotalCount = 0,
                EstimatedLocationCount = 0,
                FeatureCount = 0,
                LocationsBeforeClustering = 0,
                CacheStatus = "BYPASS",
                GeographicAreas = geographicAreas
            };
        }

        var timings = new AdaptiveSearchLocationsTimings();
        var canUseLocationsCache = request.UseCache &&
                                   !request.CreateSampleObservations &&
                                   CanUseLocationsCache(request.AdaptiveSearchStrategy);
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
                    EstimatedLocationCount = cachedLocationAggregates.Count,
                    FeatureCount = cachedLocationClusterItems.Count(item => item.Geometry != null),
                    LocationsBeforeClustering = cachedLocationAggregates.Count,
                    CacheStatus = "HIT",
                    GeographicAreas = await GetGeoJsonGeographicAreasAsync(request),
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

        if (countAndExtentResult.Count == 0)
        {
            return CreateEmptyLocationsResult(
                request,
                timings,
                bbox,
                countAndExtentResult,
                cacheStatus,
                await GetGeoJsonGeographicAreasAsync(request, searchFilter));
        }

        var clusteringBbox = request.InputBbox ?? bbox;
        var shouldFetchObservations = ShouldFetchObservations(request, countAndExtentResult.Count);
        if (!shouldFetchObservations)
        {
            if (ShouldTryRepresentativeLocations(request, countAndExtentResult.EstimatedLocationCount))
            {
                (bool success, AdaptiveSearchLocationsResult value) = await TryGetRepresentativeLocationsAsync(
                    request,
                    timings,
                    bbox,
                    searchFilter,
                    countAndExtentResult,
                    clusteringBbox);
                if (success)
                {
                    return value;
                }
            }

            var zoom = GeoTileHelper.CalculateGeotileZoom(request.InputBbox ?? bbox, request.MaxGridCells, request.MaxZoom);
            var geogridStopwatch = Stopwatch.StartNew();
            var gridResult = await _observationManager.GetGeogridTileAggregationAsync(
                request.RoleId,
                request.AuthorizationApplicationIdentifier,
                searchFilter,
                zoom);
            timings.ElasticsearchGeogrid = geogridStopwatch.Elapsed;

            var geographicAreas = await GetGeoJsonGeographicAreasAsync(request, searchFilter);
            return new AdaptiveSearchLocationsResult
            {
                ResultType = "geogrid",
                GeoJson = AppendGeographicAreasToGeoJson(
                    gridResult.GetFeatureCollectionGeoJson(CoordinateSys.WGS84, bbox),
                    geographicAreas),
                Bbox = bbox,
                ObservationsTotalCount = countAndExtentResult.Count,
                EstimatedLocationCount = countAndExtentResult.EstimatedLocationCount,
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
            EstimatedLocationCount = countAndExtentResult.EstimatedLocationCount,
            FeatureCount = locationClusterItems.Count(item => item.Geometry != null),
            LocationsBeforeClustering = locationAggregates?.Count ?? _aggregator.CountLocations(records),
            CacheStatus = cacheStatus,
            GeographicAreas = await GetGeoJsonGeographicAreasAsync(request, searchFilter),
            Timings = timings
        };
    }

    private static AdaptiveSearchLocationsResult CreateEmptyLocationsResult(
        AdaptiveSearchLocationsRequest request,
        AdaptiveSearchLocationsTimings timings,
        Lib.Models.Gis.LatLonBoundingBox? bbox,
        (long Count, Lib.Models.Gis.LatLonBoundingBox? Extent, long EstimatedLocationCount) countAndExtentResult,
        string cacheStatus,
        List<Feature>? geographicAreas)
    {
        return new AdaptiveSearchLocationsResult
        {
            ResultType = "locations",
            LocationClusterItems = Array.Empty<LocationClusterItem>(),
            Bbox = bbox,
            ObservationsTotalCount = countAndExtentResult.Count,
            EstimatedLocationCount = countAndExtentResult.EstimatedLocationCount,
            FeatureCount = 0,
            LocationsBeforeClustering = 0,
            CacheStatus = request.UseCache ? cacheStatus : "BYPASS",
            GeographicAreas = geographicAreas,
            Timings = timings
        };
    }

    private async Task<(bool success, AdaptiveSearchLocationsResult value)> TryGetRepresentativeLocationsAsync(AdaptiveSearchLocationsRequest request, AdaptiveSearchLocationsTimings timings, Lib.Models.Gis.LatLonBoundingBox bbox, SearchFilterInternal searchFilter, (long Count, Lib.Models.Gis.LatLonBoundingBox? Extent, long EstimatedLocationCount) countAndExtentResult, Lib.Models.Gis.LatLonBoundingBox clusteringBbox)
    {
        var representativeLocationsStopwatch = Stopwatch.StartNew();
        var representativeLocationsResult = await _observationManager.GetLocationRepresentativeChunkAsync(
            request.RoleId,
            request.AuthorizationApplicationIdentifier,
            searchFilter,
            request.LocationAggregationLimit!.Value + 1,
            LocationCollapseField);
        timings.ElasticsearchRepresentativeLocations = representativeLocationsStopwatch.Elapsed;

        if (representativeLocationsResult?.Records != null)
        {
            var representativeLocationRecords = representativeLocationsResult.Records.ToList();
            var representativeLocationAggregates = _aggregator.CreateAggregates(
                representativeLocationRecords,
                representativeLocationsResult.ObservationCountByLocationId,
                isEstimated: true);
            if (representativeLocationAggregates.Count < request.LocationAggregationLimit.Value)
            {
                if (!request.ReturnInputBBox || bbox == null)
                {
                    bbox = _cache.CreateBoundingBox(representativeLocationAggregates);
                }

                Dictionary<string, Geometry>? representativePolygonLocationGeometries = null;
                if (request.CreatePolygons != CreatePolygonsMode.No && representativeLocationAggregates.Count > 0)
                {
                    var polygonStopwatch = Stopwatch.StartNew();
                    representativePolygonLocationGeometries = await GetPolygonLocationGeometriesAsync(representativeLocationAggregates);
                    timings.PolygonGeometries = polygonStopwatch.Elapsed;
                }

                var representativeLocationProcessingStopwatch = Stopwatch.StartNew();
                var representativeLocationClusterItems = request.Cluster
                    ? _clusterItemFactory.CreateClusterItems(
                        representativeLocationAggregates,
                        clusteringBbox,
                        request.ClusteringAlgorithm,
                        request.MetersPerPixel,
                        request.ClusterRadiusPx,
                        request.MinLocationsPerCluster,
                        request.MinimumLocationCountBeforeClustering,
                        representativePolygonLocationGeometries,
                        request.CreatePolygons)
                    : _clusterItemFactory.CreateUnclusteredItems(
                        representativeLocationAggregates,
                        representativePolygonLocationGeometries,
                        request.CreatePolygons,
                        request.MetersPerPixel);
                timings.Clustering = representativeLocationProcessingStopwatch.Elapsed;

                return (success: true, value: new AdaptiveSearchLocationsResult
                {
                    ResultType = "locations",
                    LocationClusterItems = representativeLocationClusterItems,
                    Bbox = bbox,
                    ObservationsTotalCount = countAndExtentResult.Count,
                    EstimatedLocationCount = countAndExtentResult.EstimatedLocationCount,
                    FeatureCount = representativeLocationClusterItems.Count(item => item.Geometry != null),
                    LocationsBeforeClustering = representativeLocationAggregates.Count,
                    CacheStatus = "BYPASS",
                    LocationAggregation = "representative",
                    LocationPropertiesEstimated = true,
                    GeographicAreas = await GetGeoJsonGeographicAreasAsync(request, searchFilter),
                    Timings = timings
                });
            }
        }

        return (success: false, value: null);
    }

    private static bool ShouldFetchObservations(AdaptiveSearchLocationsRequest request, long observationCount)
    {
        return request.AdaptiveSearchStrategy switch
        {
            AdaptiveSearchLocationsStrategy.ObservationsThenGrid => observationCount <= request.ObservationsLimit,
            AdaptiveSearchLocationsStrategy.ObservationsThenLocationsThenGrid => observationCount <= request.ObservationsLimit,
            AdaptiveSearchLocationsStrategy.LocationsThenGrid => false,
            _ => observationCount <= request.ObservationsLimit
        };
    }

    private static bool CanUseLocationsCache(AdaptiveSearchLocationsStrategy strategy)
    {
        return strategy is
            AdaptiveSearchLocationsStrategy.ObservationsThenGrid or
            AdaptiveSearchLocationsStrategy.ObservationsThenLocationsThenGrid;
    }

    private static bool ShouldTryRepresentativeLocations(AdaptiveSearchLocationsRequest request, long estimatedLocationCount)
    {
        return request.AdaptiveSearchStrategy switch
        {
            AdaptiveSearchLocationsStrategy.ObservationsThenLocationsThenGrid or
            AdaptiveSearchLocationsStrategy.LocationsThenGrid =>
                request.LocationAggregationLimit.HasValue &&
                estimatedLocationCount < request.LocationAggregationLimit.Value,
            _ => false
        };
    }

    private async Task<List<Feature>?> GetGeoJsonGeographicAreasAsync(
        AdaptiveSearchLocationsRequest request,
        SearchFilterInternal? searchFilter = null)
    {
        if (!request.IncludeFilterAreasInResult)
        {
            return null;
        }

        searchFilter ??= request.Filter.ToSearchFilterInternal(
            request.UserId,
            DefaultTranslationCultureCode,
            DefaultSortBy,
            DefaultSortOrder);

        return await _observationManager.GetGeoGraphicsFiltersAsGeoJsonFeaturesAsync(searchFilter);
    }

    private static string? AppendGeographicAreasToGeoJson(string? geoJson, IReadOnlyCollection<Feature>? geographicAreas)
    {
        if (string.IsNullOrWhiteSpace(geoJson) || geographicAreas == null || geographicAreas.Count == 0)
        {
            return geoJson;
        }

        var root = JsonNode.Parse(geoJson) as JsonObject;
        var features = root?["features"] as JsonArray;
        if (features == null)
        {
            return geoJson;
        }

        var geoJsonWriter = new NetTopologySuite.IO.GeoJsonWriter();
        foreach (var area in geographicAreas)
        {
            var areaNode = JsonNode.Parse(geoJsonWriter.Write(area));
            if (areaNode != null)
            {
                features.Add(areaNode);
            }
        }

        return root?.ToJsonString() ?? geoJson;
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
