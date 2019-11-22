using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Models.DarwinCore;
using SOS.Process.Database.Interfaces;
using SOS.Process.Helpers;

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

        private readonly IProcessClient _client;

        /// <summary>
        /// Mongo db
        /// </summary>
        protected IMongoDatabase Database;

        /// <summary>
        /// Disposed
        /// </summary>
        private bool _disposed;

        private readonly string _collectionName;
        private readonly string _collectionNameInadequate;
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
            _client = client ?? throw new ArgumentNullException(nameof(client));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _batchSize = client.BatchSize;
            // Clean name from non alfa numeric chats
            var regex = new Regex(@"\w+");
            _collectionName = regex.Match(typeof(DarwinCore<DynamicProperties>).Name).Value;
            _collectionNameInadequate = $"{_collectionName}_Inadequate";
        }

        /// <inheritdoc />
        public void Initialize(string databaseName)
        {
            _client.Initialize(databaseName);
            Database = _client.GetDatabase();
        }

        /// <summary>
        /// Get client
        /// </summary>
        /// <returns></returns>
        private IMongoCollection<DarwinCore<DynamicProperties>> MongoCollection => Database.GetCollection<DarwinCore<DynamicProperties>>(_collectionName);

        /// <summary>
        /// Client for inadequate objects
        /// </summary>
        private IMongoCollection<DarwinCore<DynamicProperties>> MongoCollectionInadequate => Database.GetCollection<DarwinCore<DynamicProperties>>(_collectionNameInadequate);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        private async Task<bool> AddBatchAsync(IEnumerable<DarwinCore<DynamicProperties>> batch, bool inadequate)
        {
            if (!batch?.Any() ?? true)
            {
                return true;
            }

            try
            {
                if (inadequate)
                {
                    await MongoCollectionInadequate.InsertManyAsync(batch, new InsertManyOptions() { IsOrdered = false });
                }
                else
                {
                    await MongoCollection.InsertManyAsync(batch, new InsertManyOptions() { IsOrdered = false });
                }
                
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
                                AddBatchAsync(batch.Take(batchCount), inadequate),
                                AddBatchAsync(batch.Skip(batchCount), inadequate)
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
        /// Validate Darwin core.
        /// </summary>
        /// <param name="items"></param>
        /// <returns>Invalid items</returns>
        private IEnumerable<DarwinCore<DynamicProperties>> Validate(
           ref IEnumerable<DarwinCore<DynamicProperties>> items)
        {
            var validItems = new List<DarwinCore<DynamicProperties>>();
            var invalidItems = new List<DarwinCore<DynamicProperties>>();

            foreach (var item in items)
            {
                var invalid =
                    item.Taxon == null ||
                    !item.IsInEconomicZoneOfSweden ||
                    string.IsNullOrEmpty(item?.Occurrence.CatalogNumber);

                if (invalid)
                {
                    invalidItems.Add(item);
                }
                else
                {
                    validItems.Add(item);
                }
            }

            items = validItems;

            return invalidItems.Any() ? invalidItems : null;
        }

        /// <inheritdoc />
        public async Task<bool> AddManyAsync(IEnumerable<DarwinCore<DynamicProperties>> items)
        {
            var inadequateItems = Validate(ref items);
            
            var success = true;
            var count = 0;
            var batch = items.Skip(0).Take(_batchSize).ToArray();

            while (batch?.Any() ?? false)
            {
                success = success && await AddBatchAsync(batch, false);
                count++;
                batch = items.Skip(_batchSize * count).Take(_batchSize).ToArray();
            }

            // Save inadequate items in own collection
            count = 0;
            batch = inadequateItems.Skip(0).Take(_batchSize).ToArray();

            while (batch?.Any() ?? false)
            {
                success = success && await AddBatchAsync(batch, true);
                count++;
                batch = inadequateItems.Skip(_batchSize * count).Take(_batchSize).ToArray();
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
                await Database.CreateCollectionAsync(_collectionNameInadequate);

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
            var indexModels = new List<CreateIndexModel<DarwinCore<DynamicProperties>>>()
            {
                new CreateIndexModel<DarwinCore<DynamicProperties>>(
                    Builders<DarwinCore<DynamicProperties>>.IndexKeys.Ascending(p => p.Taxon.TaxonID))
            };

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
                await Database.DropCollectionAsync(_collectionNameInadequate);

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
                    var removeFilter = Builders<DarwinCore<DynamicProperties>>.Filter.In("_id", res.Select(x => x.DatasetID));
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
