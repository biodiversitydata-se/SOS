using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SOS.Import.MongoDb.Interfaces;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.Repositories.Destination.DarwinCoreArchive
{
    /// <summary>
    /// DwC-A event repository
    /// </summary>
    public class DarwinCoreArchiveEventRepository : VerbatimDbConfiguration<DwcEvent, ObjectId>, Interfaces.IDarwinCoreArchiveEventRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public DarwinCoreArchiveEventRepository(
            IImportClient importClient,
            ILogger<DarwinCoreArchiveEventRepository> logger) : base(importClient, logger)
        {
        }
    }
}
