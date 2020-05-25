using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Process.Database.Interfaces;

namespace SOS.Process.Repositories.Source
{
    public class DwcaVerbatimRepository : VerbatimBaseRepository<DwcObservationVerbatim, ObjectId>, Interfaces.IDwcaVerbatimRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public DwcaVerbatimRepository(
            IVerbatimClient client,
            ILogger<DwcaVerbatimRepository> logger) : base(client, logger)
        {

        }

        /// <summary>
        /// Checks if the collection exists.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CheckIfCollectionExistsAsync(
            int dataProviderId,
            string dataProviderIdentifier)
        {
            string collectionName = GetCollectionName(dataProviderId, dataProviderIdentifier);
            return await CheckIfCollectionExistsAsync(collectionName);
        }

        public async Task<IAsyncCursor<DwcObservationVerbatim>> GetAllByCursorAsync(
            int dataProviderId,
            string dataProviderIdentifier)
        {
            string collectionName = GetCollectionName(dataProviderId, dataProviderIdentifier);
            var mongoCollection = base.GetMongoCollection(collectionName);
            return await GetAllByCursorAsync(mongoCollection);
        }

        public async Task<List<DwcObservationVerbatim>> GetAllAsync(
            int dataProviderId,
            string dataProviderIdentifier)
        {
            string collectionName = GetCollectionName(dataProviderId, dataProviderIdentifier);
            var mongoCollection = base.GetMongoCollection(collectionName);
            return await GetAllAsync(mongoCollection);
        }

        /// <summary>
        /// Gets collection name. Example: "DwcaOccurrence_007_ButterflyMonitoring".
        /// </summary>
        /// <returns></returns>
        private string GetCollectionName(int dataProviderId, string dataProviderIdentifier)
        {
            return $"DwcaOccurrence_{dataProviderId:D3}_{dataProviderIdentifier}";
        }

        /// <summary>
        /// Gets temp collection name. Example: "DwcaOccurrence_007_ButterflyMonitoring_temp".
        /// </summary>
        /// <returns></returns>
        private string GetTempHarvestCollectionName(int dataProviderId, string dataProviderIdentifier)
        {
            return $"DwcaOccurrence_{dataProviderId:D3}_{dataProviderIdentifier}_temp";
        }

        private async Task<bool> RenameCollection(int dataProviderId, string dataProviderIdentifier)
        {
            var tempHarvestCollectionName = GetTempHarvestCollectionName(dataProviderId, dataProviderIdentifier);
            var collectionName = GetTempHarvestCollectionName(dataProviderId, dataProviderIdentifier);
            var tempHarvestCollectionExists = await CollectionExistsAsync(tempHarvestCollectionName);
            if (!tempHarvestCollectionExists) return false;

            var collectionExists = await CollectionExistsAsync(collectionName);
            if (collectionExists)
            {
                await base.Database.DropCollectionAsync(collectionName);
            }

            await base.Database.RenameCollectionAsync(tempHarvestCollectionName, collectionName);
            return true;
        }

        private async Task<bool> RenameCollectionUsingTransaction(int dataProviderId, string dataProviderIdentifier)
        {
            using var session = await base.Client.StartSessionAsync();
            var tempHarvestCollectionName = GetTempHarvestCollectionName(dataProviderId, dataProviderIdentifier);
            var collectionName = GetTempHarvestCollectionName(dataProviderId, dataProviderIdentifier);
            var tempHarvestCollectionExists = await CollectionExistsAsync(session, tempHarvestCollectionName);
            if (!tempHarvestCollectionExists) return false;

            var collectionExists = await CollectionExistsAsync(session, collectionName);
            if (collectionExists)
            {
                await base.Database.DropCollectionAsync(session, collectionName);
            }

            await base.Database.RenameCollectionAsync(session, tempHarvestCollectionName, collectionName);
            return true;
        }

        private async Task<bool> CollectionExistsAsync(string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var collections = await base.Database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
            return await collections.AnyAsync();
        }

        private async Task<bool> CollectionExistsAsync(IClientSessionHandle session, string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var collections = await base.Database.ListCollectionsAsync(session, new ListCollectionsOptions { Filter = filter });
            return await collections.AnyAsync();
        }

    }
}