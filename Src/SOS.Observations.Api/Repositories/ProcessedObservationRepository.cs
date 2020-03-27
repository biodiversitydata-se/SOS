using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Nest;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Database.Interfaces;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.Repositories
{
    /// <summary>
    /// Species data service
    /// </summary>
    public class ProcessedObservationRepository : ProcessBaseRepository<ProcessedObservation, ObjectId>, IProcessedObservationRepository
    {
        private readonly IElasticClient _elasticClient;
        private const int BiotaTaxonId = 0;
        private readonly ITaxonManager _taxonManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="elasticClient"></param>
        /// <param name="client"></param>
        /// <param name="taxonManager"></param>
        /// <param name="logger"></param>
        public ProcessedObservationRepository(
            IElasticClient elasticClient,
            IProcessClient client,
            ITaxonManager taxonManager,
            ILogger<ProcessedObservationRepository> logger) : base(client, true, logger)
        {
            _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
            _taxonManager = taxonManager ?? throw new ArgumentNullException(nameof(taxonManager));
        }

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetChunkAsync(SearchFilter filter, int skip, int take)
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

            return new PagedResult<dynamic>
            {
                Records = searchResponse.Documents,
                TotalCount = searchResponse.HitsMetadata.Total.Value
            };


        }
    }
}