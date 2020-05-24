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
using SOS.Lib.Models.Interfaces;
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
        /// <param name="idIdentifierTuple"></param>
        /// <returns></returns>
        private string GetCollectionName(IIdIdentifierTuple idIdentifierTuple)
        {
            return $"DwcaOccurrence_{idIdentifierTuple.Id:D3}_{idIdentifierTuple.Identifier}";
        }

        /// <summary>
        /// Gets temp collection name. Example: "DwcaOccurrence_007_ButterflyMonitoring_temp".
        /// </summary>
        /// <returns></returns>
        private string GetTempHarvestCollectionName(IIdIdentifierTuple idIdentifierTuple)
        {
            return $"{GetCollectionName(idIdentifierTuple)}_temp";
        }

        public async Task<bool> DeleteCollectionAsync(IIdIdentifierTuple idIdentifierTuple)
        {
            string collectionName = GetCollectionName(idIdentifierTuple);
            return await base.DeleteCollectionAsync(collectionName);
        }


        public async Task<bool> AddCollectionAsync(IIdIdentifierTuple idIdentifierTuple)
        {
            string collectionName = GetCollectionName(idIdentifierTuple);
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

        public async Task<bool> AddManyToTempHarvestAsync(IEnumerable<DwcObservationVerbatim> items, IIdIdentifierTuple idIdentifierTuple)
        {
            string collectionName = GetTempHarvestCollectionName(idIdentifierTuple);
            return await AddManyAsync(items, collectionName);
        }

        public async Task<bool> AddManyAsync(IEnumerable<DwcObservationVerbatim> items, IIdIdentifierTuple idIdentifierTuple)
        {
            string collectionName = GetCollectionName(idIdentifierTuple);
            return await AddManyAsync(items, collectionName);
        }

        public async Task<bool> AddManyAsync(IEnumerable<DwcObservationVerbatim> items, string collectionName)
        {
            var mongoCollection = base.GetMongoCollection(collectionName);
            return await base.AddManyAsync(items, mongoCollection);
        }

        public async Task<bool> ClearTempHarvestCollection(IIdIdentifierTuple idIdentifierTuple)
        {
            string collectionName = GetTempHarvestCollectionName(idIdentifierTuple);
            await base.DeleteCollectionAsync(collectionName);
            return await base.AddCollectionAsync(collectionName);
        }

        public async Task<bool> RenameTempHarvestCollection(IIdIdentifierTuple idIdentifierTuple)
        {
            var tempHarvestCollectionName = GetTempHarvestCollectionName(idIdentifierTuple);
            var collectionName = GetCollectionName(idIdentifierTuple);
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

        public async Task DeleteTempHarvestCollectionIfExists(IIdIdentifierTuple idIdentifierTuple)
        {
            var tempHarvestCollectionName = GetTempHarvestCollectionName(idIdentifierTuple);
            var collectionExists = await CollectionExistsAsync(tempHarvestCollectionName);
            if (collectionExists)
            {
                await base.Database.DropCollectionAsync(tempHarvestCollectionName);
            }
        }

        private async Task<bool> CollectionExistsAsync(string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var collections = await base.Database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
            return await collections.AnyAsync();
        }
    }
}