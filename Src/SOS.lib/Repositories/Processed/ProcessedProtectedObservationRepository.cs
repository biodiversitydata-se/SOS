using Microsoft.Extensions.Logging;
using Nest;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Base class for cosmos db repositories
    /// </summary>
    public class ProcessedProtectedObservationRepository : ProcessedObservationRepositoryBase,
        IProcessedProtectedObservationRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="elasticClient"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="logger"></param>
        public ProcessedProtectedObservationRepository(
            IProcessClient client,
            IElasticClient elasticClient,
            ElasticSearchConfiguration elasticConfiguration,
            ILogger<ProcessedPublicObservationRepository> logger
        ) : base(true, client, elasticClient, elasticConfiguration, logger)
        {
            
        }
    }
}