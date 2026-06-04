using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Cluster;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Caching.Memory;
using NetTopologySuite.Geometries;
using SOS.Lib;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Extensions;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Repositories.Processed;
using SOS.Observations.Api.Repositories.Interfaces;
using SOS.Observations.Api.Services;
using System.Collections.Concurrent;
using ZiggyCreatures.Caching.Fusion;
using Observation = SOS.Lib.Models.Processed.Observation.Observation;
using Location = SOS.Lib.Models.Processed.Observation.Location;

namespace SOS.Observations.Api.Repositories;

/// <summary>
///     Species data service
/// </summary>
public class ProcessedLocationRepository : ProcessedObservationBaseRepository,
    IProcessedLocationRepository
{
    internal const string LocationGeometryCacheName = "processed-location-geometry";
    internal const int MaxLocationGeometryCacheSizeUnits = 10_000;
    private readonly IFusionCache _fusionCache;
    private readonly LocationGeometryCacheMonitor _cacheMonitor;

    /// <summary>
    /// Constructor used in public mode
    /// </summary>
    /// <param name="elasticClientManager"></param>
    /// <param name="elasticConfiguration"></param>
    /// <param name="processedConfigurationCache"></param>
    /// <param name="clusterHealthCache"></param>
    /// <param name="memoryCache"></param>
    /// <param name="fusionCacheProvider"></param>
    /// <param name="cacheMonitor"></param>
    /// <param name="logger"></param>
    public ProcessedLocationRepository(
        IElasticClientManager elasticClientManager,
        ElasticSearchConfiguration elasticConfiguration,
        ICache<string, ProcessedConfiguration> processedConfigurationCache,
        IClassCache<ConcurrentDictionary<string, HealthResponse>> clusterHealthCache,
        IMemoryCache memoryCache,
        IFusionCacheProvider fusionCacheProvider,
        LocationGeometryCacheMonitor cacheMonitor,
        ILogger<ProcessedLocationRepository> logger) : base(true, elasticClientManager, processedConfigurationCache, elasticConfiguration, clusterHealthCache, memoryCache, logger)
    {
        _fusionCache = (fusionCacheProvider ?? throw new ArgumentNullException(nameof(fusionCacheProvider)))
            .GetCache(LocationGeometryCacheName);
        _cacheMonitor = cacheMonitor ?? throw new ArgumentNullException(nameof(cacheMonitor));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Location>> GetLocationsAsync(IEnumerable<string> locationIds)
    {
        if (!locationIds?.Any() ?? true)
        {
            return null;
        }

        var queries = new List<Action<QueryDescriptor<Observation>>>();
        queries.TryAddTermsCriteria("location.locationId", locationIds);

        var searchResponse = await Client.SearchAsync<Observation>(s => s
            .Index($"{PublicIndexName}, {ProtectedIndexName}")
            .Query(q => q
                .Bool(b => b
                    .Filter(queries.ToArray())
                )
            )
            .Collapse(c => c.Field("location.locationId"))
            .Size(locationIds.Count())
            .Source((Includes : new[] { "location" }, Excludes: new[] { "location.pointLocation" }).ToProjection()
            )
           .TrackTotalHits(new TrackHits(false))
        );

        searchResponse.ThrowIfInvalid();

        return searchResponse.Documents?.Select(d => d.Location);
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, Geometry>> GetLocationGeometryAsync(HashSet<string> locationIds)
    {        
        if (locationIds is not { Count: > 0 })
        {
            return new Dictionary<string, Geometry>(StringComparer.OrdinalIgnoreCase);
        }

        var result = new Dictionary<string, Geometry>(StringComparer.OrdinalIgnoreCase);
        var cacheMisses = GetCachedPointWithBuffers(locationIds, result);

        if (cacheMisses.Count == 0)
        {
            return result;
        }

        var fetched = await FetchLocationGeometriesFromElasticAsync(cacheMisses);
        CacheAndCollectGeometry(cacheMisses, fetched, result);

        return result;
    }

    private List<string> GetCachedPointWithBuffers(IEnumerable<string> locationIds, Dictionary<string, Geometry> result)
    {
        var cacheMisses = new List<string>();

        foreach (var locationId in locationIds)
        {
            var cacheValue = _fusionCache.TryGet<Geometry?>(
                CreateLocationGeometryCacheKey(locationId));

            if (!cacheValue.HasValue)
            {
                cacheMisses.Add(locationId);
                continue;
            }

            if (cacheValue.Value != null)
            {
                result[locationId] = cacheValue.Value;
            }
        }

        return cacheMisses;
    }

    private async Task<Dictionary<string, Geometry>> FetchLocationGeometriesFromElasticAsync(List<string> locationIds)
    {
        var queries = new List<Action<QueryDescriptor<Observation>>>();
        queries.TryAddTermsCriteria("location.locationId", locationIds);

        var searchResponse = await Client.SearchAsync<Observation>(s => s
            .Index($"{PublicIndexName}, {ProtectedIndexName}")
            .Query(q => q.Bool(b => b.Filter(queries.ToArray())))
            .Collapse(c => c.Field("location.locationId"))
            .Size(locationIds.Count)
            .Source((Includes: new[] { "location.locationId", "location.pointWithBuffer" }, Excludes: Array.Empty<string>()).ToProjection())
            .TrackTotalHits(new TrackHits(false))
        );

        searchResponse.ThrowIfInvalid();

        var result = new Dictionary<string, Geometry>(StringComparer.OrdinalIgnoreCase);
        if (searchResponse.Documents == null)
        {
            return result;
        }

        foreach (var document in searchResponse.Documents)
        {
            var location = document.Location;
            if (location?.LocationId == null || location.PointWithBuffer == null)
            {
                continue;
            }

            result.TryAdd(location.LocationId, location.PointWithBuffer);
        }

        return result;
    }

    private void CacheAndCollectGeometry(
        List<string> locationIds,
        Dictionary<string, Geometry> fetched,
        Dictionary<string, Geometry> result)
    {
        foreach (var locationId in locationIds)
        {
            fetched.TryGetValue(locationId, out var pointWithBuffer);

            _fusionCache.Set(
                CreateLocationGeometryCacheKey(locationId),
                pointWithBuffer,
                CreateLocationGeometryCacheOptions(pointWithBuffer));
            _cacheMonitor.TrackGeometryEntry(locationId, pointWithBuffer);

            if (pointWithBuffer != null)
            {
                result[locationId] = pointWithBuffer;
            }
        }
    }

    private static string CreateLocationGeometryCacheKey(string locationId)
    {
        return locationId;
    }

    private static FusionCacheEntryOptions CreateLocationGeometryCacheOptions(Geometry? geometry)
    {
        return new FusionCacheEntryOptions
        {
            Duration = TimeSpan.FromHours(48)
        }.SetSize(LocationGeometryCacheMonitor.CalculateGeometrySizeUnits(geometry));
    }

    public async Task<IEnumerable<LocationSearchResult>> SearchAsync(SearchFilter filter, int skip,
        int take)
    {
        var indexName = GetCurrentIndex(filter);
        var (queries, excludeQueries) = GetCoreQueries<dynamic>(filter);

        var searchResponse = await Client.SearchAsync<dynamic>(s => s
            .Index(indexName)
            .Query(q => q
                .Bool(b => b
                    .MustNot(excludeQueries.ToArray())
                    .Filter(queries.ToArray())
                )
            )
            .Aggregations(a => a
                .Add("locations", a => a
                    .Composite(c => c
                        .Size(skip + take)
                        .Sources(
                            [
                                CreateCompositeTermsAggregationSource(
                                    ("id", "location.locationId", SortOrder.Asc)
                                ),
                                CreateCompositeTermsAggregationSource(
                                    ("name", "location.locality", SortOrder.Asc)
                                ),
                                CreateCompositeTermsAggregationSource(
                                    ("county", "location.county.name", SortOrder.Asc)
                                ),CreateCompositeTermsAggregationSource(                                       
                                    ("municipality", "location.municipality.name", SortOrder.Asc)
                                ),
                                CreateCompositeTermsAggregationSource(
                                    ("parish", "location.parish.name", SortOrder.Asc)
                                ),
                                CreateCompositeTermsAggregationSource(
                                    ("longitude", "location.decimalLongitude", SortOrder.Asc)
                                ),
                                CreateCompositeTermsAggregationSource(
                                    ("latitude", "location.decimalLatitude", SortOrder.Asc)
                                )
                            ]
                         )
                    )
                )
            )
            .AddDefaultAggrigationSettings()
        );

        searchResponse.ThrowIfInvalid();

        var result = new List<LocationSearchResult>();
        foreach (var bucket in searchResponse.Aggregations.GetComposite("locations").Buckets?.Skip(skip))
        {
            bucket.Key["county"].TryGetString(out var county);
            bucket.Key["id"].TryGetString(out var id);
            bucket.Key["latitude"].TryGetDouble(out var latitude);
            bucket.Key["longitude"].TryGetDouble(out var longitude);
            bucket.Key["municipality"].TryGetString(out var municipality);
            bucket.Key["name"].TryGetString(out var name);
            bucket.Key["parish"].TryGetString(out var parish);
            result.Add(new LocationSearchResult
            {
                County = county,
                Id = id,
                Latitude = latitude ?? 0, 
                Longitude = longitude ?? 0,
                Municipality = municipality,
                Name = name,
                Parish = parish
            });
        }

        return result;
    }
}
