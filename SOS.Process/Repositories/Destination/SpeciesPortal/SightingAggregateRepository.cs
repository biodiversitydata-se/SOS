using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SOS.Process.Configuration;
using SOS.Process.Models.Aggregates;

namespace SOS.Process.Repositories.Destination.SpeciesPortal
{
    /// <summary>
    /// Species data service
    /// </summary>
    public class SightingAggregateRepository : AggregateRepository<APSightingVerbatim, int>, Interfaces.ISightingAggregateRepository
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
