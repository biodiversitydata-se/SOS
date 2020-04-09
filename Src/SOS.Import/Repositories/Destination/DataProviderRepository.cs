using Microsoft.Extensions.Logging;
using SOS.Import.MongoDb.Interfaces;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Repositories.Destination
{
    public class DataProviderRepository : VerbatimRepository<DataProvider, int>, Interfaces.IDataProviderRepostitory
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public DataProviderRepository(
            IImportClient importClient,
            ILogger<DataProviderRepository> logger) : base(importClient, logger)
        {
            
        }
    }
}
