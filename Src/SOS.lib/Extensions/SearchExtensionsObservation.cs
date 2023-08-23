using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using SOS.Lib.Enums;
using SOS.Lib.Enums.Artportalen;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Search.Filters;
using static SOS.Lib.Extensions.SearchExtensionsGeneric;

namespace SOS.Lib
{
    /// <summary>
    /// Observation specific search related extensions
    /// </summary>
    public static class SearchExtensionsObservation
    {
        /// <summary>
        /// Add filter to limit response to only show observations user is allowed to see
        /// </summary>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        private static void AddAuthorizationFilters(this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, ExtendedAuthorizationFilter filter)
        {
            if (filter.ReportedByMe)
            {
                query.TryAddTermCriteria("artportalenInternal.reportedByUserServiceUserId",
                    filter.UserId);
            }

            if (filter.ObservedByMe)
            {
                query.TryAddNestedTermAndCriteria("artportalenInternal.occurrenceRecordedByInternal", new Dictionary<string, object> { 
                    { "userServiceUserId", filter.UserId },
                    { "viewAccess", true }
                });
            }

            if (filter.ProtectionFilter.Equals(ProtectionFilter.Public))
            {
                // Just to be sure since we can query both public and protected index... Only public observations
                query.TryAddTermCriteria("sensitive", false);
                return;
            }

            // A observation can exists in both public (as diffused) and in protected (as not diffused) index.
            // If we only get non diffused observations, we make sure we don't get a observation twice when searching both indexes
            query.TryAddTermCriteria("diffusionStatus", 0);

            // At least on of the sub queries in authorized querys must match
            var authorizeQuerys = new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();

            // Match all public observations
            var publicQuery = new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();
            publicQuery.TryAddTermCriteria("sensitive", false);
            authorizeQuerys.Add(q => q
                .Bool(b => b
                    .Filter(publicQuery)
                )
            );

            // Match user specific areas and taxa
            if (filter.ExtendedAreas?.Any() ?? false)
            {
                foreach (var extendedAuthorization in filter.ExtendedAreas)
                {
                    var protectedQuery = new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();
                    protectedQuery.TryAddTermCriteria("sensitive", true);
                    protectedQuery.TryAddNumericRangeCriteria("occurrence.sensitivityCategory", extendedAuthorization.MaxProtectionLevel, SearchExtensionsGeneric.RangeTypes.LessThanOrEquals);
                    protectedQuery.TryAddTermsCriteria("taxon.id", extendedAuthorization.TaxonIds);
                    TryAddGeographicalAreaFilter(protectedQuery, extendedAuthorization.GeographicAreas);

                    authorizeQuerys.Add(q => q
                        .Bool(b => b
                            .Filter(protectedQuery)
                        )
                    );
                }
            }

            // Match observations sighted or reported by requesting user
            if (filter.UserId != 0)
            {
                // Add autorization to a users 'own' observations 
                var observedByMeQuery = new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();
                observedByMeQuery.TryAddTermCriteria("artportalenInternal.reportedByUserServiceUserId",
                    filter.UserId);

                authorizeQuerys.Add(q => q
                    .Bool(b => b
                        .Filter(observedByMeQuery)
                    )
                );

                var reportedByMeQuery = new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();
                reportedByMeQuery.TryAddNestedTermAndCriteria("artportalenInternal.occurrenceRecordedByInternal", new Dictionary<string, object> {
                    { "userServiceUserId", filter.UserId },
                    { "viewAccess", true }
                });

                authorizeQuerys.Add(q => q
                    .Bool(b => b
                        .Filter(reportedByMeQuery)
                    )
                );
            }

            query.Add(q => q
                .Bool(b => b
                    .Should(authorizeQuerys)
                )
            );  
        }


        /// <summary>
        /// Add internal filters to query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private static IEnumerable<int> AddInternalFilters(this
            ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, SearchFilterBase filter)
        {
            var internalFilter = filter as SearchFilterInternal;

            query.TryAddTermCriteria("artportalenInternal.checklistId", internalFilter.ChecklistId);
            query.TryAddTermsCriteria("artportalenInternal.fieldDiaryGroupId", internalFilter.FieldDiaryGroupIds);
            query.TryAddTermsCriteria("artportalenInternal.datasourceId", internalFilter.DatasourceIds);
            query.TryAddTermCriteria("artportalenInternal.hasTriggeredVerificationRules", internalFilter.HasTriggeredVerificationRule, true);
            query.TryAddTermCriteria("artportalenInternal.hasAnyTriggeredVerificationRuleWithWarning", internalFilter.HasTriggeredVerificationRuleWithWarning, true);
            query.TryAddTermCriteria("artportalenInternal.hasUserComments", internalFilter.OnlyWithUserComments, true);

            switch (internalFilter.UnspontaneousFilter)
            {
                case SightingUnspontaneousFilter.NotUnspontaneous:
                    query.TryAddTermCriteria("occurrence.isNaturalOccurrence", true);
                    break;
                case SightingUnspontaneousFilter.Unspontaneous:
                    query.TryAddTermCriteria("occurrence.isNaturalOccurrence", false);
                    break;
            }

            query.TryAddTermCriteria("artportalenInternal.noteOfInterest", internalFilter.OnlyWithNotesOfInterest, true);
            query.TryAddNestedTermCriteria("artportalenInternal.occurrenceRecordedByInternal", "id", internalFilter.ObservedByUserId);
            query.TryAddNestedTermCriteria("artportalenInternal.occurrenceRecordedByInternal", "userServiceUserId", internalFilter.ObservedByUserServiceUserId);

            //search by locationId, but include child-locations observations aswell
            var siteTerms = internalFilter?.SiteIds?.Select(s => $"urn:lsid:artportalen.se:site:{s}");
            if (siteTerms?.Any() ?? false)
            {

                query.Add(q => q
                    .Bool(p => p
                        .Should(s => s
                            .Terms(t => t
                                .Field("location.locationId")
                                .Terms(siteTerms)),
                            s => s
                            .Terms(t => t
                                .Field("artportalenInternal.parentLocationId")
                                .Terms(internalFilter.SiteIds))
                             )
                        )
                    );
            }

            query.TryAddTermsCriteria("artportalenInternal.regionalSightingStateId", internalFilter.RegionalSightingStateIdsFilter);
            query.TryAddTermCriteria("artportalenInternal.reportedByUserId", internalFilter.ReportedByUserId);
            query.TryAddTermCriteria("artportalenInternal.reportedByUserServiceUserId", internalFilter.ReportedByUserServiceUserId);

            if (internalFilter.OnlySecondHandInformation)
            {
                query.TryAddTermCriteria("artportalenInternal.secondHandInformation", true);
            }

            if (internalFilter.OnlyWithBarcode)
            {
                query.AddMustExistsCriteria("artportalenInternal.sightingBarcodeURL");
            }

            query.TryAddTermsCriteria("artportalenInternal.sightingPublishTypeIds", internalFilter.PublishTypeIdsFilter);

            if (internalFilter.SpeciesFactsIds?.Any() ?? false)
            {
                foreach (var factsId in internalFilter.SpeciesFactsIds)
                {
                    query.TryAddTermCriteria("artportalenInternal.speciesFactsIds", factsId);
                }
            }

            query.TryAddTermsCriteria("artportalenInternal.triggeredObservationRuleFrequencyId", internalFilter.TriggeredObservationRuleFrequencyIds);
            query.TryAddTermsCriteria("artportalenInternal.triggeredObservationRuleReproductionId", internalFilter.TriggeredObservationRuleReproductionIds);
          
            query.TryAddTermsCriteria("event.discoveryMethod.id", internalFilter.DiscoveryMethodIds);

            query.TryAddTermsCriteria("identification.verificationStatus.id", internalFilter.VerificationStatusIds);
            query.TryAddTermCriteria("institutionId", internalFilter.InstitutionId);

            query.TryAddTermsCriteria("location.attributes.projectId", internalFilter.SiteProjectIds);
            

            query.TryAddTermsCriteria("occurrence.activity.id", internalFilter.ActivityIds);
            
            if (internalFilter.OnlyWithMedia)
            {
                query.AddMustExistsCriteria("occurrence.associatedMedia");
                //    query.TryAddWildcardCriteria("occurrence.associatedMedia", "http*");
            }

            query.TryAddTermCriteria("occurrence.biotope.id", internalFilter.BiotopeId);

            switch (internalFilter.NotPresentFilter)
            {
                case SightingNotPresentFilter.DontIncludeNotPresent:
                    query.TryAddTermCriteria("occurrence.isNeverFoundObservation", false);
                    break;
                case SightingNotPresentFilter.OnlyNotPresent:
                    query.TryAddTermCriteria("occurrence.isNeverFoundObservation", true);
                    break;
            }

            if (internalFilter.Length.HasValue && !string.IsNullOrWhiteSpace(internalFilter.LengthOperator))
            {
                query.AddNumericFilterWithRelationalOperator("occurrence.length", internalFilter.Length.Value, internalFilter.LengthOperator);
            }

            query.TryAddTermsCriteria("occurrence.lifeStage.id", internalFilter.LifeStageIds);

            if (internalFilter.OnlyWithNotes)
            {
                query.AddMustExistsCriteria("occurrence.occurrenceRemarks");
                //  query.TryAddWildcardCriteria("occurrence.occurrenceRemarks", "?*");
            }

            if (internalFilter.Quantity.HasValue && !string.IsNullOrWhiteSpace(internalFilter.QuantityOperator))
            {
                query.AddNumericFilterWithRelationalOperator("occurrence.organismQuantityInt", internalFilter.Quantity.Value, internalFilter.QuantityOperator);
            }

            query.TryAddDateRangeCriteria("occurrence.reportedDate", internalFilter.ReportedDateFrom, SearchExtensionsGeneric.RangeTypes.GreaterThanOrEquals);
            query.TryAddDateRangeCriteria("occurrence.reportedDate", internalFilter.ReportedDateTo, SearchExtensionsGeneric.RangeTypes.LessThanOrEquals);

            query.TryAddTermCriteria("occurrence.substrate.id", internalFilter.SubstrateId);
            query.TryAddTermCriteria("occurrence.substrate.speciesId", internalFilter.SubstrateSpeciesId);
            
            if (internalFilter.Weight.HasValue && !string.IsNullOrWhiteSpace(internalFilter.WeightOperator))
            {
                query.AddNumericFilterWithRelationalOperator("occurrence.weight", internalFilter.Weight.Value, internalFilter.WeightOperator);
            }

            query.TryAddTermCriteria("privateCollection", internalFilter.PrivateCollection);
            query.TryAddTermCriteria("publicCollection", internalFilter.PublicCollection);

            query.TryAddTermCriteria("speciesCollectionLabel", internalFilter.SpeciesCollectionLabel);

            if (internalFilter.Months?.Any() ?? false)
            {
                switch (internalFilter.MonthsComparison)
                {
                    case DateFilterComparison.BothStartDateAndEndDate:
                        query.TryAddTermsCriteria("event.startMonth", internalFilter.Months);
                        query.TryAddTermsCriteria("event.endMonth", internalFilter.Months);
                        break;
                    case DateFilterComparison.EndDate:
                        query.TryAddTermsCriteria("event.endMonth", internalFilter.Months);
                        break;
                    case DateFilterComparison.StartDateEndDateMonthRange:
                        query.TryAddTermsCriteria("artportalenInternal.eventMonths", internalFilter.Months);
                        break;
                    default:
                        query.TryAddTermsCriteria("event.startMonth", internalFilter.Months);
                        break;
                }
            }

            if (internalFilter.Years?.Any() ?? false)
            {
                switch (internalFilter.YearsComparison)
                {
                    case DateFilterComparison.BothStartDateAndEndDate:
                        query.TryAddTermsCriteria("event.startYear", internalFilter.Years);
                        query.TryAddTermsCriteria("event.endYear", internalFilter.Years);
                        break;
                    case DateFilterComparison.EndDate:
                        query.TryAddTermsCriteria("event.endYear", internalFilter.Years);
                        break;
                    default:
                        query.TryAddTermsCriteria("event.startYear", internalFilter.Years);
                        break;
                }
            }

            if (internalFilter.Date != null && internalFilter.UsePeriodForAllYears && internalFilter.Date.StartDate.HasValue && internalFilter.Date.EndDate.HasValue)
            {
                var selector = "";

                if (filter.Date.DateFilterType == DateFilter.DateRangeFilterType.BetweenStartDateAndEndDate)
                {
                    selector = "((startMonth > fromMonth || (startMonth == fromMonth && startDay >= fromDay)) && (endMonth < toMonth || (endMonth == toMonth && endDay <= toDay)))";
                }
                else if (filter.Date.DateFilterType == DateFilter.DateRangeFilterType.OnlyStartDate)
                {
                    selector = "((startMonth > fromMonth || (startMonth == fromMonth && startDay >= fromDay)) && (startMonth < toMonth || (startMonth == toMonth && startDay <= toDay)))";
                }
                else if (filter.Date.DateFilterType == DateFilter.DateRangeFilterType.OnlyEndDate)
                {
                    selector = "((endMonth > fromMonth || (endMonth == fromMonth && endDay >= fromDay)) && (endMonth < toMonth || (endMonth == toMonth && endDay <= toDay)))";
                }
                else if (filter.Date.DateFilterType == DateFilter.DateRangeFilterType.OverlappingStartDateAndEndDate)
                {
                    selector = "((startMonth > fromMonth || (startMonth == fromMonth && startDay >= fromDay)) && (startMonth < toMonth || (startMonth == toMonth && startDay <= toDay))) || " +
                               "((endMonth > fromMonth || (endMonth == fromMonth && endDay >= fromDay)) && (endMonth < toMonth || (endMonth == toMonth && endDay <= toDay)))";
                }

                query.AddScript($@"
                            ZonedDateTime zStartDate = doc['event.startDate'].value;  
                            ZonedDateTime convertedStartDate = zStartDate.withZoneSameInstant(ZoneId.of('Europe/Stockholm'));
                            int startMonth = convertedStartDate.getMonthValue();
                            int startDay = convertedStartDate.getDayOfMonth();
                            
                            ZonedDateTime zEndDate = doc['event.endDate'].value;  
                            ZonedDateTime convertedEndDate = zEndDate.withZoneSameInstant(ZoneId.of('Europe/Stockholm'));
                            int endMonth = convertedEndDate.getMonthValue();
                            int endDay = convertedEndDate.getDayOfMonth();

                            int fromMonth = {internalFilter.Date.StartDate.Value.Month};
                            int fromDay = {internalFilter.Date.StartDate.Value.Day};
                            int toMonth = {internalFilter.Date.EndDate.Value.Month};
                            int toDay = {internalFilter.Date.EndDate.Value.Day};

                            if(
                               {selector}
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

            return internalFilter.SightingTypeSearchGroupIds;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="excludeQuery"></param>
        /// <returns></returns>
        private static void AddInternalExcludeFilters(this
            ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> excludeQuery, SearchFilterBase filter)
        {
            var internalFilter = filter as SearchFilterInternal;

            excludeQuery.TryAddTermsCriteria("identification.verificationStatus.id", internalFilter.ExcludeVerificationStatusIds);
        }

        public static void AddSightingTypeFilters<TQueryContainer>(this ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> query, 
            SearchFilterBase.SightingTypeFilter sightingTypeFilter,
            IEnumerable<int> sightingTypeSearchGroupIds) where TQueryContainer : class
        {
            var sightingTypeQuery = new List<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>>();
            var sightingTypeSearchGroupFilter = sightingTypeSearchGroupIds?.Any() ?? false ?
                    sightingTypeSearchGroupIds
                    :
                    sightingTypeFilter switch
                    {
                        SearchFilterBase.SightingTypeFilter.DoNotShowMergedIncludeReplacementChilds => new[] { // 1, 4, 16, 64, 128
                        (int)SightingTypeSearchGroup.Ordinary,
                        (int)SightingTypeSearchGroup.Aggregated,
                        (int)SightingTypeSearchGroup.AssessmentChild,
                        (int)SightingTypeSearchGroup.ReplacementChild,
                        (int)SightingTypeSearchGroup.OwnBreedingAssessment },
                        SearchFilterBase.SightingTypeFilter.DoNotShowSightingsInMerged => new[] { // 1, 2, 4, 32, 128
                        (int)SightingTypeSearchGroup.Ordinary,
                        (int)SightingTypeSearchGroup.Assessment,
                        (int)SightingTypeSearchGroup.Aggregated,
                        (int)SightingTypeSearchGroup.Replacement,
                        (int)SightingTypeSearchGroup.OwnBreedingAssessment },
                        SearchFilterBase.SightingTypeFilter.ShowBoth => new[] { // 1, 2, 4, 16, 32, 128
                        (int)SightingTypeSearchGroup.Ordinary,
                        (int)SightingTypeSearchGroup.Assessment,
                        (int)SightingTypeSearchGroup.Aggregated,
                        (int)SightingTypeSearchGroup.AssessmentChild,
                        (int)SightingTypeSearchGroup.Replacement,
                        (int)SightingTypeSearchGroup.OwnBreedingAssessment },
                        SearchFilterBase.SightingTypeFilter.ShowOnlyMerged =>
                            new[] { (int)SightingTypeSearchGroup.Assessment }, // 2
                        _ => new[] { // 1, 4, 16, 32, 128 Default DoNotShowMerged
                        (int)SightingTypeSearchGroup.Ordinary,
                        (int)SightingTypeSearchGroup.Aggregated,
                        (int)SightingTypeSearchGroup.AssessmentChild,
                        (int)SightingTypeSearchGroup.Replacement,
                        (int)SightingTypeSearchGroup.OwnBreedingAssessment }
                    };

            sightingTypeQuery.TryAddTermsCriteria("artportalenInternal.sightingTypeSearchGroupId", sightingTypeSearchGroupFilter);

            // If not only Assessment is selected
            if (!(sightingTypeSearchGroupFilter.Count().Equals(1) && sightingTypeSearchGroupFilter.First().Equals(2))) 
            {
                // Get observations from other than Artportalen too
                sightingTypeQuery.AddNotExistsCriteria("artportalenInternal.sightingTypeSearchGroupId");
            }

            query.Add(q => q
                .Bool(b => b
                    .Should(sightingTypeQuery)
                )
            );
        }

        private static void TryAddDataStewardshipFilter(this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, DataStewardshipFilter filter)
        { 
            if (filter == null)
            {
                return;
            }

            query.TryAddTermsCriteria("dataStewardship.datasetIdentifier", filter.DatasetIdentifiers);
        }

        /// <summary>
        /// Add determination filters
        /// </summary>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        private static void TryAddDeterminationFilters(this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, SearchFilterBase filter)
        {
            switch (filter.DeterminationFilter)
            {
                case SightingDeterminationFilter.NotUnsureDetermination:
                    query.TryAddTermCriteria("identification.uncertainIdentification", false);
                    break;
                case SightingDeterminationFilter.OnlyUnsureDetermination:
                    query.TryAddTermCriteria("identification.uncertainIdentification", true);
                    break;
            }
        }

        private static void TryAddEventFilter(this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, EventFilter filter)
        {
            if (filter == null)
            {
                return;
            }

            query.TryAddTermsCriteria("event.eventId", filter.Ids);
        }
       

        /// <summary>
        /// Add geometry filter to query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="geographicsFilter"></param>
        private static void TryAddGeometryFilters(
            this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query,
            GeographicsFilter geographicsFilter)
        {
            if (geographicsFilter == null)
            {
                return;
            }

            var boundingBoxContainers = new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();

            if (!(!geographicsFilter.UsePointAccuracy && geographicsFilter.UseDisturbanceRadius))
            {
                boundingBoxContainers.TryAddBoundingBoxCriteria(
                    $"location.{(geographicsFilter.UsePointAccuracy ? "pointWithBuffer" : "point")}",
                    geographicsFilter.BoundingBox);
            }

            if (geographicsFilter.UseDisturbanceRadius)
            {
                // Add both point and pointWithDisturbanceBuffer, since pointWithDisturbanceBuffer can be null if no dist buffer exists
                boundingBoxContainers.TryAddBoundingBoxCriteria(
                    "location.point",
                    geographicsFilter.BoundingBox);

                boundingBoxContainers.TryAddBoundingBoxCriteria(
                    "location.pointWithDisturbanceBuffer",
                    geographicsFilter.BoundingBox);
            }

            if (boundingBoxContainers.Any())
            {
                query.Add(q => q
                    .Bool(b => b
                        .Should(boundingBoxContainers)
                    )
                );
            }

            if (!geographicsFilter?.IsValid ?? true)
            {
                return;
            }

            var geometryContainers = new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();

            foreach (var geom in geographicsFilter.Geometries)
            {
                switch (geom.Type.ToLower())
                {
                    case "point":
                        geometryContainers.AddGeoDistanceCriteria($"location.{(geographicsFilter.UsePointAccuracy ? "pointWithBuffer" : "point")}", geom, GeoDistanceType.Arc, geographicsFilter.MaxDistanceFromPoint ?? 0);

                        if (!geographicsFilter.UseDisturbanceRadius)
                        {
                            continue;
                        }
                        geometryContainers.AddGeoDistanceCriteria("location.pointWithDisturbanceBuffer", geom, GeoDistanceType.Arc, geographicsFilter.MaxDistanceFromPoint ?? 0);
                        break;
                    case "polygon":
                    case "multipolygon":
                        var vaildGeometry = geom.TryMakeValid();
                        geometryContainers.AddGeoShapeCriteria($"location.{(geographicsFilter.UsePointAccuracy ? "pointWithBuffer" : "point")}", vaildGeometry, geographicsFilter.UsePointAccuracy ? GeoShapeRelation.Intersects : GeoShapeRelation.Within);
                        if (!geographicsFilter.UseDisturbanceRadius)
                        {
                            continue;
                        }
                        geometryContainers.AddGeoShapeCriteria("location.pointWithDisturbanceBuffer", vaildGeometry, GeoShapeRelation.Intersects);
                        break;
                }
            }

            query.Add(q => q
                .Bool(b => b
                    .Should(geometryContainers)
                )
            );
        }

        private static void TryAddModifiedDateFilter(this
                ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, ModifiedDateFilter filter)
        {
            if (filter == null)
            {
                return;
            }

            query.TryAddDateRangeCriteria("modified", filter.From, RangeTypes.GreaterThanOrEquals);
            query.TryAddDateRangeCriteria("modified", filter.To, RangeTypes.LessThanOrEquals);
        }

        /// <summary>
        /// Try to add geographic filter
        /// </summary>
        /// <param name="query"></param>
        /// <param name="geographicAreasFilter"></param>
        private static void TryAddGeographicalAreaFilter(
            this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query,
            GeographicAreasFilter geographicAreasFilter)
        {
            if (geographicAreasFilter == null)
            {
                return;
            }

            query.TryAddTermsCriteria("artportalenInternal.birdValidationAreaIds", geographicAreasFilter.BirdValidationAreaIds);
            query.TryAddTermsCriteria("location.countryRegion.featureId", geographicAreasFilter.CountryRegionIds);
            query.TryAddTermsCriteria("location.county.featureId", geographicAreasFilter.CountyIds);
            query.TryAddTermsCriteria("location.municipality.featureId", geographicAreasFilter.MunicipalityIds);
            query.TryAddTermsCriteria("location.parish.featureId", geographicAreasFilter.ParishIds);
            query.TryAddTermsCriteria("location.province.featureId", geographicAreasFilter.ProvinceIds);

            query.TryAddGeometryFilters(geographicAreasFilter.GeometryFilter);
        }

        private static void TryAddLocationFilter(
            this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query,
            LocationFilter filter)
        {
            if (filter == null)
            {
                return;
            }

            query.TryAddTermsCriteria("location.locationId", filter.LocationIds);
            query.TryAddWildcardCriteria("location.locality", filter.NameFilter);
            query.TryAddGeographicalAreaFilter(filter.AreaGeographic);
            query.TryAddGeometryFilters(filter.Geometries);
            query.TryAddNumericRangeCriteria("location.coordinateUncertaintyInMeters", filter.MaxAccuracy, SearchExtensionsGeneric.RangeTypes.LessThanOrEquals);
        }

        /// <summary>
        /// Try to add not recovered filter
        /// </summary>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        private static void TryAddNotRecoveredFilter(
            this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, SearchFilterBase filter)
        {
            switch (filter.NotRecoveredFilter)
            {
                case SightingNotRecoveredFilter.DontIncludeNotRecovered:
                    query.TryAddTermCriteria("occurrence.isNotRediscoveredObservation", false);
                    break;
                case SightingNotRecoveredFilter.OnlyNotRecovered:
                    query.TryAddTermCriteria("occurrence.isNotRediscoveredObservation", true);
                    break;
            }
        }

        /// <summary>
        /// Try to add taxon search criteria
        /// </summary>
        /// <typeparam name="TQueryContainer"></typeparam>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        private static void TryAddTaxonCriteria<TQueryContainer>(
            this ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> query, TaxonFilter filter) where TQueryContainer : class
        {
            if (filter == null)
            {
                return;
            }

            query.TryAddTermsCriteria("taxon.attributes.redlistCategoryDerived", filter.RedListCategories?.Select(m => m.ToUpper()));
            query.TryAddTermsCriteria("taxon.id", filter.Ids);
            query.TryAddTermsCriteria("occurrence.sex.id", filter.SexIds);
        }

        private static void TryAddValidationStatusFilter(this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, SearchFilterBase filter)
        {
            switch (filter.VerificationStatus)
            {
                case SearchFilterBase.StatusVerification.Verified:
                    query.TryAddTermCriteria("identification.verified", true, true);
                    break;
                case SearchFilterBase.StatusVerification.NotVerified:
                    query.TryAddTermCriteria("identification.verified", false, false);
                    break;
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
            if (aggregationType.IsDateHistogram() || aggregationType == AggregationType.SightingsPerWeek48)
            {
                // Do only include sightings whose period don't exceeds one week/year
                var maxDuration = aggregationType switch
                {
                    AggregationType.QuantityPerWeek => 7,
                    AggregationType.SightingsPerWeek => 7,
                    AggregationType.SightingsPerWeek48 => 7,
                    AggregationType.QuantityPerYear => 365,
                    AggregationType.SightingsPerYear => 365,
                    _ => 365
                };

                query.AddScript($@"ChronoUnit.DAYS.between(doc['event.startDate'].value, doc['event.endDate'].value) <= {maxDuration}");
            }

            if (aggregationType.IsSpeciesSightingsList())
            {
                query.TryAddTermsCriteria("artportalenInternal.sightingTypeId", new[] {
                    (int)SightingType.NormalSighting,
                    (int)SightingType.AggregationSighting,
                    (int)SightingType.ReplacementSighting,
                    (int)SightingType.CorrectionSighting,
                    (int)SightingType.AssessmentSightingForOwnBreeding
                });
            }
        }

        /// <summary>
        /// Create multimedia query
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> ToMultimediaQuery(
            this SearchFilterBase filter)
        {
            var query = filter.ToQuery();
            query.AddNestedMustExistsCriteria("occurrence.media");
            return query;
        }

        public static ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> ToMeasurementOrFactsQuery(
            this SearchFilterBase filter)
        {
            var query = filter.ToQuery();
            query.AddNestedMustExistsCriteria("measurementOrFacts");
            return query;
        }

        /// <summary>
        /// Add signal search specific arguments
        /// </summary>
        /// <param name="query"></param>
        /// <param name="extendedAuthorizations"></param>
        /// <param name="onlyAboveMyClearance"></param>
        public static void AddSignalSearchCriteria(
            this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, IEnumerable<ExtendedAuthorizationAreaFilter> extendedAuthorizations, bool onlyAboveMyClearance)
        {
            if (!extendedAuthorizations?.Any() ?? true)
            {
                return;
            }

            var protectedQuerys = new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();

            // Allow protected observations matching user extended authorization
            foreach (var extendedAuthorization in extendedAuthorizations)
            {
                var protectedQuery = new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();
                if (onlyAboveMyClearance)
                {
                    protectedQuery.TryAddNumericRangeCriteria("occurrence.sensitivityCategory", extendedAuthorization.MaxProtectionLevel, SearchExtensionsGeneric.RangeTypes.GreaterThan);
                }

                TryAddGeographicalAreaFilter(protectedQuery, extendedAuthorization.GeographicAreas);

                protectedQuerys.Add(q => q
                    .Bool(b => b
                        .Filter(protectedQuery)
                    )
                );
            }

            query.Add(q => q
                .Bool(b => b
                    .Should(protectedQuerys)
                )
            );
        }

        /// <summary>
        ///  Create search filter
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skipSightingTypeFilters"></param>
        /// <param name="skipAuthorizationFilters"></param>
        /// <returns></returns>
        public static ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> ToQuery(
            this SearchFilterBase filter, bool skipSightingTypeFilters = false, bool skipAuthorizationFilters = false)
        {
            var query = new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();

            if (filter == null)
            {
                return query;
            }

            if (skipAuthorizationFilters)
            {
                // A observation can exists in both public (as diffused) and in protected (as not diffused) index.
                // If we only get non diffused observations, we make sure we don't get a observation twice when searching both indexes
                query.TryAddTermCriteria("diffusionStatus", 0);
            }
            else
            {
                query.AddAuthorizationFilters(filter.ExtendedAuthorization);
            }

            query.TryAddTermsCriteria("occurrence.sensitivityCategory", filter.SensitivityCategories);    

            // If internal filter is "Use Period For All Year" we cannot apply date-range filter.
            if (!(filter is SearchFilterInternal filterInternal && filterInternal.UsePeriodForAllYears))
            {
                query.TryAddDateRangeFilters(filter.Date, "event.startDate", "event.endDate");
            }

            query.TryAddDataStewardshipFilter(filter.DataStewardship);
            query.TryAddEventFilter(filter.Event);
            query.TryAddTimeRangeFilters(filter.Date, "event.startDate");
            query.TryAddDeterminationFilters(filter);
            query.TryAddLocationFilter(filter.Location);
            query.TryAddModifiedDateFilter(filter.ModifiedDate);
            query.TryAddNotRecoveredFilter(filter);
            query.TryAddValidationStatusFilter(filter);
            query.TryAddTaxonCriteria(filter.Taxa);

            query.TryAddTermsCriteria("diffusionStatus", filter.DiffusionStatuses?.Select(ds => (int)ds));
            query.TryAddTermsCriteria("dataProviderId", filter.DataProviderIds);
            query.TryAddTermsCriteria("dataStewardship.datasetIdentifier", filter.DataStewardshipDatasetIds);
            if (filter.IsPartOfDataStewardshipDataset.GetValueOrDefault(false))
            {                
                query.AddExistsCriteria("dataStewardship");
            }
            
            query.TryAddTermCriteria("occurrence.isPositiveObservation", filter.PositiveSightings);                        
            query.TryAddNestedTermsCriteria("projects", "id", filter.ProjectIds); 
            query.TryAddNumericRangeCriteria("occurrence.birdNestActivityId", filter.BirdNestActivityLimit, SearchExtensionsGeneric.RangeTypes.LessThanOrEquals);

            // Cos4Cloud specific
            query.TryAddTermsCriteria("taxon.kingdom", filter.Taxa?.Kingdoms);
            query.TryAddTermsCriteria("taxon.scientificName", filter.Taxa?.ScientificNames);
            query.TryAddTermsCriteria("license", filter.Licenses);

            IEnumerable<int> sightingTypeSearchGroupIds = null!;
            if (filter is SearchFilterInternal)
            {
                sightingTypeSearchGroupIds = query.AddInternalFilters(filter);
            }
           
            if (!skipSightingTypeFilters || (sightingTypeSearchGroupIds?.Any() ?? false))
            {
                query.AddSightingTypeFilters(filter.TypeFilter, sightingTypeSearchGroupIds);
            }

            return query;
        }

        /// <summary>
        /// Create a exclude query
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> ToExcludeQuery(this SearchFilterBase filter)
        {
            if (filter == null)
            {
                return null;
            }

            var query = new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();

            if (filter.Location?.AreaGeographic?.GeometryFilter?.IsValid ?? false)
            {
                foreach (var geom in filter.Location.AreaGeographic?.GeometryFilter.Geometries)
                {
                    switch (geom.Type.ToLower())
                    {
                        case "holepolygon":
                            query.AddGeoShapeCriteria($"location.{(filter.Location.AreaGeographic.GeometryFilter.UsePointAccuracy ? "pointWithBuffer" : "point")}", geom, GeoShapeRelation.Intersects);
                            if (!filter.Location.AreaGeographic.GeometryFilter.UseDisturbanceRadius) // Not sure this should be used here
                            {
                                continue;
                            }
                            query.AddGeoShapeCriteria("location.pointWithDisturbanceBuffer", geom, GeoShapeRelation.Intersects);

                            break;
                    }
                }
            }
            
            query.TryAddTermsCriteria("occurrence.occurrenceId", filter.ExcludeFilter?.OccurrenceIds);

            if (filter is SearchFilterInternal)
            {
                query.AddInternalExcludeFilters(filter);
            }

            return query;
        }

        /// <summary>
        /// Build a projection string
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="isInternal"></param>        
        /// <returns></returns>
        public static Func<SourceFilterDescriptor<dynamic>, ISourceFilter> ToProjection(this IEnumerable<string> properties,
            bool isInternal)
        {
            var projection = new SourceFilterDescriptor<dynamic>();/*.Excludes(e => e
                .Field("event.endDay")
                .Field("event.endMonth")
                .Field("event.endYear")
                .Field("event.startDay")
                .Field("event.startMonth")
                .Field("event.startYear")
            );*/
            if (isInternal)
            {
                projection.Excludes(e => e
                    .Field("defects")
                    .Field("event.endHistogramWeek")
                    .Field("event.startHistogramWeek")
                    .Field("artportalenInternal.activityCategoryId")
                    .Field("artportalenInternal.triggeredObservationRuleUnspontaneous")
                    .Field("location.attributes.isPrivate")
                    .Field("location.point")
                    .Field("location.pointLocation")
                    .Field("location.pointWithBuffer")
                    .Field("location.pointWithDisturbanceBuffer")
                    .Field("location.isInEconomicZoneOfSweden")); 
            }
            else
            {
                projection.Excludes(e => e
                    .Field("defects")
                    .Field("event.endHistogramWeek")
                    .Field("event.startHistogramWeek")
                    .Field("artportalenInternal")
                    .Field("location.attributes.isPrivate")
                    .Field("location.point")
                    .Field("location.pointLocation")
                    .Field("location.pointWithBuffer")
                    .Field("location.pointWithDisturbanceBuffer")
                    .Field("location.isInEconomicZoneOfSweden")
                );
            }

            if (properties?.Any() ?? false)
            {
                projection.Includes(i => i.Fields(properties.Select(p => p.ToField())));
            }

            return p => projection;
        }

        /// <summary>
        /// Build a projection string
        /// </summary>        
        public static Func<SourceFilterDescriptor<dynamic>, ISourceFilter> ToProjection(this IEnumerable<string> includes, IEnumerable<string> excludes = null)
        {
            var projection = new SourceFilterDescriptor<dynamic>();

            if (includes?.Any() ?? false)
            {
                projection.Includes(i => i.Fields(includes.Select(p => p.ToField())));
            }

            if (excludes?.Any() ?? false)
            {
                projection.Excludes(i => i.Fields(excludes.Select(p => p.ToField())));
            }

            return p => projection;
        }
    }
}