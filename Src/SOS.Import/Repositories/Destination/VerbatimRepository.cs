using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Interfaces;

namespace SOS.Import.Repositories.Destination
{
    /// <summary>
    ///     Base class for cosmos db repositories
    /// </summary>
    public class VerbatimRepository<TEntity, TKey> : IVerbatimRepository<TEntity, TKey> where TEntity : IEntity<TKey>
    {
        private readonly int _batchSize;

        private readonly string _collectionName;

        private string CollectionName => IncrementalMode ? $"{_collectionName}_incremental" : _collectionName;

        protected readonly IMongoClient Client;

        /// <summary>
        ///     Mongo db
        /// </summary>
        protected readonly IMongoDatabase Database;

        /// <summary>
        ///     Logger
        /// </summary>
        protected readonly ILogger<VerbatimRepository<TEntity, TKey>> Logger;

        /// <summary>
        ///     Disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        protected VerbatimRepository(
            IVerbatimClient importClient,
            ILogger<VerbatimRepository<TEntity, TKey>> logger
        )
        {
            if (importClient == null)
            {
                throw new ArgumentNullException(nameof(importClient));
            }

            Client = importClient;
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Database = importClient.GetDatabase();

            _batchSize = importClient.WriteBatchSize;

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
            var batch = entities.Skip(0).Take(_batchSize).ToArray();

            while (batch?.Any() ?? false)
            {
                success = success && await AddBatchAsync(batch, mongoCollection);
                count++;
                batch = entities.Skip(_batchSize * count).Take(_batchSize).ToArray();
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
        public async Task<List<TEntity>> GetAllAsync()
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
                    new ReplaceOptions {IsUpsert = true});
                return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected IMongoCollection<TEntity> GetMongoCollection(string collectionName)
        {
            return Database.GetCollection<TEntity>(collectionName)
                .WithWriteConcern(new WriteConcern(1, journal: true));
        }

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
                    new InsertManyOptions {IsOrdered = false, BypassDocumentValidation = true});
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
    }
}