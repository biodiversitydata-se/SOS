using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SOS.Import.DarwinCore;
using SOS.Import.MongoDb.Interfaces;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.Repositories.Destination.DarwinCoreArchive
{
    /// <summary>
    /// DwC-A event repository
    /// </summary>
    public class DarwinCoreArchiveEventRepository : VerbatimRepository<DwcEvent, ObjectId>, Interfaces.IDarwinCoreArchiveEventRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public DarwinCoreArchiveEventRepository(
            IImportClient importClient,
            ILogger<DarwinCoreArchiveEventRepository> logger) : base(importClient, logger)
        {
        }

        /// <summary>
        /// Gets collection name. Example: "DwcaEvent_007_ButterflyMonitoring".
        /// </summary>
        /// <param name="datasetInfo"></param>
        /// <returns></returns>
        private string GetCollectionName(DwcaDatasetInfo datasetInfo)
        {
            return $"DwcaEvent_{datasetInfo.DataProviderId:D3}_{datasetInfo.DataProviderIdentifier}";
        }

        public async Task<bool> DeleteCollectionAsync(DwcaDatasetInfo datasetInfo)
        {
            string collectionName = GetCollectionName(datasetInfo);
            return await base.DeleteCollectionAsync(collectionName);
        }

        public async Task<bool> AddCollectionAsync(DwcaDatasetInfo datasetInfo)
        {
            string collectionName = GetCollectionName(datasetInfo);
            return await base.AddCollectionAsync(collectionName);
        }

        public async Task<bool> AddManyAsync(IEnumerable<DwcEvent> items, DwcaDatasetInfo datasetInfo)
        {
            string collectionName = GetCollectionName(datasetInfo);
            var mongoCollection = base.GetMongoCollection(collectionName);
            return await base.AddManyAsync(items, mongoCollection);
        }
    }
}