using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Nest;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.Repositories
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public class ProcessedObservationRepository : ProcessBaseRepository<ProcessedObservation, string>,
        IProcessedObservationRepository
    {
        private readonly IElasticClient _elasticClient;
        private readonly TelemetryClient _telemetry;
        private readonly string _indexName;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="elasticClient"></param>
        /// <param name="client"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="logger"></param>
        public ProcessedObservationRepository(
            IElasticClient elasticClient,
            IProcessClient client,
            ElasticSearchConfiguration elasticConfiguration,
            TelemetryClient telemetry,
            ILogger<ProcessedObservationRepository> logger) : base(client, true, logger)
        {
            _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));

            _indexName = string.IsNullOrEmpty(elasticConfiguration.IndexPrefix)
                ? $"{CollectionName.ToLower()}"
                : $"{elasticConfiguration.IndexPrefix.ToLower()}-{CollectionName.ToLower()}";

            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry)); ;
        }

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetChunkAsync(SearchFilter filter, int skip, int take, string sortBy,
            SearchSortOrder sortOrder)
        {
            if (!filter?.IsFilterActive ?? true)
            {
                return null;
            }

            var query = filter.ToQuery();
            query = AddSightingTypeFilters(filter, query);
            query = InternalFilterBuilder.AddFilters(filter, query);

            var excludeQuery = CreateExcludeQuery(filter);
            excludeQuery = InternalFilterBuilder.AddExcludeFilters(filter, excludeQuery);

            var sortDescriptor = sortBy.ToSortDescriptor<ProcessedObservation>(sortOrder);

            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search");

            operation.Telemetry.Properties["Filter"] = filter.ToString(); 

            var searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
                .Index(_indexName)
                .Source(filter.OutputFields.ToProjection(filter is SearchFilterInternal))
                .From(skip)
                .Size(take)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Sort(sort => sortDescriptor)
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            var totalCount = searchResponse.HitsMetadata.Total.Value;

            if (filter is SearchFilterInternal)
            {
                var internalFilter = filter as SearchFilterInternal;
                if (internalFilter.IncludeRealCount)
                {
                    var countResponse = await _elasticClient.CountAsync<dynamic>(s => s
                        .Index(_indexName)
                        .Query(q => q
                            .Bool(b => b
                                .MustNot(excludeQuery)
                                .Filter(query)
                            )
                        )
                    );
                    if (!countResponse.IsValid) throw new InvalidOperationException(countResponse.DebugInformation);
                    totalCount = countResponse.Count;
                }
            }

            // Optional: explicitly send telemetry item:
            _telemetry.StopOperation(operation);

            return new PagedResult<dynamic>
            {
                Records = searchResponse.Documents,
                Skip = skip,
                Take = take,
                TotalCount = totalCount
            };

            // When operation is disposed, telemetry item is sent.
        }

        private static IEnumerable<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> AddSightingTypeFilters(SearchFilter filter, IEnumerable<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query)
        {
            var queryList = query.ToList();
            
            if(filter is SearchFilterInternal)
            {
                var internalFilter = filter as SearchFilterInternal;                
                int[] sightingTypeSearchGroupFilter = null;
                if (internalFilter.TypeFilter == SearchFilterInternal.SightingTypeFilter.DoNotShowMerged)
                {                    
                    sightingTypeSearchGroupFilter = new int[] { 0, 1, 4, 16, 32, 128 };
                }
                else if (internalFilter.TypeFilter == SearchFilterInternal.SightingTypeFilter.ShowBoth)
                {                 
                    sightingTypeSearchGroupFilter = new int[] { 0, 1, 2, 4, 16, 32, 128 };
                }
                else if (internalFilter.TypeFilter == SearchFilterInternal.SightingTypeFilter.ShowOnlyMerged)
                {                 
                    sightingTypeSearchGroupFilter = new int[] { 0, 2 };
                }
                else if (internalFilter.TypeFilter == SearchFilterInternal.SightingTypeFilter.DoNotShowSightingsInMerged)
                {                 
                    sightingTypeSearchGroupFilter = new int[] { 0, 1, 2, 4, 32, 128 };
                }             
                queryList.Add(q => q
                        .Terms(t => t
                            .Field("occurrence.sightingTypeSearchGroupId")
                            .Terms(sightingTypeSearchGroupFilter)
                        )
                    );
            }
            else
            {
                queryList.Add(q => q
                        .Terms(t => t
                            .Field("occurrence.sightingTypeId")
                            .Terms(new int[] { 0, 3 })
                        )
                    );
                queryList.Add(q => q
                        .Terms(t => t
                            .Field("occurrence.sightingTypeSearchGroupId")
                            .Terms(new int[] { 0, 1, 32 })
                        )
                    );

            }
            query = queryList;
            return query;
        }

        private static List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> CreateExcludeQuery(
            FilterBase filter)
        {
            var queryContainers = new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();

            if (filter.GeometryFilter?.IsValid ?? false)
            {
                foreach (var geom in filter.GeometryFilter.Geometries)
                {
                    switch (geom.Type.ToLower())
                    {
                        case "holepolygon":
                            if (filter.GeometryFilter.UsePointAccuracy)
                            {
                                queryContainers.Add(q => q
                                    .GeoShape(gd => gd
                                        .Field("location.pointWithBuffer")
                                        .Shape(s => geom)
                                        .Relation(GeoShapeRelation.Intersects)
                                    )
                                );
                            }
                            else
                            {
                                queryContainers.Add(q => q
                                    .GeoShape(gd => gd
                                        .Field("location.point")
                                        .Shape(s => geom)
                                        .Relation(GeoShapeRelation.Within)
                                    )
                                );
                            }

                            break;
                    }
                }
            }

            return queryContainers;
        }
    }
}