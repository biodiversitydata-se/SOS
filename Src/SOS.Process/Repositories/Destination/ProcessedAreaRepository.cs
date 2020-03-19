using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Repositories.Destination
{
    /// <summary>
    /// Repository for retrieving processed areas.
    /// </summary>
    public class ProcessedAreaRepository : ProcessBaseRepository<Area, int>, IProcessedAreaRepository
    {
        private new IMongoCollection<Area> MongoCollection => Database.GetCollection<Area>(_collectionName);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public ProcessedAreaRepository(
            IProcessClient client, 
            ILogger<ProcessedAreaRepository> logger) 
            : base(client, false, logger)
        {

        }

        /// <summary>
        /// Gets processed taxa.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Area>> GetAreasAsync()
        {
            try
            {
                const int batchSize = 200000;
                var skip = 0;
                var areasBatch = (await GetChunkAsync(skip, batchSize)).ToArray();
                var areas = new List<Area>();

                while (areasBatch?.Any() ?? false)
                {
                    areas.AddRange(areasBatch);
                    skip += areasBatch.Count();
                    areasBatch = (await GetChunkAsync(skip, batchSize)).ToArray();
                }

                return areas;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to get chunk of areas");
                return null;
            }
        }


        private async Task<IEnumerable<Area>> GetChunkAsync(int skip, int take)
        {
            var res = await MongoCollection
                .Find(x => true)
                .Skip(skip)
                .Limit(take)
                .ToListAsync();

            return res;
        }
    }
}
