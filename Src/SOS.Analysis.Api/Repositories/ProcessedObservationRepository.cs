using Nest;
using SOS.Analysis.Api.Repositories.Interfaces;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Extensions;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Repositories.Processed;
using System.Collections.Concurrent;

namespace SOS.Analysis.Api.Repositories
{
    public class ProcessedObservationRepository : ProcessedObservationCoreRepository, IProcessedObservationRepository
    {
        public ProcessedObservationRepository(
            IElasticClientManager elasticClientManager,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ElasticSearchConfiguration elasticConfiguration,
            ITaxonManager taxonManager,
            IClassCache<ConcurrentDictionary<string, HealthResponse>> clusterHealthCache,
            ILogger<ProcessedObservationRepository> logger) : base(elasticClientManager, elasticConfiguration, processedConfigurationCache, taxonManager, clusterHealthCache, logger)
        {

        }

        /// <inheritdoc />
        public async Task<SearchAfterResult<dynamic>> AggregateByUserFieldAsync(SearchFilter filter, string aggregationField, int? precisionThreshold, string? afterKey = null, int? take = 10)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            var tz = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .RuntimeFields(rf => rf // Since missing field seems replaced by MissingBucket in Nest implementation, we need to make a script field to handle empty string and null the same way
                    .RuntimeField("scriptField", FieldType.Keyword, s => s
                            .Script(@$"
                            if (!doc['{aggregationField}'].empty){{  
                                String value = '' + doc['{aggregationField}'].value; 
                                if (value != '') {{ 
                                    emit(value); 
                                }} 
                            }}"
                         )
                    )
                )
                .Aggregations(a => a
                    .Composite("aggregation", c => c
                        .After(string.IsNullOrEmpty(afterKey) ? null : new CompositeKey(new Dictionary<string, object>() { { "termAggregation", afterKey } }))
                        .Size(take)
                        .Sources(s => s
                            .Terms("termAggregation", t => t
                                .Field("scriptField")
                                .MissingBucket(true)
                            )
                        )
                        .Aggregations(a => a
                            .Cardinality("unique_taxonids", c => c
                                .Field("taxon.id")
                                .PrecisionThreshold(precisionThreshold ?? 3000)
                            )
                        )
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
            );

            searchResponse.ThrowIfInvalid();
            afterKey = searchResponse
               .Aggregations
               .Composite("aggregation")
               .AfterKey?.Values.FirstOrDefault()?.ToString()!;

            return new SearchAfterResult<dynamic>
            {
                SearchAfter = new[] { afterKey },
                Records = searchResponse
                    .Aggregations
                    .Composite("aggregation")
                    .Buckets?
                    .Select(b =>
                        new
                        {
                            AggregationField = b.Key.Values.First(),
                            b.DocCount,
                            UniqueTaxon = b.Cardinality("unique_taxonids").Value
                        }
                    )?.ToArray()
            };
        }
    }
}
