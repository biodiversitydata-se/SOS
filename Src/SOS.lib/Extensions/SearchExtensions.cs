using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nest;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;

namespace SOS.Lib.Extensions
{
    public static class SearchExtensions
    {
        private static void AddDateRangeFilters(FilterBase filter, List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> queryContainers)
        {
            if (filter.DateFilterType == FilterBase.DateRangeFilterType.BetweenStartDateAndEndDate)
            {
                if (filter.StartDate.HasValue)
                {
                    queryContainers.Add(q => q
                        .DateRange(r => r
                                .Field("event.startDate")
                                .GreaterThanOrEquals(
                                    DateMath.Anchored(filter.StartDate.Value
                                        .ToUniversalTime())) //.RoundTo(DateMathTimeUnit.Day))
                        )
                    );
                }

                if (filter.EndDate.HasValue)
                {
                    var endDate = filter.EndDate.Value;
                    if (endDate.Hour == 0 && endDate.Minute == 0 && endDate.Second == 0)
                    {
                        //Assume whole day search if time is 00:00:00
                        endDate = endDate.AddDays(1).AddSeconds(-1);
                    }
                    queryContainers.Add(q => q
                        .DateRange(r => r
                                .Field("event.endDate")
                                .LessThanOrEquals(
                                    DateMath.Anchored(endDate
                                        .ToUniversalTime())) //.RoundTo(DateMathTimeUnit.Day))
                        )
                    );
                }
            }
            else if (filter.DateFilterType == FilterBase.DateRangeFilterType.OverlappingStartDateAndEndDate)
            {
                if (filter.EndDate.HasValue)
                {
                    queryContainers.Add(q => q
                        .DateRange(r => r
                                .Field("event.startDate")
                                .LessThanOrEquals(
                                    DateMath.Anchored(filter.EndDate.Value
                                        .ToUniversalTime())) //.RoundTo(DateMathTimeUnit.Day))
                        )
                    );
                }

                if (filter.StartDate.HasValue)
                {
                    queryContainers.Add(q => q
                        .DateRange(r => r
                                .Field("event.endDate")
                                .GreaterThanOrEquals(
                                    DateMath.Anchored(filter.StartDate.Value
                                        .ToUniversalTime())) //.RoundTo(DateMathTimeUnit.Day))
                        )
                    );
                }
            }
            else if (filter.DateFilterType == FilterBase.DateRangeFilterType.OnlyStartDate)
            {
                if (filter.StartDate.HasValue && filter.EndDate.HasValue)
                {
                    queryContainers.Add(q => q
                       .DateRange(r => r
                               .Field("event.startDate")
                               .GreaterThanOrEquals(
                                   DateMath.Anchored(filter.StartDate.Value
                                       .ToUniversalTime())) //.RoundTo(DateMathTimeUnit.Day))
                       )
                   );
                    queryContainers.Add(q => q
                        .DateRange(r => r
                                .Field("event.startDate")
                                .LessThanOrEquals(
                                    DateMath.Anchored(filter.EndDate.Value
                                        .ToUniversalTime())) //.RoundTo(DateMathTimeUnit.Day))
                        )
                    );
                }
            }
            else if (filter.DateFilterType == FilterBase.DateRangeFilterType.OnlyEndDate)
            {
                if (filter.StartDate.HasValue && filter.EndDate.HasValue)
                {
                    queryContainers.Add(q => q
                       .DateRange(r => r
                               .Field("event.endDate")
                               .GreaterThanOrEquals(
                                   DateMath.Anchored(filter.StartDate.Value
                                       .ToUniversalTime())) //.RoundTo(DateMathTimeUnit.Day))
                       )
                   );
                    queryContainers.Add(q => q
                        .DateRange(r => r
                                .Field("event.endDate")
                                .LessThanOrEquals(
                                    DateMath.Anchored(filter.EndDate.Value
                                        .ToUniversalTime())) //.RoundTo(DateMathTimeUnit.Day))
                        )
                    );
                }
            }
        }

        /// <summary>
        /// Add internal filters to query
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private static ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> AddInternalFilters(
            FilterBase filter, ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query)
        {
            var queryInternal = query.ToList();

            var internalFilter = filter as SearchFilterInternal;

            if (internalFilter.ProjectIds?.Any() ?? false)
            {
                queryInternal.Add(q => q
                    .Nested(n => n
                        .Path("artportalenInternal.projects")
                        .Query(q => q
                            .Terms(t => t
                                .Field("artportalenInternal.projects.id")
                                .Terms(internalFilter.ProjectIds)
                            )
                        )));
            }

            if (internalFilter.ReportedByUserId.HasValue)
            {
                queryInternal.Add(q => q
                    .Terms(t => t
                        .Field(new Field("artportalenInternal.reportedByUserId"))
                        .Terms(internalFilter.ReportedByUserId)
                    )
                );
            }

            if (internalFilter.ObservedByUserId.HasValue)
            {
                queryInternal.Add(q => q
                    .Terms(t => t
                        .Field(new Field("artportalenInternal.occurrenceRecordedByInternal.id"))
                        .Terms(internalFilter.ObservedByUserId)
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
                        .Field("artportalenInternal.noteOfInterest")
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
                        .Field("artportalenInternal.hasTriggeredValidationRules")
                        .Value(true)));
            }

            if (internalFilter.HasTriggerdValidationRuleWithWarning)
            {
                queryInternal.Add(q => q
                    .Term(m => m
                        .Field("artportalenInternal.hasAnyTriggeredValidationRuleWithWarning")
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
                        .Field("occurrence.publicCollection.keyword")
                        .Value(internalFilter.PublicCollection)));
            }

            if (!string.IsNullOrEmpty(internalFilter.PrivateCollection))
            {
                queryInternal.Add(q => q
                    .Term(m => m
                        .Field("artportalenInternal.privateCollection.keyword")
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
                queryInternal.Add(q => q
                    .Wildcard(w => w
                        .Field("occurrence.recordedBy")
                        .Value("Via*")));

                queryInternal.Add(q => q
                    .Script(s => s
                        .Script(sc => sc
                            .Source("doc['reportedByUserId'].value ==  doc['occurrence.recordedByInternal.id'].value")
                        )
                    )
                );
            }

            if (internalFilter.RegionalSightingStateIdsFilter?.Any() ?? false)
            {
                queryInternal.Add(q => q
                    .Terms(t => t
                        .Field("artportalenInternal.regionalSightingStateId")
                        .Terms(internalFilter.RegionalSightingStateIdsFilter)
                    )
                );
            }

            if (internalFilter.PublishTypeIdsFilter?.Any() ?? false)
            {
                queryInternal.Add(q => q
                    .Terms(t => t
                        .Field("artportalenInternal.sightingPublishTypeIds")
                        .Terms(internalFilter.PublishTypeIdsFilter)
                    )
                );
            }

            if (internalFilter.SiteIds?.Any() ?? false)
            {
                queryInternal.Add(q => q
                    .Terms(t => t
                        .Field("location.locationId")
                        .Terms(internalFilter.SiteIds.Select(s => $"urn:lsid:artportalen.se:site:{s}"))
                    )
                );
            }

            if (internalFilter.SpeciesFactsIds?.Any() ?? false)
            {
                foreach (var factsId in internalFilter.SpeciesFactsIds)
                {
                    queryInternal.Add(q => q
                        .Term(t => t
                            .Field("artportalenInternal.speciesFactsIds")
                            .Value(factsId)
                        )
                    );
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

            return queryInternal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="excludeQuery"></param>
        /// <returns></returns>
        private static List<Func<QueryContainerDescriptor<object>, QueryContainer>> AddInternalExcludeFilters(
            FilterBase filter, List<Func<QueryContainerDescriptor<object>, QueryContainer>> excludeQuery)
        {
            var excludeQueryInternal = excludeQuery.ToList();
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
       

            return excludeQueryInternal;
        }

        /// <summary>
        /// Add numeric filter with relation operator
        /// </summary>
        /// <param name="queryInternal"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <param name="relationalOperator"></param>
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

        private static ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> AddSightingTypeFilters(FilterBase filter, ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query)
        {
            var sightingTypeQuery = new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();

            // Default DoNotShowMerged
            var sightingTypeSearchGroupFilter = new[] { 0, 1, 4, 16, 32, 128 };

            if (filter.TypeFilter == SearchFilterInternal.SightingTypeFilter.ShowBoth)
            {
                sightingTypeSearchGroupFilter = new[] { 0, 1, 2, 4, 16, 32, 128 };
            }
            else if (filter.TypeFilter == SearchFilterInternal.SightingTypeFilter.ShowOnlyMerged)
            {
                sightingTypeSearchGroupFilter = new[] { 0, 2 };
            }
            else if (filter.TypeFilter == SearchFilterInternal.SightingTypeFilter.DoNotShowSightingsInMerged)
            {
                sightingTypeSearchGroupFilter = new[] { 0, 1, 2, 4, 32, 128 };
            }

            sightingTypeQuery.Add(q => q
                .Terms(t => t
                    .Field("artportalenInternal.sightingTypeSearchGroupId")
                    .Terms(sightingTypeSearchGroupFilter)
                )
            );

            if (filter.TypeFilter != SearchFilterInternal.SightingTypeFilter.ShowOnlyMerged)
            {
                // Get observations from other than Artportalen too
                sightingTypeQuery.Add(q => q
                        !.Exists(e => e.Field("artportalenInternal.sightingTypeSearchGroupId"))
                );
            }

            query.Add(q => q
                .Bool(b => b
                    .Should(sightingTypeQuery)
                )
            );

            return query;
        }

        private static ICollection<Func<QueryContainerDescriptor<Observation>, QueryContainer>> CreateTypedQuery(FilterBase filter)
        {
            var queryContainers = new List<Func<QueryContainerDescriptor<Observation>, QueryContainer>>();

            if (filter.BirdValidationAreaIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field(f => f.ArtportalenInternal.BirdValidationAreaIds)
                        .Terms(filter.BirdValidationAreaIds)
                    )
                );
            }

            if (filter.CountyIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field(f => f.Location.County.FeatureId)
                        .Terms(filter.CountyIds)
                    )
                );
            }

            if (filter.GeometryFilter?.IsValid ?? false)
            {
                foreach (var geom in filter.GeometryFilter.Geometries)
                {
                    switch (geom.Type.ToLower())
                    {
                        case "point":
                            queryContainers.Add(q => q
                                .GeoDistance(gd => gd
                                    .Field(f => f.Location.PointLocation)
                                    .DistanceType(GeoDistanceType.Arc)
                                    .Location(geom.ToGeoLocation())
                                    .Distance(filter.GeometryFilter.MaxDistanceFromPoint ?? 0, DistanceUnit.Meters)
                                    .ValidationMethod(GeoValidationMethod.IgnoreMalformed)
                                )
                            );

                            break;
                        case "polygon":
                        case "multipolygon":
                            if (filter.GeometryFilter.UsePointAccuracy)
                            {
                                queryContainers.Add(q => q
                                    .GeoShape(gd => gd
                                        .Field(f => f.Location.PointWithBuffer)
                                        .Shape(s => geom)
                                        .Relation(GeoShapeRelation.Intersects)
                                    )
                                );
                            }
                            else
                            {
                                queryContainers.Add(q => q
                                    .GeoShape(gd => gd
                                        .Field(f => f.Location.Point)
                                        .Shape(s => geom)
                                        .Relation(GeoShapeRelation.Within)
                                    )
                                );
                            }

                            break;
                    }
                }
            }

            if (filter.EndDate.HasValue)
            {
                queryContainers.Add(q => q
                    .DateRange(r => r
                            .Field(f => f.Event.EndDate)
                            .LessThanOrEquals(
                                DateMath.Anchored(filter.EndDate.Value
                                    .ToUniversalTime())) //.RoundTo(DateMathTimeUnit.Day))
                    )
                );
            }

            if (filter.GenderIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field(f => f.Occurrence.Gender.Id)
                        .Terms(filter.GenderIds)
                    )
                );
            }

            if (filter.MunicipalityIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field(f => f.Location.Municipality.FeatureId)
                        .Terms(filter.MunicipalityIds)
                    )
                );
            }

            if (filter.OnlyValidated.HasValue && filter.OnlyValidated.Value.Equals(true))
            {
                queryContainers.Add(q => q
                    .Term(m => m
                        .Field(f => f.Identification.Validated)
                        .Value(true)));
            }

            if (filter.PositiveSightings.HasValue)
            {
                queryContainers.Add(q => q
                    .Term(m => m
                        .Field(f => f.Occurrence.IsPositiveObservation)
                        .Value(filter.PositiveSightings.Value)));
            }

            if (filter.DataProviderIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field(f => f.DataProviderId)
                        .Terms(filter.DataProviderIds)
                    )
                );
            }

            if (filter.ProvinceIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field(f => f.Location.Province.FeatureId)
                        .Terms(filter.ProvinceIds)
                    )
                );
            }

            if (filter.RedListCategories?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field(f => f.Taxon.RedlistCategory)
                        .Terms(filter.RedListCategories)
                    )
                );
            }

            if (filter.StartDate.HasValue)
            {
                queryContainers.Add(q => q
                    .DateRange(r => r
                            .Field(f => f.Event.StartDate)
                            .GreaterThanOrEquals(
                                DateMath.Anchored(filter.StartDate.Value
                                    .ToUniversalTime())) //.RoundTo(DateMathTimeUnit.Day))
                    )
                );
            }

            if (filter.TaxonIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field(f => f.Taxon.Id)
                        .Terms(filter.TaxonIds)
                    )
                );
            }

            return queryContainers;
        }


        private static ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> CreateQuery(FilterBase filter)
        {
            var queryContainers = new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();

            if (filter.BirdValidationAreaIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field("artportalenInternal.birdValidationAreaIds")
                        .Terms(filter.BirdValidationAreaIds)
                    )
                );
            }

            if (filter.CountyIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field("location.county.featureId")
                        .Terms(filter.CountyIds)
                    )
                );
            }

            if (filter.GeometryFilter?.IsValid ?? false)
            {
                var geometryContainers = new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();
                foreach (var geom in filter.GeometryFilter.Geometries)
                {
                    switch (geom.Type.ToLower())
                    {
                        case "point":
                            geometryContainers.Add(q => q
                                .GeoDistance(gd => gd
                                    .Field("location.pointLocation")
                                    .DistanceType(GeoDistanceType.Arc)
                                    .Location(geom.ToGeoLocation())
                                    .Distance(filter.GeometryFilter.MaxDistanceFromPoint ?? 0, DistanceUnit.Meters)
                                    .ValidationMethod(GeoValidationMethod.IgnoreMalformed)
                                )
                            );

                            break;
                        case "polygon":
                        case "multipolygon":
                            if (filter.GeometryFilter.UsePointAccuracy)
                            {
                                geometryContainers.Add(q => q
                                    .GeoShape(gd => gd
                                        .Field("location.pointWithBuffer")
                                        .Shape(s => geom)
                                        .Relation(GeoShapeRelation.Intersects)
                                    )
                                );
                            }
                            else
                            {
                                geometryContainers.Add(q => q
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

                queryContainers.Add(q => q
                    .Bool(b => b
                        .Should(geometryContainers)
                    )
                );
            }

            if (filter.GenderIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field("occurrence.gender.id")
                        .Terms(filter.GenderIds)
                    )
                );
            }

            if (filter.MunicipalityIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field("location.municipality.featureId")
                        .Terms(filter.MunicipalityIds)
                    )
                );
            }

            if (filter.OnlyValidated.HasValue && filter.OnlyValidated.Value.Equals(true))
            {
                queryContainers.Add(q => q
                    .Term(m => m.Field("identification.validated").Value(true)));
            }

            if (filter.PositiveSightings.HasValue)
            {
                queryContainers.Add(q => q
                    .Term(m => m.Field("occurrence.isPositiveObservation").Value(filter.PositiveSightings.Value)));
            }

            if (filter.DataProviderIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field("dataProviderId")
                        .Terms(filter.DataProviderIds)
                    )
                );
            }

            if (filter.ParishIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field("location.parish.featureId")
                        .Terms(filter.ParishIds)
                    )
                );
            }

            if (filter.ProvinceIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field("location.province.featureId")
                        .Terms(filter.ProvinceIds)
                    )
                );
            }

            if (filter.RedListCategories?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field("taxon.redlistCategory")
                        .Terms(filter.RedListCategories.Select(m => m.ToLower()))
                    )
                );
            }

            // If internal filter is "Use Period For All Year" we cannot apply date-range filter.
            if (!(filter is SearchFilterInternal filterInternal && filterInternal.UsePeriodForAllYears))
            {
                AddDateRangeFilters(filter, queryContainers);
            }

            if (filter.TaxonIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field("taxon.id")
                        .Terms(filter.TaxonIds)
                    )
                );
            }

            return queryContainers;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aggregationType"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static void AddAggregationFilter(this ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>> query, AggregationType aggregationType)
        {
            if (aggregationType.IsDateHistogram())
            {
                // Do only include sightings whose period don't exceeds one week/year
                var maxDuration = aggregationType switch
                {
                    AggregationType.QuantityPerWeek => 7,
                    AggregationType.SightingsPerWeek => 7,
                    AggregationType.QuantityPerYear => 365,
                    AggregationType.SightingsPerYear => 365,
                    _ => 365
                };

                query.Add(q => q
                    .Script(s => s
                        .Script(sc => sc
                            .Source($@" (doc['event.endDate'].value.toInstant().toEpochMilli() - doc['event.startDate'].value.toInstant().toEpochMilli()) / 1000 / 86400 < {maxDuration} ")
                        )
                    )
                );
            }

            if (aggregationType.IsSpeciesSightingsList())
            {
                query.Add(q => q
                    .Terms(t => t
                            .Field("artportalenInternal.sightingTypeId")
                            .Terms(new int[] { 0, 1, 3, 8, 10 })    // Got this filter from Artportalen.Infrastructure.Repositories.SearchRepository.cs:7092
                    )
                );
            }
        }

        private static Field ToField(this string property)
        {
            return new Field(string.Join('.', property.Split('.').Select(p => p
                .ToCamelCase()
            )));
        }

        public static ICollection<Func<QueryContainerDescriptor<Observation>, QueryContainer>> ToMultimediaQuery(
            this FilterBase filter)
        {
            var query = CreateTypedQuery(filter);
            query.Add(q => q
                .Nested(n => n
                    .Path(observation => observation.Media)
                    .Query(nq => nq
                        .Exists(e => e
                            .Field(observation => observation.Media))
                    ))
            );

            return query;
        }

        public static ICollection<Func<QueryContainerDescriptor<Observation>, QueryContainer>> ToMeasurementOrFactsQuery(
            this FilterBase filter)
        {
            var query = CreateTypedQuery(filter);
            query.Add(q => q
                .Nested(n => n
                    .Path(observation => observation.MeasurementOrFacts)
                    .Query(nq => nq
                        .Exists(e => e
                            .Field(observation => observation.MeasurementOrFacts))
                    ))
            );

            return query;
        }

        /// <summary>
        ///     Create search filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> ToQuery(
            this FilterBase filter)
        {
            if (!filter.IsFilterActive)
            {
                return new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();
            }

            var query = CreateQuery(filter);
            query = AddSightingTypeFilters(filter, query);

            if (filter is SearchFilterInternal)
            {
                query = AddInternalFilters(filter, query);
            }

            return query;
        }

        /// <summary>
        /// Create a exclude query
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> ToExcludeQuery(this FilterBase filter)
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

            if (filter is SearchFilterInternal)
            {
                queryContainers = AddInternalExcludeFilters(filter, queryContainers);
            }

            return queryContainers;
        }

        public static IEnumerable<Func<QueryContainerDescriptor<Observation>, QueryContainer>> ToTypedObservationQuery(
            this FilterBase filter)
        {
            if (!filter.IsFilterActive)
            {
                return new List<Func<QueryContainerDescriptor<Observation>, QueryContainer>>();
            }

            return CreateTypedQuery(filter);
        }


        /// <summary>
        ///     Build a projection string
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static Func<SourceFilterDescriptor<dynamic>, ISourceFilter> ToProjection(this IEnumerable<string> properties,
            bool isInternal)
        {
            var projection = new SourceFilterDescriptor<dynamic>();
            if (isInternal)
            {
                projection.Excludes(e => e
                    .Field("defects")
                    .Field("occurrence.sightingTypeSearchGroupId")
                    .Field("location.point")
                    .Field("location.pointLocation")
                    .Field("location.pointWithBuffer")
                    .Field("isInEconomicZoneOfSweden"));
            }
            else
            {
                projection.Excludes(e => e
                    .Field("defects")
                    /*.Field("artportalenInternal.reportedByUserAlias")
                    .Field("artportalenInternal.identifiedByInternal")*/
                    .Field("artportalenInternal")
                    .Field("location.point")
                    .Field("location.pointLocation")
                    .Field("location.pointWithBuffer")
                    .Field("location.parentLocationId")
                    .Field("isInEconomicZoneOfSweden")
                );
            }

            if (properties?.Any() ?? false)
            {
                projection.Includes(i => i.Fields(properties.Select(p => p.ToField())));
            }

            return p => projection;
        }

        /// <summary>
        /// Create a sort descriptor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sortBy"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        public static SortDescriptor<dynamic> ToSortDescriptor<T>(this string sortBy, SearchSortOrder sortOrder)
        {
            var sortDescriptor = new SortDescriptor<dynamic>();

            if (!string.IsNullOrEmpty(sortBy))
            {
                // Split sort string 
                var propertyNames = sortBy.Split('.');
                // Create a object of current class
                var parent = Activator.CreateInstance(typeof(T));
                var targetProperty = (PropertyInfo)null;

                // Loop throw all levels in passed sort string
                for (var i = 0; i < propertyNames.Length; i++)
                {
                    // Get property info for current property
                    targetProperty = parent?.GetProperty(propertyNames[i]);

                    // As long it's not the last property, it must be a sub object. Create a instance of it since it's the new parent
                    if (i != propertyNames.Length - 1)
                    {
                        parent = Activator.CreateInstance(targetProperty.GetPropertyType());
                    }
                }

                // Target property found, get it's type
                var propertyType = targetProperty?.GetPropertyType();

                // If it's a string, add keyword in order to make the sorting work
                if (propertyType == typeof(string))
                {
                    //check if the string property already has the keyword attribute, if it does, we do not need the .keyword
                    KeywordAttribute isKeywordAttribute =
                        (KeywordAttribute)Attribute.GetCustomAttribute(targetProperty, typeof(KeywordAttribute));
                    if (isKeywordAttribute == null)
                    {
                        sortBy = $"{sortBy}.keyword";
                    }
                }

                sortDescriptor.Field(sortBy,
                    sortOrder == SearchSortOrder.Desc ? SortOrder.Descending : SortOrder.Ascending);
            }

            return sortDescriptor;
        }
    }
}