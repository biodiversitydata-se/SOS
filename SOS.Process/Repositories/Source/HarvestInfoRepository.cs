using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Database.Interfaces;

namespace SOS.Process.Repositories.Source
{
    public class HarvestInfoRepository : VerbatimBaseRepository<HarvestInfo, string>, Interfaces.IHarvestInfoRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public HarvestInfoRepository(
            IVerbatimClient client,
            ILogger<HarvestInfoRepository> logger) : base(client, logger)
        {
            
        }

        /// <inheritdoc />
        public async Task<IEnumerable<HarvestInfo>> GetAllAsync()
        {
            try
            {
                var searchFilter = Builders<HarvestInfo>.Filter.Empty;
                return await MongoCollection.FindSync(Builders<HarvestInfo>.Filter.Empty).ToListAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return default;
            }
        }
    }
}
