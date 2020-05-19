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
using SOS.Observations.Api.Enum;
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
        public async Task<PagedResult<dynamic>> GetChunkAsync(SearchFilter filter, int skip, int take, string sortBy, SearchSortOrder sortOrder)
        {
            if (!filter?.IsFilterActive ?? true)
            {
                return null;
            }

            var query = filter.ToQuery();
            query = AddInternalFilters(filter, query);

            var excludeQuery = CreateExcludeQuery(filter);

            var sortDescriptor = new SortDescriptor<dynamic>();
            if (!string.IsNullOrEmpty(sortBy))
            {
                sortDescriptor.Field(sortBy, sortOrder == SearchSortOrder.Desc ? SortOrder.Descending : SortOrder.Ascending);
            }

            var searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
                .Index(CollectionName.ToLower())
                .Source(filter.OutputFields.ToProjection())
                .From(skip)
                .Size(take)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Sort(s => sortDescriptor)
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            var totalCount = searchResponse.HitsMetadata.Total.Value;

            if(filter is SearchFilterInternal)
            {
                var internalFilter = filter as SearchFilterInternal;
                if (internalFilter.IncludeRealCount)
                {
                    var countResponse = await _elasticClient.CountAsync<dynamic>(s => s
                                                    .Index(CollectionName.ToLower())                                                    
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
            return new PagedResult<dynamic>
            {
                Records = searchResponse.Documents,
                Skip = skip,
                Take = take,
                TotalCount = totalCount
            };
        }
        private static List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> CreateExcludeQuery(FilterBase filter)
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
        private static IEnumerable<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> AddInternalFilters(SearchFilter filter, IEnumerable<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query)
        {
            var queryInternal = query.ToList();
            if (filter is SearchFilterInternal)
            {
                var internalFilter = filter as SearchFilterInternal;

                if (internalFilter.ProjectId.HasValue)
                {
                    queryInternal.Add(q => q
                        .Nested(n=>n
                            .Path("projects")
                            .Query(q=>q
                                .Match(m=>m
                                .Field(new Field("projects.id"))
                                .Query(internalFilter.ProjectId.ToString())
                        )
                    )));
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