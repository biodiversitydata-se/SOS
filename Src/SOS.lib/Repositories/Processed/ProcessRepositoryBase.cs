using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Helpers;
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
        private readonly ICache<string, ProcessedConfiguration> _processedConfigurationCache;
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
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="toggleable"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ProcessRepositoryBase(
            IProcessClient client,
            bool toggleable,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ElasticSearchConfiguration elasticConfiguration,
            ILogger<ProcessRepositoryBase<TEntity, TKey>> logger
        )
        {
            _toggleable = toggleable;
            _processedConfigurationCache = processedConfigurationCache ?? throw new ArgumentNullException(nameof(processedConfigurationCache));
            ReadBatchSize = elasticConfiguration?.ReadBatchSize ?? throw new ArgumentNullException(nameof(elasticConfiguration));
            ScrollBatchSize = elasticConfiguration.ScrollBatchSize;
            WriteBatchSize = elasticConfiguration.WriteBatchSize;
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

        /// <inheritdoc />
        public bool LiveMode { get; set; }

        /// <inheritdoc />
        public int ReadBatchSize { get; }

        /// <inheritdoc />
        public int ScrollBatchSize { get; }

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
        public int WriteBatchSize { get; }
    }
}