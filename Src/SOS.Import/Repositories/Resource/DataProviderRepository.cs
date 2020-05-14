using Microsoft.Extensions.Logging;
using SOS.Import.MongoDb.Interfaces;
using SOS.Import.Repositories.Resource.Interfaces;
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
    }
}
