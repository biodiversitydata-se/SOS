using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Observations.Api.Database.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.Repositories
{
    /// <summary>
    /// Base class for cosmos db repositories
    /// </summary>
    public class ProcessBaseRepository<TEntity, TKey> : IBaseRepository<TEntity, TKey> where TEntity : IEntity<TKey>
    {
        /// <summary>
        /// Logger 
        /// </summary>
        protected readonly ILogger<ProcessBaseRepository<TEntity, TKey>> Logger;

        private readonly IProcessClient _client;
        /// <summary>
        /// Mongo db
        /// </summary>
        protected readonly IMongoDatabase Database;
        
        /// <summary>
        /// Batch size.
        /// </summary>
        protected int BatchSize { get; }
        
        /// <summary>
        /// Disposed
        /// </summary>
        private bool _disposed;

        private readonly string _collectionName;
        private readonly string _collectionNameConfiguration = typeof(ProcessedConfiguration).Name;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="multipleInstances"></param>
        /// <param name="logger"></param>
        protected ProcessBaseRepository(
            IProcessClient client,
            bool multipleInstances,
            ILogger<ProcessBaseRepository<TEntity, TKey>> logger
        )
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            BatchSize = _client.BatchSize;
            Database = _client.GetDatabase();
            _collectionName = multipleInstances ? $"{ typeof(TEntity).Name.UntilNonAlfanumeric() }-{ ActiveInstance }" : typeof(TEntity).Name.UntilNonAlfanumeric();
        }

        /// <summary>
        /// Get configuration object
        /// </summary>
        /// <returns></returns>
        private ProcessedConfiguration GetConfiguration()
        {
            try
            {
                return MongoCollectionConfiguration
                    .Find(Builders<ProcessedConfiguration>.Filter.Empty)
                    .FirstOrDefault();
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());

                return default;
            }
        }

        /// <summary>
        /// Get active instance
        /// </summary>
        /// <returns></returns>
        protected byte ActiveInstance => GetConfiguration().ActiveInstance;

        /// <summary>
        /// Get client
        /// </summary>
        /// <returns></returns>
        protected IMongoCollection<TEntity> MongoCollection => Database.GetCollection<TEntity>(_collectionName);

        /// <summary>
        /// Configuration collection
        /// </summary>
        protected IMongoCollection<ProcessedConfiguration> MongoCollectionConfiguration => Database.GetCollection<ProcessedConfiguration>(_collectionNameConfiguration);

        /// <inheritdoc />
        public async Task<TEntity> GetAsync(TKey id)
        {
            try
            {
                var searchFilter = Builders<TEntity>.Filter.Eq("_id", id);
                return await MongoCollection.Find(searchFilter).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return default;
            }
        }

        /// <inheritdoc />
        public async Task<IAsyncCursor<TEntity>> GetAllByCursorAsync()
        {
            return await MongoCollection.FindAsync(FilterDefinition<TEntity>.Empty);
        }

        /// <inheritdoc />
        public async Task<List<TEntity>> GetAllAsync()
        {
            return await MongoCollection.AsQueryable().ToListAsync();
        }

        /// <summary>
        /// Dispose
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
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
