using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using SOS.Import.MongoDb.Interfaces;
using SOS.Lib.Models.Shared;
using JsonSerializer = System.Text.Json.JsonSerializer;


namespace SOS.Import.Repositories.Destination.Artportalen
{
    /// <summary>
    /// Area repository
    /// </summary>
    public class AreaVerbatimRepository : VerbatimRepository<Area, int>, Interfaces.IAreaVerbatimRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public AreaVerbatimRepository(
            IImportClient importClient,
            ILogger<AreaVerbatimRepository> logger) : base(importClient, logger)
        {
        }

        /// <inheritdoc />
        public async Task CreateIndexAsync()
        {
            var indexModels = new List<CreateIndexModel<Area>>
            {
                new CreateIndexModel<Area>(Builders<Area>.IndexKeys.Ascending(a => a.AreaType)),
                new CreateIndexModel<Area>(Builders<Area>.IndexKeys.Geo2DSphere(a => a.Geometry))
            };
             
            await MongoCollection.Indexes.CreateManyAsync(indexModels);
        }
    }
}
