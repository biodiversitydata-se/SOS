using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Base class for cosmos db repositories
    /// </summary>
    public class ProcessInfoRepository : MongoDbProcessedRepositoryBase<ProcessInfo, string>, IProcessInfoRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public ProcessInfoRepository(
            IProcessClient client,
            ILogger<ProcessInfoRepository> logger
        ) : base(client, false, logger)
        {
        }
    }
}