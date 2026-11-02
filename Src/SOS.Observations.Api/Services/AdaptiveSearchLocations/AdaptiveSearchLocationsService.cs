using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Requests;
using NetTopologySuite.Geometries;
using NetTopologySuite.Features;
using SOS.Lib.Enums;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.Gis;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Managers;
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
    private readonly IAdaptiveSearchLocationFeatureFactory _clusterItemFactory;

    public AdaptiveSearchLocationsService(
        IObservationManager observationManager,
        ILocationGeometryLookupService locationGeometryLookupService,
        IAdaptiveSearchLocationsCache cache,
        IAdaptiveSearchLocationAggregator aggregator,
        IAdaptiveSearchLocationFeatureFactory clusterItemFactory)
    {
        _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
        _locationGeometryLookupService = locationGeometryLookupService ?? throw new ArgumentNullException(nameof(locationGeometryLookupService));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
        _clusterItemFactory = clusterItemFactory ?? throw new ArgumentNullException(nameof(clusterItemFactory));
    }

    public async Task<AdaptiveSearchLocationsResult> SearchAsync(AdaptiveSearchLocationsRequest request)
    {
        request.CancellationToken.ThrowIfCancellationRequested();
        var searchFilter = request.Filter.ToSearchFilterInternal(
            request.UserId,
            DefaultTranslationCultureCode,
            DefaultSortBy,
            DefaultSortOrder);
        var strategy = AdaptiveSearchLocationsStrategySelector.Resolve(request, searchFilter);
        var clusterPixels = AdaptiveSearchLocationsClusteringPolicy.ShouldClusterPixels(request);
        var proximityClusterRadiusPx = AdaptiveSearchLocationsClusteringPolicy.GetProximityClusterRadiusPx(request);
        if (request.BoundingBoxIsEmpty)
        {
            var geographicAreas = await GetGeoJsonGeographicAreasAsync(request, searchFilter);
            return new AdaptiveSearchLocationsResult
            {
                ResultType = "locations",
                AdaptiveSearchStrategy = strategy,
                LocationFeatures = Array.Empty<LocationFeature>(),
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
        var context = new LocationResultContext(request, strategy, searchFilter, timings, clusterPixels, proximityClusterRadiusPx);
        var canUseLocationsCache = request.UseCache &&
                                   request.Filter.ProtectionFilter == ProtectionFilterDto.Public &&
                                   request.Filter.ObservedByMe != true &&
                                   request.Filter.ReportedByMe != true &&
                                   !request.CreateSampleObservations &&
                                   CanUseLocationsCache(strategy);
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
        var requireExactBoundingBox = request.Filter.Geographics?.ConsiderObservationAccuracy == true ||
                                      request.Filter.Geographics?.ConsiderDisturbanceRadius == true;
        var cacheHit = canUseLocationsCache &&
                       _cache.TryGet(locationsCacheKey!, request.InputBbox, out cachedLocationAggregates, requireExactBoundingBox);
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

                return await CreateLocationsResultAsync(context,
                    new LocationResultData(cachedLocationAggregates, null, cachedObservationCount, cachedLocationAggregates.Count, "HIT"),
                    bbox, request.InputBbox ?? bbox);
            }
        }

        searchFilter.Output ??= new OutputFilter();
        searchFilter.Output.Fields = GetLocationFeatureOutputFields();
        searchFilter.Output.ExcludeFields = new List<string>();

        var countAndExtentStopwatch = Stopwatch.StartNew();
        var preparedFilter = await _observationManager.PrepareObservationFilterAsync(
            request.RoleId,
            request.AuthorizationApplicationIdentifier,
            searchFilter).WaitAsync(request.CancellationToken);
        preparedFilter = preparedFilter.WithCancellation(request.CancellationToken);
        var includeExtent = !request.ReturnInputBBox || bbox == null;
        var countAndExtentResult = await _observationManager.GetCountAndExtentAsync(preparedFilter, includeExtent);
        request.CancellationToken.ThrowIfCancellationRequested();
        timings.ElasticsearchCountAndExtent = countAndExtentStopwatch.Elapsed;

        if (includeExtent)
        {
            bbox = countAndExtentResult.Extent;
        }

        if (countAndExtentResult.Count == 0)
        {
            return CreateEmptyLocationsResult(
                request,
                strategy,
                timings,
                bbox,
                countAndExtentResult,
                cacheStatus,
                await GetGeoJsonGeographicAreasAsync(request, searchFilter));
        }

        var clusteringBbox = request.InputBbox ?? bbox;
        var shouldFetchObservations = ShouldFetchObservations(strategy, request.ObservationsLimit, countAndExtentResult.Count);
        if (!shouldFetchObservations)
        {
            if (ShouldTryRepresentativeLocations(strategy, request.LocationAggregationLimit, countAndExtentResult.EstimatedLocationCount))
            {
                var representativeLocationsResult = await TryGetRepresentativeLocationsAsync(context, bbox, preparedFilter, countAndExtentResult, clusteringBbox);
                if (representativeLocationsResult != null)
                {
                    return representativeLocationsResult;
                }
            }

            var zoom = GeoTileHelper.CalculateGeotileZoom(request.InputBbox ?? bbox, request.MaxGridCells, request.MaxZoom);
            var geogridStopwatch = Stopwatch.StartNew();
            var gridResult = await _observationManager.GetGeogridTileAggregationAsync(
                preparedFilter,
                zoom);
            request.CancellationToken.ThrowIfCancellationRequested();
        timings.ElasticsearchGeogrid = geogridStopwatch.Elapsed;

            var geographicAreas = await GetGeoJsonGeographicAreasAsync(request, searchFilter);
            return new AdaptiveSearchLocationsResult
            {
                ResultType = "geogrid",
                AdaptiveSearchStrategy = strategy,
                GeoJson = AppendGeographicAreasToGeoJson(
                    gridResult.GetFeatureCollectionGeoJson(CoordinateSys.WGS84, bbox),
                    geographicAreas),
                Bbox = bbox,
                ObservationsTotalCount = countAndExtentResult.Count,
                EstimatedLocationCount = countAndExtentResult.EstimatedLocationCount,
                FeatureCount = gridResult.GridCellTileCount,
                Zoom = zoom,
                CacheStatus = "BYPASS",
                LocationSummaryApproximated = false,
                Timings = timings
            };
        }

        var observationsStopwatch = Stopwatch.StartNew();
        var result = await _observationManager.GetChunkAsync(
            preparedFilter,
            0,
            request.ObservationsLimit,
            options: new SearchExecutionOptions { KnownTotalCount = countAndExtentResult.Count });
        request.CancellationToken.ThrowIfCancellationRequested();
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
        request.CancellationToken.ThrowIfCancellationRequested();
        if (canUseLocationsCache && locationAggregates != null)
        {
            _cache.Set(locationsCacheKey!, request.InputBbox ?? bbox, locationAggregates);
        }

        return await CreateLocationsResultAsync(context,
            new LocationResultData(locationAggregates, locationAggregates is null ? records : null,
                countAndExtentResult.Count, countAndExtentResult.EstimatedLocationCount, cacheStatus),
            bbox, clusteringBbox);
    }

    private static AdaptiveSearchLocationsResult CreateEmptyLocationsResult(
        AdaptiveSearchLocationsRequest request,
        AdaptiveSearchLocationsStrategy strategy,
        AdaptiveSearchLocationsTimings timings,
        LatLonBoundingBoxDto? bbox,
        (long Count, LatLonBoundingBoxDto? Extent, long EstimatedLocationCount) countAndExtentResult,
        string cacheStatus,
        List<Feature>? geographicAreas)
    {
        return new AdaptiveSearchLocationsResult
        {
            ResultType = "locations",
            AdaptiveSearchStrategy = strategy,
            LocationFeatures = Array.Empty<LocationFeature>(),
            Bbox = bbox,
            ObservationsTotalCount = countAndExtentResult.Count,
            EstimatedLocationCount = countAndExtentResult.EstimatedLocationCount,
            FeatureCount = 0,
            LocationsBeforeClustering = 0,
            CacheStatus = request.UseCache ? cacheStatus : "BYPASS",
            LocationSummaryApproximated = false,
            GeographicAreas = geographicAreas,
            Timings = timings
        };
    }

    private async Task<AdaptiveSearchLocationsResult?> TryGetRepresentativeLocationsAsync(
        LocationResultContext context, LatLonBoundingBoxDto? bbox, PreparedObservationFilter preparedFilter,
        (long Count, LatLonBoundingBoxDto? Extent, long EstimatedLocationCount) countAndExtentResult,
        LatLonBoundingBoxDto? clusteringBbox)
    {
        var request = context.Request;
        var searchFilter = context.Filter;
        var timings = context.Timings;
        var representativeLocationsStopwatch = Stopwatch.StartNew();
        var representativeLocationsResult = await _observationManager.GetLocationRepresentativeChunkAsync(
            preparedFilter,
            request.LocationAggregationLimit + 1,
            LocationCollapseField);
        request.CancellationToken.ThrowIfCancellationRequested();
        timings.ElasticsearchRepresentativeLocations = representativeLocationsStopwatch.Elapsed;

        if (representativeLocationsResult?.Records != null)
        {
            var representativeLocationRecords = representativeLocationsResult.Records.ToList();
            // Check the raw representatives before coincident locations reduce their count.
            if (representativeLocationRecords.Count >= request.LocationAggregationLimit)
            {
                return null;
            }
            var representativeLocationAggregates = _aggregator.CreateAggregates(
                representativeLocationRecords,
                representativeLocationsResult.ObservationCountByLocationId,
                isSummaryApproximated: !IsFilteredToSingleOccurrence(searchFilter),
                isPresentationStatusApproximated: !IsFilteredToSingleOccurrence(searchFilter) &&
                    !AdaptiveSearchLocationsStrategySelector.CanDetermineRepresentativePresentationStatus(searchFilter, request.CutoffYear));
            if (!request.ReturnInputBBox || bbox == null)
            {
                bbox = _cache.CreateBoundingBox(representativeLocationAggregates);
            }

            return await CreateLocationsResultAsync(
                context,
                new LocationResultData(representativeLocationAggregates, null, countAndExtentResult.Count,
                    countAndExtentResult.EstimatedLocationCount, "BYPASS", "representative"),
                bbox, clusteringBbox);
        }

        return null;
    }
    private sealed record LocationResultContext(AdaptiveSearchLocationsRequest Request,
        AdaptiveSearchLocationsStrategy Strategy, SearchFilterInternal Filter,
        AdaptiveSearchLocationsTimings Timings, bool ClusterPixels, double ProximityRadiusPx);

    private sealed record LocationResultData(List<CachedLocationAggregate>? Aggregates,
        List<JsonObject>? SampleRecords, long ObservationCount, long? EstimatedLocationCount,
        string CacheStatus, string? AggregationMode = null);

    private async Task<AdaptiveSearchLocationsResult> CreateLocationsResultAsync(
        LocationResultContext context, LocationResultData data,
        LatLonBoundingBoxDto? bbox, LatLonBoundingBoxDto? clusteringBbox)
    {
        var request = context.Request;
        var locationsBeforeClustering = data.Aggregates?.Count ?? _aggregator.CountLocations(data.SampleRecords!);
        Dictionary<string, Geometry>? polygons = null;
        if (request.CreatePolygons != CreatePolygonsMode.No && (data.Aggregates?.Count ?? data.SampleRecords!.Count) > 0)
        {
            var stopwatch = Stopwatch.StartNew();
            polygons = data.Aggregates is not null
                ? await GetPolygonLocationGeometriesAsync(data.Aggregates, request.CancellationToken)
                : await GetPolygonLocationGeometriesAsync(data.SampleRecords!, request.CancellationToken);
            context.Timings.PolygonGeometries = stopwatch.Elapsed;
        }

        request.CancellationToken.ThrowIfCancellationRequested();
        var clusteringStopwatch = Stopwatch.StartNew();
        var options = new LocationFeatureOptions
        {
            BoundingBox = clusteringBbox, ClusterRadiusMeters = request.ClusterRadiusMeters,
            ClusterPixels = context.ClusterPixels, MetersPerPixel = request.MetersPerPixel,
            ClusterRadiusPx = request.ClusterRadiusPx, MinLocationsPerCluster = request.MinLocationsPerCluster,
            MinLocationCountBeforeClustering = request.MinimumLocationCountBeforeClustering,
            PolygonGeometries = polygons, CreatePolygons = request.CreatePolygons,
            ProximityClusterRadiusPx = context.ProximityRadiusPx
        };
        var features = data.Aggregates is not null
            ? _clusterItemFactory.CreateItems(data.Aggregates, options)
            : _clusterItemFactory.CreateItems(data.SampleRecords!, options);
        request.CancellationToken.ThrowIfCancellationRequested();
        context.Timings.Clustering = clusteringStopwatch.Elapsed;

        return new AdaptiveSearchLocationsResult
        {
            ResultType = "locations",
            AdaptiveSearchStrategy = context.Strategy,
            LocationFeatures = features,
            Bbox = bbox,
            ObservationsTotalCount = data.ObservationCount,
            EstimatedLocationCount = data.EstimatedLocationCount,
            FeatureCount = features.Count(feature => feature.Geometry != null),
            LocationsBeforeClustering = locationsBeforeClustering,
            CacheStatus = data.CacheStatus,
            LocationAggregationMode = data.AggregationMode,
            LocationSummaryApproximated = HasApproximatedLocationSummary(features),
            LocationPresentationStatusApproximated = HasApproximatedPresentationStatus(features),
            GeographicAreas = await GetGeoJsonGeographicAreasAsync(request, context.Filter).WaitAsync(request.CancellationToken),
            Timings = context.Timings
        };
    }

    private static bool ShouldFetchObservations(AdaptiveSearchLocationsStrategy strategy, int observationsLimit, long observationCount)
    {
        return strategy switch
        {
            AdaptiveSearchLocationsStrategy.ObservationsThenGrid => observationCount <= observationsLimit,
            AdaptiveSearchLocationsStrategy.ObservationsThenLocationsThenGrid => observationCount <= observationsLimit,
            AdaptiveSearchLocationsStrategy.LocationsThenGrid => false,
            _ => observationCount <= observationsLimit
        };
    }

    private static bool CanUseLocationsCache(AdaptiveSearchLocationsStrategy strategy)
    {
        return strategy is
            AdaptiveSearchLocationsStrategy.ObservationsThenGrid or
            AdaptiveSearchLocationsStrategy.ObservationsThenLocationsThenGrid;
    }

    private static bool ShouldTryRepresentativeLocations(AdaptiveSearchLocationsStrategy strategy, int locationAggregationLimit, long estimatedLocationCount)
    {
        return strategy switch
        {
            AdaptiveSearchLocationsStrategy.ObservationsThenLocationsThenGrid or
            AdaptiveSearchLocationsStrategy.LocationsThenGrid =>
                estimatedLocationCount < locationAggregationLimit,
            _ => false
        };
    }

    private static bool HasApproximatedLocationSummary(IEnumerable<LocationFeature>? locationFeatures)
    {
        return locationFeatures?.Any(feature => feature.IsSummaryApproximated == true) == true;
    }

    private static bool HasApproximatedPresentationStatus(IEnumerable<LocationFeature> locationFeatures)
    {
        return locationFeatures.Any(feature => feature.IsPresentationStatusApproximated);
    }

    /// <summary>
    /// Determines whether the search filter guarantees that at most one observation can match,
    /// i.e. the include filter contains exactly one occurrence id. In that case the representative
    /// location summary can never be approximated, since there is nothing more to aggregate.
    /// </summary>
    private static bool IsFilteredToSingleOccurrence(SearchFilterInternal searchFilter)
    {
        return searchFilter?.IncludeFilter?.OccurrenceIds?.Count() == 1;
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

        return await _observationManager.GetGeoGraphicsFiltersAsGeoJsonFeaturesAsync(searchFilter)
            .WaitAsync(request.CancellationToken);
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

    private static readonly string[] LocationFeatureOutputFields =
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

    public static ICollection<string> GetLocationFeatureOutputFields()
    {
        return LocationFeatureOutputFields;
    }

    private async Task<Dictionary<string, Geometry>?> GetPolygonLocationGeometriesAsync(IEnumerable<JsonObject> records, CancellationToken cancellationToken)
    {
        return await _locationGeometryLookupService.GetPolygonLocationGeometriesAsync(records).WaitAsync(cancellationToken);
    }

    private async Task<Dictionary<string, Geometry>?> GetPolygonLocationGeometriesAsync(IEnumerable<CachedLocationAggregate> locations, CancellationToken cancellationToken)
    {
        return await _locationGeometryLookupService.GetPolygonLocationGeometriesAsync(
            locations.Select(location => (
                (ProcessedLocationType?)location.Location.Type,
                location.LocationIds.Count > 0
                    ? location.LocationIds
                    : string.IsNullOrWhiteSpace(location.Location.LocationId)
                        ? Enumerable.Empty<string>()
                        : new[] { location.Location.LocationId }))).WaitAsync(cancellationToken);
    }
}
