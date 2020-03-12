using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Processed.Validation;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Repositories.Destination
{
    /// <summary>
    /// Base class for cosmos db repositories
    /// </summary>
    public class ProcessedSightingRepository : ProcessBaseRepository<ProcessedSighting, ObjectId>, IProcessedSightingRepository
    {
        private readonly IInadequateItemRepository _inadequateItemRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="darwinCoreInadequateRepository"></param>
        /// <param name="logger"></param>
        public ProcessedSightingRepository(
            IProcessClient client,
            IInadequateItemRepository darwinCoreInadequateRepository,
            ILogger<ProcessedSightingRepository> logger
        ) : base(client, true, logger)
        {
            _inadequateItemRepository = darwinCoreInadequateRepository ?? throw new ArgumentNullException(nameof(darwinCoreInadequateRepository));
        }

        /// <summary>
        /// Validate Darwin core.
        /// </summary>
        /// <param name="items"></param>
        /// <returns>Invalid items</returns>
        private IEnumerable<InadequateItem> Validate(
           ref IEnumerable<ProcessedSighting> items)
        {
            var validItems = new List<ProcessedSighting>();
            var invalidItems = new List<InadequateItem>();

            foreach (var item in items)
            {
                var inadequateItem = new InadequateItem(item.DatasetId, item.DatasetName, item.Occurrence.Id);

                if (item.Taxon == null)
                {
                    inadequateItem.Defects.Add("Taxon not found");
                }

                if ((item.Location?.CoordinateUncertaintyInMeters ?? 0) > 100000)
                {
                    inadequateItem.Defects.Add("CoordinateUncertaintyInMeters exceeds max value 100 km");
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

        private new IMongoCollection<ProcessedSighting> MongoCollection => Database.GetCollection<ProcessedSighting>(_collectionName);

        /// <inheritdoc />
        public new async Task<int> AddManyAsync(IEnumerable<ProcessedSighting> items)
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
                    Builders<ProcessedSighting>.Filter.Eq(dwc => dwc.Provider, provider));

            // switch to inactive instance and add data 
            SetCollectionName(InstanceToUpdate);

            return await AddManyAsync(source.ToEnumerable()) != 0;
        }

        /// <inheritdoc />
        public async Task CreateIndexAsync()
        {
            var indexModels = new List<CreateIndexModel<ProcessedSighting>>()
            {
                new CreateIndexModel<ProcessedSighting>(
                    Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Event.EndDate), 
                    new CreateIndexOptions<ProcessedSighting>{ PartialFilterExpression = Builders<ProcessedSighting>.Filter.Ne(d => d.Event.EndDate, null) } ),
                new CreateIndexModel<ProcessedSighting>(
                    Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Event.StartDate),
                    new CreateIndexOptions<ProcessedSighting>{ PartialFilterExpression = Builders<ProcessedSighting>.Filter.Ne(d => d.Event.StartDate, null) }),
                new CreateIndexModel<ProcessedSighting>(
                    Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Identification.Validated)),
                new CreateIndexModel<ProcessedSighting>(
                    Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Location.CountyId.Id)),
                new CreateIndexModel<ProcessedSighting>(
                    Builders<ProcessedSighting>.IndexKeys.Geo2DSphere(a => a.Location.Point)),
                new CreateIndexModel<ProcessedSighting>(
                    Builders<ProcessedSighting>.IndexKeys.Geo2DSphere(a => a.Location.PointWithBuffer)),
                new CreateIndexModel<ProcessedSighting>(
                    Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Location.ProvinceId.Id)),
                new CreateIndexModel<ProcessedSighting>(
                    Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Location.MunicipalityId.Id)),
                new CreateIndexModel<ProcessedSighting>(
                    Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Occurrence.IsPositiveObservation),
                    new CreateIndexOptions<ProcessedSighting>{ PartialFilterExpression = Builders<ProcessedSighting>.Filter.Ne(d => d.Occurrence.IsPositiveObservation, null) }),
                new CreateIndexModel<ProcessedSighting>(
                    Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Occurrence.GenderId.Id),
                    new CreateIndexOptions<ProcessedSighting>{ PartialFilterExpression = Builders<ProcessedSighting>.Filter.Ne(d => d.Occurrence.GenderId.Id, 0) }),
                new CreateIndexModel<ProcessedSighting>(
                    Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Provider)),
                new CreateIndexModel<ProcessedSighting>(
                    Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Taxon.Id)),
                new CreateIndexModel<ProcessedSighting>(
                    Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Taxon.RedlistCategory),
                    new CreateIndexOptions<ProcessedSighting>{ PartialFilterExpression = Builders<ProcessedSighting>.Filter.Ne(d => d.Taxon.RedlistCategory, null) })
            };

            await MongoCollection.Indexes.CreateManyAsync(indexModels);

            /*   indexModels.Add(new CreateIndexModel<DarwinCore>(Builders<DarwinCore>.IndexKeys.Combine(
                   Builders<DarwinCore>.IndexKeys.Ascending(x => x.ParentIds),
                   Builders<ImageADarwinCoreggregate>.IndexKeys.Ascending(x => x.Class))));
                   */

            /*
            Logger.LogDebug("Start creating End date index");
            await MongoCollection.Indexes.CreateOneAsync(new CreateIndexModel<ProcessedSighting>(
                Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Event.EndDate)));
            Logger.LogDebug("Finish creating End date index");

            Logger.LogDebug("Start creating Start date index");
            await MongoCollection.Indexes.CreateOneAsync(new CreateIndexModel<ProcessedSighting>(
                Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Event.StartDate)));
            Logger.LogDebug("Finish creating Start date index");

            Logger.LogDebug("Start creating Validated index");
            await MongoCollection.Indexes.CreateOneAsync(new CreateIndexModel<ProcessedSighting>(
                Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Identification.Validated)));
            Logger.LogDebug("Finish creating Validated index");

            Logger.LogDebug("Start creating County index");
            await MongoCollection.Indexes.CreateOneAsync(new CreateIndexModel<ProcessedSighting>(
                Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Location.CountyId.Id)));
            Logger.LogDebug("Finish creating County index");

            Logger.LogDebug("Start creating Point index");
            await MongoCollection.Indexes.CreateOneAsync(new CreateIndexModel<ProcessedSighting>(
                Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Location.Point)));
            Logger.LogDebug("Finish creating Point index");

            Logger.LogDebug("Start creating Point with buffer index");
            await MongoCollection.Indexes.CreateOneAsync(new CreateIndexModel<ProcessedSighting>(
                Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Location.PointWithBuffer)));
            Logger.LogDebug("Finish creating Point with buffer index");

            Logger.LogDebug("Start creating Province index");
            await MongoCollection.Indexes.CreateOneAsync(new CreateIndexModel<ProcessedSighting>(
                Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Location.ProvinceId.Id)));
            Logger.LogDebug("Finish creating Province index");

            Logger.LogDebug("Start creating Municipality index");
            await MongoCollection.Indexes.CreateOneAsync(new CreateIndexModel<ProcessedSighting>(
                Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Location.MunicipalityId.Id)));
            Logger.LogDebug("Finish creating Municipality index");

            Logger.LogDebug("Start creating Is Positive Observation index");
            await MongoCollection.Indexes.CreateOneAsync(new CreateIndexModel<ProcessedSighting>(
                Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Occurrence.IsPositiveObservation)));
            Logger.LogDebug("Finish creating Is Positive Observation index");

            Logger.LogDebug("Start creating Gender index");
            await MongoCollection.Indexes.CreateOneAsync(new CreateIndexModel<ProcessedSighting>(
                Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Occurrence.GenderId.Id)));
            Logger.LogDebug("Finish creating Gender index");

            Logger.LogDebug("Start creating Provider index");
            await MongoCollection.Indexes.CreateOneAsync(new CreateIndexModel<ProcessedSighting>(
                Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Provider)));
            Logger.LogDebug("Finish creating Provider index");

            Logger.LogDebug("Start creating Taxon id index");
            await MongoCollection.Indexes.CreateOneAsync(new CreateIndexModel<ProcessedSighting>(
                Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Taxon.Id)));
            Logger.LogDebug("Finish creating Taxon id index");

            Logger.LogDebug("Start creating Taxon Redlist Category index");
            await MongoCollection.Indexes.CreateOneAsync(new CreateIndexModel<ProcessedSighting>(
                Builders<ProcessedSighting>.IndexKeys.Ascending(p => p.Taxon.RedlistCategory)));
            Logger.LogDebug("Finish creating Taxon Redlist Category index");*/
        }

        /// <inheritdoc />
        public async Task<bool> DeleteProviderDataAsync(DataProvider provider)
        {
            try
            {
                // Create the collection
                var res = await MongoCollection.DeleteManyAsync(Builders<ProcessedSighting>.Filter.Eq(dwc => dwc.Provider, provider));

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
        public override async Task<bool> VerifyCollectionAsync()
        {
            var newCreated = await base.VerifyCollectionAsync();

            // Make sure inadequate collection is empty 
            await _inadequateItemRepository.DeleteCollectionAsync();
            await _inadequateItemRepository.AddCollectionAsync();

            return newCreated;
        }
    }
}
