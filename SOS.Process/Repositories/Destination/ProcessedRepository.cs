using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Models.DarwinCore;
using SOS.Process.Database.Interfaces;

namespace SOS.Process.Repositories.Destination
{
    /// <summary>
    /// Base class for cosmos db repositories
    /// </summary>
    public class ProcessedRepository : Interfaces.IProcessedRepository 
    {
        /// <summary>
        /// Logger 
        /// </summary>
        protected readonly ILogger<ProcessedRepository> Logger;

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
        public ProcessedRepository(
            IProcessClient client,
            ILogger<ProcessedRepository> logger
        )
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Database = client.GetDatabase();
            _batchSize = client.BatchSize;
            _collectionName = typeof(DarwinCore<DynamicProperties>).Name;
        }

        /// <summary>
        /// Get client
        /// </summary>
        /// <returns></returns>
        protected IMongoCollection<DarwinCore<DynamicProperties>> MongoCollection => Database.GetCollection<DarwinCore<DynamicProperties>>(_collectionName);

        /// <inheritdoc />
        public async Task<bool> AddAsync(DarwinCore<DynamicProperties> item)
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
        private async Task<bool> AddBatchAsync(IEnumerable<DarwinCore<DynamicProperties>> batch)
        {
            var items = batch?.ToArray();
            try
            {
                await MongoCollection.InsertManyAsync(items, new InsertManyOptions() { IsOrdered = false });
                return true;
            }
            catch (MongoCommandException e)
            {
                switch (e.Code)
                {
                    case 16500: //Request Rate too Large
                        // If first atempt failed, try add items one in a time
 
                        var success = true;

                        foreach (var item in items)
                        {
                            success = success && await AddAsync(item);
                        }

                        if (!success)
                        {
                            Logger.LogError(e.ToString());
                            return false;
                        }

                        return true;
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
        public async Task<bool> AddManyAsync(IEnumerable<DarwinCore<DynamicProperties>> items)
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
        public async Task CreateIndexAsync()
        {
            var indexModels = new List<CreateIndexModel<DarwinCore<DynamicProperties>>>();

            indexModels.Add(new CreateIndexModel<DarwinCore<DynamicProperties>>(Builders<DarwinCore<DynamicProperties>>.IndexKeys.Ascending(p => p.Taxon.TaxonID)));

         /*   indexModels.Add(new CreateIndexModel<DarwinCore>(Builders<DarwinCore>.IndexKeys.Combine(
                Builders<DarwinCore>.IndexKeys.Ascending(x => x.ParentIds),
                Builders<ImageADarwinCoreggregate>.IndexKeys.Ascending(x => x.Class))));
                */
            await MongoCollection.Indexes.CreateManyAsync(indexModels);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var removeFilter = Builders<DarwinCore<DynamicProperties>>.Filter.Eq("_id", id);
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
        public async Task<bool> DeleteManyAsync(IEnumerable<string> ids)
        {
            try
            {
                var res = await MongoCollection.Find(x => ids.Contains(x.DatasetID)).ToListAsync();
                if (res != null && res.Any())
                {
                    var removeFilter = Builders<DarwinCore<DynamicProperties>>.Filter.In("_id", res.Select(x=>x.DatasetID));
                    var deleteResult = await Database.GetCollection<DarwinCore<DynamicProperties>>(typeof(DarwinCore<DynamicProperties>).Name)
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
        public async Task<bool> UpdateAsync(string id, DarwinCore<DynamicProperties> entity)
        {
            try
            {
                var updateResult = await MongoCollection.ReplaceOneAsync(
                    x => x.DatasetID.Equals(id),
                    entity,
                    new UpdateOptions {IsUpsert = true});
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
