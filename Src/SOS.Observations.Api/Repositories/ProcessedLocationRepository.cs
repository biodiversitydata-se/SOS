using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Cluster;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Caching.Memory;
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
using SOS.Observations.Api.Repositories.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Repositories
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public class ProcessedLocationRepository : ProcessedObservationBaseRepository,
        IProcessedLocationRepository
    {

        /// <summary>
        /// Constructor used in public mode
        /// </summary>
        /// <param name="elasticClientManager"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="clusterHealthCache"></param>
        /// <param name="memoryCache"></param>
        /// <param name="logger"></param>
        public ProcessedLocationRepository(
            IElasticClientManager elasticClientManager,
            ElasticSearchConfiguration elasticConfiguration,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            IClassCache<ConcurrentDictionary<string, HealthResponse>> clusterHealthCache,
            IMemoryCache memoryCache,
            ILogger<ProcessedLocationRepository> logger) : base(true, elasticClientManager, processedConfigurationCache, elasticConfiguration, clusterHealthCache, memoryCache, logger)
        {

        }

        /// <inheritdoc />
        public async Task<IEnumerable<Location>> GetLocationsAsync(IEnumerable<string> locationIds)
        {
            if (!locationIds?.Any() ?? true)
            {
                return null;
            }

            var queries = new List<Action<QueryDescriptor<Observation>>>();
            queries.TryAddTermsCriteria("location.locationId", locationIds);

            var searchResponse = await Client.SearchAsync<Observation>(s => s
                .Index($"{PublicIndexName}, {ProtectedIndexName}")
                .Query(q => q
                    .Bool(b => b
                        .Filter(queries.ToArray())
                    )
                )
                .Collapse(c => c.Field("location.locationId"))
                .Size(locationIds.Count())
                .Source((Includes : new[] { "location" }, Excludes: new[] { "location.pointLocation" }).ToProjection()
                )
               .TrackTotalHits(new TrackHits(false))
            );

            searchResponse.ThrowIfInvalid();

            return searchResponse.Documents?.Select(d => d.Location);
        }

        public async Task<IEnumerable<LocationSearchResult>> SearchAsync(SearchFilter filter, int skip,
            int take)
        {
            var indexName = GetCurrentIndex(filter);
            var (queries, excludeQueries) = GetCoreQueries<dynamic>(filter);

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexName)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQueries.ToArray())
                        .Filter(queries.ToArray())
                    )
                )
                .Aggregations(a => a
                    .Add("locations", a => a
                        .Composite(c => c
                            .Size(skip + take)
                            .Sources(
                                [
                                    CreateCompositeTermsAggregationSource(
                                        ("id", "location.locationId", SortOrder.Asc)
                                    ),
                                    CreateCompositeTermsAggregationSource(
                                        ("name", "location.locality", SortOrder.Asc)
                                    ),
                                    CreateCompositeTermsAggregationSource(
                                        ("county", "location.county.name", SortOrder.Asc)
                                    ),CreateCompositeTermsAggregationSource(                                       
                                        ("municipality", "location.municipality.name", SortOrder.Asc)
                                    ),
                                    CreateCompositeTermsAggregationSource(
                                        ("parish", "location.parish.name", SortOrder.Asc)
                                    ),
                                    CreateCompositeTermsAggregationSource(
                                        ("longitude", "location.decimalLongitude", SortOrder.Asc)
                                    ),
                                    CreateCompositeTermsAggregationSource(
                                        ("latitude", "location.decimalLatitude", SortOrder.Asc)
                                    )
                                ]
                             )
                        )
                    )
                )
                .AddDefaultAggrigationSettings()
            );

            searchResponse.ThrowIfInvalid();

            var result = new List<LocationSearchResult>();
            foreach (var bucket in searchResponse.Aggregations.GetComposite("locations").Buckets?.Skip(skip))
            {
                bucket.Key["county"].TryGetString(out var county);
                bucket.Key["id"].TryGetString(out var id);
                bucket.Key["latitude"].TryGetDouble(out var latitude);
                bucket.Key["longitude"].TryGetLong(out var longitude);
                bucket.Key["municipality"].TryGetString(out var municipality);
                bucket.Key["name"].TryGetString(out var name);
                bucket.Key["parish"].TryGetString(out var parish);
                result.Add(new LocationSearchResult
                {
                    County = county,
                    Id = id,
                    Latitude = latitude ?? 0, 
                    Longitude = longitude ?? 0,
                    Municipality = municipality,
                    Name = name,
                    Parish = parish
                });
            }

            return result;
        }
    }
}
