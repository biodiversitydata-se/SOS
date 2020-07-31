using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using SOS.Lib.Models.Search;

namespace SOS.Observations.Api.Repositories
{
    /// <summary>
    /// Adds Elastic queries for SearchFilterInternal.
    /// </summary>
    public static class InternalFilterBuilder
    {
        public static IEnumerable<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> AddFilters(
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
                                .Source($@"return [{string.Join(',', internalFilter.Months.Select(m => $"{m}"))}].contains(doc['event.startDate'].value.getMonthValue());")
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
                    queryInternal.Add(q => q
                        .Terms(t => t
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

                if (internalFilter.Quantity.HasValue && !string.IsNullOrWhiteSpace(internalFilter.QuantityOperator))
                {
                    switch (internalFilter.QuantityOperator.ToLower())
                    {
                        case "eq":
                            queryInternal.Add(q => q
                                .Term(r => r
                                    .Field("occurrence.organismQuantityInt")
                                    .Value(internalFilter.Quantity.Value)
                                )
                            );
                            break;
                        case "gte":
                            queryInternal.Add(q => q
                                .Range(r => r
                                    .Field("occurrence.organismQuantityInt")
                                    .GreaterThanOrEquals(internalFilter.Quantity.Value)
                                )
                            );
                            break;
                        case "lte":
                            queryInternal.Add(q => q
                                .Range(r => r
                                    .Field("occurrence.organismQuantityInt")
                                    .LessThanOrEquals(internalFilter.Quantity.Value)
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