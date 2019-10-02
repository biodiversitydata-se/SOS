using Microsoft.Extensions.Logging;
using SOS.Process.Database.Interfaces;
using SOS.Process.Models.Verbatim.SpeciesPortal;

namespace SOS.Process.Repositories.Source
{
    public class SpeciesPortalVerbatimRepository : VerbatimBaseRepository<APSightingVerbatim, int>, Interfaces.ISpeciesPortalVerbatimRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public SpeciesPortalVerbatimRepository(IVerbatimClient client,
            ILogger<SpeciesPortalVerbatimRepository> logger) : base(client, logger)
        {

        }
    }
}
