using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SOS.Import.MongoDb.Interfaces;
using SOS.Lib.Models.Verbatim.ClamTreePortal;

namespace SOS.Import.Repositories.Destination.ClamTreePortal
{
    /// <summary>
    /// Clam verbatim repository
    /// </summary>
    public class TreeObservationVerbatimRepository : VerbatimRepository<TreeObservationVerbatim, ObjectId>, Interfaces.ITreeObservationVerbatimRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public TreeObservationVerbatimRepository(
            IImportClient importClient,
            ILogger<TreeObservationVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}