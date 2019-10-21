using Microsoft.Extensions.Logging;
using SOS.Import.MongoDb.Interfaces;
using SOS.Lib.Models.Verbatim.SpeciesPortal;

namespace SOS.Import.Repositories.Destination.SpeciesPortal
{
    /// <summary>
    /// Species data service
    /// </summary>
    public class SightingVerbatimRepository : VerbatimRepository<APSightingVerbatim, int>, Interfaces.ISightingVerbatimRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public SightingVerbatimRepository(
            IImportClient importClient,
            ILogger<SightingVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}
