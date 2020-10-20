using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Lib.Repositories.Verbatim
{
    public class HarvestInfoRepository : VerbatimRepositoryBase<HarvestInfo, string>, IHarvestInfoRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public HarvestInfoRepository(
            IVerbatimClient importClient,
            ILogger<HarvestInfoRepository> logger) : base(importClient, logger)
        {
        }
    }
}