using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Repositories.Source
{
    public class ArtportalenVerbatimRepository : VerbatimBaseRepository<ArtportalenVerbatimObservation, int>,
        IArtportalenVerbatimRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public ArtportalenVerbatimRepository(IVerbatimClient client,
            ILogger<ArtportalenVerbatimRepository> logger) : base(client, logger)
        {
        }
    }
}