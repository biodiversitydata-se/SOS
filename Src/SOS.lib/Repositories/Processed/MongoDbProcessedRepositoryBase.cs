using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Base class for cosmos db repositories
    /// </summary>
    public class MongoDbProcessedRepositoryBase<TEntity, TKey> : ProcessRepositoryBase<TEntity>, IMongoDbProcessedRepositoryBase<TEntity, TKey>
        where TEntity : IEntity<TKey>
    {
        private readonly IProcessClient _client;

       
        /// <summary>
        ///     Mongo db
        /// </summary>
        protected IMongoDatabase Database;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="toggleable"></param>
        /// <param name="logger"></param>
        public MongoDbProcessedRepositoryBase(
            IProcessClient client,
            bool toggleable,
            ILogger<ProcessRepositoryBase<TEntity>> logger
        ) : base (client, toggleable, logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            
            Database = _client.GetDatabase();
        }

        /// <summary>
        ///     Get collection
        /// </summary>
        /// <returns></returns>
        protected IMongoCollection<TEntity> MongoCollection => Database.GetCollection<TEntity>(CurrentInstanceName);

       
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
        public virtual async Task<bool> AddCollectionAsync()
        {
            try
            {
                // Create the collection
                await Database.CreateCollectionAsync(CurrentInstanceName);

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
                await Database.DropCollectionAsync(CurrentInstanceName);

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
                        Filter = new BsonDocument("name", CurrentInstanceName)
                    }))
                .Any();

            //check for existence
            if (!exists)
            {
                // Create the collection
                await Database.CreateCollectionAsync(CurrentInstanceName);

                return true;
            }

            return false;
        }

        /// <summary>
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
                await MongoCollection.InsertManyAsync(batch,
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
    }
}