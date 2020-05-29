using Microsoft.Extensions.Logging;
using SOS.Import.MongoDb.Interfaces;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Repositories.Destination
{
    public class HarvestInfoRepository : VerbatimRepository<HarvestInfo, string>, IHarvestInfoRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public HarvestInfoRepository(
            IImportClient importClient,
            ILogger<HarvestInfoRepository> logger) : base(importClient, logger)
        {
        }
    }
}