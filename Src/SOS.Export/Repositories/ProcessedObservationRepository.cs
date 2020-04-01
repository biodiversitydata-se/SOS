using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using SOS.Export.Managers.Interfaces;
using SOS.Export.MongoDb.Interfaces;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;

namespace SOS.Export.Repositories
{
    /// <summary>
    /// Species data service
    /// </summary>
    public class ProcessedObservationRepository : BaseRepository<ProcessedObservation, string>, IProcessedObservationRepository
    {
        private readonly IElasticClient _elasticClient;
        private ITaxonManager _taxonManager;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="elasticClient"></param>
        /// <param name="exportClient"></param>
        /// <param name="taxonManager"></param>
        /// <param name="logger"></param>
        public ProcessedObservationRepository(
            IElasticClient elasticClient,
            IExportClient exportClient,
            ITaxonManager taxonManager,
            ILogger<ProcessedObservationRepository> logger) : base(exportClient, true, logger)
        {
            _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
            _taxonManager = taxonManager ?? throw new ArgumentNullException(nameof(taxonManager));
        }

        private SearchFilter PrepareFilter(FilterBase filter)
        {
            var preparedFilter = filter.Clone();

            if (preparedFilter.IncludeUnderlyingTaxa && preparedFilter.TaxonIds != null && preparedFilter.TaxonIds.Any())
            {
                if (preparedFilter.TaxonIds.Contains(0)) // If Biota, then clear taxon filter
                {
                    preparedFilter.TaxonIds = new List<int>();
                }
                else
                {
                    preparedFilter.TaxonIds = _taxonManager.TaxonTree.GetUnderlyingTaxonIds(preparedFilter.TaxonIds, true);
                }
            }

            return preparedFilter;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ProcessedObservation>> GetChunkAsync(FilterBase filter, int skip, int take)
        {
            if (!filter?.IsFilterActive ?? true)
            {
                return null;
            }

            var searchResponse = await _elasticClient.SearchAsync<ProcessedObservation>(s => s
                .Index(CollectionName.ToLower())
                .From(skip)
                .Size(take)
                .Query(q => q
                    .Bool(b => b
                        .Filter(filter.ToQuery()))));

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            return searchResponse.Documents;
        }

        public async Task<IEnumerable<ProcessedProject>> GetProjectParameters(
            FilterBase filter, 
            int skip,
            int take)
        {
            // Not sure this query works
            var searchResponse = await _elasticClient.SearchAsync<ProcessedObservation>(s => s
                .Index(CollectionName.ToLower())
                .From(skip)
                .Size(take)
                .Query(q => q
                    .Bool(b => b
                        .Filter(filter.ToProjectParameteQuery()))));

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            return searchResponse.Documents.SelectMany(d => d.Projects);
        }
    }
}
