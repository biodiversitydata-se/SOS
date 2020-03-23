using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Export.MongoDb.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Configuration;

namespace SOS.Export.Repositories
{
    /// <summary>
    /// Base class for cosmos db repositories
    /// </summary>
    public class BaseRepository<TEntity, TKey> : Interfaces.IBaseRepository<TEntity, TKey> where TEntity : IEntity<TKey>
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

        private readonly bool _toggleable;

        private readonly string _collectionNameConfiguration = typeof(ProcessedConfiguration).Name;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exportClient"></param>
        /// <param name="toggleable"></param>
        /// <param name="logger"></param>
        protected BaseRepository(
            IExportClient exportClient,
            bool toggleable,
            ILogger<BaseRepository<TEntity, TKey>> logger
        )
        {
            if (exportClient == null)
            {
                throw new ArgumentNullException(nameof(exportClient));
            }

            _toggleable = toggleable;

            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Database = exportClient.GetDatabase();
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
        public byte ActiveInstance => GetConfiguration().ActiveInstance;

        public string CollectionName => _toggleable ? $"{ typeof(TEntity).Name.UntilNonAlfanumeric() }-{ ActiveInstance }" : $"{ typeof(TEntity).Name.UntilNonAlfanumeric() }";

        /// <summary>
        /// Get client
        /// </summary>
        /// <returns></returns>
        protected IMongoCollection<TEntity> MongoCollection => Database.GetCollection<TEntity>(CollectionName);

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
