using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.MongoDb.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Repositories.Destination
{
    public class HarvestInfoRepository : VerbatimRepository<HarvestInfo, string>, Interfaces.IHarvestInfoRepository
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

        /// <inheritdoc />
        public async Task<bool> UpdateHarvestInfoAsync(string id, DataProviderId provider, int sightingCount)
        {
            return await AddOrUpdateAsync(new HarvestInfo
            {
                Id = id,
                DataProvider = DataProviderId.ClamAndTreePortal,
                IssueDate = DateTime.Now,
                SightingCount = sightingCount
            });
        }
    }
}
