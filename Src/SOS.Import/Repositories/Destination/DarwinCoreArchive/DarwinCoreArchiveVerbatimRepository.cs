using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Import.DarwinCore;
using SOS.Import.MongoDb.Interfaces;
using SOS.Lib.Models.Statistics;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.Repositories.Destination.DarwinCoreArchive
{
    /// <summary>
    /// DwC-A observation repository
    /// </summary>
    public class DarwinCoreArchiveVerbatimRepository : VerbatimRepository<DwcObservationVerbatim, ObjectId>, Interfaces.IDarwinCoreArchiveVerbatimRepository
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


        public List<DistinictValueCount<string>> GetDistinctValuesCount(
            string collectionName,
            Expression<Func<DwcObservationVerbatim, DistinctValueObject<string>>> expression,
            int limit)
        {            
            List<DistinictValueCount<string>> result = GetMongoCollection(collectionName).Aggregate(new AggregateOptions { AllowDiskUse = true })
                .Project(expression)
                .Group(o => o.Value,
                    grouping => new DistinictValueCount<string> { Value = grouping.Key, Count = grouping.Count() })
                .SortByDescending(o => o.Count)
                .Limit(limit)
                .ToList();

            return result;
        }

        public async Task<bool> AddManyAsync(IEnumerable<DwcObservationVerbatim> items, DwcaDatasetInfo datasetInfo)
        {
            string collectionName = GetCollectionName(datasetInfo);
            return await AddManyAsync(items, collectionName);
        }

        public async Task<bool> AddManyAsync(IEnumerable<DwcObservationVerbatim> items, string collectionName)
        {
            var mongoCollection = base.GetMongoCollection(collectionName);
            return await base.AddManyAsync(items, mongoCollection);
        }
    }
}