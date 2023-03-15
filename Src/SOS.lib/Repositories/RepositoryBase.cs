using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Repositories.Interfaces;

namespace SOS.Lib.Repositories
{

    /// <summary>
    ///     Base class for cosmos db repositories
    /// </summary>
    public class RepositoryBase<TEntity, TKey> : IRepositoryBase<TEntity, TKey> where TEntity : IEntity<TKey>
    {
        private readonly string _collectionName;

        /// <summary>
        ///     Disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        private async Task<bool> AddBatchAsync(IEnumerable<TEntity> batch)
        {
            return await AddBatchAsync(batch, MongoCollection);
        }

        /// <summary>
        /// Add batch of items to mongodb
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="mongoCollection"></param>
        /// <param name="attempt"></param>
        /// <returns></returns>
        private async Task<bool> AddBatchAsync(IEnumerable<TEntity> batch, IMongoCollection<TEntity> mongoCollection, byte attempt = 1)
        {
            var items = batch?.ToArray();
            try
            {
                await mongoCollection.InsertManyAsync(items,
                    new InsertManyOptions { IsOrdered = false, BypassDocumentValidation = true });
                return true;
            }
            catch (MongoCommandException e)
            {
                switch (e.Code)
                {
                    case 16500: //Request Rate too Large
                        // If attempt failed, try split items in half and try again
                        var batchCount = items.Length / 2;

                        // If we are down to less than 10 items something must be wrong
                        if (batchCount > 5)
                        {
                            var addTasks = new List<Task<bool>>
                            {
                                AddBatchAsync(items.Take(batchCount)),
                                AddBatchAsync(items.Skip(batchCount))
                            };

                            // Run all tasks async
                            await Task.WhenAll(addTasks);
                            return addTasks.All(t => t.Result);
                        }

                        break;
                }

                Logger.LogError(e.ToString());

                throw;
            }
            catch (Exception e)
            {
                if (attempt < 3)
                {
                    Logger.LogWarning($"Add batch to mongodb collection ({MongoCollection}). Attempt {attempt} failed. Tries again...");
                    Thread.Sleep(attempt * 1000);
                    attempt++;
                    return await AddBatchAsync(items, mongoCollection, attempt);
                }

                Logger.LogError(e, $"Failed to add batch to mongodb collection ({MongoCollection})");
                throw;
            }
        }

        /// <summary>
        /// Name of collection
        /// </summary>
        protected virtual string CollectionName => $"{_collectionName}{(Mode == JobRunModes.IncrementalActiveInstance ? "_incrementalActive" : Mode == JobRunModes.IncrementalInactiveInstance ? "_incrementalInactive" : "")}";

        protected readonly IMongoClient Client;

        /// <summary>
        ///     Mongo db
        /// </summary>
        protected readonly IMongoDatabase Database;

        /// <summary>
        ///     Logger
        /// </summary>
        protected readonly ILogger Logger;

        protected IMongoCollection<TEntity> GetMongoCollection(string collectionName)
        {
            return Database.GetCollection<TEntity>(collectionName)
                .WithWriteConcern(new WriteConcern(1, journal: true));
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
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        protected RepositoryBase(
            IMongoDbClient client,
            ILogger logger
        ) : this(client, typeof(TEntity).Name, logger)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="collectionName"></param>
        /// <param name="logger"></param>
        protected RepositoryBase(
            IMongoDbClient client,
            string collectionName,
            ILogger logger
        )
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Database = client.GetDatabase();

            BatchSizeRead = client.ReadBatchSize;
            BatchSizeWrite = client.WriteBatchSize;

            // Clean name from non alfa numeric chats
            _collectionName = collectionName.UntilNonAlfanumeric();
        }


        /// <summary>
        ///     Get client
        /// </summary>
        /// <returns></returns>
        protected IMongoCollection<TEntity> MongoCollection => GetMongoCollection(CollectionName);

        /// <inheritdoc />
        public virtual async Task<bool> AddAsync(TEntity item)
        {
            return await AddAsync(item, MongoCollection);
        }

        /// <inheritdoc />
        public virtual async Task<bool> AddAsync(TEntity item, IMongoCollection<TEntity> mongoCollection)
        {
            try
            {
                await mongoCollection.InsertOneAsync(item);

                return true;
            }
            catch (MongoWriteException e)
            {
                Logger.LogError("Failed to add item", e);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to add item", e);
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> AddCollectionAsync()
        {
            return await AddCollectionAsync(CollectionName);
        }

        /// <inheritdoc />
        public async Task<bool> AddCollectionAsync(string collectionName)
        {
            try
            {
                // Create the collection
                await Database.CreateCollectionAsync(collectionName);
                Logger.LogInformation($"The following MongoDB collection was created: [{collectionName}]");

                return true;
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to add collection", e);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task AddIndexes(IEnumerable<CreateIndexModel<TEntity>> indexModels)
        {
            await MongoCollection.Indexes.CreateManyAsync(indexModels);
        }

        /// <inheritdoc />
        public virtual async Task<bool> AddManyAsync(IEnumerable<TEntity> items)
        {
            return await AddManyAsync(items, MongoCollection);
        }

        /// <inheritdoc />
        public async Task<bool> AddManyAsync(IEnumerable<TEntity> items, IMongoCollection<TEntity> mongoCollection)
        {
            if (!items?.Any() ?? true)
            {
                return true;
            }

            var success = true;
            var count = 0;
            var entities = items.ToArray();
            var batch = entities.Skip(0).Take(BatchSizeWrite)?.ToArray();

            while (batch?.Any() ?? false)
            {
                success = success && await AddBatchAsync(batch, mongoCollection);
                count++;
                batch = entities.Skip(BatchSizeWrite * count).Take(BatchSizeWrite)?.ToArray();
            }

            return success;
        }

        /// <inheritdoc />
        public virtual async Task<bool> AddOrUpdateAsync(TEntity item)
        {
            return await AddOrUpdateAsync(item, MongoCollection);
        }

        /// <inheritdoc />
        public async Task<bool> AddOrUpdateAsync(TEntity item, IMongoCollection<TEntity> mongoCollection)
        {
            var filter = Builders<TEntity>.Filter.Eq("_id", item.Id);

            var entity = await mongoCollection.Find(filter).FirstOrDefaultAsync();
            if (entity == null)
            {
                return await AddAsync(item);
            }

            return await UpdateAsync(item.Id, item);
        }

        /// <inheritdoc />
        public int BatchSizeRead { get; set; }

        /// <inheritdoc />
        public int BatchSizeWrite { get; set; }

        /// <inheritdoc />
        public virtual async Task<bool> CheckIfCollectionExistsAsync()
        {
            return await CheckIfCollectionExistsAsync(CollectionName);
        }

        /// <inheritdoc />
        public async Task<bool> CheckIfCollectionExistsAsync(string collectionName)
        {
            //filter by collection name
            var exists = await (await Database
                    .ListCollectionNamesAsync(new ListCollectionNamesOptions
                    {
                        Filter = new BsonDocument("name", collectionName)
                    }))
                .AnyAsync();

            return exists;
        }

        /// <inheritdoc />
        public virtual async Task<bool> DeleteAsync(TKey id)
        {
            return await DeleteAsync(id, MongoCollection);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(TKey id, IMongoCollection<TEntity> mongoCollection)
        {
            try
            {
                var removeFilter = Builders<TEntity>.Filter.Eq("_id", id);
                var deleteResult = await mongoCollection.DeleteOneAsync(removeFilter);

                return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to delete item", e);

                throw; 
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> DeleteCollectionAsync()
        {
            return await DeleteCollectionAsync(CollectionName);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteCollectionAsync(string collectionName)
        {
            try
            {
                // Create the collection
                await Database.DropCollectionAsync(collectionName);
                Logger.LogInformation($"The following MongoDB collection was deleted: [{collectionName}]");

                return true;
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to delete collection", e);
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<long> CountAllDocumentsAsync()
        {
            return await CountAllDocumentsAsync(MongoCollection);
        }

        /// <inheritdoc />
        public async Task<long> CountAllDocumentsAsync(IMongoCollection<TEntity> mongoCollection)
        {
            try
            {
                return await mongoCollection.CountDocumentsAsync(FilterDefinition<TEntity>.Empty);
            }
            catch
            {
                return 0;
            }
            
        }

        /// <inheritdoc />
        public virtual async Task<bool> DeleteManyAsync(IEnumerable<TKey> ids)
        {
            return await DeleteManyAsync(ids, MongoCollection);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteManyAsync(IEnumerable<TKey> ids, IMongoCollection<TEntity> mongoCollection)
        {
            try
            {
                var res = await mongoCollection.Find(x => ids.Contains(x.Id)).ToListAsync();
                if (res != null && res.Any())
                {
                    var removeFilter = Builders<TEntity>.Filter.In("_id", res.Select(x => x.Id));
                    var deleteResult = await mongoCollection // todo - is this correct?
                        .DeleteManyAsync(removeFilter);
                    //var deleteResult = await Database.GetCollection<TEntity>(typeof(TEntity).Name)
                    //    .DeleteManyAsync(removeFilter);
                    return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to delete batch", e);

                throw;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> GetAsync(TKey id)
        {
            try
            {
                var searchFilter = Builders<TEntity>.Filter.Eq("_id", id);
                return await MongoCollection.FindSync(searchFilter).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to get item by id", e);
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> GetAsync(IEnumerable<TKey> ids)
        {
            try
            {
                return await MongoCollection.Find(x => ids.Contains(x.Id)).ToListAsync();
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to get many by id's", e);

                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> GetBatchAsync(int skip)
        {
            return await GetBatchAsync(skip, MongoCollection);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetBatchAsync(int skip, IMongoCollection<TEntity> mongoCollection)
        {
            try
            {
                var res = await mongoCollection
                    .Find(FilterDefinition<TEntity>.Empty)
                    //.Sort(Builders<TEntity>.Sort.Descending("id"))
                    .Skip(skip)
                    .Limit(BatchSizeRead)
                    .ToListAsync();

                return res;
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to get batch", e);

                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<List<TEntity>> GetAllAsync()
        {
            return await GetAllAsync(MongoCollection);
        }

        /// <inheritdoc />
        public async Task<List<TEntity>> GetAllAsync(IMongoCollection<TEntity> mongoCollection)
        {
            var res = await mongoCollection.AsQueryable().ToListAsync();

            return res;
        }

        /// <inheritdoc />
        public async Task<IAsyncCursor<TEntity>> GetAllByCursorAsync()
        {
            return await GetAllByCursorAsync(MongoCollection);
        }

        /// <inheritdoc />
        public async Task<IAsyncCursor<TEntity>> GetAllByCursorAsync(IMongoCollection<TEntity> mongoCollection,
            bool noCursorTimeout = false)
        {
            return await mongoCollection.FindAsync(FilterDefinition<TEntity>.Empty,
                new FindOptions<TEntity, TEntity> { NoCursorTimeout = noCursorTimeout, BatchSize = BatchSizeRead, AllowPartialResults = true, CursorType = CursorType.NonTailable });
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> GetBatchAsync(TKey startId, TKey endId)
        {
            return await GetBatchAsync(startId, endId, MongoCollection);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetBatchAsync(TKey startId, TKey endId,
            IMongoCollection<TEntity> mongoCollection)
        {
            try
            {
                var filters = new List<FilterDefinition<TEntity>>
                {
                    Builders<TEntity>.Filter.Gte(d => d.Id, startId),
                    Builders<TEntity>.Filter.Lte(d => d.Id, endId)
                };

                var res = await mongoCollection
                    .Find(Builders<TEntity>.Filter.And(filters))
                    .ToListAsync();

                return res;
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to get batch", e);

                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<TKey> GetMaxIdAsync()
        {
            return await GetMaxIdAsync(MongoCollection);
        }

        /// <inheritdoc />
        public async Task<TKey> GetMaxIdAsync(IMongoCollection<TEntity> mongoCollection)
        {
            try
            {
                Logger.LogDebug($"Try to get max id for ({mongoCollection.CollectionNamespace})");
                var max = await mongoCollection
                    .Find(FilterDefinition<TEntity>.Empty)
                    .Project(d => d.Id)
                    .Sort(Builders<TEntity>.Sort.Descending("_id"))
                    .Limit(1)
                    .FirstOrDefaultAsync();

                return max;
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to get max id", e);

                throw;
            }
        }

        /// <inheritdoc />
        public JobRunModes Mode { get; set; }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> QueryAsync(FilterDefinition<TEntity> filter)
        {
            try
            {
                return await MongoCollection
                    .Find(filter)
                    .ToListAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> UpdateAsync(TKey id, TEntity entity)
        {
            return await UpdateAsync(id, entity, MongoCollection);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(TKey id, TEntity entity, IMongoCollection<TEntity> mongoCollection)
        {
            try
            {
                var updateResult = await mongoCollection.ReplaceOneAsync(
                    x => x.Id.Equals(id),
                    entity,
                    new ReplaceOptions { IsUpsert = true });
                return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to update entity", e);
                throw;
            }
        }
    }
}