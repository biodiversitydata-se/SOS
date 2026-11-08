using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Requests;
using NetTopologySuite.Geometries;
using NetTopologySuite.Features;
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

/// <summary>
/// Searches observations within a bounding box and returns them as location features, or as a geogrid
/// when there are too many to return per location. See Docs/AdaptiveSearchLocationsInternal.md.
/// </summary>
/// <remarks>
/// The search is done in the following order, where each step returns if it can produce a result:
/// <list type="number">
/// <item>Cached location aggregates (public, user independent searches only).</item>
/// <item>Count and extent. An empty result is returned when nothing matches.</item>
/// <item>All observations, when the count is within <see cref="AdaptiveSearchLocationsRequest.ObservationsLimit"/>.</item>
/// <item>One representative observation per location, when the location count is within <see cref="AdaptiveSearchLocationsRequest.LocationAggregationLimit"/>.</item>
/// <item>Geogrid aggregation.</item>
/// </list>
/// </remarks>
internal sealed class AdaptiveSearchLocationsService : IAdaptiveSearchLocationsService
{
    private const string DefaultSortBy = "";
    private const SearchSortOrder DefaultSortOrder = SearchSortOrder.Asc;
    private const string DefaultTranslationCultureCode = CultureCodeHelper.DefaultCultureCode;
    private const string LocationCollapseField = "location.locationId";

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

    private readonly IObservationManager _observationManager;
    private readonly ILocationGeometryLookupService _locationGeometryLookupService;
    private readonly IAdaptiveSearchLocationsCache _cache;
    private readonly IAdaptiveSearchLocationAggregator _aggregator;
    private readonly IAdaptiveSearchLocationFeatureFactory _featureFactory;

    public AdaptiveSearchLocationsService(
        IObservationManager observationManager,
        ILocationGeometryLookupService locationGeometryLookupService,
        IAdaptiveSearchLocationsCache cache,
        IAdaptiveSearchLocationAggregator aggregator,
        IAdaptiveSearchLocationFeatureFactory featureFactory)
    {
        _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
        _locationGeometryLookupService = locationGeometryLookupService ?? throw new ArgumentNullException(nameof(locationGeometryLookupService));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
        _featureFactory = featureFactory ?? throw new ArgumentNullException(nameof(featureFactory));
    }

    /// <summary>
    /// The observation fields needed to create location features.
    /// </summary>
    public static ICollection<string> GetLocationFeatureOutputFields()
    {
        return LocationFeatureOutputFields;
    }

    public async Task<AdaptiveSearchLocationsResult> SearchAsync(AdaptiveSearchLocationsRequest request)
    {
        request.CancellationToken.ThrowIfCancellationRequested();
        var searchFilter = CreateSearchFilter(request);
        var strategy = AdaptiveSearchLocationsStrategySelector.Resolve(request, searchFilter);
        var context = new SearchContext(
            request,
            strategy,
            searchFilter,
            new AdaptiveSearchLocationsTimings(),
            // Started here so that the area lookup runs in parallel with the observation search.
            GetGeographicAreasAsync(request),
            AdaptiveSearchLocationsClusteringPolicy.ShouldClusterPixels(request),
            AdaptiveSearchLocationsClusteringPolicy.GetDetailClusterRadiusPx(request));

        if (request.BoundingBoxIsEmpty)
        {
            return await CreateEmptyResultAsync(
                context,
                request.ReturnInputBBox ? request.InputBbox : null,
                AdaptiveSearchLocationsCacheStatus.Bypass);
        }

        var cacheKey = CanUseLocationsCache(request, strategy)
            ? _cache.CreateCacheKey(
                request.RoleId,
                request.AuthorizationApplicationIdentifier,
                request.SensitiveObservations,
                request.Filter)
            : null;
        if (cacheKey != null)
        {
            var cachedResult = await TryCreateResultFromCacheAsync(context, cacheKey);
            if (cachedResult != null)
            {
                return cachedResult;
            }
        }

        var cacheStatus = cacheKey != null
            ? AdaptiveSearchLocationsCacheStatus.Miss
            : AdaptiveSearchLocationsCacheStatus.Bypass;
        var returnInputBbox = request.ReturnInputBBox && request.InputBbox != null;
        var countAndExtentStart = Stopwatch.GetTimestamp();
        var preparedFilter = await PrepareFilterAsync(request, searchFilter);
        var countAndExtent = await _observationManager.GetCountAndExtentAsync(preparedFilter, includeExtent: !returnInputBbox);
        request.CancellationToken.ThrowIfCancellationRequested();
        context.Timings.ElasticsearchCountAndExtent = Stopwatch.GetElapsedTime(countAndExtentStart);

        var bbox = returnInputBbox ? request.InputBbox : countAndExtent.Extent;
        var counts = new ObservationCounts(countAndExtent.Count, countAndExtent.EstimatedLocationCount);
        if (counts.ObservationCount == 0)
        {
            return await CreateEmptyResultAsync(context, bbox, cacheStatus);
        }

        if (ShouldFetchObservations(strategy, request.ObservationsLimit, counts.ObservationCount))
        {
            return await CreateResultFromObservationsAsync(context, preparedFilter, counts, bbox, cacheKey);
        }

        if (ShouldTryRepresentativeLocations(strategy, request.LocationAggregationLimit, counts.EstimatedLocationCount))
        {
            var representativeResult = await TryCreateResultFromRepresentativeLocationsAsync(context, preparedFilter, counts, bbox);
            if (representativeResult != null)
            {
                return representativeResult;
            }
        }

        return await CreateGeogridResultAsync(context, preparedFilter, counts, bbox);
    }

    private async Task<AdaptiveSearchLocationsResult?> TryCreateResultFromCacheAsync(SearchContext context, string cacheKey)
    {
        var request = context.Request;
        var requireExactBoundingBox = request.Filter.Geographics?.ConsiderObservationAccuracy == true ||
                                      request.Filter.Geographics?.ConsiderDisturbanceRadius == true;
        var start = Stopwatch.GetTimestamp();
        var cacheHit = _cache.TryGet(cacheKey, request.InputBbox, out var aggregates, requireExactBoundingBox);
        context.Timings.CacheLookup = Stopwatch.GetElapsedTime(start);
        if (!cacheHit)
        {
            return null;
        }

        var observationCount = aggregates.Sum(aggregate => (long)aggregate.ObservationSummary.ObservationCount);
        if (observationCount > request.ObservationsLimit)
        {
            return null;
        }

        var bbox = request.ReturnInputBBox && request.InputBbox != null
            ? request.InputBbox
            : _cache.CreateBoundingBox(aggregates);
        return await CreateLocationsResultAsync(
            context,
            LocationSource.FromAggregates(aggregates),
            new ObservationCounts(observationCount, aggregates.Count),
            AdaptiveSearchLocationsCacheStatus.Hit,
            bbox,
            clusteringBbox: request.InputBbox ?? bbox);
    }

    private async Task<AdaptiveSearchLocationsResult> CreateResultFromObservationsAsync(
        SearchContext context,
        PreparedObservationFilter preparedFilter,
        ObservationCounts counts,
        LatLonBoundingBoxDto? bbox,
        string? cacheKey)
    {
        var request = context.Request;
        var start = Stopwatch.GetTimestamp();
        var result = await _observationManager.GetChunkAsync(
            preparedFilter,
            0,
            request.ObservationsLimit,
            options: new SearchExecutionOptions { KnownTotalCount = counts.ObservationCount });
        request.CancellationToken.ThrowIfCancellationRequested();
        context.Timings.ElasticsearchObservations = Stopwatch.GetElapsedTime(start);
        if (result == null)
        {
            throw new Exception("Something went wrong when your query was executed. Make sure your filter is correct.");
        }

        var records = result.Records?.ToList() ?? new List<JsonObject>();
        var clusteringBbox = request.InputBbox ?? bbox;
        LocationSource source;
        if (request.CreateSampleObservations)
        {
            source = LocationSource.FromSampleRecords(AdaptiveSearchSampleObservationGenerator.Create(records));
        }
        else
        {
            var aggregates = _aggregator.CreateAggregates(records);
            request.CancellationToken.ThrowIfCancellationRequested();
            if (cacheKey != null)
            {
                _cache.Set(cacheKey, clusteringBbox, aggregates);
            }

            source = LocationSource.FromAggregates(aggregates);
        }

        var cacheStatus = cacheKey != null
            ? AdaptiveSearchLocationsCacheStatus.Miss
            : AdaptiveSearchLocationsCacheStatus.Bypass;
        return await CreateLocationsResultAsync(context, source, counts, cacheStatus, bbox, clusteringBbox);
    }

    private async Task<AdaptiveSearchLocationsResult?> TryCreateResultFromRepresentativeLocationsAsync(
        SearchContext context,
        PreparedObservationFilter preparedFilter,
        ObservationCounts counts,
        LatLonBoundingBoxDto? bbox)
    {
        var request = context.Request;
        var start = Stopwatch.GetTimestamp();
        // One more than the limit is requested to detect when the limit is exceeded.
        var representatives = await _observationManager.GetLocationRepresentativeChunkAsync(
            preparedFilter,
            request.LocationAggregationLimit + 1,
            LocationCollapseField);
        request.CancellationToken.ThrowIfCancellationRequested();
        context.Timings.ElasticsearchRepresentativeLocations = Stopwatch.GetElapsedTime(start);

        var records = representatives?.Records?.ToList();
        // Check the raw representatives before coincident locations reduce their count.
        if (records == null || records.Count >= request.LocationAggregationLimit)
        {
            return null;
        }

        var isFilteredToSingleOccurrence = IsFilteredToSingleOccurrence(context.Filter);
        var aggregates = _aggregator.CreateAggregates(
            records,
            representatives!.ObservationCountByLocationId,
            isSummaryApproximated: !isFilteredToSingleOccurrence,
            isPresentationStatusApproximated: !isFilteredToSingleOccurrence &&
                !AdaptiveSearchLocationsStrategySelector.CanDetermineRepresentativePresentationStatus(context.Filter, request.CutoffYear));
        var clusteringBbox = request.InputBbox ?? bbox;
        if (!request.ReturnInputBBox || bbox == null)
        {
            bbox = _cache.CreateBoundingBox(aggregates);
        }

        return await CreateLocationsResultAsync(
            context,
            LocationSource.FromAggregates(aggregates),
            counts,
            AdaptiveSearchLocationsCacheStatus.Bypass,
            bbox,
            clusteringBbox,
            AdaptiveSearchLocationsAggregationMode.Representative);
    }

    private async Task<AdaptiveSearchLocationsResult> CreateGeogridResultAsync(
        SearchContext context,
        PreparedObservationFilter preparedFilter,
        ObservationCounts counts,
        LatLonBoundingBoxDto? bbox)
    {
        var request = context.Request;
        var zoom = GeoTileHelper.CalculateGeotileZoom(request.InputBbox ?? bbox, request.MaxGridCells, request.MaxZoom);
        var start = Stopwatch.GetTimestamp();
        var gridResult = await _observationManager.GetGeogridTileAggregationAsync(preparedFilter, zoom);
        request.CancellationToken.ThrowIfCancellationRequested();
        context.Timings.ElasticsearchGeogrid = Stopwatch.GetElapsedTime(start);

        return new AdaptiveSearchLocationsResult
        {
            ResultType = AdaptiveSearchLocationsResultType.Geogrid,
            AdaptiveSearchStrategy = context.Strategy,
            GridResult = gridResult,
            Bbox = bbox,
            ObservationsTotalCount = counts.ObservationCount,
            EstimatedLocationCount = counts.EstimatedLocationCount,
            FeatureCount = gridResult.GridCellTileCount,
            Zoom = zoom,
            CacheStatus = AdaptiveSearchLocationsCacheStatus.Bypass,
            GeographicAreas = await context.GeographicAreasTask.WaitAsync(request.CancellationToken),
            Timings = context.Timings
        };
    }

    private static async Task<AdaptiveSearchLocationsResult> CreateEmptyResultAsync(
        SearchContext context,
        LatLonBoundingBoxDto? bbox,
        string cacheStatus)
    {
        return new AdaptiveSearchLocationsResult
        {
            ResultType = AdaptiveSearchLocationsResultType.Locations,
            AdaptiveSearchStrategy = context.Strategy,
            LocationFeatures = Array.Empty<LocationFeature>(),
            Bbox = bbox,
            ObservationsTotalCount = 0,
            EstimatedLocationCount = 0,
            FeatureCount = 0,
            LocationsBeforeClustering = 0,
            CacheStatus = cacheStatus,
            GeographicAreas = await context.GeographicAreasTask.WaitAsync(context.Request.CancellationToken),
            Timings = context.Timings
        };
    }

    private async Task<AdaptiveSearchLocationsResult> CreateLocationsResultAsync(
        SearchContext context,
        LocationSource source,
        ObservationCounts counts,
        string cacheStatus,
        LatLonBoundingBoxDto? bbox,
        LatLonBoundingBoxDto? clusteringBbox,
        string? aggregationMode = null)
    {
        var request = context.Request;
        var locationsBeforeClustering = source.Aggregates?.Count ?? _aggregator.CountLocations(source.SampleRecords!);
        Dictionary<string, Geometry>? polygons = null;
        if (request.CreatePolygons != CreatePolygonsMode.No && !source.IsEmpty)
        {
            var polygonsStart = Stopwatch.GetTimestamp();
            polygons = await GetPolygonGeometriesAsync(source).WaitAsync(request.CancellationToken);
            context.Timings.PolygonGeometries = Stopwatch.GetElapsedTime(polygonsStart);
        }

        request.CancellationToken.ThrowIfCancellationRequested();
        var clusteringStart = Stopwatch.GetTimestamp();
        var options = new LocationFeatureOptions
        {
            BoundingBox = clusteringBbox,
            ClusterRadiusMeters = request.ClusterRadiusMeters,
            ClusterPixels = context.ClusterPixels,
            MetersPerPixel = request.MetersPerPixel,
            ClusterRadiusPx = request.ClusterRadiusPx,
            MinLocationsPerCluster = request.MinLocationsPerCluster,
            MinLocationCountBeforeClustering = request.MinimumLocationCountBeforeClustering,
            PolygonGeometries = polygons,
            CreatePolygons = request.CreatePolygons,
            DetailClusterRadiusPx = context.DetailClusterRadiusPx
        };
        var features = source.Aggregates != null
            ? _featureFactory.CreateItems(source.Aggregates, options)
            : _featureFactory.CreateItems(source.SampleRecords!, options);
        request.CancellationToken.ThrowIfCancellationRequested();
        context.Timings.Clustering = Stopwatch.GetElapsedTime(clusteringStart);

        return new AdaptiveSearchLocationsResult
        {
            ResultType = AdaptiveSearchLocationsResultType.Locations,
            AdaptiveSearchStrategy = context.Strategy,
            LocationFeatures = features,
            Bbox = bbox,
            ObservationsTotalCount = counts.ObservationCount,
            EstimatedLocationCount = counts.EstimatedLocationCount,
            FeatureCount = features.Count(feature => feature.Geometry != null),
            LocationsBeforeClustering = locationsBeforeClustering,
            CacheStatus = cacheStatus,
            LocationAggregationMode = aggregationMode,
            LocationSummaryApproximated = features.Any(feature => feature.IsSummaryApproximated == true),
            LocationPresentationStatusApproximated = features.Any(feature => feature.IsPresentationStatusApproximated),
            GeographicAreas = await context.GeographicAreasTask.WaitAsync(request.CancellationToken),
            Timings = context.Timings
        };
    }

    private Task<Dictionary<string, Geometry>?> GetPolygonGeometriesAsync(LocationSource source)
    {
        if (source.Aggregates == null)
        {
            return _locationGeometryLookupService.GetPolygonLocationGeometriesAsync(source.SampleRecords!);
        }

        return _locationGeometryLookupService.GetPolygonLocationGeometriesAsync(
            source.Aggregates.Select(aggregate => ((ProcessedLocationType?)aggregate.Location.Type, aggregate.LocationIds)));
    }

    private async Task<PreparedObservationFilter> PrepareFilterAsync(AdaptiveSearchLocationsRequest request, SearchFilterInternal searchFilter)
    {
        searchFilter.Output ??= new OutputFilter();
        searchFilter.Output.Fields = LocationFeatureOutputFields.ToList();
        searchFilter.Output.ExcludeFields = new List<string>();
        var preparedFilter = await _observationManager.PrepareObservationFilterAsync(
            request.RoleId,
            request.AuthorizationApplicationIdentifier,
            searchFilter).WaitAsync(request.CancellationToken);
        return preparedFilter.WithCancellation(request.CancellationToken);
    }

    private async Task<List<Feature>?> GetGeographicAreasAsync(AdaptiveSearchLocationsRequest request)
    {
        if (!request.IncludeFilterAreasInResult)
        {
            return null;
        }

        // A separate filter instance, since the search filter is modified when the search is prepared.
        return await _observationManager.GetGeoGraphicsFiltersAsGeoJsonFeaturesAsync(CreateSearchFilter(request))
            .WaitAsync(request.CancellationToken);
    }

    private static SearchFilterInternal CreateSearchFilter(AdaptiveSearchLocationsRequest request)
    {
        return request.Filter.ToSearchFilterInternal(
            request.UserId,
            DefaultTranslationCultureCode,
            DefaultSortBy,
            DefaultSortOrder);
    }

    /// <summary>
    /// The cache is shared between users, so only searches whose result doesn't depend on the user are cached.
    /// </summary>
    private static bool CanUseLocationsCache(AdaptiveSearchLocationsRequest request, AdaptiveSearchLocationsStrategy strategy)
    {
        return request.UseCache &&
               request.Filter.ProtectionFilter == ProtectionFilterDto.Public &&
               request.Filter.ObservedByMe != true &&
               request.Filter.ReportedByMe != true &&
               !request.CreateSampleObservations &&
               (strategy is AdaptiveSearchLocationsStrategy.ObservationsThenGrid or
                   AdaptiveSearchLocationsStrategy.ObservationsThenLocationsThenGrid);
    }

    private static bool ShouldFetchObservations(AdaptiveSearchLocationsStrategy strategy, int observationsLimit, long observationCount)
    {
        return strategy != AdaptiveSearchLocationsStrategy.LocationsThenGrid &&
               observationCount <= observationsLimit;
    }

    private static bool ShouldTryRepresentativeLocations(AdaptiveSearchLocationsStrategy strategy, int locationAggregationLimit, long estimatedLocationCount)
    {
        return (strategy is AdaptiveSearchLocationsStrategy.ObservationsThenLocationsThenGrid or
                   AdaptiveSearchLocationsStrategy.LocationsThenGrid) &&
               estimatedLocationCount < locationAggregationLimit;
    }

    /// <summary>
    /// Determines whether the search filter guarantees that at most one observation can match,
    /// i.e. the include filter contains exactly one occurrence id. In that case the representative
    /// location summary can never be approximated, since there is nothing more to aggregate.
    /// </summary>
    private static bool IsFilteredToSingleOccurrence(SearchFilterInternal searchFilter)
    {
        return searchFilter.IncludeFilter?.OccurrenceIds?.Count() == 1;
    }

    /// <summary>
    /// State shared by the steps of one search.
    /// </summary>
    private sealed record SearchContext(
        AdaptiveSearchLocationsRequest Request,
        AdaptiveSearchLocationsStrategy Strategy,
        SearchFilterInternal Filter,
        AdaptiveSearchLocationsTimings Timings,
        Task<List<Feature>?> GeographicAreasTask,
        bool ClusterPixels,
        double DetailClusterRadiusPx);

    private readonly record struct ObservationCounts(long ObservationCount, long EstimatedLocationCount);

    /// <summary>
    /// The locations to create features from: location aggregates, or raw records when sample observations are created.
    /// </summary>
    private sealed class LocationSource
    {
        private LocationSource(List<LocationAggregate>? aggregates, List<JsonObject>? sampleRecords)
        {
            Aggregates = aggregates;
            SampleRecords = sampleRecords;
        }

        public List<LocationAggregate>? Aggregates { get; }

        public List<JsonObject>? SampleRecords { get; }

        public bool IsEmpty => (Aggregates?.Count ?? SampleRecords!.Count) == 0;

        public static LocationSource FromAggregates(List<LocationAggregate> aggregates) => new(aggregates, null);

        public static LocationSource FromSampleRecords(List<JsonObject> sampleRecords) => new(null, sampleRecords);
    }
}
