using Microsoft.Extensions.Logging;
using Elastic.Clients.Elasticsearch;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Repositories.Processed.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch.Cluster;
using SOS.Lib.Extensions;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using Microsoft.Extensions.Caching.Memory;

namespace SOS.Lib.Repositories.Processed;

/// <summary>
///     Base class for cosmos db repositories
/// </summary>
public abstract class ProcessRepositoryBase<TEntity, TKey> : IProcessRepositoryBase<TEntity, TKey> where TEntity : class, IEntity<TKey>, IElasticEntity
{
    private readonly IElasticClientManager _elasticClientManager;
    private readonly ICache<string, ProcessedConfiguration> _processedConfigurationCache;
    private readonly bool _toggleable;
    private int _maxDiskUsed;
    private int _writeCallsSinceLastDiskUsageCheck;
    protected readonly IClassCache<ConcurrentDictionary<string, HealthResponse>> _clusterHealthCache;
    protected readonly ElasticSearchConfiguration _elasticConfiguration;
    private readonly ElasticSearchIndexConfiguration _elasticSearchIndexConfiguration;
    private readonly IMemoryCache _memoryCache;
    private string _cacheKeyFieldType = $"FieldType-{typeof(TEntity).Name}";

    protected string _id = typeof(TEntity).Name;
     
    /// <summary>
    ///     Disposed
    /// </summary>
    private bool _disposed;


    /// <summary>
    ///     Get configuration object
    /// </summary>
    /// <returns></returns>
    private ProcessedConfiguration GetConfiguration()
    {
        try
        {
            var processedConfig = _processedConfigurationCache.GetAsync(_id)?.Result;

            return processedConfig ?? new ProcessedConfiguration { Id = _id };
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());

            return default;
        }
    }

    private async Task<IDictionary<string, string>> GetFieldTypesAsync()
    {
        var fieldTypes = new Dictionary<string, string>();
        var mappingResponse = await Client.Indices.GetMappingAsync<TEntity>(o => o
            .Indices(IndexName)
        );
        if (mappingResponse.IsValidResponse)
        {
            foreach (var value in mappingResponse.Indices.Values)
            {
                PopulateFieldTypes(value.Mappings.Properties, ref fieldTypes, null);
            }
        }

        return fieldTypes;
    }

    private void PopulateFieldTypes(Properties properties, ref Dictionary<string, string> fieldTypes, string parents = null)
    {
        foreach (var property in properties)
        {
            var name = $"{(string.IsNullOrEmpty(parents) ? "" : $"{parents}.")}{property.Key.Name}";

            if (property.Value is ObjectProperty op)
            {
                PopulateFieldTypes(op.Properties, ref fieldTypes, name);
            }
            else
            {
                var type = property.Value switch
                {
                    ByteNumberProperty or
                    IntegerNumberProperty or
                    LongNumberProperty or
                    ShortNumberProperty => "long",
                    DoubleNumberProperty or
                    FloatNumberProperty => "double",
                    _ => "string"
                };

                fieldTypes.Add(name, type);
            }
        }
    }

    protected enum SourceTypes
    {
        Term,
        GeoTileGrid
    }

    /// <summary>
    /// Add many items to db
    /// </summary>
    /// <param name="items"></param>
    /// <param name="indexName"></param>
    /// <param name="refreshIndex"></param>
    /// <returns></returns>
    protected async Task<int> AddManyAsync(IEnumerable<TEntity> items, string indexName, bool refreshIndex = false)
    {
        // Save valid processed data
        Logger.LogDebug($"Start indexing batch for searching in {indexName} with {items.Count()} items");
        var indexResult = await WriteToElasticAsync(items, indexName, refreshIndex);
        Logger.LogDebug($"Finished indexing batch for searching in {indexName}");
        if (indexResult == null || indexResult.TotalNumberOfFailedBuffers > 0) return 0;
        return items.Count();
    }

    protected ElasticsearchClient Client => ClientCount == 1 ? _elasticClientManager.Clients.First() : _elasticClientManager.Clients[CurrentInstance];

    protected ElasticsearchClient InActiveClient => ClientCount == 1 ? _elasticClientManager.Clients.First() : _elasticClientManager.Clients[InActiveInstance];

    protected int ClientCount => _elasticClientManager.Clients.Length;

    protected IDictionary<string, CompositeAggregationSource> CreateCompositeTermsAggregationSource(params (string Key, string Term, SortOrder SortOrder)[] sources)
    {
        return CreateCompositeTermsAggregationSource(sources.Select(s => (s.Key, s.Term, s.SortOrder, MissingBucket: false, IsScript: false)).ToArray());
    }
    protected IDictionary<string, CompositeAggregationSource> CreateCompositeTermsAggregationSource(params (string Key, string Term, SortOrder SortOrder, bool MissingBucket)[] sources)
    {
        return CreateCompositeTermsAggregationSource(sources.Select(s => (s.Key, s.Term, s.SortOrder, s.MissingBucket, IsScript: false)).ToArray());
    }

    protected IDictionary<string, CompositeAggregationSource> CreateCompositeTermsAggregationSource(params (string Key, string Term, SortOrder SortOrder, bool MissingBucket, bool IsScript)[] sources)
    {
        var requestParams = sources.Select(s => ((SourceTypes Type, string Key, string Field, SortOrder SortOrder, bool? MissingBucket, bool? IsScript, int? Precision))(Type: SourceTypes.Term, s.Key, Field: s.Term, s.SortOrder, s.MissingBucket, s.IsScript, Precision: null));
        return CreateCompositeAggregationSource(requestParams.ToArray());
    }

    protected IDictionary<string, CompositeAggregationSource> CreateCompositeAggregationSource(params (SourceTypes Type, string Key, string Field, SortOrder SortOrder, bool? MissingBucket, bool? IsScript, int? Precision)[] sources)
    {
        var response = new Dictionary<string, CompositeAggregationSource>();

        foreach (var source in sources)
        {
            response.Add(source.Key, new CompositeAggregationSource
            {
                GeotileGrid = source.Type == SourceTypes.GeoTileGrid ? new CompositeGeoTileGridAggregation
                {
                    Script = source.IsScript ?? false ? new Script
                    {
                        Source = source.Field
                    } : null,
                    Field = source.IsScript ?? false ? null : source.Field,
                    Precision = source.Precision,
                    Order = source.SortOrder
                } : null,
                Terms = source.Type == SourceTypes.Term ? new CompositeTermsAggregation
                {
                    Script = source.IsScript ?? false ? new Script
                    {
                        Source = source.Field
                    } : null,
                    Field = source.IsScript ?? false ? null : source.Field,
                    MissingBucket = source.MissingBucket,
                    Order = source.SortOrder
                } : null
            });
        }
        return response;
    }

    /// <summary>
    ///     Dispose
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
        }

        _disposed = true;
    }

    /// <summary>
    /// Delete all documents
    /// </summary>
    /// <param name="indexName"></param>
    /// <param name="waitForCompletion"></param>
    /// <returns></returns>
    protected async Task<bool> DeleteAllDocumentsAsync(string indexName, bool waitForCompletion = false)
    {
        try
        {
            var res = await Client.DeleteByQueryAsync<TEntity>(indexName, d => d
                .Query(q => q.MatchAll(new Elastic.Clients.Elasticsearch.QueryDsl.MatchAllQuery()))
                .Refresh(waitForCompletion)
                .WaitForCompletion(waitForCompletion)
            );

            return res.IsValidResponse;
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
            return false;
        }
    }

    protected async Task<bool> DeleteCollectionAsync(string indexName)
    {
        try
        {
            var res = await Client.Indices.DeleteAsync(indexName);
            return res.IsValidResponse;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Logger
    /// </summary>
    protected readonly ILogger<ProcessRepositoryBase<TEntity, TKey>> Logger;

    protected async Task<string> GetFieldTypeAsync(string field)
    {
        if (!_memoryCache.TryGetValue(_cacheKeyFieldType, out IDictionary<string, string> fieldTypes))
        {
            fieldTypes = await GetFieldTypesAsync();
            if ((fieldTypes?.Count() ?? 0) != 0)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60 * 12));
                _memoryCache.Set(_cacheKeyFieldType, fieldTypes, cacheEntryOptions);
            }
        }

        if (fieldTypes?.TryGetValue(field, out var fieldType) ?? false)
        {
            return fieldType;
        }

        return "string";
    }

    /// <summary>
    /// Get name of instance
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="protectedObservations"></param>
    /// <returns></returns>
    protected string GetInstanceName(byte instance, bool protectedObservations) =>
        IndexHelper.GetInstanceName<TEntity>(_toggleable, instance, protectedObservations);

    /// <summary>
    /// Index prefix (if any)
    /// </summary>
    protected string IndexPrefix => _elasticConfiguration.IndexPrefix;

    /// <summary>
    /// Count documents in index
    /// </summary>
    /// <param name="indexName"></param>
    /// <returns></returns>
    protected virtual async Task<long> IndexCountAsync(string indexName)
    {
        try
        {
            var countResponse = await Client.CountAsync(indexName);

            countResponse.ThrowIfInvalid();
            return countResponse.Count;
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
            return -1;
        }
    }

    /// <summary>
    /// number of replicas
    /// </summary>
    protected int NumberOfReplicas => _elasticSearchIndexConfiguration.NumberOfReplicas;

    /// <summary>
    /// Number of shards
    /// </summary>
    protected int NumberOfShards => _elasticSearchIndexConfiguration.NumberOfShards;

    /// <summary>
    /// Number of shards
    /// </summary>
    protected int NumberOfShardsProtected => _elasticSearchIndexConfiguration.NumberOfShardsProtected;

    /// <summary>
    /// Scroll batch size
    /// </summary>
    protected int ScrollBatchSize => _elasticSearchIndexConfiguration.ScrollBatchSize;

    /// <summary>
    /// Scroll timeout
    /// </summary>
    protected string ScrollTimeout => _elasticSearchIndexConfiguration.ScrollTimeout;

    /// <summary>
    /// Execute search after query
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="searchIndex"></param>
    /// <param name="searchDescriptor"></param>
    /// <param name="pointInTimeId"></param>
    /// <param name="nextPageKey"></param>
    /// <returns></returns>
    protected async Task<SearchResponse<T>> SearchAfterAsync<T>(
       string searchIndex,
       SearchRequestDescriptor<T> searchDescriptor,
       string pointInTimeId = null,
       ICollection<FieldValue> nextPageKey = null) where T : class
    {
        var keepAlive = new Duration(1000 * 60 * 5);
        if (string.IsNullOrEmpty(pointInTimeId))
        {
            var pitResponse = await Client.OpenPointInTimeAsync(searchIndex, pit => pit
                .RequestConfiguration(c => c
                    .RequestTimeout(TimeSpan.FromSeconds(30))
                )
                .KeepAlive(keepAlive)
            );

            pitResponse.ThrowIfInvalid();

            pointInTimeId = pitResponse.Id;
        }

        // Retry policy by Polly
        var searchResponse = await PollyHelper.GetRetryPolicy(3, 100).ExecuteAsync(async () =>
        {
            var queryResponse = await Client.SearchAsync<T>(searchDescriptor
               .Indices(searchIndex)
               .Sort(s => s.Field("_shard_doc"))
               .SearchAfter(nextPageKey)
               .Size(ScrollBatchSize)
               .TrackTotalHits(new Elastic.Clients.Elasticsearch.Core.Search.TrackHits(false))
               .Pit(pointInTimeId, pit => pit.KeepAlive(keepAlive))
            );

            queryResponse.ThrowIfInvalid();

            return queryResponse;
        });

        if (!string.IsNullOrEmpty(pointInTimeId) && (searchResponse?.Hits?.Count ?? 0) == 0)
        {
            await Client.ClosePointInTimeAsync(pitr => pitr.Id(pointInTimeId));
        }

        return searchResponse;
    }

    /// <summary>
    /// Set index refresh interval
    /// </summary>
    /// <param name="index"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    protected async Task<bool> SetIndexRefreshIntervalAsync(string index, Duration duration)
    {
        var indexState = new IndexState() { Settings = new IndexSettings() };
        indexState.Settings.RefreshInterval = duration;
        var setResponse = await Client.Indices.PutSettingsAsync<TEntity>(indexState.Settings, index);
        
        return setResponse.IsValidResponse;
    }

    /// <summary>
    /// Write data to Elastic Search
    /// </summary>
    /// <param name="items"></param>
    /// <param name="indexName"></param>
    /// <param name="refreshIndex"></param>
    /// <returns></returns>
    protected async Task<BulkAllObserver> WriteToElasticAsync(IEnumerable<TEntity> items, string indexName, bool refreshIndex = false)
    {
        if (!items.Any())
        {
            return null;
        }
        // We don't need to check disk space every time
        if (_maxDiskUsed == 0 || 
            (_maxDiskUsed < 50 && _writeCallsSinceLastDiskUsageCheck >= 20) ||
            (_maxDiskUsed < 80 && _writeCallsSinceLastDiskUsageCheck >= 10)
        )
        {
            _writeCallsSinceLastDiskUsageCheck = 0;
            var percentagesUsed = await GetDiskUsageAsync();
            foreach (var percentageUsed in percentagesUsed)
            {
                if (percentageUsed.Value > 90)
                {
                    Logger.LogError($"Disk usage too high in cluster, node {percentageUsed.Key}: ({percentageUsed.Value}%), aborting indexing");
                    return null;
                }
                _maxDiskUsed = percentageUsed.Value > _maxDiskUsed ? percentageUsed.Value : _maxDiskUsed;
                Logger.LogDebug($"Current diskusage in cluster, node {percentageUsed.Key}: ({percentageUsed.Value}%");
            }
        }
        
        _writeCallsSinceLastDiskUsageCheck++;
        
        var count = 0;
        return Client.BulkAll(items, b => b
                .Index(indexName)
                // Set _id to unique value
                .BufferToBulk((descriptor, buffer) =>
                {                        
                    foreach (var item in buffer)
                    {                           
                        descriptor.Index<TEntity>(item, op => op
                           .Index(indexName)
                           .Id(item.ElasticsearchId)                           
                       );
                    }
                })
                // how long to wait between retries
                .BackOffTime("30s")
                // how many retries are attempted if a failure occurs                        .
                .BackOffRetries(2)
                // how many concurrent bulk requests to make
                .MaxDegreeOfParallelism(Environment.ProcessorCount)
                // number of items per bulk request
                .Size(WriteBatchSize)
                .RefreshOnCompleted(refreshIndex)
                .DroppedDocumentCallback((r, o) =>
                {
                    if (r.Error != null)
                    {
                        Logger.LogError($"Failed to add {nameof(TEntity)} with id: {o.Id}, Error: {r.Error.Reason}");
                    }
                })
            )
            .Wait(TimeSpan.FromHours(1),
                next =>
                {
                    Logger.LogDebug($"Indexing item for search:{count += next.Items.Count}");
                });
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="toggleable"></param>
    /// <param name="elasticClientManager"></param>
    /// <param name="processedConfigurationCache"></param>
    /// <param name="elasticConfiguration"></param>
    /// <param name="clusterHealthCache"></param>
    /// <param name="memoryCache"></param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    protected ProcessRepositoryBase(
        bool toggleable,
        IElasticClientManager elasticClientManager,
        ICache<string, ProcessedConfiguration> processedConfigurationCache,
        ElasticSearchConfiguration elasticConfiguration,
        IClassCache<ConcurrentDictionary<string, HealthResponse>> clusterHealthCache,
        IMemoryCache memoryCache,
        ILogger<ProcessRepositoryBase<TEntity, TKey>> logger
    )
    {
        _toggleable = toggleable;
        _elasticClientManager = elasticClientManager ?? throw new ArgumentNullException(nameof(elasticClientManager));
        _processedConfigurationCache = processedConfigurationCache ?? throw new ArgumentNullException(nameof(processedConfigurationCache));
        _elasticConfiguration = elasticConfiguration ?? throw new ArgumentNullException(nameof(elasticConfiguration));
        _clusterHealthCache = clusterHealthCache;
        _elasticSearchIndexConfiguration = elasticConfiguration.IndexSettings?.FirstOrDefault(i => i.Name.Equals(IndexHelper.GetInstanceName<TEntity>(), StringComparison.CurrentCultureIgnoreCase));
        if (_elasticSearchIndexConfiguration == null)
        {
            logger.LogError($"Settings for index {IndexHelper.GetInstanceName<TEntity>()} is missing. Default settings is used.");
            _elasticSearchIndexConfiguration = new ElasticSearchIndexConfiguration() { Name = IndexHelper.GetInstanceName<TEntity>() };
        }
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        // Default use non live instance
        LiveMode = false;
    }

    /// <inheritdoc />
    public async Task<int> AddManyAsync(IEnumerable<TEntity> items, bool refreshIndex = false)
    {
        return await AddManyAsync(items, IndexName, refreshIndex);
    }

    /// <summary>
    ///     Dispose
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public byte ActiveInstance => GetConfiguration()?.ActiveInstance ?? 1;

    /// <inheritdoc />
    public byte InActiveInstance => (byte)(ActiveInstance == 0 ? 1 : 0);

    /// <inheritdoc />
    public byte CurrentInstance => LiveMode ? ActiveInstance : InActiveInstance;

    /// <inheritdoc />
    public async Task ClearConfigurationCacheAsync()
    {
        await _processedConfigurationCache.ClearAsync();
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAllDocumentsAsync(bool waitForCompletion = false)
    {
        return await DeleteAllDocumentsAsync(IndexName, waitForCompletion);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteCollectionAsync()
    {
        return await DeleteCollectionAsync(IndexName);
    }

    public async Task RefreshIndexAsync()
    {            
        await Client.Indices.FlushAsync(IndexName);
        await Client.Indices.RefreshAsync(IndexName);            
    }

    /// <inheritdoc />
    public async Task<bool> DisableIndexingAsync()
    {
        return await SetIndexRefreshIntervalAsync(IndexName, Duration.MinusOne);
    }

    /// <inheritdoc />
    public async Task EnableIndexingAsync()
    {
        await SetIndexRefreshIntervalAsync(IndexName, new Duration(5000.0));
    }

    /// <inheritdoc/>
    public async Task<IDictionary<string, int>> GetDiskUsageAsync()
    {
        var diskUsage = new Dictionary<string, int>();

        var response = await Client.Nodes.StatsAsync(s => s.Metric(new Metrics("fs")));
        if (response.IsValidResponse)
        {
            // Get the disk usage from the response
            foreach (var node in response.Nodes)
            {
                var free = node.Value.Fs.Total.AvailableInBytes ?? 0;
                var total = node.Value.Fs.Total.TotalInBytes ?? 1;
                diskUsage.Add(node.Key, 100 - (int)Math.Abs(((double)free / (double)total) * 100));
            }
        }

        return diskUsage;
    }

    /// <inheritdoc />
    public virtual async Task<long> IndexCountAsync()
    {
        return await IndexCountAsync(IndexName);
    }

    public string IndexName => IndexHelper.GetIndexName<TEntity>(_elasticConfiguration.IndexPrefix, ClientCount == 1, LiveMode ? ActiveInstance : InActiveInstance, false);

    /// <inheritdoc />
    public bool LiveMode { get; set; }

    /// <summary>
    /// Max number of aggregation buckets
    /// </summary>
    public int MaxNrElasticSearchAggregationBuckets => _elasticSearchIndexConfiguration.MaxNrAggregationBuckets;

    /// <inheritdoc />
    public int ReadBatchSize => _elasticSearchIndexConfiguration?.ReadBatchSize ?? 1000;

    /// <inheritdoc />
    public async Task<bool> SetActiveInstanceAsync(byte instance)
    {
        try
        {
            var config = GetConfiguration();

            config.ActiveInstance = instance;

            return await _processedConfigurationCache.AddOrUpdateAsync(config);
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());

            return default;
        }
    }

    /// <inheritdoc />
    public int WriteBatchSize => _elasticSearchIndexConfiguration.WriteBatchSize;

    /// <summary>
    /// Get all records.
    /// </summary>
    /// <param name="take">The max number of records to get.</param>
    /// <returns></returns>
    public async Task<List<TEntity>> GetAllAsync(int take = 10000)
    {
        var searchResponse = await Client.SearchAsync<TEntity>(s => s
            .Indices(IndexName)
            .Query(q => q.MatchAll(q => q.QueryName("GetAllQuery")))
            .Size(take));
        return searchResponse.Documents.ToList();
    }
}