using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Database.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.Repositories
{
    /// <summary>
    /// Species data service
    /// </summary>
    public class ProcessedObservationRepository : ProcessBaseRepository<ProcessedObservation, string>, IProcessedObservationRepository
    {
        private readonly IElasticClient _elasticClient;


        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="elasticClient"></param>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public ProcessedObservationRepository(
            IElasticClient elasticClient,
            IProcessClient client,
            ILogger<ProcessedObservationRepository> logger) : base(client, true, logger)
        {
            _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        }

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetChunkAsync(SearchFilter filter, int skip, int take)
        {
            if (!filter?.IsFilterActive ?? true)
            {
                return null;
            }
            var query = filter.ToQuery();
            query = AddInternalFilters(filter, query);

            var searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
                .Index(CollectionName.ToLower())
                .Source(filter.OutputFields.ToProjection())
                .From(skip)
                .Size(take)
                .Query(q => q
                    .Bool(b => b
                        .Filter(query))));

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            return new PagedResult<dynamic>
            {
                Records = searchResponse.Documents,
                //Records = searchResponse.Hits,
                TotalCount = searchResponse.HitsMetadata.Total.Value
            };
        }

        private static IEnumerable<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> AddInternalFilters(SearchFilter filter, IEnumerable<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query)
        {
            var queryInternal = query.ToList();
            if (filter is SearchFilterInternal)
            {
                var internalFilter = filter as SearchFilterInternal;

                if (internalFilter.ProjectId.HasValue)
                {
                    queryInternal.Add(q => q
                        .Match(t => t
                            .Field(new Field("projects.id"))
                            .Query(internalFilter.ProjectId.ToString())
                        )
                    );
                }
                if (internalFilter.UserId.HasValue)
                {
                    queryInternal.Add(q => q
                        .Terms(t => t
                            .Field(new Field("reportedByUserId"))
                            .Terms(internalFilter.UserId)
                        )
                    );
                }
            }
            return queryInternal;
        }
    }
}