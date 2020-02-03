using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Search;
using SOS.Search.Service.Enum;
using SOS.Search.Service.Factories.Interfaces;
using SOS.Search.Service.Repositories.Interfaces;

namespace SOS.Search.Service.Repositories
{
    /// <summary>
    /// Species data service
    /// </summary>
    public class ProcessedSightingRepository : BaseRepository<ProcessedSighting, ObjectId>, IProcessedSightingRepository
    {
        private const int BiotaTaxonId = 0;
        private readonly ITaxonFactory _taxonFactory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mongoClient"></param>
        /// <param name="mongoDbConfiguration"></param>
        /// <param name="taxonFactory"></param>
        /// <param name="logger"></param>
        public ProcessedSightingRepository(
            IMongoClient mongoClient,
            IOptions<MongoDbConfiguration> mongoDbConfiguration,
            ITaxonFactory taxonFactory,
            ILogger<ProcessedSightingRepository> logger) : base(mongoClient, mongoDbConfiguration, true, logger)
        {
            _taxonFactory = taxonFactory ?? throw new ArgumentNullException(nameof(taxonFactory));
        }

        private AdvancedFilter PrepareFilter(AdvancedFilter filter)
        {
            var preparedFilter = filter.Clone();

            if (preparedFilter.SearchUnderlyingTaxa && preparedFilter.TaxonIds != null && preparedFilter.TaxonIds.Any())
            {
                if (preparedFilter.TaxonIds.Contains(BiotaTaxonId)) // If Biota, then clear taxon filter
                {
                    preparedFilter.TaxonIds = new List<int>();
                }
                else
                {
                    preparedFilter.TaxonIds = _taxonFactory.TaxonTree.GetUnderlyingTaxonIds(preparedFilter.TaxonIds, true);
                }
            }

            return preparedFilter;
        }

        private SortDefinition<ProcessedSighting> PrepareSorting(string sortBy, SearchSortOrder sortOrder)
        {
            return string.IsNullOrEmpty(sortBy) ?
                null : sortOrder.Equals(SearchSortOrder.Desc) ?
                    Builders<ProcessedSighting>.Sort.Descending(sortBy) : Builders<ProcessedSighting>.Sort.Ascending(sortBy);
        }

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetChunkAsync(AdvancedFilter filter, int skip, int take, string sortBy, SearchSortOrder sortOrder)
        {
            var sorting = PrepareSorting(sortBy, sortOrder);
            filter = PrepareFilter(filter);
            if (filter?.OutputFields?.Any() ?? false)
            {
                var query = MongoCollection
                    .Find(filter.ToFilterDefinition())
                    .Sort(sorting)
                    .Project(filter.OutputFields.ToProjection());

                var count = await query.CountDocumentsAsync();
                
                query
                    .Sort(sorting)
                    .Skip(skip)
                    .Limit(take);

                return new PagedResult<dynamic>()
                {
                    Records = (await query.ToListAsync()).ConvertAll(BsonTypeMapper.MapToDotNetValue),
                    TotalCount = count
                };
            }
            else
            {
                var query = MongoCollection
                    .Find(filter.ToFilterDefinition());

                var count = await query.CountDocumentsAsync();
                query
                    .Sort(sorting)
                    .Skip(skip)
                    .Limit(take);

                return new PagedResult<dynamic>()
                {
                    Records = await query.ToListAsync(),
                    TotalCount = count
                };
            }
        }
    }
}