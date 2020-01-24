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

        /// <inheritdoc />
        public async Task<IEnumerable<dynamic>> GetChunkAsync(AdvancedFilter filter, int skip, int take)
        {
            SortDefinition<ProcessedSighting> sorting = string.IsNullOrEmpty(filter.SortBy) ? 
                null : filter.SortDescending ? 
                    Builders<ProcessedSighting>.Sort.Descending(filter.SortBy) : Builders<ProcessedSighting>.Sort.Ascending(x => x.Event.StartDate);

            if (filter?.OutputFields?.Any() ?? false)
            {
                filter = PrepareFilter(filter);
                var res = await MongoCollection
                    .Find(filter.ToFilterDefinition())
                    .Sort(sorting)
                    .Project(filter.OutputFields.ToProjection())
                    .Skip(skip)
                    .Limit(take)
                    .ToListAsync();

                return res.ConvertAll(BsonTypeMapper.MapToDotNetValue);
            }
            else
            {
                filter = PrepareFilter(filter);
                var res = await MongoCollection
                    .Find(filter.ToFilterDefinition())
                    .Sort(sorting)
                    .Skip(skip)
                    .Limit(take)
                    .ToListAsync();

                return res.ToDarwinCore();
            }
        }

        private AdvancedFilter PrepareFilter(AdvancedFilter filter)
        {
            AdvancedFilter preparedFilter = filter.Clone();

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
    }
}