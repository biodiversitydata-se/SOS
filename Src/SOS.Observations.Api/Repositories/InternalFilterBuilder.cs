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

                if (internalFilter.ProjectIds?.Any() ?? false)
                {
                    queryInternal.Add(q => q
                        .Nested(n => n
                            .Path("projects")
                            .Query(q => q
                                .Terms(t => t
                                    .Field("projects.id")
                                    .Terms(internalFilter.ProjectIds)
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
                    AddNumericFilterWithRelationalOperator(queryInternal, "occurrence.length", internalFilter.Length.Value, internalFilter.LengthOperator);
                }

                if (internalFilter.Weight.HasValue && !string.IsNullOrWhiteSpace(internalFilter.WeightOperator))
                {
                    AddNumericFilterWithRelationalOperator(queryInternal, "occurrence.weight", internalFilter.Weight.Value, internalFilter.WeightOperator);
                }

                if (internalFilter.Quantity.HasValue && !string.IsNullOrWhiteSpace(internalFilter.QuantityOperator))
                {
                    AddNumericFilterWithRelationalOperator(queryInternal, "occurrence.organismQuantityInt", internalFilter.Quantity.Value, internalFilter.QuantityOperator);
                }

                if (internalFilter.ValidationStatusIds?.Any() ?? false)
                {
                    queryInternal.Add(q => q
                        .Terms(t => t
                            .Field("identification.validationStatus.id")
                            .Terms(internalFilter.ValidationStatusIds)
                        )
                    );
                }

                if (internalFilter.OnlyWithBarcode)
                {
                    queryInternal.Add(q => q
                        .Wildcard(w => w
                            .Field("taxon.individualId")
                            .Value("?*")
                        )
                    );
                }

                switch (internalFilter.DeterminationFilter)
                {
                    case SearchFilterInternal.SightingDeterminationFilter.NotUnsureDetermination:
                        queryInternal.Add(q => q
                            .Term(m => m
                                .Field("identification.uncertainDetermination")
                                .Value(false)));
                        break;
                    case SearchFilterInternal.SightingDeterminationFilter.OnlyUnsureDetermination:
                        queryInternal.Add(q => q
                            .Term(m => m
                                .Field("identification.uncertainDetermination")
                                .Value(true)));
                        break;
                }

                switch (internalFilter.UnspontaneousFilter)
                {
                    case SearchFilterInternal.SightingUnspontaneousFilter.NotUnspontaneous:
                        queryInternal.Add(q => q
                            .Term(m => m
                                .Field("occurrence.isNaturalOccurrence")
                                .Value(true)));
                        break;
                    case SearchFilterInternal.SightingUnspontaneousFilter.Unspontaneous:
                        queryInternal.Add(q => q
                            .Term(m => m
                                .Field("occurrence.isNaturalOccurrence")
                                .Value(false)));
                        break;
                }

                switch (internalFilter.NotRecoveredFilter)
                {
                    case SearchFilterInternal.SightingNotRecoveredFilter.DontIncludeNotRecovered:
                        queryInternal.Add(q => q
                            .Term(m => m
                                .Field("occurrence.isNotRediscoveredObservation")
                                .Value(false)));
                        break;
                    case SearchFilterInternal.SightingNotRecoveredFilter.OnlyNotRecovered:
                        queryInternal.Add(q => q
                            .Term(m => m
                                .Field("occurrence.isNotRediscoveredObservation")
                                .Value(true)));
                        break;
                }

                if (!string.IsNullOrEmpty(internalFilter.SpeciesCollectionLabel))
                {
                    queryInternal.Add(q => q
                        .Term(m => m
                            .Field("collectionId.keyword")
                            .Value(internalFilter.SpeciesCollectionLabel)));
                }

                if (!string.IsNullOrEmpty(internalFilter.PublicCollection))
                {
                    queryInternal.Add(q => q
                        .Term(m => m
                            .Field("publicCollection.keyword")
                            .Value(internalFilter.PublicCollection)));
                }

                if (!string.IsNullOrEmpty(internalFilter.PrivateCollection))
                {
                    queryInternal.Add(q => q
                        .Term(m => m
                            .Field("privateCollection.keyword")
                            .Value(internalFilter.PrivateCollection)));
                }

                if (internalFilter.SubstrateSpeciesId.HasValue)
                {
                    queryInternal.Add(q => q
                        .Term(m => m
                            .Field("event.substrateSpeciesId")
                            .Value(internalFilter.SubstrateSpeciesId.Value)));
                }

                if (internalFilter.SubstrateId.HasValue)
                {
                    queryInternal.Add(q => q
                        .Term(m => m
                            .Field("event.substrate.id")
                            .Value(internalFilter.SubstrateId.Value)));
                }

                if (internalFilter.BiotopeId.HasValue)
                {
                    queryInternal.Add(q => q
                        .Term(m => m
                            .Field("event.biotope.id")
                            .Value(internalFilter.BiotopeId.Value)));
                }

                switch (internalFilter.NotPresentFilter)
                {
                    case SearchFilterInternal.SightingNotPresentFilter.DontIncludeNotPresent:
                        queryInternal.Add(q => q
                            .Term(m => m
                                .Field("occurrence.isNeverFoundObservation")
                                .Value(false)));
                        break;
                    case SearchFilterInternal.SightingNotPresentFilter.OnlyNotPresent:
                        queryInternal.Add(q => q
                            .Term(m => m
                                .Field("occurrence.isNeverFoundObservation")
                                .Value(true)));
                        break;
                }

                if (internalFilter.OnlySecondHandInformation)
                {
                    queryInternal.Add(q=>q
                        .Wildcard(w=>w
                            .Field("occurrence.recordedBy")
                            .Value("Via*")));

                    queryInternal.Add(q=>q
                        .Script(s=>s
                            .Script(sc=>sc
                                .Source("doc['reportedByUserId'].value ==  doc['occurrence.recordedByInternal.id'].value")
                            )
                        )
                    );
                }

                if (internalFilter.RegionalSightingStateIdsFilter?.Any() ?? false)
                {
                    queryInternal.Add(q => q
                        .Terms(t => t
                            .Field("occurrence.regionalSightingStateId")
                            .Terms(internalFilter.RegionalSightingStateIdsFilter)
                        )
                    );
                }

                if (internalFilter.PublishTypeIdsFilter?.Any() ?? false)
                {
                    queryInternal.Add(q => q
                        .Terms(t => t
                            .Field("occurrence.sightingPublishTypeIds")
                            .Terms(internalFilter.PublishTypeIdsFilter)
                        )
                    );
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

        private static void AddNumericFilterWithRelationalOperator(
            ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> queryInternal, string fieldName,
            int value, string relationalOperator)
        {
            switch (relationalOperator.ToLower())
            {
                case "eq":
                    queryInternal.Add(q => q
                        .Term(r => r
                            .Field(fieldName)
                            .Value(value)
                        )
                    );
                    break;
                case "gte":
                    queryInternal.Add(q => q
                        .Range(r => r
                            .Field(fieldName)
                            .GreaterThanOrEquals(value)
                        )
                    );
                    break;
                case "lte":
                    queryInternal.Add(q => q
                        .Range(r => r
                            .Field(fieldName)
                            .LessThanOrEquals(value)
                        )
                    );
                    break;
            }
        }

        public static List<Func<QueryContainerDescriptor<object>, QueryContainer>> AddExcludeFilters(
            SearchFilter filter, List<Func<QueryContainerDescriptor<object>, QueryContainer>> excludeQuery)
        {
            var excludeQueryInternal = excludeQuery.ToList();
            if (filter is SearchFilterInternal)
            {
                var internalFilter = filter as SearchFilterInternal;

                if (internalFilter.ExcludeValidationStatusIds?.Any() ?? false)
                {
                    excludeQueryInternal.Add(q => q
                        .Terms(t => t
                            .Field("identification.validationStatus.id")
                            .Terms(internalFilter.ExcludeValidationStatusIds)
                        )
                    );
                }
            }

            return excludeQueryInternal;
        }
    }
}