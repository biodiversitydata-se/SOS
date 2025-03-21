using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Cluster;
using Microsoft.Extensions.Logging;
using SOS.Lib;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Extensions;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Repositories.Processed;
using SOS.Observations.Api.LiveIntegrationTests.Repositories.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Observations.Api.LiveIntegrationTests.Repositories
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
            IClassCache<ConcurrentDictionary<string, HealthResponse>> clusterHealthCache,
            ILogger<ProcessedObservationRepositoryTest> logger) : base(true, elasticClientManager, processedConfigurationCache, elasticConfiguration, clusterHealthCache, logger)
        {

        }

        public async Task<SearchAfterResult<Observation, ICollection<FieldValue>>> GetNaturalisChunkAsync(SearchFilterInternal filter, string pointInTimeId = null,
           ICollection<FieldValue> searchAfter = null)
        {
            var searchIndex = GetCurrentIndex(filter);
            var (queries, excludeQueries) = GetCoreQueries(filter);
            var searchResponse = await SearchAfterAsync(searchIndex, new SearchRequestDescriptor<dynamic>()
                .Index(searchIndex)
                .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQueries.ToArray())
                            .Filter(queries.ToArray())
                        )
                    )
                .Source(filter.Output?.Fields.ToProjection(false)),
                pointInTimeId,
                searchAfter);
           
            return new SearchAfterResult<Observation, ICollection<FieldValue>>
            {
                Records = searchResponse.Documents.ToObservations(),
                PointInTimeId = searchResponse.PitId,
                SearchAfter = searchResponse.Hits?.LastOrDefault()?.Sort?.ToCollection()
            };
        }
    }
}
