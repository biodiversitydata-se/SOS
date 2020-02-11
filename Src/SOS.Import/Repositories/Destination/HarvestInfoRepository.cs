using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.MongoDb.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Repositories.Destination
{
    public class HarvestInfoRepository : VerbatimDbConfiguration<HarvestInfo, string>, Interfaces.IHarvestInfoRepository
    {
        /// <summary>
        /// Constructor
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
