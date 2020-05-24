using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Database.Interfaces;

namespace SOS.Process.Repositories.Destination
{
    /// <summary>
    /// Repository for data providers.
    /// </summary>
    public class DataProviderRepository : ProcessBaseRepository<DataProvider, int>, Interfaces.IDataProviderRepository
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public DataProviderRepository(
            IProcessClient client,
            ILogger<DataProviderRepository> logger)
            : base(client, false, logger)
        {

        }

        public async Task<bool> UpdateProcessInfo(int dataProviderId, string collectionName, ProviderInfo providerInfo)
        {
            try
            {
                if (collectionName == "ProcessedObservation-0")
                {
                    var filter = Builders<DataProvider>.Filter.Eq(dataProvider => dataProvider.Id, dataProviderId);
                    var update = Builders<DataProvider>.Update.Set(dataProvider => dataProvider.ProcessInfoInstance0, providerInfo);
                    var updateResult = await MongoCollection.UpdateOneAsync(filter, update);
                    return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
                }
                else if (collectionName == "ProcessedObservation-1")
                {
                    var filter = Builders<DataProvider>.Filter.Eq(dataProvider => dataProvider.Id, dataProviderId);
                    var update = Builders<DataProvider>.Update.Set(dataProvider => dataProvider.ProcessInfoInstance1, providerInfo);
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