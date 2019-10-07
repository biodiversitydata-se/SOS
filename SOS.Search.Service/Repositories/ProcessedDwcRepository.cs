using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Search.Service.Configuration;
using SOS.Search.Service.Models;
using SOS.Search.Service.Repositories.Interfaces;

namespace SOS.Search.Service.Repositories
{
    /// <summary>
    /// Species data service
    /// </summary>
    public class ProcessedDarwinCoreRepository : AggregateRepository<DarwinCore<DynamicProperties>, ObjectId>, IProcessedDarwinCoreRepository
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
        private FilterDefinition<DarwinCore<DynamicProperties>> CreateFilter(IEnumerable<int> taxonIds)
        {
            var ids = taxonIds?.Select(i => i.ToString()) ?? new string[0];
            var filter = Builders<DarwinCore<DynamicProperties>>.Filter.Where(m => ids.Contains(m.Taxon.TaxonID));

            return filter;
        }

        /// <summary>
        /// Build a projection string
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        private string CreateProjection(IEnumerable<string> fields)
        {
            var projection = $"{{ _id: 0, { fields?.Where(f => !string.IsNullOrEmpty(f)).Select((f, i) => $"'{f}': {i+1}").Join(",") } }}";
            return projection;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DarwinCore<DynamicProperties>>> GetChunkAsync(int taxonId, int skip, int take)
        {
            var filter = CreateFilter(new [] { taxonId });

            var res = await MongoCollection
                .Find(filter)
               // .Sort(Builders<DarwinCore<DynamicProperties>>.Sort.Descending("id"))
                .Skip(skip)
                .Limit(take)
                .ToListAsync();

            return res;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<dynamic>> GetChunkAsync(int taxonId, IEnumerable<string> fields, int skip, int take)
        {
            var filter = CreateFilter(new[] { taxonId });

            var res = await MongoCollection
                .Find(filter)
                .Project(CreateProjection(fields))
                .Skip(skip)
                .Limit(take)
                .ToListAsync();

            return res.ConvertAll(BsonTypeMapper.MapToDotNetValue);
        }
    }
}
