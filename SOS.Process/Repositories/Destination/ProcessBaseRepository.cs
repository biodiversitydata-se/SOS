using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Repositories.Destination
{
    /// <summary>
    /// Base class for cosmos db repositories
    /// </summary>
    public class ProcessBaseRepository<TEntity, TKey> : IProcessBaseRepository<TEntity, TKey> where TEntity : IEntity<TKey>
    { 
        /// <summary>
        /// Logger 
        /// </summary>
        protected readonly ILogger<ProcessBaseRepository<TEntity, TKey>> Logger;

        private readonly IProcessClient _client;

        /// <summary>
        /// Mongo db
        /// </summary>
        protected IMongoDatabase Database;

        /// <summary>
        /// Disposed
        /// </summary>
        private bool _disposed;

        protected readonly string _collectionName;
        private readonly string _collectionNameConfiguration = typeof(ProcessedConfiguration).Name;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="toggleable"></param>
        /// <param name="logger"></param>
        public ProcessBaseRepository(
            IProcessClient client,
            bool toggleable,
            ILogger<ProcessBaseRepository<TEntity, TKey>> logger
        )
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            BatchSize = _client.BatchSize;
            Database = _client.GetDatabase();

            // Init config
            InitializeConfiguration();
            
            _collectionName = toggleable ? $"{ typeof(TEntity).Name.UntilNonAlfanumeric() }-{ InstanceToUpdate }" : $"{ typeof(TEntity).Name.UntilNonAlfanumeric() }";
        }

        private async Task<bool> AddAsync(TEntity item)
        {
            try
            {
                await MongoCollection.InsertOneAsync(item);

                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }
        }

        /// <summary>
        /// Make sure configuration collection exists
        /// </summary>
        private void InitializeConfiguration()
        {
            //filter by collection name
            var exists = Database
                .ListCollectionNames(new ListCollectionNamesOptions
                {
                    Filter = new BsonDocument("name", _collectionNameConfiguration)
                })
                .Any();

            //check for existence
            if (!exists)
            {
                // Create the collection
                Database.CreateCollection(_collectionNameConfiguration);

                MongoCollectionConfiguration.InsertOne(new ProcessedConfiguration
                {
                    ActiveInstance = 1
                });
            }
        }

        /// <summary>
        /// Update one item
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task<bool> UpdateAsync(TKey id, TEntity entity)
        {
            try
            {
                var updateResult = await MongoCollection.ReplaceOneAsync(
                    x => x.Id.Equals(id),
                    entity,
                    new UpdateOptions { IsUpsert = true });
                return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }
        }

        /// <summary>
        /// Get configuration object
        /// </summary>
        /// <returns></returns>
        protected ProcessedConfiguration GetConfiguration()
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
        /// Check config for instance to update
        /// </summary>
        /// <returns></returns>
        public byte InstanceToUpdate => (byte)((GetConfiguration()?.ActiveInstance ?? 1) == 0 ? 1 : 0);
        
        protected int BatchSize { get; }

        /// <summary>
        /// Get collection
        /// </summary>
        /// <returns></returns>
        protected IMongoCollection<TEntity> MongoCollection => Database.GetCollection<TEntity>(_collectionName);

        /// <summary>
        /// Configuration collection
        /// </summary>
        protected IMongoCollection<ProcessedConfiguration> MongoCollectionConfiguration => Database.GetCollection<ProcessedConfiguration>(_collectionNameConfiguration);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        protected async Task<bool> AddBatchAsync(IEnumerable<TEntity> batch)
        {
            if (!batch?.Any() ?? true)
            {
                return true;
            }

            try
            {
                await MongoCollection.InsertManyAsync(batch, new InsertManyOptions() { IsOrdered = false });
                
                return true;
            }
            catch (MongoCommandException e)
            {
                switch (e.Code)
                {
                    case 16500: //Request Rate too Large
                        // If attempt failed, try split items in half and try again
                        var batchCount = batch.Count() / 2;

                        // If we are down to less than 10 items something must be wrong
                        if (batchCount > 5)
                        {
                            var addTasks = new List<Task<bool>>
                            {
                                AddBatchAsync(batch.Take(batchCount)),
                                AddBatchAsync(batch.Skip(batchCount))
                            };

                            // Run all tasks async
                            await Task.WhenAll(addTasks);
                            return addTasks.All(t => t.Result);
                        }

                        break;
                }

                Logger.LogError(e.ToString());

                return false;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
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

        /// <inheritdoc />
        public async Task<bool> AddCollectionAsync()
        {
            try
            {
                // Create the collection
                await Database.CreateCollectionAsync(_collectionName);

                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> AddManyAsync(IEnumerable<TEntity> items)
        {
            var success = true;
            var count = 0;
            var batch = items.Skip(0).Take(BatchSize).ToArray();

            while (batch?.Any() ?? false)
            {
                success = success && await AddBatchAsync(batch);
                count++;
                batch = items.Skip(BatchSize * count).Take(BatchSize).ToArray();
            }

            return success;
        }

        /// <inheritdoc />
        public async Task<bool> AddOrUpdateAsync(TEntity item)
        {
            var entity = await GetAsync(item.Id);
            if (entity == null)
            {
                return await AddAsync(item);
            }

            return await UpdateAsync(item.Id, item);
        }


        /// <inheritdoc />
        public async Task<bool> DeleteCollectionAsync()
        {
            try
            {
                // Create the collection
                await Database.DropCollectionAsync(_collectionName);

                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<TEntity> GetAsync(TKey id)
        {
            try
            {
                var filter = Builders<TEntity>.Filter.Eq("_id", id);

                return await MongoCollection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());

                return default;
            }
        }

        /// <inheritdoc />
        public async Task VerifyCollectionAsync()
        {
            //filter by collection name
            var exists = (await Database
                .ListCollectionNamesAsync(new ListCollectionNamesOptions
                {
                    Filter = new BsonDocument("name", _collectionName)
                }))
                .Any();

            //check for existence
            if (!exists)
            {
                // Create the collection
                await Database.CreateCollectionAsync(_collectionName);
            }
        }
    }
}
