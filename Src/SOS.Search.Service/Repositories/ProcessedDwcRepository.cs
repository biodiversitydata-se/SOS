using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.DarwinCore;
using SOS.Lib.Models.Search;
using SOS.Search.Service.Repositories.Interfaces;

namespace SOS.Search.Service.Repositories
{
    /// <summary>
    /// Species data service
    /// </summary>
    public class ProcessedDarwinCoreRepository : BaseRepository<DarwinCore<DynamicProperties>, ObjectId>, IProcessedDarwinCoreRepository
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
            ILogger<ProcessedDarwinCoreRepository> logger) : base(mongoClient, mongoDbConfiguration, true, logger)
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<dynamic>> GetChunkAsync(AdvancedFilter filter, int skip, int take)
        {
            if (filter?.OutputFields?.Any() ?? false)
            {
                var res = await MongoCollection
                    .Find(filter.ToFilterDefinition())
                    .Project(filter.OutputFields.ToProjection())
                    .Skip(skip)
                    .Limit(take)
                    .ToListAsync();

                return res.ConvertAll(BsonTypeMapper.MapToDotNetValue);
            }
            else
            {
                var res = await MongoCollection
                    .Find(filter.ToFilterDefinition())
                    // .Sort(Builders<DarwinCore<DynamicProperties>>.Sort.Descending("id"))
                    .Skip(skip)
                    .Limit(take)
                    .ToListAsync();

                return res.ToDarwinCore();
            }
        }
    }
}
