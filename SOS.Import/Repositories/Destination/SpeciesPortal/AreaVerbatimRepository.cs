using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Import.Configuration;
using SOS.Import.Models.Shared;

namespace SOS.Import.Repositories.Destination.SpeciesPortal
{
    /// <summary>
    /// Area repository
    /// </summary>
    public class AreaVerbatimRepository : VerbatimRepository<Area, int>, Interfaces.IAreaVerbatimRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mongoClient"></param>
        /// <param name="mongoDbConfiguration"></param>
        /// <param name="logger"></param>
        public AreaVerbatimRepository(
            IMongoClient mongoClient,
            MongoDbConfiguration mongoDbConfiguration, 
            ILogger<AreaVerbatimRepository> logger) : base(mongoClient, mongoDbConfiguration, logger)
        {
        }

        /// <inheritdoc />
        public async Task CreateIndexAsync()
        {
            var indexModels = new List<CreateIndexModel<Area>>
            {
                new CreateIndexModel<Area>(Builders<Area>.IndexKeys.Ascending(a => a.AreaType)),
                new CreateIndexModel<Area>(Builders<Area>.IndexKeys.Geo2DSphere("Geometry.Coordinates"))
            };

            await MongoCollection.Indexes.CreateManyAsync(indexModels);
        }
    }
}
