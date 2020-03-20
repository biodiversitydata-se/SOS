using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SOS.Import.MongoDb.Interfaces;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.Repositories.Destination.DarwinCoreArchive
{
    /// <summary>
    /// Clam verbatim repository
    /// </summary>
    public class DarwinCoreArchiveVerbatimRepository : VerbatimDbConfiguration<DarwinCoreObservationVerbatim, ObjectId>, Interfaces.IDarwinCoreArchiveVerbatimRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public DarwinCoreArchiveVerbatimRepository(
            IImportClient importClient,
            ILogger<DarwinCoreArchiveVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}
