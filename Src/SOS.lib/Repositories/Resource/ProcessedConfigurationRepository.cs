using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Lib.Repositories.Resource
{
    /// <summary>
    ///     Base class for cosmos db repositories
    /// </summary>
    public class ProcessedConfigurationRepository : RepositoryBase<ProcessedConfiguration, string>, IProcessedConfigurationRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public ProcessedConfigurationRepository(
            IProcessClient client,
            ILogger<ProcessedConfigurationRepository> logger
        ) : base(client, logger)
        {

        }

    }
}