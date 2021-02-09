using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Lib.Repositories.Verbatim
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public class ArtportalenVerbatimRepository : VerbatimRepositoryBase<ArtportalenObservationVerbatim, int>,
        IArtportalenVerbatimRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public ArtportalenVerbatimRepository(
            IVerbatimClient importClient,
            ILogger<ArtportalenVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}