using Microsoft.Extensions.Logging;
using Nest;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Repositories.Processed;
using SOS.Observations.Api.IntegrationTests.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib;
using System.Linq;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Observations.Api.IntegrationTests.Repositories
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public class ProcessedObservationRepositoryTest : ProcessedObservationBaseRepository,
        IProcessedObservationRepositoryTest
    {
        /// Constructor used in public mode
        /// </summary>
        /// <param name="elasticClientManager"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="telemetry"></param>
        /// <param name="taxonManager"></param>
        /// <param name="logger"></param>
        public ProcessedObservationRepositoryTest(
            IElasticClientManager elasticClientManager,
            ElasticSearchConfiguration elasticConfiguration,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ILogger<ProcessedObservationRepositoryTest> logger) : base(true, elasticClientManager, processedConfigurationCache, elasticConfiguration, logger)
        {
           
        }

        public async Task<SearchAfterResult<Observation>> GetNaturalisChunkAsync(SearchFilterInternal filter, string pointInTimeId = null,
           IEnumerable<object> searchAfter = null)
        {
            var searchIndex = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);
            var searchResponse = await SearchAfterAsync(searchIndex, new SearchDescriptor<dynamic>()
                .Index(searchIndex)
                .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQuery)
                            .Filter(query)
                        )
                    )
                .Source(filter.Output?.Fields.ToProjection(false)),
                pointInTimeId,
                searchAfter);

            return new SearchAfterResult<Observation>
            {
                Records = searchResponse.Documents.ToObservations(),
                PointInTimeId = searchResponse.PointInTimeId,
                SearchAfter = searchResponse.Hits?.LastOrDefault()?.Sorts
            };
        }
    }
}
