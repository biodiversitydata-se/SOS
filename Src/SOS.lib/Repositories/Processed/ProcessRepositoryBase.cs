using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Linq;
using System.Threading.Tasks;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Base class for cosmos db repositories
    /// </summary>
    public class ProcessRepositoryBase<TEntity, TKey> : IProcessRepositoryBase<TEntity, TKey> where TEntity : IEntity<TKey>
    {
        private readonly IElasticClientManager _elasticClientManager;
        private readonly ICache<string, ProcessedConfiguration> _processedConfigurationCache;
        private readonly ElasticSearchConfiguration _elasticConfiguration;
        private readonly bool _toggleable;
        private readonly string _id = typeof(TEntity).Name;

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

        protected IElasticClient Client => ClientCount == 1 ? _elasticClientManager.Clients.FirstOrDefault() : _elasticClientManager.Clients[CurrentInstance];

        protected IElasticClient InActiveClient => ClientCount == 1 ? _elasticClientManager.Clients.FirstOrDefault() : _elasticClientManager.Clients[InActiveInstance];

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
        ///     Logger
        /// </summary>
        protected readonly ILogger<ProcessRepositoryBase<TEntity, TKey>> Logger;

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
        protected int NumberOfReplicas => _elasticConfiguration.NumberOfReplicas;

        /// <summary>
        /// Number of shards
        /// </summary>
        protected int NumberOfShards => _elasticConfiguration.NumberOfShards;

        /// <summary>
        /// Protected scope
        /// </summary>
        protected string ProtectedScope => _elasticConfiguration.ProtectedScope;

        /// <summary>
        /// Scroll batch size
        /// </summary>
        protected int ScrollBatchSize => _elasticConfiguration.ScrollBatchSize;

        /// <summary>
        /// Scroll timeout
        /// </summary>
        protected string ScrollTimeout => _elasticConfiguration.ScrollTimeout;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="toggleable"></param>
        /// <param name="elasticClientManager"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected ProcessRepositoryBase(
        bool toggleable,
        IElasticClientManager elasticClientManager,
        ICache<string, ProcessedConfiguration> processedConfigurationCache,
        ElasticSearchConfiguration elasticConfiguration,
        ILogger<ProcessRepositoryBase<TEntity, TKey>> logger
    )
        {
            _toggleable = toggleable;
            _elasticClientManager = elasticClientManager ?? throw new ArgumentNullException(nameof(elasticClientManager));
            _processedConfigurationCache = processedConfigurationCache ?? throw new ArgumentNullException(nameof(processedConfigurationCache));
            _elasticConfiguration = elasticConfiguration ?? throw new ArgumentNullException(nameof(elasticConfiguration));
          
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Default use non live instance
            LiveMode = false;
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
       
        public string IndexName => IndexHelper.GetIndexName<TEntity>(_elasticConfiguration.IndexPrefix, ClientCount == 1, LiveMode ? ActiveInstance : InActiveInstance, false);

        /// <inheritdoc />
        public bool LiveMode { get; set; }

        /// <summary>
        /// Max number of aggregation buckets
        /// </summary>
        public int MaxNrElasticSearchAggregationBuckets => _elasticConfiguration.MaxNrAggregationBuckets;

        /// <inheritdoc />
        public int ReadBatchSize => _elasticConfiguration?.ReadBatchSize ?? 1000;

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
        public int WriteBatchSize => _elasticConfiguration.WriteBatchSize;
    }
}