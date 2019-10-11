using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SOS.Import.Configuration;
using SOS.Import.Models.Aggregates.Artportalen;

namespace SOS.Import.Repositories.Destination.SpeciesPortal
{
    /// <summary>
    /// Species data service
    /// </summary>
    public class SightingVerbatimRepository : VerbatimRepository<APSightingVerbatim, int>, Interfaces.ISightingVerbatimRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mongoClient"></param>
        /// <param name="mongoDbConfiguration"></param>
        /// <param name="logger"></param>
        public SightingVerbatimRepository(
            IMongoClient mongoClient,
            IOptions<MongoDbConfiguration> mongoDbConfiguration, 
            ILogger<SightingVerbatimRepository> logger) : base(mongoClient, mongoDbConfiguration, logger)
        {
        }
    }
}
