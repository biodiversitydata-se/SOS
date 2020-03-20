using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Import.MongoDb.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Interfaces;

namespace SOS.Import.Repositories.Destination
{
    /// <summary>
    /// Base class for cosmos db repositories
    /// </summary>
    public class VerbatimDbConfiguration<TEntity, TKey> : Interfaces.IVerbatimRepository<TEntity, TKey> where TEntity : IEntity<TKey>
    {
        /// <summary>
        /// Logger 
        /// </summary>
        protected readonly ILogger<VerbatimDbConfiguration<TEntity, TKey>> Logger;

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
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        protected VerbatimDbConfiguration(
            IImportClient importClient,
            ILogger<VerbatimDbConfiguration<TEntity, TKey>> logger
        )
        {
            if (importClient == null)
            {
                throw new ArgumentNullException(nameof(importClient));
            }

            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Database = importClient.GetDatabase();
            
            _batchSize = importClient.BatchSize;

            // Clean name from non alfa numeric chats
            _collectionName = typeof(TEntity).Name.UntilNonAlfanumeric();
        }

        /// <summary>
        /// Get client
        /// </summary>
        /// <returns></returns>
        protected IMongoCollection<TEntity> MongoCollection => Database.GetCollection<TEntity>(_collectionName)
            .WithWriteConcern(new WriteConcern(w: 1, journal: true ));

        /// <inheritdoc />
        public async Task<bool> AddAsync(TEntity item)
        {
            try
            {
                await MongoCollection.InsertOneAsync(item);

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        private async Task<bool> AddBatchAsync(IEnumerable<TEntity> batch)
        {
            var items = batch?.ToArray();
            try
            {
                await MongoCollection.InsertManyAsync(batch, new InsertManyOptions() { IsOrdered = false, BypassDocumentValidation = true });
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

        /// <inheritdoc />
        public async Task<bool> AddManyAsync(IEnumerable<TEntity> items)
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
                success = success && await AddBatchAsync(batch);
                count++;
                batch = entities.Skip(_batchSize * count).Take(_batchSize).ToArray();
            }

            return success;
        }

        /// <inheritdoc />
        public async Task<bool> AddOrUpdateAsync(TEntity item)
        {
            var filter = Builders<TEntity>.Filter.Eq("_id", item.Id);

            var entity = await MongoCollection.Find(filter).FirstOrDefaultAsync();
            if (entity == null)
            {
                return await AddAsync(item);
            }

            return await UpdateAsync(item.Id, item);
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
        public async Task<bool> DeleteAsync(TKey id)
        {
            try
            {
                var removeFilter = Builders<TEntity>.Filter.Eq("_id", id);
                var deleteResult = await MongoCollection.DeleteOneAsync(removeFilter);

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
        public async Task<bool> DeleteManyAsync(IEnumerable<TKey> ids)
        {
            try
            {
                var res = await MongoCollection.Find(x => ids.Contains(x.Id)).ToListAsync();
                if (res != null && res.Any())
                {
                    var removeFilter = Builders<TEntity>.Filter.In("_id", res.Select(x=>x.Id));
                    var deleteResult = await Database.GetCollection<TEntity>(typeof(TEntity).Name)
                        .DeleteManyAsync(removeFilter);
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

        /// <inheritdoc />
        public async Task<List<TEntity>> GetAllAsync()
        {
            var res = await MongoCollection.AsQueryable().ToListAsync();

            return res;
        }


        /// <inheritdoc />
        public async Task<bool> UpdateAsync(TKey id, TEntity entity)
        {
            try
            {
                var updateResult = await MongoCollection.ReplaceOneAsync(
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
