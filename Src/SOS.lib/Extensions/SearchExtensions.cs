using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nest;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search;

namespace SOS.Lib.Extensions
{
    /// <summary>
    /// ElasticSearch query extensions
    /// </summary>
    public static class SearchExtensions
    {
        private enum RangeTypes
        {
            GreaterThan,
            GreaterThanOrEquals,
            LessThan,
            LessThanOrEquals
        }

        /// <summary>
        /// Add date range query to filter
        /// </summary>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        private static void AddDateRangeFilters(this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, FilterBase filter)
        {
            if (filter.DateFilterType == FilterBase.DateRangeFilterType.BetweenStartDateAndEndDate)
            {
                query.TryAddDateRangeCriteria("event.startDate", filter.StartDate, RangeTypes.GreaterThanOrEquals);
                query.TryAddDateRangeCriteria("event.endDate", filter.EndDate, RangeTypes.LessThanOrEquals);
            }
            else if (filter.DateFilterType == FilterBase.DateRangeFilterType.OverlappingStartDateAndEndDate)
            {
                query.TryAddDateRangeCriteria("event.startDate", filter.EndDate, RangeTypes.LessThanOrEquals);
                query.TryAddDateRangeCriteria("event.endDate", filter.StartDate, RangeTypes.GreaterThanOrEquals);

            }
            else if (filter.DateFilterType == FilterBase.DateRangeFilterType.OnlyStartDate)
            {
                if (filter.StartDate.HasValue && filter.EndDate.HasValue)
                {
                    query.TryAddDateRangeCriteria("event.startDate", filter.StartDate, RangeTypes.GreaterThanOrEquals);
                    query.TryAddDateRangeCriteria("event.startDate", filter.EndDate, RangeTypes.LessThanOrEquals);
                }
            }
            else if (filter.DateFilterType == FilterBase.DateRangeFilterType.OnlyEndDate)
            {
                if (filter.StartDate.HasValue && filter.EndDate.HasValue)
                {
                    query.TryAddDateRangeCriteria("event.endDate", filter.StartDate, RangeTypes.GreaterThanOrEquals);
                    query.TryAddDateRangeCriteria("event.endDate", filter.EndDate, RangeTypes.LessThanOrEquals);
                }
            }
        }

        /// <summary>
        /// Add geo shape criteria
        /// </summary>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="geometry"></param>
        /// <param name="relation"></param>
        private static void AddGeoShapeCriteria(this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, string field, IGeoShape geometry, GeoShapeRelation relation)
        {
            query.Add(q => q
                .GeoShape(gd => gd
                    .Field(field)
                    .Shape(s => geometry)
                    .Relation(relation)
                )
            );
        }

        /// <summary>
        /// Add geometry filter to query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        private static void AddGeometryFilters(
            this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query,
            FilterBase filter)
        {
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
                                geometryContainers.AddGeoShapeCriteria("location.pointWithBuffer", geom, GeoShapeRelation.Intersects);
                            }
                            else
                            {
                                geometryContainers.AddGeoShapeCriteria("location.point", geom, GeoShapeRelation.Within);
                            }

                            break;
                    }
                }

                query.Add(q => q
                    .Bool(b => b
                        .Should(geometryContainers)
                    )
                );
            }
        }

        /// <summary>
        /// Add internal filters to query
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private static void AddInternalFilters(this
            ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, FilterBase filter)
        {
            var internalFilter = filter as SearchFilterInternal;

            if (internalFilter.ProjectIds?.Any() ?? false)
            {
                query.Add(q => q
                    .Nested(n => n
                        .Path("artportalenInternal.projects")
                        .Query(q => q
                            .Terms(t => t
                                .Field("artportalenInternal.projects.id")
                                .Terms(internalFilter.ProjectIds)
                            )
                        )));
            }

            query.TryAddTermCriteria("artportalenInternal.reportedByUserId", internalFilter.ReportedByUserId);
            query.TryAddTermCriteria("artportalenInternal.occurrenceRecordedByInternal.id", internalFilter.ObservedByUserId);


            if (internalFilter.BoundingBox != null)
            {
                query.Add(q => q
                    .GeoBoundingBox(g => g
                        .Field(new Field("location.pointLocation"))
                        .BoundingBox(internalFilter.BoundingBox[1],
                            internalFilter.BoundingBox[0],
                            internalFilter.BoundingBox[3],
                            internalFilter.BoundingBox[2])));
            }
            if (internalFilter.OnlyWithMedia)
            {
                query.AddMustExistsCriteria("occurrence.associatedMedia");
            //    query.AddWildcardCriteria("occurrence.associatedMedia", "http*");
            }

            if (internalFilter.OnlyWithNotes)
            {
                query.AddMustExistsCriteria("occurrence.occurrenceRemarks");
              //  query.AddWildcardCriteria("occurrence.occurrenceRemarks", "?*");
            }

            query.TryAddTermCriteria("artportalenInternal.noteOfInterest", internalFilter.OnlyWithNotesOfInterest, true);
            query.TryAddDateRangeCriteria("reportedDate", internalFilter.ReportedDateFrom, RangeTypes.GreaterThanOrEquals);
            query.TryAddDateRangeCriteria("reportedDate", internalFilter.ReportedDateTo, RangeTypes.LessThanOrEquals);
            query.TryAddNumericRangeCriteria("location.coordinateUncertaintyInMeters", internalFilter.MaxAccuracy, RangeTypes.LessThanOrEquals);

            if (internalFilter.Months?.Any() ?? false)
            {
                query.AddScript($@"return [{string.Join(',', internalFilter.Months.Select(m => $"{m}"))}].contains(doc['event.startDate'].value.getMonthValue());");
            }

            query.TryAddTermsCriteria("occurrence.discoveryMethod.id", internalFilter.DiscoveryMethodIds);
            query.TryAddTermsCriteria("occurrence.lifeStage.id", internalFilter.LifeStageIds);
            query.TryAddTermsCriteria("occurrence.activity.id", internalFilter.ActivityIds);

            query.TryAddTermCriteria("artportalenInternal.hasTriggeredValidationRules", internalFilter.HasTriggerdValidationRule, true);
            query.TryAddTermCriteria("artportalenInternal.hasAnyTriggeredValidationRuleWithWarning", internalFilter.HasTriggerdValidationRuleWithWarning, true);


            if (internalFilter.Length.HasValue && !string.IsNullOrWhiteSpace(internalFilter.LengthOperator))
            {
                AddNumericFilterWithRelationalOperator(query, "occurrence.length", internalFilter.Length.Value, internalFilter.LengthOperator);
            }

            if (internalFilter.Weight.HasValue && !string.IsNullOrWhiteSpace(internalFilter.WeightOperator))
            {
                AddNumericFilterWithRelationalOperator(query, "occurrence.weight", internalFilter.Weight.Value, internalFilter.WeightOperator);
            }

            if (internalFilter.Quantity.HasValue && !string.IsNullOrWhiteSpace(internalFilter.QuantityOperator))
            {
                AddNumericFilterWithRelationalOperator(query, "occurrence.organismQuantityInt", internalFilter.Quantity.Value, internalFilter.QuantityOperator);
            }

            query.TryAddTermsCriteria("identification.validationStatus.id", internalFilter.ValidationStatusIds);

            if (internalFilter.OnlyWithBarcode)
            {
                query.AddMustExistsCriteria("taxon.individualId");
               // query.AddWildcardCriteria("taxon.individualId", "?*");
            }

            switch (internalFilter.DeterminationFilter)
            {
                case SightingDeterminationFilter.NotUnsureDetermination:
                    query.TryAddTermCriteria("identification.uncertainDetermination", false);
                    break;
                case SightingDeterminationFilter.OnlyUnsureDetermination:
                    query.TryAddTermCriteria("identification.uncertainDetermination", true);
                    break;
            }

            switch (internalFilter.UnspontaneousFilter)
            {
                case SightingUnspontaneousFilter.NotUnspontaneous:
                    query.TryAddTermCriteria("occurrence.isNaturalOccurrence", true);
                    break;
                case SightingUnspontaneousFilter.Unspontaneous:
                    query.TryAddTermCriteria("occurrence.isNaturalOccurrence", false);
                    break;
            }

            switch (internalFilter.NotRecoveredFilter)
            {
                case SightingNotRecoveredFilter.DontIncludeNotRecovered:
                    query.TryAddTermCriteria("occurrence.isNotRediscoveredObservation", false);
                    break;
                case SightingNotRecoveredFilter.OnlyNotRecovered:
                    query.TryAddTermCriteria("occurrence.isNotRediscoveredObservation", true);
                    break;
            }

            query.TryAddTermCriteria("collectionId.keyword", internalFilter.SpeciesCollectionLabel);
            query.TryAddTermCriteria("occurrence.publicCollection.keyword", internalFilter.PublicCollection);
            query.TryAddTermCriteria("artportalenInternal.privateCollection.keyword", internalFilter.PrivateCollection);
            query.TryAddTermCriteria("event.substrateSpeciesId", internalFilter.SubstrateSpeciesId);
            query.TryAddTermCriteria("event.substrate.id", internalFilter.SubstrateId);
            query.TryAddTermCriteria("event.biotope.id", internalFilter.BiotopeId);


            switch (internalFilter.NotPresentFilter)
            {
                case SightingNotPresentFilter.DontIncludeNotPresent:
                    query.TryAddTermCriteria("occurrence.isNeverFoundObservation", false);
                    break;
                case SightingNotPresentFilter.OnlyNotPresent:
                    query.TryAddTermCriteria("occurrence.isNeverFoundObservation", true);
                    break;
            }

            if (internalFilter.OnlySecondHandInformation)
            {
                query.AddWildcardCriteria("occurrence.recordedBy", "Via*");
                query.AddScript("doc['reportedByUserId'].value ==  doc['occurrence.recordedByInternal.id'].value");
            }

            query.TryAddTermsCriteria("artportalenInternal.regionalSightingStateId", internalFilter.RegionalSightingStateIdsFilter);
            query.TryAddTermsCriteria("artportalenInternal.sightingPublishTypeIds", internalFilter.PublishTypeIdsFilter);
            query.TryAddTermsCriteria("location.locationId", internalFilter?.SiteIds?.Select(s => $"urn:lsid:artportalen.se:site:{s}"));



            if (internalFilter.SpeciesFactsIds?.Any() ?? false)
            {
                foreach (var factsId in internalFilter.SpeciesFactsIds)
                {
                    query.TryAddTermCriteria("artportalenInternal.speciesFactsIds", factsId);
                }
            }

            if (internalFilter.UsePeriodForAllYears && internalFilter.StartDate.HasValue && internalFilter.EndDate.HasValue)
            {
                query.AddScript($@"
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
                ");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="excludeQuery"></param>
        /// <returns></returns>
        private static void AddInternalExcludeFilters(this
            ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> excludeQuery, FilterBase filter)
        {
            var internalFilter = filter as SearchFilterInternal;

            excludeQuery.TryAddTermsCriteria("identification.validationStatus.id", internalFilter.ExcludeValidationStatusIds);
        }

        private static void AddMustExistsCriteria(
            this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, string field)
        {
            query.Add(q => q
                .Regexp(re => re.Field(field).Value(".+"))
            );
        }

        /// <summary>
        /// Add numeric filter with relation operator
        /// </summary>
        /// <param name="queryInternal"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <param name="relationalOperator"></param>
        private static void AddNumericFilterWithRelationalOperator(this
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

        /// <summary>
        /// Add script source
        /// </summary>
        /// <param name="query"></param>
        /// <param name="source"></param>
        private static void AddScript(this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, string source)
        {
            query.Add(q => q
                .Script(s => s
                    .Script(sc => sc
                        .Source(source)
                    )
                )
            );
        }

        private static void AddSightingTypeFilters(this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, FilterBase filter)
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

            sightingTypeQuery.TryAddTermsCriteria("artportalenInternal.sightingTypeSearchGroupId", sightingTypeSearchGroupFilter);

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
        }

        /// <summary>
        /// Add wildcard criteria
        /// </summary>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="wildcard"></param>
        private static void AddWildcardCriteria(this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, string field, string wildcard)
        {
            query.Add(q => q
                .Wildcard(w => w
                    .Field(field)
                    .Value(wildcard)));
        }

        private static Field ToField(this string property)
        {
            return new Field(string.Join('.', property.Split('.').Select(p => p
                .ToCamelCase()
            )));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="dateTime"></param>
        /// <param name="type"></param>
        private static void TryAddDateRangeCriteria(this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, string field, DateTime? dateTime, RangeTypes type)
        {
            if (dateTime.HasValue)
            {
                switch (type)
                {
                    case RangeTypes.GreaterThan:
                        query.Add(q => q
                            .DateRange(r => r
                                .Field(field)
                                .GreaterThan(
                                    DateMath.Anchored(
                                        dateTime.Value.ToUniversalTime()
                                    )
                                )
                            )
                        );
                        break;
                    case RangeTypes.GreaterThanOrEquals:
                        query.Add(q => q
                            .DateRange(r => r
                                .Field(field)
                                .GreaterThanOrEquals(
                                    DateMath.Anchored(
                                        dateTime.Value.ToUniversalTime()
                                    )
                                )
                            )
                        );
                        break;
                    case RangeTypes.LessThan:
                        query.Add(q => q
                            .DateRange(r => r
                                .Field(field)
                                .LessThan(
                                    DateMath.Anchored(
                                        dateTime.Value.ToUniversalTime()
                                    )
                                )
                            )
                        );
                        break;
                    case RangeTypes.LessThanOrEquals:
                        query.Add(q => q
                            .DateRange(r => r
                                .Field(field)
                                .LessThanOrEquals(
                                    DateMath.Anchored(
                                        dateTime.Value.ToUniversalTime()
                                    )
                                )
                            )
                        );
                        break;
                }
            }

        }

        /// <summary>
        /// Add numeric range criteria if value is not null 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        private static void TryAddNumericRangeCriteria(this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, string field, double? value, RangeTypes type)
        {
            if (value.HasValue)
            {
                switch (type)
                {
                    case RangeTypes.GreaterThan:
                        query.Add(q => q
                            .Range(r => r
                                .Field(field)
                                .GreaterThan(value)
                            )
                        );
                        break;
                    case RangeTypes.GreaterThanOrEquals:
                        query.Add(q => q
                            .Range(r => r
                                .Field(field)
                                .GreaterThanOrEquals(value)
                            )
                        );
                        break;
                    case RangeTypes.LessThan:
                        query.Add(q => q
                            .Range(r => r
                                .Field(field)
                                .LessThan(value)
                            )
                        );
                        break;
                    case RangeTypes.LessThanOrEquals:
                        query.Add(q => q
                            .Range(r => r
                                .Field(field)
                                .LessThanOrEquals(value)
                            )
                        );
                        break;
                }
            }
        }

        /// <summary>
        /// Try to add query criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="terms"></param>
        private static void TryAddTermsCriteria<T>(
                this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, string field, IEnumerable<T> terms)
        {
            if (terms?.Any() ?? false)
            {
                query.Add(q => q
                    .Terms(t => t
                        .Field(field)
                        .Terms(terms)
                    )
                );
            }
        }

        /// <summary>
        /// Try to add query criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        private static void TryAddTermCriteria<T>(
            this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, string field, T value)
        {
            if (!string.IsNullOrEmpty(value?.ToString()))
            {
                query.Add(q => q
                    .Term(m => m.Field(field).Value(value))); // new Field(field)
            }
        }

        /// <summary>
        /// Try to add query criteria where property must match a specified value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="matchValue"></param>
        private static void TryAddTermCriteria<T>(
            this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, string field, T value, T matchValue)
        {
            if (!string.IsNullOrEmpty(value?.ToString()) && matchValue.Equals(value))
            {
                query.Add(q => q
                    .Term(m => m.Field(field).Value(value)));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aggregationType"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static void AddAggregationFilter(this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, AggregationType aggregationType)
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

                query.AddScript($@" (doc['event.endDate'].value.toInstant().toEpochMilli() - doc['event.startDate'].value.toInstant().toEpochMilli()) / 1000 / 86400 < {maxDuration} ");
            }

            if (aggregationType.IsSpeciesSightingsList())
            {
                query.TryAddTermsCriteria("artportalenInternal.sightingTypeId", new[] { 0, 1, 3, 8, 10 });
            }
        }

        /// <summary>
        /// Create multimedia query
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> ToMultimediaQuery(
            this FilterBase filter)
        {
            var query = filter.ToQuery();
            query.Add(q => q
                .Nested(n => n
                    .Path("media")
                    .Query(nq => nq
                        .Exists(e => e
                            .Field("media"))
                    ))
            );

            return query;
        }

        public static ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> ToMeasurementOrFactsQuery(
            this FilterBase filter)
        {
            var query = filter.ToQuery();
            query.Add(q => q
                .Nested(n => n
                    .Path("measurementOrFacts")
                    .Query(nq => nq
                        .Exists(e => e
                            .Field("measurementOrFacts"))
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
            var query = new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();

            query.TryAddTermsCriteria("artportalenInternal.birdValidationAreaIds", filter.BirdValidationAreaIds);
            query.TryAddTermsCriteria("location.county.featureId", filter.CountyIds);
            query.TryAddTermsCriteria("dataProviderId", filter.DataProviderIds);
            query.TryAddTermsCriteria("occurrence.gender.id", filter.GenderIds);
            query.TryAddTermsCriteria("location.municipality.featureId", filter.MunicipalityIds);
            query.TryAddTermCriteria("identification.validated", filter.OnlyValidated, true);
            query.TryAddTermCriteria("occurrence.isPositiveObservation", filter.PositiveSightings);
            query.TryAddTermsCriteria("location.parish.featureId", filter.ParishIds);
            query.TryAddTermsCriteria("location.province.featureId", filter.ProvinceIds);
            query.TryAddTermsCriteria("taxon.redlistCategory", filter.RedListCategories?.Select(m => m.ToLower()));
            query.TryAddTermsCriteria("taxon.id", filter.TaxonIds);

            // If internal filter is "Use Period For All Year" we cannot apply date-range filter.
            if (!(filter is SearchFilterInternal filterInternal && filterInternal.UsePeriodForAllYears))
            {
                query.AddDateRangeFilters(filter);
            }

            query.AddGeometryFilters(filter);
            query.AddSightingTypeFilters(filter);

            if (filter is SearchFilterInternal)
            {
                query.AddInternalFilters(filter);
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
            var query = new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();

            if (filter.GeometryFilter?.IsValid ?? false)
            {
                foreach (var geom in filter.GeometryFilter.Geometries)
                {
                    switch (geom.Type.ToLower())
                    {
                        case "holepolygon":
                            if (filter.GeometryFilter.UsePointAccuracy)
                            {
                                query.AddGeoShapeCriteria("location.pointWithBuffer", geom, GeoShapeRelation.Intersects);
                            }
                            else
                            {
                                query.AddGeoShapeCriteria("location.point", geom, GeoShapeRelation.Within);
                            }

                            break;
                    }
                }
            }

            if (filter is SearchFilterInternal)
            {
                query.AddInternalExcludeFilters(filter);
            }

            return query;
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