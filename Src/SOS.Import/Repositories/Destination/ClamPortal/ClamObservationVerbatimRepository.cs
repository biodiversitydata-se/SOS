using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SOS.Import.MongoDb.Interfaces;
using SOS.Lib.Models.Verbatim.ClamPortal;

namespace SOS.Import.Repositories.Destination.ClamPortal
{
    /// <summary>
    /// Clam verbatim repository
    /// </summary>
    public class ClamObservationVerbatimRepository : VerbatimDbConfiguration<ClamObservationVerbatim, ObjectId>, Interfaces.IClamObservationVerbatimRepository
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
