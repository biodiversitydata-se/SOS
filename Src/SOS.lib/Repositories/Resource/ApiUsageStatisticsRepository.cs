using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Repositories.Resource
{
    /// <summary>
    ///     Report repository
    /// </summary>
    public class ApiUsageStatisticsRepository : RepositoryBase<ApiUsageStatistics, ObjectId>, Interfaces.IApiUsageStatisticsRepository
    {

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processClient"></param>
        /// <param name="logger"></param>
        public ApiUsageStatisticsRepository(
            IProcessClient processClient,
            ILogger<ApiUsageStatisticsRepository> logger) : base(processClient, logger)
        {

        }

        public async Task<DateTime?> GetLatestHarvestDate()
        {
            var collectionExists = await CheckIfCollectionExistsAsync();
            if (!collectionExists)
            {
                return null;
            }

            var list = MongoCollection
                .Find(Builders<ApiUsageStatistics>.Filter.Empty)
                .SortByDescending(e => e.Date)
                .Limit(1)
                .ToList();

            if (list.Count == 1)
            {
                return list.First().Date;
            }

            return null;
        }

        public async Task VerifyCollection()
        {
            var collectionExists = await CheckIfCollectionExistsAsync();
            if (!collectionExists)
            {
                await Database.CreateCollectionAsync(CollectionName);
                await CreateIndexAsync();
            }
        }

        public async Task CreateIndexAsync()
        {
            var indexModels = new List<CreateIndexModel<ApiUsageStatistics>>
            {
                new CreateIndexModel<ApiUsageStatistics>(
                    Builders<ApiUsageStatistics>.IndexKeys.Ascending(io => io.Date))
            };

            Logger.LogDebug("Creating ApiUsageStatistics indexes");
            await MongoCollection.Indexes.CreateManyAsync(indexModels);
        }
    }
}