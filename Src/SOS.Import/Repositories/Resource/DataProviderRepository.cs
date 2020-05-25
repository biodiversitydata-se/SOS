using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Import.MongoDb.Interfaces;
using SOS.Import.Repositories.Resource.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Repositories.Resource
{
    public class DataProviderRepository : ResourceRepositoryBase<DataProvider, int>, IDataProviderRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resourceDbClient"></param>
        /// <param name="logger"></param>
        public DataProviderRepository(
            IResourceDbClient resourceDbClient,
            ILogger<DataProviderRepository> logger) : base(resourceDbClient, false, logger)
        {
            
        }

        public async Task<bool> UpdateHarvestInfo(int dataProviderId, HarvestInfo harvestInfo)
        {
            try
            {
                var filter = Builders<DataProvider>.Filter.Eq(dataProvider => dataProvider.Id, dataProviderId);
                var update = Builders<DataProvider>.Update.Set(dataProvider => dataProvider.HarvestInfo, harvestInfo);
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
