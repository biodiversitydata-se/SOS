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

        /// <summary>
        /// Create search filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private FilterDefinition<DarwinCore<DynamicProperties>> CreateFilter(AdvancedFilter filter)
        {
            if (!filter.IsFilterActive)
            {
                return FilterDefinition<DarwinCore<DynamicProperties>>.Empty;
            }

            var filters = new List<FilterDefinition<DarwinCore<DynamicProperties>>>();

            if (filter.TaxonIds?.Any() ?? false)
            {
                filters.Add(Builders<DarwinCore<DynamicProperties>>.Filter.In(m => m.Taxon.Id, filter.TaxonIds));
            }

            if (filter.StartDate.HasValue)
            {
            //    filters.Add(Builders<DarwinCore<DynamicProperties>>.Filter.Gte(m => m.Event.EventDate, filter.StartDate));
            }

            if (filter.EndDate.HasValue)
            {
           //     filters.Add(Builders<DarwinCore<DynamicProperties>>.Filter.Lte(m => m.Event.EventDate, filter.EndDate));
            }

            if (filter.Counties?.Any() ?? false)
            {
                filters.Add(Builders<DarwinCore<DynamicProperties>>.Filter.In(m => m.Location.County, filter.Counties));
            }

            if (filter.Municipalities?.Any() ?? false)
            {
                filters.Add(Builders<DarwinCore<DynamicProperties>>.Filter.In(m => m.Location.Municipality, filter.Municipalities));
            }

            if (filter.Provinces?.Any() ?? false)
            {
                filters.Add(Builders<DarwinCore<DynamicProperties>>.Filter.In(m => m.Location.StateProvince, filter.Provinces));
            }

            if (filter.Sex?.Any() ?? false)
            {
                filters.Add(Builders<DarwinCore<DynamicProperties>>.Filter.In(m => m.Occurrence.Sex, filter.Sex));
            }

            return Builders<DarwinCore<DynamicProperties>>.Filter.And(filters);
        }

        /// <summary>
        /// Build a projection string
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        private string CreateProjection(IEnumerable<string> fields)
        {
            var projection = $"{{ _id: 0, { string.Join(",", fields?.Where(f => !string.IsNullOrEmpty(f)).Select((f, i) => $"'{f}': {i+1}") ?? new string[0]) } }}";
            return projection;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<dynamic>> GetChunkAsync(AdvancedFilter filter, int skip, int take)
        {
            if (filter?.OutputFields?.Any() ?? false)
            {
                var res = await MongoCollection
                    .Find(CreateFilter(filter))
                    .Project(CreateProjection(filter.OutputFields))
                    .Skip(skip)
                    .Limit(take)
                    .ToListAsync();

                return res.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                
            }
            else
            {
                var res = await MongoCollection
                    .Find(CreateFilter(filter))
                    // .Sort(Builders<DarwinCore<DynamicProperties>>.Sort.Descending("id"))
                    .Skip(skip)
                    .Limit(take)
                    .ToListAsync();

                return res.ToDarwinCore();
            }
        }
    }
}
