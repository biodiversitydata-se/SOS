using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Cluster;
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
        public async Task<SearchAfterResult<dynamic, IReadOnlyCollection<FieldValue>>> AggregateByUserFieldAsync(SearchFilter filter, string aggregationField, int? precisionThreshold, string? afterKey = null, int? take = 10)
        {
            var indexNames = GetCurrentIndex(filter);
            var (queries, excludeQueries) = GetCoreQueries(filter);

            var tz = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQueries.ToArray())
                        .Filter(queries.ToArray())
                    )
                )
                .ScriptFields(sf => sf // Since missing field seems replaced by MissingBucket in Nest implementation, we need to make a script field to handle empty string and null the same way
                    .Add("scriptField", a => a
                        .Script(s => s
                            .Source(@$"
                                if (!doc['{aggregationField}'].empty){{  
                                    String value = '' + doc['{aggregationField}'].value; 
                                    if (value != '') {{ 
                                        emit(value); 
                                    }} 
                                }}"
                            )// FieldType.Keyword
                        )
                        
                    )
                )
                .Aggregations(a => a
                    .Add("aggregation", a => a
                        .Composite(c => c
                        .After(a => a.Add("scriptField".ToField(), afterKey))
                        .Size(take)
                        .Sources(
                            [
                                CreateCompositeTermsAggregationSource(
                                    ("termAggregation", "scriptField", SortOrder.Asc, true)
                                )
                            ]
                        )
                    )
                    .Aggregations(a => a
                        .Add("unique_taxonids", a => a
                            .Cardinality(c => c
                                .Field("taxon.id")
                                .PrecisionThreshold(precisionThreshold ?? 3000)
                                )
                            )
                        )
                    )
                )
                .AddDefaultAggrigationSettings()
            );

            searchResponse.ThrowIfInvalid();
            var compositeAgg = searchResponse.Aggregations?.GetComposite("aggregation");
            compositeAgg?.AfterKey?.Values.FirstOrDefault().TryGetString(out afterKey);

            return new SearchAfterResult<dynamic, IReadOnlyCollection<FieldValue>>
            {
                SearchAfter = string.IsNullOrEmpty(afterKey) ? null! : [afterKey],
                Records = compositeAgg?
                    .Buckets?
                    .Select(b =>
                        new
                        {
                            AggregationField = b.Key.Values.First(),
                            b.DocCount,
                            UniqueTaxon = b.Aggregations?.GetCardinality("unique_taxonids")?.Value
                        }
                    )?.ToArray()
            };
        }
    }
}
