using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Lib.Repositories.Verbatim
{
    /// <summary>
    ///    Repository for checklist verbatim
    /// </summary>
    public class ArtportalenChecklistVerbatimRepository : VerbatimRepositoryBase<ArtportalenChecklistVerbatim, int>
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public ArtportalenChecklistVerbatimRepository(
            IVerbatimClient importClient,
            ILogger<ArtportalenChecklistVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}