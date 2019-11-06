using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Models.Interfaces;
using SOS.Search.Service.Repositories.Interfaces;

namespace SOS.Search.Service.Repositories
{
    /// <summary>
    /// Base class for cosmos db repositories
    /// </summary>
    public class AggregateRepository<TEntity, TKey> : IAggregateRepository<TEntity, TKey> where TEntity : IEntity<TKey>
    {
        /// <summary>
        /// Logger 
        /// </summary>
        protected readonly ILogger<AggregateRepository<TEntity, TKey>> Logger;

        /// <summary>
        /// Mongo db
        /// </summary>
        protected readonly IMongoDatabase Database;

        /// <summary>
        /// Disposed
        /// </summary>
        private bool _disposed;

        private readonly string _collectionName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mongoClient"></param>
        /// <param name="mongoDbConfiguration"></param>
        /// <param name="logger"></param>
        protected AggregateRepository(
            IMongoClient mongoClient,
            IOptions<MongoDbConfiguration> mongoDbConfiguration,
            ILogger<AggregateRepository<TEntity, TKey>> logger
        )
        {
            if (mongoDbConfiguration?.Value == null)
            {
                throw new ArgumentNullException(nameof(mongoDbConfiguration));
            }

            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Database = mongoClient.GetDatabase($"{mongoDbConfiguration.Value.DatabaseName}");

            // Clean name from non alfa numeric chats
            var regex = new Regex(@"\w+");
            var match = regex.Match(typeof(TEntity).Name);

            _collectionName = match.Value;
        }

        /// <summary>
        /// Get client
        /// </summary>
        /// <returns></returns>
        protected IMongoCollection<TEntity> MongoCollection => Database.GetCollection<TEntity>(_collectionName);

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
                return default(TEntity);
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
