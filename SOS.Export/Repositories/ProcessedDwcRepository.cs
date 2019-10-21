using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Export.MongoDb.Interfaces;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Models.DarwinCore;

namespace SOS.Export.Repositories
{
    /// <summary>
    /// Species data service
    /// </summary>
    public class ProcessedDarwinCoreRepository : AggregateRepository<DarwinCore<DynamicProperties>, ObjectId>, IProcessedDarwinCoreRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exportClient"></param>
        /// <param name="logger"></param>
        public ProcessedDarwinCoreRepository(
            IExportClient exportClient,
            ILogger<ProcessedDarwinCoreRepository> logger) : base(exportClient, logger)
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DarwinCore<DynamicProperties>>> GetChunkAsync(int skip, int take)
        {
            var res = await MongoCollection
                .Find(_ => true)
                .Skip(skip)
                .Limit(take)
                .ToListAsync();

            return res;
        }
    }
}
