using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Repository for data providers.
    /// </summary>
    public class DataProviderRepository : MongoDbProcessedRepositoryBase<DataProvider, int>, IDataProviderRepository
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
                var filter = Builders<DataProvider>.Filter.Eq(dataProvider => dataProvider.Id, dataProviderId);
                var update = Builders<DataProvider>.Update.Set(dataProvider => 
                        collectionName == "ProcessedObservation-0" ? dataProvider.ProcessInfoInstance0 : dataProvider.ProcessInfoInstance1,
                    providerInfo);
                var updateResult = await MongoCollection.UpdateOneAsync(filter, update);
                return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }
        }
    }
}