using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Processed.Observation;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Repositories.Destination
{
    /// <summary>
    /// Repository for retrieving processd taxa.
    /// </summary>
    public class ProcessedTaxonRepository : ProcessBaseRepository<ProcessedTaxon, int>, IProcessedTaxonRepository
    {
        /// <summary>
        /// Constructor.
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
