﻿using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Base class MongoDB repositories
    /// </summary>
    public class MongoDbProcessedRepositoryBase<TEntity, TKey> where TEntity : IEntity<TKey>
    {
        protected readonly IProcessClient _client;
        protected ILogger<MongoDbProcessedRepositoryBase<TEntity, TKey>> Logger;

        /// <summary>
        ///     Mongo db
        /// </summary>
        protected IMongoDatabase Database;

        /// <summary>
        ///     Get collection
        /// </summary>
        /// <returns></returns>
        protected IMongoCollection<TEntity> MongoCollection => Database.GetCollection<TEntity>(IndexName);
        private string IndexName => IndexHelper.GetInstanceName<TEntity>();

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="toggleable"></param>
        /// <param name="logger"></param>
        public MongoDbProcessedRepositoryBase(
            IProcessClient client,
            bool toggleable,
            ILogger<MongoDbProcessedRepositoryBase<TEntity, TKey>> logger
        )
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Database = _client.GetDatabase();
        }
        private async Task<bool> AddAsync(TEntity item, IMongoCollection<TEntity> mongoCollection, byte attempt)
        {
            try
            {
                await mongoCollection.InsertOneAsync(item);

                return true;
            }
            catch (MongoExecutionTimeoutException)
            {
                if (attempt < 3)
                {
                    return await AddAsync(item, mongoCollection, ++attempt);
                }

                throw new Exception("Failed to add item to mongodb. Request time out.");
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

        /// <summary>
        /// Add batch of items
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="attempt"></param>
        /// <returns></returns>
        private async Task<bool> AddBatchAsync(IEnumerable<TEntity> batch, byte attempt)
        {
            if (!batch?.Any() ?? true)
            {
                return true;
            }

            try
            {
                await MongoCollection.InsertManyAsync(batch,
                    new InsertManyOptions { IsOrdered = false, BypassDocumentValidation = true });

                return true;
            }
            catch (MongoExecutionTimeoutException)
            {
                if (attempt < 3)
                {
                    return await AddBatchAsync(batch, ++attempt);
                }

                throw new Exception("Failed to add batch to mongodb. Request time out.");
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
        /// </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        protected async Task<bool> AddBatchAsync(IEnumerable<TEntity> batch)
        {
            return await AddBatchAsync(batch, 1);
        }

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

        public async Task<IAsyncCursor<TEntity>> GetAllByCursorAsync()
        {
            return await GetAllByCursorAsync(MongoCollection);
        }

        /// <inheritdoc />
        public async Task<IAsyncCursor<TEntity>> GetAllByCursorAsync(IMongoCollection<TEntity> mongoCollection,
            bool noCursorTimeout = false)
        {
            return await mongoCollection.FindAsync(FilterDefinition<TEntity>.Empty,
                new FindOptions<TEntity, TEntity> { NoCursorTimeout = noCursorTimeout, BatchSize = _client.ReadBatchSize, AllowPartialResults = true, CursorType = CursorType.NonTailable });
        }

        /// <inheritdoc />
        public virtual async Task<List<TEntity>> GetAllAsync()
        {
            var res = await MongoCollection.AsQueryable().ToListAsync();

            return res;
        }

        /// <inheritdoc />
        public async Task<bool> AddAsync(TEntity item)
        {
            return await AddAsync(item, MongoCollection);
        }

        /// <inheritdoc />
        public async Task<bool> AddAsync(TEntity item, IMongoCollection<TEntity> mongoCollection)
        {
            return await AddAsync(item, mongoCollection, 1);
        }

        /// <inheritdoc />
        public virtual async Task<bool> AddCollectionAsync()
        {
            try
            {
                // Create the collection
                await Database.CreateCollectionAsync(IndexName);

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
        public virtual async Task<bool> DeleteCollectionAsync()
        {
            try
            {
                // Create the collection
                await Database.DropCollectionAsync(IndexName);

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
        public virtual async Task<bool> VerifyCollectionAsync()
        {
            //filter by collection name
            var exists = (await Database
                    .ListCollectionNamesAsync(new ListCollectionNamesOptions
                    {
                        Filter = new BsonDocument("name", IndexName)
                    }))
                .Any();

            //check for existence
            if (!exists)
            {
                // Create the collection
                await Database.CreateCollectionAsync(IndexName);

                return true;
            }

            return false;
        }
    }
}