using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SOS.Import.Repositories.Destination.ClamPortal.Interfaces;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.ClamPortal;

namespace SOS.Import.Repositories.Destination.ClamPortal
{
    /// <summary>
    ///     Clam verbatim repository
    /// </summary>
    public class ClamObservationVerbatimRepository : VerbatimRepository<ClamObservationVerbatim, ObjectId>,
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