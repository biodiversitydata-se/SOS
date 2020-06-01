using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Repositories.Destination
{
    /// <summary>
    ///     Repository for data providers.
    /// </summary>
    public class DataProviderRepository : ProcessBaseRepository<DataProvider, int>, IDataProviderRepository
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public DataProviderRepository(
            IProcessClient client,
            ILogger<DataProviderRepository> logger)
            : base(client, false, logger)
        {
        }

        public override async Task<List<DataProvider>> GetAllAsync()
        {
            var allDataProviders = await base.GetAllAsync();
            return allDataProviders.OrderBy(provider => provider.Id).ToList();
        }

        public async Task<bool> UpdateProcessInfo(int dataProviderId, string collectionName, ProviderInfo providerInfo)
        {
            try
            {
                if (collectionName == "ProcessedObservation-0")
                {
                    var filter = Builders<DataProvider>.Filter.Eq(dataProvider => dataProvider.Id, dataProviderId);
                    var update = Builders<DataProvider>.Update.Set(dataProvider => dataProvider.ProcessInfoInstance0,
                        providerInfo);
                    var updateResult = await MongoCollection.UpdateOneAsync(filter, update);
                    return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
                }

                if (collectionName == "ProcessedObservation-1")
                {
                    var filter = Builders<DataProvider>.Filter.Eq(dataProvider => dataProvider.Id, dataProviderId);
                    var update = Builders<DataProvider>.Update.Set(dataProvider => dataProvider.ProcessInfoInstance1,
                        providerInfo);
                    var updateResult = await MongoCollection.UpdateOneAsync(filter, update);
                    return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
                }

                return false;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }
        }
    }
}