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
    /// DwC-A observation repository
    /// </summary>
    public class DarwinCoreArchiveVerbatimRepository : VerbatimDbConfiguration<DwcObservationVerbatim, ObjectId>, Interfaces.IDarwinCoreArchiveVerbatimRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public DarwinCoreArchiveVerbatimRepository(
            IImportClient importClient,
            ILogger<DarwinCoreArchiveVerbatimRepository> logger) : base(importClient, logger)
        {
            
        }

        /// <summary>
        /// Gets collection name. Example: "DwcaOccurrence_007_ButterflyMonitoring".
        /// </summary>
        /// <param name="datasetInfo"></param>
        /// <returns></returns>
        private string GetCollectionName(DwcaDatasetInfo datasetInfo)
        {
            return $"DwcaOccurrence_{datasetInfo.DataProviderId:D3}_{datasetInfo.DataProviderIdentifier}";
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

        public async Task<bool> AddManyAsync(IEnumerable<DwcObservationVerbatim> items, DwcaDatasetInfo datasetInfo)
        {
            string collectionName = GetCollectionName(datasetInfo);
            var mongoCollection = base.GetMongoCollection(collectionName);
            return await base.AddManyAsync(items, mongoCollection);
        }
    }
}