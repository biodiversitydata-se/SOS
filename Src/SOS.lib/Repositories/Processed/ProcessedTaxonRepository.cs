using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Repository for retrieving processd taxa.
    /// </summary>
    public class ProcessedTaxonRepository : MongoDbProcessedRepositoryBase<ProcessedTaxon, int>, IProcessedTaxonRepository
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public ProcessedTaxonRepository(
            IProcessClient client,
            ILogger<ProcessedTaxonRepository> logger)
            : base(client, false, logger)
        {
        }
    }
}