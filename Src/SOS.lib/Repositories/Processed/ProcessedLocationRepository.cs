using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.Repositories.Processed
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
        /// <param name="logger"></param>
        public ProcessedLocationRepository(
            IElasticClientManager elasticClientManager,
            ElasticSearchConfiguration elasticConfiguration,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ILogger<ProcessedLocationRepository> logger) : base(true, elasticClientManager, processedConfigurationCache, elasticConfiguration, logger)
        {
           
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Location>> GetLocationsAsync(IEnumerable<string> locationIds)
        {
            if (!locationIds?.Any() ?? true)
            {
                return null;
            }

            var searchResponse = await Client.SearchAsync<Observation>(s => s
                .Index($"{PublicIndexName}, {ProtectedIndexName}")
                .Query(q => q
                    .Bool(b => b
                        .Filter(f => f
                            .Terms(t => t
                                .Field("location.locationId")
                                .Terms(locationIds)
                            )
                        )
                    )
                )
                .Collapse(c => c.Field("location.locationId"))
               .Source(s => s
                    .Includes(i => i
                        .Field("location")
                    )
                )
            );

            if (!searchResponse.IsValid)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return searchResponse.Documents?.Select(d => d.Location);
        }

        public async Task<IEnumerable<LocationSearchResult>> SearchAsync(SearchFilter filter, int skip,
            int take)
        {
            var indexName = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexName)
                .Size(0)
                .Aggregations(a => a
                    .Composite("locations", g => g
                        .Size(skip + take)
                        .Sources(src => src
                            .Terms("id", tt => tt
                                .Field("location.locationId")
                            )
                            .Terms("name", tt => tt
                                .Field("location.locality")
                            )
                            .Terms("county", tt => tt
                                .Field("location.county.name")
                            )
                            .Terms("municipality", tt => tt
                                .Field("location.municipality.name")
                            )
                            .Terms("parish", tt => tt
                                .Field("location.parish.name")
                            )
                            .Terms("longitude", tt => tt
                                .Field("location.decimalLongitude")
                            )
                            .Terms("latitude", tt => tt
                                .Field("location.decimalLatitude")
                            )
                        )
                    )
                )
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
            );

            if (!searchResponse.IsValid)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            var result = new List<LocationSearchResult>();
            foreach (var bucket in searchResponse.Aggregations.Composite("locations").Buckets?.Skip(skip))
            {
                result.Add(new LocationSearchResult
                {
                    County = (string)bucket.Key["county"],
                    Id = (string)bucket.Key["id"],
                    Latitude = (double)bucket.Key["latitude"],
                    Longitude = (double)bucket.Key["longitude"],
                    Municipality = (string)bucket.Key["municipality"],
                    Name = (string)bucket.Key["name"],
                    Parish = (string)bucket.Key["parish"]
                });
            }

            return result;
        }
    }
}
