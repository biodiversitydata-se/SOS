using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Lib.Database.Interfaces;
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
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="mongoCollection"></param>
        /// <returns></returns>
        private async Task<bool> AddBatchAsync(IEnumerable<TEntity> batch, IMongoCollection<TEntity> mongoCollection)
        {
            var items = batch?.ToArray();
            try
            {
                await mongoCollection.InsertManyAsync(batch,
                    new InsertManyOptions { IsOrdered = false, BypassDocumentValidation = true });
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
        /// Name of collection
        /// </summary>
        protected string CollectionName => IncrementalMode ? $"{_collectionName}_incremental" : _collectionName;

        protected readonly IMongoClient Client;

        /// <summary>
        ///     Mongo db
        /// </summary>
        protected readonly IMongoDatabase Database;

        /// <summary>
        ///     Logger
        /// </summary>
        protected readonly ILogger<RepositoryBase<TEntity, TKey>> Logger;

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
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        protected RepositoryBase(
            IMongoDbClient client,
            ILogger<RepositoryBase<TEntity, TKey>> logger
        )
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Database = client.GetDatabase();

            BatchSizeRead = client.ReadBatchSize;
            BatchSizeWrite = client.WriteBatchSize;

            // Clean name from non alfa numeric chats
            _collectionName = typeof(TEntity).Name.UntilNonAlfanumeric();
        }

        /// <summary>
        ///     Get client
        /// </summary>
        /// <returns></returns>
        protected IMongoCollection<TEntity> MongoCollection => GetMongoCollection(CollectionName);

        /// <inheritdoc />
        public async Task<bool> AddAsync(TEntity item)
        {
            return await AddAsync(item, MongoCollection);
        }

        /// <inheritdoc />
        public async Task<bool> AddAsync(TEntity item, IMongoCollection<TEntity> mongoCollection)
        {
            try
            {
                await mongoCollection.InsertOneAsync(item);

                return true;
            }
            catch (MongoWriteException)
            {
                // Item allready exists
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
            return await AddCollectionAsync(CollectionName);
        }

        /// <inheritdoc />
        public async Task<bool> AddCollectionAsync(string collectionName)
        {
            try
            {
                // Create the collection
                await Database.CreateCollectionAsync(collectionName);

                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> AddManyAsync(IEnumerable<TEntity> items)
        {
            return await AddManyAsync(items, MongoCollection);
        }

        /// <inheritdoc />
        public async Task<bool> AddManyAsync(IEnumerable<TEntity> items, IMongoCollection<TEntity> mongoCollection)
        {
            var entities = items?.ToArray();
            if (!entities?.Any() ?? true)
            {
                return false;
            }

            var success = true;
            var count = 0;
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
        public async Task<bool> AddOrUpdateAsync(TEntity item)
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
        public async Task<bool> CheckIfCollectionExistsAsync()
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
        public async Task<bool> DeleteAsync(TKey id)
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
                Logger.LogError(e.ToString());

                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteCollectionAsync()
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

                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteManyAsync(IEnumerable<TKey> ids)
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
                Logger.LogError(e.ToString());

                return false;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public async Task<TEntity> GetAsync(TKey id)
        {
            try
            {
                var searchFilter = Builders<TEntity>.Filter.Eq("_id", id);
                return await MongoCollection.FindSync(searchFilter).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return default;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetBatchAsync(int skip)
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
                Logger.LogError(e.ToString());

                return default;
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
                new FindOptions<TEntity, TEntity> { NoCursorTimeout = noCursorTimeout, BatchSize = BatchSizeRead, AllowPartialResults = true,  });
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetBatchAsync(TKey startId, TKey endId)
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
                Logger.LogError(e.ToString());

                return default;
            }
        }

        /// <inheritdoc />
        public async Task<Tuple<TKey, TKey>> GetIdSpanAsync()
        {
            return await GetIdSpanAsync(MongoCollection);
        }

        /// <inheritdoc />
        public async Task<Tuple<TKey, TKey>> GetIdSpanAsync(IMongoCollection<TEntity> mongoCollection)
        {
            try
            {
                var min = await mongoCollection
                    .Find(FilterDefinition<TEntity>.Empty)
                    .Project(d => d.Id)
                    .Sort(Builders<TEntity>.Sort.Ascending("_id"))
                    .Limit(1)
                    .FirstOrDefaultAsync();

                var max = await mongoCollection
                    .Find(FilterDefinition<TEntity>.Empty)
                    .Project(d => d.Id)
                    .Sort(Builders<TEntity>.Sort.Descending("_id"))
                    .Limit(1)
                    .FirstOrDefaultAsync();

                return new Tuple<TKey, TKey>(min, max);
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());

                return default;
            }
        }

        /// <inheritdoc />
        public bool IncrementalMode { get; set; }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(TKey id, TEntity entity)
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
                Logger.LogError(e.ToString());
                return false;
            }
        }

       
    }
}