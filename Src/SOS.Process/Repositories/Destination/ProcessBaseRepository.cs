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
        private readonly bool _toggleable;
        protected string _collectionName;

        /// <summary>
        /// Mongo db
        /// </summary>
        protected IMongoDatabase Database;

        /// <summary>
        /// Disposed
        /// </summary>
        private bool _disposed;


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
            _toggleable = toggleable;
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            BatchSize = _client.BatchSize;
            Database = _client.GetDatabase();

            _collectionName = GetInstanceName(InActiveInstance);

            // Init config
            InitializeConfiguration();
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
                    new ReplaceOptions { IsUpsert = true });
                return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }
        }

        public byte ActiveInstance => (byte)(GetConfiguration()?.ActiveInstance ?? 1);
        public byte InActiveInstance => (byte)(ActiveInstance == 0 ? 1 : 0);

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

        protected string GetInstanceName(byte instance)
        {
            return _toggleable ? $"{ typeof(TEntity).Name.UntilNonAlfanumeric() }-{ instance }" : $"{ typeof(TEntity).Name.UntilNonAlfanumeric() }";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        protected void SetCollectionName(byte instance)
        {
            _collectionName = GetInstanceName(instance);
        }

        /// <summary>
        /// Get collection
        /// </summary>
        /// <returns></returns>
        protected IMongoCollection<TEntity> MongoCollection => Database.GetCollection<TEntity>(_collectionName);

        /// <summary>
        /// Configuration collection
        /// </summary>
        private IMongoCollection<ProcessedConfiguration> MongoCollectionConfiguration => Database
            .GetCollection<ProcessedConfiguration>(_collectionNameConfiguration)
            .WithWriteConcern(new WriteConcern(w:1, journal: true ));

        /// <inheritdoc />
        public async Task<List<TEntity>> GetAllAsync()
        {
            var res = await MongoCollection.AsQueryable().ToListAsync();

            return res;
        }


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
                await MongoCollection.InsertManyAsync(batch, new InsertManyOptions() { IsOrdered = false, BypassDocumentValidation = true});
                
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

        public int BatchSize { get; }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string ActiveCollectionName =>  GetInstanceName(ActiveInstance);

        public string InActiveCollectionName => GetInstanceName(InActiveInstance);

        /// <inheritdoc />
        public async Task<bool> AddAsync(TEntity item)
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
            return await AddBatchAsync(items);
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
        public async Task<bool> SetActiveInstanceAsync(byte instance)
        {
            try
            {
                var config = GetConfiguration();

                config.ActiveInstance = instance;

                var updateResult = await MongoCollectionConfiguration.ReplaceOneAsync(
                    x => x.Id.Equals(config.Id),
                    config,
                    new ReplaceOptions { IsUpsert = true });

                return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());

                return default;
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> VerifyCollectionAsync()
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

                return true;
            }

            return false;
        }
    }
}
