using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.ClamPortal;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Lib.Repositories.Verbatim
{
    /// <summary>
    ///     Clam verbatim repository
    /// </summary>
    public class ClamObservationVerbatimRepository : VerbatimRepositoryBase<ClamObservationVerbatim, int>,
        IClamObservationVerbatimRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public ClamObservationVerbatimRepository(
            IVerbatimClient importClient,
            ILogger<ClamObservationVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}