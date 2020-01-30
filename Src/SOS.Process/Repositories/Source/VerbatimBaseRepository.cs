using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Models.Interfaces;
using SOS.Process.Database.Interfaces;

namespace SOS.Process.Repositories.Source
{
    /// <summary>
    /// Base class for verbatim repositories
    /// </summary>
    public class VerbatimBaseRepository<TEntity, TKey> : Interfaces.IVerbatimBaseRepository<TEntity, TKey> where TEntity : IEntity<TKey>
    {
        /// <summary>
        /// Logger 
        /// </summary>
        protected readonly ILogger<VerbatimBaseRepository<TEntity, TKey>> Logger;

        /// <summary>
        /// Mongo db
        /// </summary>
        protected readonly IMongoDatabase Database;

        /// <summary>
        /// Disposed
        /// </summary>
        private bool _disposed;

        private readonly string _collectionName;

        private readonly int _batchSize;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        protected VerbatimBaseRepository(
            IVerbatimClient client,
            ILogger<VerbatimBaseRepository<TEntity, TKey>> logger
        )
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Database = client.GetDatabase();
            _batchSize = client.BatchSize;
            _collectionName = typeof(TEntity).Name;
        }

        /// <summary>
        /// Get client
        /// </summary>
        /// <returns></returns>
        protected IMongoCollection<TEntity> MongoCollection => Database.GetCollection<TEntity>(_collectionName);

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetBatchAsync(TKey startId)
        {
            try
            {
                
                var res = await MongoCollection
                    .Find(Builders<TEntity>.Filter.Gt(e => e.Id, startId))
                    .Sort(Builders<TEntity>.Sort.Ascending(e => e.Id))
                    .Limit(_batchSize)
                    .ToListAsync();

                return res;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());

                return default;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetBatchBySkipAsync(int skip)
        {
            try
            {
                var res = await MongoCollection
                    .Find(FilterDefinition<TEntity>.Empty)
                    //.Sort(Builders<TEntity>.Sort.Descending("id"))
                    .Skip(skip)
                    .Limit(_batchSize)
                    .ToListAsync();

                return res;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());

                return default;
            }
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
