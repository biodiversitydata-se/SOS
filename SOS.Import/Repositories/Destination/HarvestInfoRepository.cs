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
        public async Task<bool> UpdateHarvestInfoAsync(string id, DataProvider provider, DateTime start, DateTime end, int count)
        {
            return await AddOrUpdateAsync(new HarvestInfo(id, provider)
            {
                End = end,
                Count = count,
                Start = start
            });
        }
    }
}
