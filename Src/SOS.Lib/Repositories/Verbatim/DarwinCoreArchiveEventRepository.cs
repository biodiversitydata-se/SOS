﻿using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Verbatim
{
    /// <summary>
    ///     DwC-A event repository
    /// </summary>
    public class DarwinCoreArchiveEventRepository : RepositoryBase<DwcEvent, ObjectId>,
        IDarwinCoreArchiveEventRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public DarwinCoreArchiveEventRepository(
            IVerbatimClient importClient,
            ILogger<DarwinCoreArchiveEventRepository> logger) : base(importClient, logger)
        {
        }


        public async Task<bool> DeleteCollectionAsync(IIdIdentifierTuple idIdentifierTuple)
        {
            var collectionName = GetCollectionName(idIdentifierTuple);
            return await base.DeleteCollectionAsync(collectionName);
        }

        public async Task<bool> AddCollectionAsync(IIdIdentifierTuple idIdentifierTuple)
        {
            var collectionName = GetCollectionName(idIdentifierTuple);
            return await base.AddCollectionAsync(collectionName);
        }

        public async Task<bool> AddManyAsync(IEnumerable<DwcEvent> items, IIdIdentifierTuple idIdentifierTuple)
        {
            var collectionName = GetCollectionName(idIdentifierTuple);
            return await AddManyAsync(items, collectionName);
        }

        public async Task<bool> AddManyAsync(IEnumerable<DwcEvent> items, string collectionName)
        {
            var mongoCollection = GetMongoCollection(collectionName);
            return await base.AddManyAsync(items, mongoCollection);
        }

        public async Task<bool> AddManyToTempHarvestAsync(IEnumerable<DwcEvent> items,
            IIdIdentifierTuple idIdentifierTuple)
        {
            var collectionName = GetTempHarvestCollectionName(idIdentifierTuple);
            return await AddManyAsync(items, collectionName);
        }

        public async Task<bool> ClearTempHarvestCollection(IIdIdentifierTuple idIdentifierTuple)
        {
            var collectionName = GetTempHarvestCollectionName(idIdentifierTuple);
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
                await Database.DropCollectionAsync(collectionName);
            }

            await Database.RenameCollectionAsync(tempHarvestCollectionName, collectionName);
            return true;
        }

        /// <summary>
        ///     Gets collection name. Example: "DwcaOccurrence_007_ButterflyMonitoring".
        /// </summary>
        /// <param name="idIdentifierTuple"></param>
        /// <returns></returns>
        private string GetCollectionName(IIdIdentifierTuple idIdentifierTuple)
        {
            return $"DwcaEvent_{idIdentifierTuple.Id:D3}_{idIdentifierTuple.Identifier}";
        }

        /// <summary>
        ///     Gets temp collection name. Example: "DwcaOccurrence_007_ButterflyMonitoring_temp".
        /// </summary>
        /// <returns></returns>
        private string GetTempHarvestCollectionName(IIdIdentifierTuple idIdentifierTuple)
        {
            return $"{GetCollectionName(idIdentifierTuple)}_temp";
        }

        private async Task<bool> CollectionExistsAsync(string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var collections = await Database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
            return await collections.AnyAsync();
        }
    }
}