using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Import.MongoDb.Interfaces;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Repositories.Destination.Artportalen
{
    /// <summary>
    /// Area repository
    /// </summary>
    public class AreaVerbatimRepository : VerbatimDbConfiguration<Area, int>, Interfaces.IAreaVerbatimRepository
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
