using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Lib.Repositories.Verbatim
{
    /// <summary>
    ///    Repository for check list verbatim
    /// </summary>
    public class ArtportalenCheckListVerbatimRepository : VerbatimRepositoryBase<ArtportalenCheckListVerbatim, int>
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public ArtportalenCheckListVerbatimRepository(
            IVerbatimClient importClient,
            ILogger<ArtportalenCheckListVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}