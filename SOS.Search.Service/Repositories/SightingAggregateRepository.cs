using System.Linq;
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
    public class SightingAggregateRepository : AggregateRepository<SightingAggregate, int>, ISightingAggregateRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settingsService"></param>
        /// <param name="mongoClient"></param>
        /// <param name="mongoDbConfiguration"></param>
        /// <param name="logger"></param>
        public SightingAggregateRepository(
            IMongoClient mongoClient,
            IOptions<MongoDbConfiguration> mongoDbConfiguration, 
            ILogger<SightingAggregateRepository> logger) : base(mongoClient, mongoDbConfiguration, logger)
        {
        }

        private FilterDefinition<SightingAggregate> CreateFilter(int[] taxonIds)
        {
            var filter = Builders<SightingAggregate>.Filter.Where(m => taxonIds.Contains(m.TaxonId));

           
            return filter;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SightingAggregate>> GetChunkAsync(int taxonId, int skip, int take)
        {
            var filter = CreateFilter(new [] { taxonId });

            var res = await MongoCollection
                .Find(filter)
                .Sort(Builders<SightingAggregate>.Sort.Descending("id"))
                .Skip(skip)
                .Limit(take)
                .ToListAsync();

            return res;
        }
    }
}
