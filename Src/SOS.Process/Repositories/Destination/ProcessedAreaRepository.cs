using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Models.Shared;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Repositories.Destination
{
    /// <summary>
    /// Repository for retrieving processed areas.
    /// </summary>
    public class ProcessedAreaRepository : ProcessBaseRepository<Area, int>, IProcessedAreaRepository
    {
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

        /// <inheritdoc />
        public async Task<List<AreaBase>> GetAllAreaBaseAsync()
        {
            var res = await MongoCollection
                .Find(x => true)
                .Project(m => new AreaBase(m.AreaType)
                {
                    FeatureId = m.FeatureId,
                    Id = m.Id,
                    Name = m.Name,
                    ParentId = m.ParentId,
                })
                .ToListAsync();

            return res;
        }

        /// <inheritdoc />
        public async Task CreateIndexAsync()
        {
            var indexModels = new List<CreateIndexModel<Area>>()
            {
                new CreateIndexModel<Area>(
                    Builders<Area>.IndexKeys.Ascending(a => a.Name)),
                new CreateIndexModel<Area>(
                    Builders<Area>.IndexKeys.Ascending(a => a.AreaType)),
                new CreateIndexModel<Area>(
                    Builders<Area>.IndexKeys.Geo2DSphere(a => a.Geometry))
            };

            Logger.LogDebug("Creating Area indexes");
            await MongoCollection.Indexes.CreateManyAsync(indexModels);
        }
    }
}