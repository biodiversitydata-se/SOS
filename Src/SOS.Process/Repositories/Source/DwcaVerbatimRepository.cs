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
    }
}