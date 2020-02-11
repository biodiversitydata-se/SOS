using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Export.Factories.Interfaces;
using SOS.Export.MongoDb.Interfaces;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Search;

namespace SOS.Export.Repositories
{
    /// <summary>
    /// Species data service
    /// </summary>
    public class ProcessedSightingRepository : BaseRepository<ProcessedSighting, ObjectId>, IProcessedSightingRepository
    {
        private ITaxonFactory _taxonFactory;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exportClient"></param>
        /// <param name="logger"></param>
        public ProcessedSightingRepository(
            IExportClient exportClient,
            ITaxonFactory taxonFactory,
            ILogger<ProcessedSightingRepository> logger) : base(exportClient, true, logger)
        {
            _taxonFactory = taxonFactory ?? throw new ArgumentNullException(nameof(taxonFactory));
        }

        private AdvancedFilter PrepareFilter(AdvancedFilter filter)
        {
            var preparedFilter = filter.Clone();

            if (preparedFilter.SearchUnderlyingTaxa && preparedFilter.TaxonIds != null && preparedFilter.TaxonIds.Any())
            {
                if (preparedFilter.TaxonIds.Contains(0)) // If Biota, then clear taxon filter
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

        /// <inheritdoc />
        public async Task<IEnumerable<ProcessedSighting>> GetChunkAsync(AdvancedFilter filter, int skip, int take)
        {
            filter = PrepareFilter(filter);

            var query = MongoCollection
                .Find(filter.ToFilterDefinition())
                .Limit(take)
                .Skip(skip);

            return await query.ToListAsync();

        }

        public async Task<IEnumerable<ProcessedProject>> GetProjectParameters(
            AdvancedFilter filter, 
            int skip,
            int take)
        {
            List<IEnumerable<ProcessedProject>> res = await MongoCollection
                .Find(filter.ToProjectParameteFilterDefinition())
                .Skip(skip)
                .Limit(take)
                .Project(x => x.Projects)
                .ToListAsync();

            var projectParameters = res.SelectMany(pp => pp);
            return projectParameters;
        }
    }
}
