using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SOS.Import.MongoDb.Interfaces;
using SOS.Lib.Models.Verbatim.ClamTreePortal;

namespace SOS.Import.Repositories.Destination.ClamTreePortal
{
    /// <summary>
    /// Clam verbatim repository
    /// </summary>
    public class ClamObservationVerbatimRepository : VerbatimRepository<ClamObservationVerbatim, ObjectId>, Interfaces.IClamObservationVerbatimRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public ClamObservationVerbatimRepository(
            IImportClient importClient,
            ILogger<ClamObservationVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}
