using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using SOS.Analysis.Api.Repositories.Interfaces;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Search.Filters;
using Nest;
using SOS.Lib.Models.Search.Result;

namespace SOS.Analysis.Api.Repositories
{
    public class ProcessedObservationRepository : ProcessedObservationCoreRepository, IProcessedObservationRepository
    {
        public ProcessedObservationRepository(
            IElasticClientManager elasticClientManager,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            TelemetryClient telemetry,
            ElasticSearchConfiguration elasticConfiguration,
            ILogger<ProcessedObservationRepository> logger) : base(elasticClientManager, elasticConfiguration, processedConfigurationCache, telemetry, logger)
        {

        }

        /// <inheritdoc />
        public async Task<SearchAfterResult<dynamic>> AggregateByUserFieldAsync(SearchFilter filter, string aggregationField, string? afterKey, int? take = 10)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);
           
            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Analysis_Aggregate_By_User_Field");
            operation.Telemetry.Properties["Filter"] = filter.ToString();
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
                            )
                        )
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
            );

            searchResponse.ThrowIfInvalid();

            _telemetry.StopOperation(operation);

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
