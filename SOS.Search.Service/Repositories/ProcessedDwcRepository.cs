using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SOS.Search.Service.Configuration;
using SOS.Search.Service.Models;
using SOS.Search.Service.Repositories.Interfaces;

namespace SOS.Search.Service.Repositories
{
    /// <summary>
    /// Species data service
    /// </summary>
    public class ProcessedDarwinCoreRepository : AggregateRepository<DarwinCore<DynamicProperties>, string>, IProcessedDarwinCoreRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mongoClient"></param>
        /// <param name="mongoDbConfiguration"></param>
        /// <param name="logger"></param>
        public ProcessedDarwinCoreRepository(
            IMongoClient mongoClient,
            IOptions<MongoDbConfiguration> mongoDbConfiguration, 
            ILogger<ProcessedDarwinCoreRepository> logger) : base(mongoClient, mongoDbConfiguration, logger)
        {
        }

        /// <summary>
        /// Create search filter
        /// </summary>
        /// <param name="taxonIds"></param>
        /// <returns></returns>
        private FilterDefinition<DarwinCore<DynamicProperties>> CreateFilter(int[] taxonIds)
        {   // NOT IMPLEMENTED, place holder code
            var filter = Builders<DarwinCore<DynamicProperties>>.Filter.Where(m => taxonIds.IsReadOnly);

           
            return filter;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DarwinCore<DynamicProperties>>> GetChunkAsync(int taxonId, int skip, int take)
        {
            var filter = CreateFilter(new [] { taxonId });

            var res = await MongoCollection
                .Find(filter)
                .Sort(Builders<DarwinCore<DynamicProperties>>.Sort.Descending("id"))
                .Skip(skip)
                .Limit(take)
                .ToListAsync();

            return res;
        }
    }
}
