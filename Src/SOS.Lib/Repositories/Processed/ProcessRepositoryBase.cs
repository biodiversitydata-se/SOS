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

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Base class for cosmos db repositories
    /// </summary>
    public abstract class ProcessRepositoryBase<TEntity, TKey> : IProcessRepositoryBase<TEntity, TKey> where TEntity : class, IEntity<TKey>
    {
        private readonly IElasticClientManager _elasticClientManager;
        private readonly ICache<string, ProcessedConfiguration> _processedConfigurationCache;
        protected readonly IClassCache<ConcurrentDictionary<string, HealthResponse>> _clusterHealthCache;
        protected readonly ElasticSearchConfiguration _elasticConfiguration;
        private readonly ElasticSearchIndexConfiguration _elasticSearchIndexConfiguration;
        private readonly bool _toggleable;
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

        protected ElasticsearchClient Client => ClientCount == 1 ? _elasticClientManager.Clients.FirstOrDefault() : _elasticClientManager.Clients[CurrentInstance];

        protected ElasticsearchClient InActiveClient => ClientCount == 1 ? _elasticClientManager.Clients.FirstOrDefault() : _elasticClientManager.Clients[InActiveInstance];

        protected int ClientCount => _elasticClientManager.Clients.Length;

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
                    .Query(q => q.MatchAll(ma => ma.Boost(1)))
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
            var res = await Client.Indices.DeleteAsync(indexName);
            return res.IsValidResponse;
        }

        /// <summary>
        ///     Logger
        /// </summary>
        protected readonly ILogger<ProcessRepositoryBase<TEntity, TKey>> Logger;

        /// <summary>
        /// Get free disk space
        /// </summary>
        /// <returns></returns>
        protected async Task<IDictionary<string, int>> GetDiskUsageAsync()
        {
            var diskUsage = new Dictionary<string, int>();
            var response = await Client.Nodes.StatsAsync(stats => stats.Metric(new Metrics("fs")));
            if (response.IsValidResponse)
            {
                // Get the disk usage from the response
                foreach (var node in response.Nodes)
                {
                    foreach (var data in node.Value.Fs.Data)
                    {
                        diskUsage.Add(data.Path, (int)((data.FreeInBytes ?? 0) / (data.TotalInBytes ?? 1)));
                    }
                }
            }

            return diskUsage;
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
        /// Set index refresh interval
        /// </summary>
        /// <param name="index"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        protected async Task<bool> SetIndexRefreshIntervalAsync(string index, Duration duration)
        {
            var getResponse = await Client.Indices.GetSettingsAsync<TEntity>(r => r.Name(new[] { index }));

            if (getResponse.IsValidResponse)
            {
                if (getResponse.Values.TryGetValue(index, out var indexState))
                {
                    indexState.Settings.RefreshInterval = duration;
                    var setResponse = await Client.Indices.PutSettingsAsync<TEntity>(indexState.Settings);

                    return setResponse.IsValidResponse;
                }
            }

            return false;
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
            var percentagesUsed = await GetDiskUsageAsync();
            foreach (var percentageUsed in percentagesUsed)
            {
                if (percentageUsed.Value > 90)
                {
                    Logger.LogError($"Disk usage too high in cluster, node {percentageUsed.Key}: ({percentageUsed.Value}%), aborting indexing");
                    return null;
                }
                Logger.LogDebug($"Current diskusage in cluster, node {percentageUsed.Key}: ({percentageUsed.Value}%");
            }

            var count = 0;
            return Client.BulkAll(items, b => b
                    .Index(IndexName)
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
        /// Write items to default index
        /// </summary>
        /// <param name="items"></param>
        /// <param name="refreshIndex"></param>
        /// <returns></returns>
        protected async Task<BulkAllObserver> WriteToElasticAsync(IEnumerable<TEntity> items, bool refreshIndex = false)
        {
            return await WriteToElasticAsync(items, IndexName, refreshIndex);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="toggleable"></param>
        /// <param name="elasticClientManager"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="clusterHealthCache"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected ProcessRepositoryBase(
            bool toggleable,
            IElasticClientManager elasticClientManager,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ElasticSearchConfiguration elasticConfiguration,
            IClassCache<ConcurrentDictionary<string, HealthResponse>> clusterHealthCache,
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
                .Index(IndexName)
                .Query(q => q.MatchAll(q => q.QueryName("GetAllQuery")))
                .Size(take));
            return searchResponse.Documents.ToList();
        }
    }
}