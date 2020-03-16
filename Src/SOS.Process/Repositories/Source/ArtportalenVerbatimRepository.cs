using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Process.Database.Interfaces;

namespace SOS.Process.Repositories.Source
{
    public class ArtportalenVerbatimRepository : VerbatimBaseRepository<ArtportalenVerbatimObservation, int>, Interfaces.IArtportalenVerbatimRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public ArtportalenVerbatimRepository(IVerbatimClient client,
            ILogger<ArtportalenVerbatimRepository> logger) : base(client, logger)
        {

        }
    }
}
