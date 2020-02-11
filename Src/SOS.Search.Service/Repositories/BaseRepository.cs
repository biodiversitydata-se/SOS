using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Search.Service.Repositories.Interfaces;

namespace SOS.Search.Service.Repositories
{
    /// <summary>
    /// Base class for cosmos db repositories
    /// </summary>
    public class BaseRepository<TEntity, TKey> : IBaseRepository<TEntity, TKey> where TEntity : IEntity<TKey>
    {
        /// <summary>
        /// Logger 
        /// </summary>
        protected readonly ILogger<BaseRepository<TEntity, TKey>> Logger;

        /// <summary>
        /// Mongo db
        /// </summary>
        protected readonly IMongoDatabase Database;

        /// <summary>
        /// Disposed
        /// </summary>
        private bool _disposed;

        private readonly string _collectionName;
        private readonly string _collectionNameConfiguration = typeof(ProcessedConfiguration).Name;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mongoClient"></param>
        /// <param name="processedDbConfiguration"></param>
        /// <param name="multipleInstances"></param>
        /// <param name="logger"></param>
        protected BaseRepository(
            IMongoClient mongoClient,
            IOptions<MongoDbConfiguration> processedDbConfiguration,
            bool multipleInstances,
            ILogger<BaseRepository<TEntity, TKey>> logger
        )
        {
            if (processedDbConfiguration?.Value == null)
            {
                throw new ArgumentNullException(nameof(processedDbConfiguration));
            }

            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Database = mongoClient.GetDatabase($"{processedDbConfiguration.Value.DatabaseName}");

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
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            var res = await MongoCollection.AsQueryable().ToListAsync();

            return res;
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
