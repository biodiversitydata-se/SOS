using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.DarwinCore;
using SOS.Lib.Models.Processed.Validation;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Repositories.Destination
{
    /// <summary>
    /// Base class for cosmos db repositories
    /// </summary>
    public class DarwinCoreRepository : ProcessBaseRepository<DarwinCore<DynamicProperties>, ObjectId>, IDarwinCoreRepository
    {
        private readonly IInadequateItemRepository _inadequateItemRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="darwinCoreInadequateRepository"></param>
        /// <param name="logger"></param>
        public DarwinCoreRepository(
            IProcessClient client,
            IInadequateItemRepository darwinCoreInadequateRepository,
            ILogger<DarwinCoreRepository> logger
        ):base(client, true, logger)
        {
            _inadequateItemRepository = darwinCoreInadequateRepository ?? throw new ArgumentNullException(nameof(darwinCoreInadequateRepository));
        }

        /// <summary>
        /// Validate Darwin core.
        /// </summary>
        /// <param name="items"></param>
        /// <returns>Invalid items</returns>
        private IEnumerable<InadequateItem> Validate(
           ref IEnumerable<DarwinCore<DynamicProperties>> items)
        {
            var validItems = new List<DarwinCore<DynamicProperties>>();
            var invalidItems = new List<InadequateItem>();

            foreach (var item in items)
            {
                var inadequateItem = new InadequateItem(item.DatasetID, item.DatasetName, item.Occurrence.OccurrenceID);

                if (item.Taxon == null)
                {
                    inadequateItem.Defects.Add("Taxon not found");
                }

                if (!item.IsInEconomicZoneOfSweden)
                {
                    inadequateItem.Defects.Add("Sighting outside Swedish economic zone");
                }

                if (string.IsNullOrEmpty(item?.Occurrence.CatalogNumber))
                {
                    inadequateItem.Defects.Add("CatalogNumber is missing");
                }

                if (inadequateItem.Defects.Any())
                {
                    invalidItems.Add(inadequateItem);
                }
                else
                {
                    validItems.Add(item);
                }
            }

            items = validItems;

            return invalidItems.Any() ? invalidItems : null;
        }

        private new IMongoCollection<DarwinCore<DynamicProperties>> MongoCollection => Database.GetCollection<DarwinCore<DynamicProperties>>(_collectionName);

        /// <inheritdoc />
        public async Task<int> AddManyAsync(IEnumerable<DarwinCore<DynamicProperties>> items)
        {
            // Separate adequate and inadequate data
            var inadequateItems = Validate(ref items);

            // Save adequate processed data
            var success = await base.AddManyAsync(items);
            
            // No inadequate items, we are done here
            if (success && (inadequateItems?.Any() ?? false))
            {
                await _inadequateItemRepository.AddManyAsync(inadequateItems);
            }

            // Save inadequate items 
            return success ? items.Count() : 0;
        }

        /// <inheritdoc />
        public async Task<bool> CopyProviderDataAsync(DataProvider provider)
        {
            // Get data from active instance
            SetCollectionName(ActiveInstance);
            
            var source = await
                MongoCollection.FindAsync(
                    Builders<DarwinCore<DynamicProperties>>.Filter.Eq(dwc => dwc.Provider, provider));

            // switch to inactive instance and add data 
            SetCollectionName(InstanceToUpdate);

            return await AddManyAsync(source.ToEnumerable()) != 0;
        }

        /// <inheritdoc />
        public async Task CreateIndexAsync()
        {
            var indexModels = new List<CreateIndexModel<DarwinCore<DynamicProperties>>>()
            {
                new CreateIndexModel<DarwinCore<DynamicProperties>>(
                    Builders<DarwinCore<DynamicProperties>>.IndexKeys.Ascending(p => p.Taxon.Id)),
                new CreateIndexModel<DarwinCore<DynamicProperties>>(
                    Builders<DarwinCore<DynamicProperties>>.IndexKeys.Ascending(p => p.Provider))
            };

            /*   indexModels.Add(new CreateIndexModel<DarwinCore>(Builders<DarwinCore>.IndexKeys.Combine(
                   Builders<DarwinCore>.IndexKeys.Ascending(x => x.ParentIds),
                   Builders<ImageADarwinCoreggregate>.IndexKeys.Ascending(x => x.Class))));
                   */
            Logger.LogDebug("Creating indexes");
            await MongoCollection.Indexes.CreateManyAsync(indexModels);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteProviderDataAsync(DataProvider provider)
        {
            try
            {
                // Create the collection
                var res = await MongoCollection.DeleteManyAsync(Builders<DarwinCore<DynamicProperties>>.Filter.Eq(dwc => dwc.Provider, provider));

                return res.IsAcknowledged;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }
        }

        /// <inheritdoc />
        public async Task DropIndexAsync()
        {
            Logger.LogDebug("Dropping current indexes");
            await MongoCollection.Indexes.DropAllAsync();
        }

        /// <inheritdoc />
        public override async Task VerifyCollectionAsync()
        {
            await base.VerifyCollectionAsync();

            // Make sure inadequate collection is empty 
            await _inadequateItemRepository.DeleteCollectionAsync();
            await _inadequateItemRepository.AddCollectionAsync();
        }
    }
}
