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
            query = AddInternalFilters(filter, query);

            var excludeQuery = CreateExcludeQuery(filter);
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

        private static IEnumerable<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> AddInternalFilters(
            SearchFilter filter, IEnumerable<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query)
        {
            var queryInternal = query.ToList();
            if (filter is SearchFilterInternal)
            {
                var internalFilter = filter as SearchFilterInternal;

                if (internalFilter.ProjectId.HasValue)
                {
                    queryInternal.Add(q => q
                        .Nested(n => n
                            .Path("projects")
                            .Query(q => q
                                .Match(m => m
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

                if (internalFilter.BoundingBox != null)
                {
                    queryInternal.Add(q => q
                        .GeoBoundingBox(g => g
                            .Field(new Field("location.pointLocation"))
                            .BoundingBox(internalFilter.BoundingBox[1],
                                internalFilter.BoundingBox[0],
                                internalFilter.BoundingBox[3],
                                internalFilter.BoundingBox[2])));
                }
                if (internalFilter.OnlyWithMedia)
                {
                    queryInternal.Add(q => q
                        .Wildcard(w => w
                            .Field("occurrence.associatedMedia")
                            .Value("?*")));
                }

                if (internalFilter.OnlyWithNotes)
                {
                    queryInternal.Add(q => q
                        .Wildcard(w => w
                            .Field("occurrence.occurrenceRemarks")
                            .Value("?*")));
                }

                if (internalFilter.OnlyWithNotesOfInterest)
                {
                    queryInternal.Add(q => q
                        .Term(m => m
                            .Field("occurrence.noteOfInterest")
                            .Value(true)));
                }

                if (internalFilter.ReportedDateFrom.HasValue)
                {
                    queryInternal.Add(q => q
                        .DateRange(r => r
                            .Field("reportedDate")
                            .GreaterThanOrEquals(
                                DateMath.Anchored(
                                    internalFilter.ReportedDateFrom.Value.ToUniversalTime()
                                )
                            )
                        )
                    );
                }

                if (internalFilter.ReportedDateTo.HasValue)
                {
                    queryInternal.Add(q => q
                        .DateRange(r => r
                            .Field("reportedDate")
                            .LessThanOrEquals(
                                DateMath.Anchored(
                                    internalFilter.ReportedDateTo.Value.ToUniversalTime()
                                )
                            )
                        )
                    );
                }

                if (internalFilter.MaxAccuracy.HasValue)
                {
                    queryInternal.Add(q => q
                        .Range(r => r
                            .Field("location.coordinateUncertaintyInMeters")
                            .LessThanOrEquals(internalFilter.MaxAccuracy)
                        )
                    );
                }

                if (internalFilter.Months?.Any() ?? false)
                {
                    queryInternal.Add(q => q
                        .Script(s => s
                            .Script(sc => sc
                                .Source($@"return [{string.Join(',',internalFilter.Months.Select(m=>$"{m}"))}].contains(doc['event.startDate'].value.getMonthValue());")
                            )
                        )
                    );
                }

                if (internalFilter.DiscoveryMethodIds?.Any() ?? false)
                {
                    queryInternal.Add(q => q
                        .Terms(t => t
                            .Field("occurrence.discoveryMethod.id")
                            .Terms(internalFilter.DiscoveryMethodIds)
                        )
                    );
                }

                if (internalFilter.LifeStageIds?.Any() ?? false)
                {
                    queryInternal.Add(q => q
                        .Terms(t => t
                            .Field("occurrence.lifeStage.id")
                            .Terms(internalFilter.LifeStageIds)
                        )
                    );
                }

                if (internalFilter.ActivityIds?.Any() ?? false)
                {
                    queryInternal.Add(q=>q
                        .Terms(t=>t
                            .Field("occurrence.activity.id")
                            .Terms(internalFilter.ActivityIds)
                        )
                    );
                }

                if (internalFilter.HasTriggerdValidationRule)
                {
                    queryInternal.Add(q => q
                        .Term(m => m
                            .Field("hasTriggeredValidationRules")
                            .Value(true)));
                }

                if (internalFilter.HasTriggerdValidationRuleWithWarning)
                {
                    queryInternal.Add(q => q
                        .Term(m => m
                            .Field("hasAnyTriggeredValidationRuleWithWarning")
                            .Value(true)));
                }

                if (internalFilter.Length.HasValue && !string.IsNullOrWhiteSpace(internalFilter.LengthOperator))
                {
                    switch (internalFilter.LengthOperator.ToLower())
                    {
                        case "eq":
                            queryInternal.Add(q => q
                                .Term(r => r
                                    .Field("occurrence.length")
                                    .Value(internalFilter.Length.Value)
                                )
                            );
                            break;
                        case "gte":
                            queryInternal.Add(q => q
                                .Range(r => r
                                    .Field("occurrence.length")
                                    .GreaterThanOrEquals(internalFilter.Length.Value)
                                )
                            );
                            break;
                        case "lte":
                            queryInternal.Add(q => q
                                .Range(r => r
                                    .Field("occurrence.length")
                                    .LessThanOrEquals(internalFilter.Length.Value)
                                )
                            );
                            break;
                    }
                }

                if (internalFilter.Weight.HasValue && !string.IsNullOrWhiteSpace(internalFilter.WeightOperator))
                {
                    switch (internalFilter.WeightOperator.ToLower())
                    {
                        case "eq":
                            queryInternal.Add(q => q
                                .Term(r => r
                                    .Field("occurrence.weight")
                                    .Value(internalFilter.Weight.Value)
                                )
                            );
                            break;
                        case "gte":
                            queryInternal.Add(q => q
                                .Range(r => r
                                    .Field("occurrence.weight")
                                    .GreaterThanOrEquals(internalFilter.Weight.Value)
                                )
                            );
                            break;
                        case "lte":
                            queryInternal.Add(q => q
                                .Range(r => r
                                    .Field("occurrence.weight")
                                    .LessThanOrEquals(internalFilter.Weight.Value)
                                )
                            );
                            break;
                    }
                }

                if (internalFilter.UsePeriodForAllYears && internalFilter.StartDate.HasValue && internalFilter.EndDate.HasValue)
                {
                    queryInternal.Add(q => q
                        .Script(s => s
                            .Script(sc => sc
                                .Source($@"
                                    int startYear = doc['event.startDate'].value.getYear();
                                    int startMonth = doc['event.startDate'].value.getMonthValue();
                                    int startDay = doc['event.startDate'].value.getDayOfMonth();

                                    int fromMonth = {internalFilter.StartDate.Value.Month};
                                    int fromDay = {internalFilter.StartDate.Value.Day};
                                    int toMonth = {internalFilter.EndDate.Value.Month};
                                    int toDay = {internalFilter.EndDate.Value.Day};

                                    if(
                                        (startMonth == fromMonth && startDay >= fromDay)
                                        || (startMonth > fromMonth && startMonth < toMonth)
                                        || (startMonth == toMonth && startDay <= toDay)
                                    )
                                    {{ 
                                        return true;
                                    }} 
                                    else 
                                    {{
                                        return false;
                                    }}
                                ")
                            )
                        )
                    );
                }
            }

            return queryInternal;
        }
    }
}