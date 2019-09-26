using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SOS.Batch.Import.AP.Configuration;
using SOS.Batch.Import.AP.Models.Aggregates;
using SOS.Batch.Import.AP.Repositories.Destination.Interfaces;

namespace DataPopulateService.Repositories.DocumentDb
{
    /// <summary>
    /// Species data service
    /// </summary>
    public class SightingAggregateRepository : AggregateRepository<APSightingVerbatim, int>, ISightingAggregateRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mongoClient"></param>
        /// <param name="mongoDbConfiguration"></param>
        /// <param name="logger"></param>
        public SightingAggregateRepository(
            IMongoClient mongoClient,
            IOptions<MongoDbConfiguration> mongoDbConfiguration, 
            ILogger<SightingAggregateRepository> logger) : base(mongoClient, mongoDbConfiguration, logger)
        {
        }
    }
}
