using AgileObjects.AgileMapper.Extensions.Internal;
using CSharpFunctionalExtensions;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Enums.Artportalen;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Search.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using static SOS.Lib.Extensions.SearchExtensionsGeneric;

namespace SOS.Lib
{
    /// <summary>
    /// Observation specific search related extensions
    /// </summary>
    public static class SearchExtensionsObservation
    {
        private static void AddAuthorizationFilters<TQueryDescriptor>(this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, ExtendedAuthorizationFilter filter) where TQueryDescriptor : class
        {
            if (filter.ReportedByMe ?? false)
            {
                queries.TryAddTermCriteria("artportalenInternal.reportedByUserServiceUserId",
                    filter.UserId);
            }

            if (filter.ObservedByMe ?? false)
            {
                var observedByMeQueries = new List<Action<QueryDescriptor<TQueryDescriptor>>>();
                observedByMeQueries.TryAddTermCriteria($"artportalenInternal.occurrenceRecordedByInternal.userServiceUserId", filter.UserId);
                observedByMeQueries.TryAddTermCriteria($"artportalenInternal.occurrenceRecordedByInternal.viewAccess", true);
                queries.Add(q => q
                    .Bool(b => b
                        .Filter(observedByMeQueries.ToArray())
                    )
                 );
            }

            if (filter.ProtectionFilter.Equals(ProtectionFilter.Public))
            {
                // Just to be sure since we can query both public and protected index... Only public observations
                queries.TryAddTermCriteria("sensitive", false);
                return;
            }

            // A observation can exists in both public (as diffused) and in protected (as not diffused) index.
            // If we only get non diffused observations, we make sure we don't get a observation twice when searching both indexes
            //query.TryAddTermCriteria("diffusionStatus", 0);

            // At least on of the sub queries in authorized querys must match
            var authorizeQuerys = new List<Action<QueryDescriptor<TQueryDescriptor>>>();

            // Match all public observations
            var publicQueries = new List<Action<QueryDescriptor<TQueryDescriptor>>>();
            publicQueries.TryAddTermCriteria("sensitive", false);
            authorizeQuerys.Add(q => q
                .Bool(b => b
                    .Filter(publicQueries.ToArray())
                )
            );

            // Match user specific areas and taxa
            if (filter.ExtendedAreas?.Any() ?? false)
            {
                foreach (var extendedAuthorization in filter.ExtendedAreas)
                {
                    var protectedQueries = new List<Action<QueryDescriptor<TQueryDescriptor>>>();
                    protectedQueries.TryAddTermCriteria("sensitive", true);
                    protectedQueries.TryAddNumericRangeCriteria("occurrence.sensitivityCategory", extendedAuthorization.MaxProtectionLevel, RangeTypes.LessThanOrEquals);
                    protectedQueries.TryAddTermsCriteria("taxon.id", extendedAuthorization.TaxonIds);
                    TryAddGeographicalAreaFilter(protectedQueries, extendedAuthorization.GeographicAreas);

                    authorizeQuerys.Add(q => q
                        .Bool(b => b
                            .Filter(protectedQueries.ToArray())
                        )
                    );
                }
            }

            // Match observations sighted or reported by requesting user
            if (filter.UserId != 0)
            {
                // Add autorization to a users 'own' observations 
                var observedByMeQueries = new List<Action<QueryDescriptor<TQueryDescriptor>>>();
                observedByMeQueries.TryAddTermCriteria("artportalenInternal.reportedByUserServiceUserId",
                    filter.UserId);

                authorizeQuerys.Add(q => q
                    .Bool(b => b
                        .Filter(observedByMeQueries.ToArray())
                    )
                );

                var reportedByMeQueries = new List<Action<QueryDescriptor<TQueryDescriptor>>>();
                reportedByMeQueries.TryAddTermCriteria($"artportalenInternal.occurrenceRecordedByInternal.userServiceUserId", filter.UserId);
                reportedByMeQueries.TryAddTermCriteria($"artportalenInternal.occurrenceRecordedByInternal.viewAccess", true);

                authorizeQuerys.Add(q => q
                    .Bool(b => b
                        .Filter(reportedByMeQueries.ToArray())
                    )
                );
            }

            queries.Add(q => q
                .Bool(b => b
                    .Should(authorizeQuerys.ToArray())
                )
            );
        }

        /// <summary>
        /// Add internal filters to query
        /// </summary>
        /// <param name="queries"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private static IEnumerable<int> TryAddInternalFilters<TQueryDescriptor>(this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, SearchFilterBase filter) where TQueryDescriptor : class
        {
            if (filter is SearchFilterInternal internalFilter)
            {
                queries.TryAddTermCriteria("artportalenInternal.checklistId", internalFilter.ChecklistId);
                queries.TryAddTermsCriteria("artportalenInternal.fieldDiaryGroupId", internalFilter.FieldDiaryGroupIds);
                queries.TryAddTermsCriteria("artportalenInternal.datasourceId", internalFilter.DatasourceIds);
                queries.TryAddTermCriteria("artportalenInternal.hasTriggeredVerificationRules", internalFilter.HasTriggeredVerificationRule, true);
                queries.TryAddTermCriteria("artportalenInternal.hasAnyTriggeredVerificationRuleWithWarning", internalFilter.HasTriggeredVerificationRuleWithWarning, true);
                queries.TryAddTermCriteria("artportalenInternal.hasUserComments", internalFilter.OnlyWithUserComments, true);
                queries.TryAddTermCriteria("artportalenInternal.noteOfInterest", internalFilter.OnlyWithNotesOfInterest, true);
                queries.TryAddTermCriteria("artportalenInternal.occurrenceRecordedByInternal.id", internalFilter.ObservedByUserId);
                queries.TryAddTermCriteria("artportalenInternal.occurrenceRecordedByInternal.userServiceUserId", internalFilter.ObservedByUserServiceUserId);
                queries.TryAddTermsCriteria("artportalenInternal.regionalSightingStateId", internalFilter.RegionalSightingStateIdsFilter);
                queries.TryAddTermCriteria("artportalenInternal.reportedByUserId", internalFilter.ReportedByUserId);
                queries.TryAddTermCriteria("artportalenInternal.reportedByUserServiceUserId", internalFilter.ReportedByUserServiceUserId);
                queries.TryAddTermsCriteria("artportalenInternal.sightingPublishTypeIds", internalFilter.PublishTypeIdsFilter);
                queries.TryAddTermsCriteria("artportalenInternal.triggeredObservationRuleFrequencyId", internalFilter.TriggeredObservationRuleFrequencyIds);
                queries.TryAddTermsCriteria("artportalenInternal.triggeredObservationRuleReproductionId", internalFilter.TriggeredObservationRuleReproductionIds);
                queries.TryAddTermsCriteria("artportalenInternal.invasiveSpeciesTreatment.id", internalFilter.InvasiveSpeciesTreatmentIds);
                queries.TryAddTermsCriteria("event.discoveryMethod.id", internalFilter.DiscoveryMethodIds);
                queries.TryAddTermsCriteria("identification.verificationStatus.id", internalFilter.VerificationStatusIds);
                queries.TryAddTermCriteria("institutionId", internalFilter.InstitutionId);
                queries.TryAddTermsCriteria("location.attributes.projectId", internalFilter.SiteProjectIds);
                queries.TryAddTermsCriteria("occurrence.activity.id", internalFilter.ActivityIds);
                queries.TryAddTermCriteria("occurrence.biotope.id", internalFilter.BiotopeId);
                queries.TryAddTermsCriteria("occurrence.lifeStage.id", internalFilter.LifeStageIds);
                queries.TryAddDateRangeCriteria("occurrence.reportedDate", internalFilter.ReportedDateFrom, RangeTypes.GreaterThanOrEquals);
                queries.TryAddDateRangeCriteria("occurrence.reportedDate", internalFilter.ReportedDateTo, RangeTypes.LessThanOrEquals);
                queries.TryAddTermCriteria("occurrence.substrate.id", internalFilter.SubstrateId);
                queries.TryAddTermCriteria("occurrence.substrate.speciesId", internalFilter.SubstrateSpeciesId);
                queries.TryAddTermCriteria("privateCollection", internalFilter.PrivateCollection);
                queries.TryAddTermCriteria("publicCollection", internalFilter.PublicCollection);
                queries.TryAddTermCriteria("speciesCollectionLabel", internalFilter.SpeciesCollectionLabel);

                switch (internalFilter.UnspontaneousFilter)
                {
                    case SightingUnspontaneousFilter.NotUnspontaneous:
                        queries.TryAddTermCriteria("occurrence.isNaturalOccurrence", true);
                        break;
                    case SightingUnspontaneousFilter.Unspontaneous:
                        queries.TryAddTermCriteria("occurrence.isNaturalOccurrence", false);
                        break;
                }

                //search by locationId, but include child-locations observations aswell
                var siteTerms = internalFilter?.SiteIds?.Select(s => $"urn:lsid:artportalen.se:site:{s}");
                if (siteTerms?.Any() ?? false)
                {
                    var locationQueries = new List<Action<QueryDescriptor<TQueryDescriptor>>>();
                    locationQueries.TryAddTermsCriteria("location.locationId", siteTerms);
                    locationQueries.TryAddTermsCriteria("artportalenInternal.parentLocationId", internalFilter.SiteIds);
                    queries.Add(q => q.Bool(p => p
                        .Should(locationQueries.ToArray())
                    ));
                }

                if (internalFilter.OnlySecondHandInformation)
                {
                    queries.TryAddTermCriteria("artportalenInternal.secondHandInformation", true);
                }

                if (internalFilter.OnlyWithBarcode)
                {
                    queries.AddMustExistsCriteria("artportalenInternal.sightingBarcodeURL");
                }

                if (internalFilter.SpeciesFactsIds?.Any() ?? false)
                {
                    var speciesFactQuerys = new List<Action<QueryDescriptor<TQueryDescriptor>>>();
                    foreach (var factsId in internalFilter.SpeciesFactsIds)
                    {
                        speciesFactQuerys.TryAddTermCriteria("artportalenInternal.speciesFactsIds", factsId);
                    }
                    queries.Add(q => q.Bool(b => b
                        .Should(speciesFactQuerys.ToArray())
                    ));
                }

                if (internalFilter.OnlyWithMedia)
                {
                    var mediaQueries = new List<Action<QueryDescriptor<TQueryDescriptor>>>();
                    mediaQueries.AddMustExistsCriteria("occurrence.associatedMedia");
                    mediaQueries.AddMustExistsCriteria("artportalenInternal.associatedMedia");
                    queries.Add(q => q
                        .Bool(b => b
                            .Should(mediaQueries.ToArray())
                        )
                    );
                }

                switch (internalFilter.NotPresentFilter)
                {
                    case SightingNotPresentFilter.DontIncludeNotPresent:
                        queries.TryAddTermCriteria("occurrence.isNeverFoundObservation", false);
                        break;
                    case SightingNotPresentFilter.OnlyNotPresent:
                        queries.TryAddTermCriteria("occurrence.isNeverFoundObservation", true);
                        break;
                }

                if (internalFilter.Length.HasValue && !string.IsNullOrWhiteSpace(internalFilter.LengthOperator))
                {
                    queries.AddNumericFilterWithRelationalOperator("occurrence.length", internalFilter.Length.Value, internalFilter.LengthOperator);
                }
                if (internalFilter.OnlyWithNotes)
                {
                    queries.AddMustExistsCriteria("occurrence.occurrenceRemarks");
                }
                if (internalFilter.Quantity.HasValue && !string.IsNullOrWhiteSpace(internalFilter.QuantityOperator))
                {
                    queries.AddNumericFilterWithRelationalOperator("occurrence.organismQuantityInt", internalFilter.Quantity.Value, internalFilter.QuantityOperator);
                }
                if (internalFilter.QuantityOperator?.ToLower() == "missing")
                {
                    queries.AddNotExistsCriteria("occurrence.organismQuantityInt");
                }
                if (internalFilter.Weight.HasValue && !string.IsNullOrWhiteSpace(internalFilter.WeightOperator))
                {
                    queries.AddNumericFilterWithRelationalOperator("occurrence.weight", internalFilter.Weight.Value, internalFilter.WeightOperator);
                }
                if (internalFilter.Months?.Any() ?? false)
                {
                    switch (internalFilter.MonthsComparison)
                    {
                        case DateFilterComparison.BothStartDateAndEndDate:
                            queries.TryAddTermsCriteria("event.startMonth", internalFilter.Months);
                            queries.TryAddTermsCriteria("event.endMonth", internalFilter.Months);
                            break;
                        case DateFilterComparison.EndDate:
                            queries.TryAddTermsCriteria("event.endMonth", internalFilter.Months);
                            break;
                        case DateFilterComparison.StartDateEndDateMonthRange:
                            queries.TryAddTermsCriteria("artportalenInternal.eventMonths", internalFilter.Months);
                            break;
                        default:
                            queries.TryAddTermsCriteria("event.startMonth", internalFilter.Months);
                            break;
                    }
                }

                if (internalFilter.Years?.Any() ?? false)
                {
                    switch (internalFilter.YearsComparison)
                    {
                        case DateFilterComparison.BothStartDateAndEndDate:
                            queries.TryAddTermsCriteria("event.startYear", internalFilter.Years);
                            queries.TryAddTermsCriteria("event.endYear", internalFilter.Years);
                            break;
                        case DateFilterComparison.EndDate:
                            queries.TryAddTermsCriteria("event.endYear", internalFilter.Years);
                            break;
                        default:
                            queries.TryAddTermsCriteria("event.startYear", internalFilter.Years);
                            break;
                    }
                }

                if (internalFilter.Date != null && internalFilter.UsePeriodForAllYears && internalFilter.Date.StartDate.HasValue && internalFilter.Date.EndDate.HasValue)
                {
                    var selector = "";
                    if (filter.Date.DateFilterType == DateFilter.DateRangeFilterType.BetweenStartDateAndEndDate)
                    {
                        selector = "(daysOfStartYear.contains(startDayOfYear) && daysOfEndYear.contains(endDayOfYear))";
                    }
                    else if (filter.Date.DateFilterType == DateFilter.DateRangeFilterType.OnlyStartDate)
                    {
                        selector = "(daysOfStartYear.contains(startDayOfYear))";
                    }
                    else if (filter.Date.DateFilterType == DateFilter.DateRangeFilterType.OnlyEndDate)
                    {
                        selector = "(daysOfEndYear.contains(endDayOfYear))";
                    }
                    else if (filter.Date.DateFilterType == DateFilter.DateRangeFilterType.OverlappingStartDateAndEndDate)
                    {
                        selector = "(daysOfStartYear.contains(startDayOfYear)) || (daysOfEndYear.contains(endDayOfYear))";
                    }

                    // Interval can start or end in a leap year. If it's starts with a leap year and spans to a new year, add day 366.
                    // If it's end with a leap year and spans over feb, add leap day
                    var daysOfLeapYear = new HashSet<int>();
                    PopulateDaysInInterval(ref daysOfLeapYear, 2000, internalFilter.Date.StartDate.Value, internalFilter.Date.EndDate.Value);
                    PopulateDaysInInterval(ref daysOfLeapYear, 2003, internalFilter.Date.StartDate.Value, internalFilter.Date.EndDate.Value);

                    var daysOfNonLeapYear = new HashSet<int>();
                    PopulateDaysInInterval(ref daysOfNonLeapYear, 2002, internalFilter.Date.StartDate.Value, internalFilter.Date.EndDate.Value);

                    queries.TryAddScript($@"
                        HashSet daysOfLeapYear = new HashSet([{string.Join(',', daysOfLeapYear)}]);
                        HashSet daysOfNonLeapYear = new HashSet([{string.Join(',', daysOfNonLeapYear)}]);
                    
                        int startYear = (int)doc['event.startYear'].value;
                        HashSet daysOfStartYear = (startYear % 400 === 0 || startYear % 100 !== 0 && startYear % 4 === 0) ? daysOfLeapYear : daysOfNonLeapYear;
                        int startDayOfYear = (int)doc['event.startDayOfYear'].value;
                    
                        int endYear = (int)doc['event.endYear'].value;
                        HashSet daysOfEndYear =  (endYear % 400 === 0 || endYear % 100 !== 0 && endYear % 4 === 0) ? daysOfLeapYear : daysOfNonLeapYear;
                        int endDayOfYear = (int)doc['event.endDayOfYear'].value;

                        if({selector})
                            return true;

                        return false;"
                    );
                }

                return internalFilter.SightingTypeSearchGroupIds;
            }

            return null;
        }

        private static void PopulateDaysInInterval(ref HashSet<int> daysOfYear, int startYear, DateTime startDate, DateTime endDate)
        {
            var filterStartDate = CreateDate(startYear, startDate.Month, startDate.Day);
            var filterEndDate = CreateDate(startYear + endDate.Year - startDate.Year, endDate.Month, endDate.Day);

            var count = 0;
            while (filterStartDate <= filterEndDate && count < 365)
            {
                daysOfYear.Add(filterStartDate.DayOfYear);
                filterStartDate = filterStartDate.AddDays(1);
                count++;
            }
            daysOfYear = daysOfYear.OrderBy(d => d).ToHashSet();
        }

        private static DateTime CreateDate(int year, int month, int day)
        {
            return new DateTime(year, month, month.Equals(2) && day.Equals(29) && !DateTime.IsLeapYear(year) ? 28 : day);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="excludeQueries"></param>
        /// <returns></returns>
        private static void TryAddInternalExcludeFilters<TQueryDescriptor>(this
            ICollection<Action<QueryDescriptor<TQueryDescriptor>>> excludeQueries, SearchFilterBase filter) where TQueryDescriptor : class
        {
            if (filter is SearchFilterInternal internalFilter)
            {
                excludeQueries.TryAddTermsCriteria("identification.verificationStatus.id", internalFilter.ExcludeVerificationStatusIds);
            }
        }

        private static void TryAddDataStewardshipFilter<TQueryDescriptor>(this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, DataStewardshipFilter filter) where TQueryDescriptor : class
        {
            if (filter != null)
            {
                queries.TryAddTermsCriteria("dataStewardship.datasetIdentifier", filter.DatasetIdentifiers);
            }
        }

        /// <summary>
        /// Add determination filters
        /// </summary>
        /// <param name="queries"></param>
        /// <param name="filter"></param>
        private static void TryAddDeterminationFilters<TQueryDescriptor>(this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, SearchFilterBase filter) where TQueryDescriptor : class
        {
            switch (filter.DeterminationFilter)
            {
                case SightingDeterminationFilter.NotUnsureDetermination:
                    queries.TryAddTermCriteria("identification.uncertainIdentification", false);
                    break;
                case SightingDeterminationFilter.OnlyUnsureDetermination:
                    queries.TryAddTermCriteria("identification.uncertainIdentification", true);
                    break;
            }
        }

        private static void TryAddEventFilter<TQueryDescriptor>(this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, EventFilter filter) where TQueryDescriptor : class
        {
            if (filter != null)
            {
                queries.TryAddTermsCriteria("event.eventId", filter.Ids);
            }
        }


        /// <summary>
        /// Add geometry filter to query
        /// </summary>
        /// <param name="queries"></param>
        /// <param name="geographicsFilter"></param>
        private static void TryAddGeometryFilters<TQueryDescriptor>(
            this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries,
            GeographicsFilter geographicsFilter) where TQueryDescriptor : class
        {
            if (geographicsFilter != null)
            {
                var boundingBoxContainers = new List<Action<QueryDescriptor<TQueryDescriptor>>>();

                if (!(!geographicsFilter.UsePointAccuracy && geographicsFilter.UseDisturbanceRadius))
                {
                    boundingBoxContainers.TryAddBoundingBoxCriteria(
                        $"location.{(geographicsFilter.UsePointAccuracy ? "pointWithBuffer" : "point")}",
                        geographicsFilter.BoundingBox);
                }

                if (geographicsFilter.UseDisturbanceRadius)
                {
                    // Add both point and pointWithDisturbanceBuffer, since pointWithDisturbanceBuffer can be null if no dist buffer exists
                    boundingBoxContainers.TryAddBoundingBoxCriteria("location.point", geographicsFilter.BoundingBox);
                    boundingBoxContainers.TryAddBoundingBoxCriteria("location.pointWithDisturbanceBuffer", geographicsFilter.BoundingBox);
                }

                if (boundingBoxContainers.Any())
                {
                    queries.Add(q => q.Bool(b => b
                            .Should(boundingBoxContainers.ToArray())
                        )
                    );
                }

                if (geographicsFilter?.IsValid ?? false)
                {
                    var geometryContainers = new List<Action<QueryDescriptor<TQueryDescriptor>>>();

                    foreach (var geom in geographicsFilter.Geometries)
                    {
                        switch (geom.OgcGeometryType)
                        {
                            case OgcGeometryType.Point:
                                geometryContainers.TryAddGeoDistanceCriteria($"location.{(geographicsFilter.UsePointAccuracy ? "pointWithBuffer" : "point")}", geom as Point, GeoDistanceType.Arc, geographicsFilter.MaxDistanceFromPoint ?? 0);

                                if (!geographicsFilter.UseDisturbanceRadius)
                                {
                                    continue;
                                }
                                geometryContainers.TryAddGeoDistanceCriteria("location.pointWithDisturbanceBuffer", geom as Point, GeoDistanceType.Arc, geographicsFilter.MaxDistanceFromPoint ?? 0);
                                break;
                            case OgcGeometryType.Polygon:
                            case OgcGeometryType.MultiPolygon:
                                var vaildGeometry = geom.TryMakeValid();
                                geometryContainers.TryAddGeoShapeCriteria($"location.{(geographicsFilter.UsePointAccuracy ? "pointWithBuffer" : "point")}", vaildGeometry, geographicsFilter.UsePointAccuracy ? GeoShapeRelation.Intersects : GeoShapeRelation.Within);
                                if (!geographicsFilter.UseDisturbanceRadius)
                                {
                                    continue;
                                }
                                geometryContainers.TryAddGeoShapeCriteria("location.pointWithDisturbanceBuffer", vaildGeometry, GeoShapeRelation.Intersects);
                                break;
                        }
                    }

                    queries.Add(q => q.Bool(b => b
                            .Should(geometryContainers.ToArray())
                        )
                    );
                }
            };
        }

        private static void TryAddModifiedDateFilter<TQueryDescriptor>(this
                ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, ModifiedDateFilter filter) where TQueryDescriptor : class
        {
            if (filter != null)
            {
                queries.TryAddDateRangeCriteria("modified", filter.From, RangeTypes.GreaterThanOrEquals);
                queries.TryAddDateRangeCriteria("modified", filter.To, RangeTypes.LessThanOrEquals);
            }
        }

        /// <summary>
        /// Try to add geographic filter
        /// </summary>
        /// <param name="queries"></param>
        /// <param name="geographicAreasFilter"></param>
        private static void TryAddGeographicalAreaFilter<TQueryDescriptor>(
            this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries,
            GeographicAreasFilter geographicAreasFilter) where TQueryDescriptor : class
        {
            if (geographicAreasFilter != null)
            {
                queries.TryAddGeometryFilters(geographicAreasFilter.GeometryFilter);
                queries.TryAddTermsCriteria("artportalenInternal.birdValidationAreaIds", geographicAreasFilter.BirdValidationAreaIds);
                queries.TryAddTermsCriteria("location.countryRegion.featureId", geographicAreasFilter.CountryRegionIds);
                queries.TryAddTermsCriteria("location.county.featureId", geographicAreasFilter.CountyIds);
                queries.TryAddTermsCriteria("location.municipality.featureId", geographicAreasFilter.MunicipalityIds);
                queries.TryAddTermsCriteria("location.parish.featureId", geographicAreasFilter.ParishIds);
                queries.TryAddTermsCriteria("location.province.featureId", geographicAreasFilter.ProvinceIds);
            }     
        }

        private static void TryAddLocationFilter<TQueryDescriptor>(
            this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries,
            LocationFilter filter) where TQueryDescriptor : class
        {
            if (filter != null)
            {
                queries.TryAddGeographicalAreaFilter(filter.AreaGeographic);
                queries.TryAddGeometryFilters(filter.Geometries);
                queries.TryAddTermsCriteria("location.locationId", filter.LocationIds);
                queries.TryAddWildcardCriteria("location.locality", filter.NameFilter);
                queries.TryAddNumericRangeCriteria("location.coordinateUncertaintyInMeters", filter.MaxAccuracy, RangeTypes.LessThanOrEquals);
            }
        }

        /// <summary>
        /// Try to add not recovered filter
        /// </summary>
        /// <param name="queries"></param>
        /// <param name="filter"></param>
        private static void TryAddNotRecoveredFilter<TQueryDescriptor>(
            this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, SearchFilterBase filter) where TQueryDescriptor : class
        {
            if (filter != null)
            {
                switch (filter.NotRecoveredFilter)
                {
                    case SightingNotRecoveredFilter.DontIncludeNotRecovered:
                        queries.TryAddTermCriteria("occurrence.isNotRediscoveredObservation", false);
                        break;
                    case SightingNotRecoveredFilter.OnlyNotRecovered:
                        queries.TryAddTermCriteria("occurrence.isNotRediscoveredObservation", true);
                        break;
                }

            }
        }

        /// <summary>
        /// Try to add taxon search criteria
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="queries"></param>
        /// <param name="filter"></param>
        private static void TryAddTaxonCriteria<TQueryDescriptor>(
            this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, TaxonFilter filter) where TQueryDescriptor : class
        {
            if (filter == null)
            {
                return;
            }

            queries.TryAddTermsCriteria("taxon.attributes.redlistCategoryDerived", filter.RedListCategories?.Select(m => m.ToUpper()));
            queries.TryAddTermsCriteria("taxon.id", filter.Ids);
            queries.TryAddTermsCriteria("occurrence.sex.id", filter.SexIds);

            if (filter.IsInvasiveInSweden.HasValue)
            {
                queries.TryAddTermCriteria("taxon.attributes.isInvasiveInSweden", filter.IsInvasiveInSweden.Value);
            }
        }

        private static void TryAddValidationStatusFilter<TQueryDescriptor>(this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, SearchFilterBase filter) where TQueryDescriptor : class
        {
            if (filter != null)
            {
                switch (filter.VerificationStatus)
                {
                    case SearchFilterBase.StatusVerification.Verified:
                        queries.TryAddTermCriteria("identification.verified", true, true);
                        break;
                    case SearchFilterBase.StatusVerification.NotVerified:
                        queries.TryAddTermCriteria("identification.verified", false, false);
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aggregationType"></param>
        /// <param name="queries"></param>
        /// <returns></returns>
        public static void AddAggregationFilter<TQueryDescriptor>(this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, AggregationType aggregationType) where TQueryDescriptor : class
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

                queries.TryAddScript($@"ChronoUnit.DAYS.between(doc['event.startDate'].value, doc['event.endDate'].value) <= {maxDuration}");
            }

            if (aggregationType.IsSpeciesSightingsList())
            {
                queries.TryAddTermsCriteria("artportalenInternal.sightingTypeId", new[] {
                    (int)SightingType.NormalSighting,
                    (int)SightingType.AggregationSighting,
                    (int)SightingType.ReplacementSighting,
                    (int)SightingType.CorrectionSighting,
                    (int)SightingType.AssessmentSightingForOwnBreeding
                });
            }
        }

        public static void AddSightingTypeFilters<TQueryDescriptor>(this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries,
            SearchFilterBase.SightingTypeFilter sightingTypeFilter,
            IEnumerable<int> sightingTypeSearchGroupIds) where TQueryDescriptor : class
        {
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
                        SearchFilterBase.SightingTypeFilter.DoNotShowMerged => new[] { // 1, 4, 16, 32, 128 DoNotShowMerged
                            (int)SightingTypeSearchGroup.Ordinary,
                            (int)SightingTypeSearchGroup.Aggregated,
                            (int)SightingTypeSearchGroup.AssessmentChild,
                            (int)SightingTypeSearchGroup.Replacement,
                            (int)SightingTypeSearchGroup.OwnBreedingAssessment },
                        SearchFilterBase.SightingTypeFilter.ShowChildrenAndReplacements => new[] { // 1, 8, 16, 32, 256 ShowChildrenAndReplacements
                            (int)SightingTypeSearchGroup.Ordinary,
                            (int)SightingTypeSearchGroup.AggregatedChild,
                            (int)SightingTypeSearchGroup.AssessmentChild,
                            (int)SightingTypeSearchGroup.Replacement,
                            (int)SightingTypeSearchGroup.OwnBreedingAssessmentChild },
                        _ => new[] { // Default to DoNotShowMerged
                            (int)SightingTypeSearchGroup.Ordinary,
                            (int)SightingTypeSearchGroup.Aggregated,
                            (int)SightingTypeSearchGroup.AssessmentChild,
                            (int)SightingTypeSearchGroup.Replacement,
                            (int)SightingTypeSearchGroup.OwnBreedingAssessment },
                    };

            var sightingTypeQueries = new List<Action<QueryDescriptor<TQueryDescriptor>>>();
            sightingTypeQueries.TryAddTermsCriteria("artportalenInternal.sightingTypeSearchGroupId", sightingTypeSearchGroupFilter);

            // If not only Assessment is selected
            if (!(sightingTypeSearchGroupFilter.Count().Equals(1) && sightingTypeSearchGroupFilter.First().Equals(2)))
            {
                // Get observations from other than Artportalen too
                sightingTypeQueries.AddNotExistsCriteria("artportalenInternal.sightingTypeSearchGroupId");
            }

            queries.Add(q => q
                .Bool(b => b
                    .Should(sightingTypeQueries.ToArray())
                )
            );
        }

        /// <summary>
        /// Create multimedia query
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> ToMultimediaQuery<TQueryDescriptor>(
            this SearchFilterBase filter) where TQueryDescriptor : class
        {
            var queries = filter.ToQuery<TQueryDescriptor>();
            queries.AddMustExistsCriteria("occurrence.media");
            return queries;
        }

        public static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> ToMeasurementOrFactsQuery<TQueryDescriptor>(
            this SearchFilterBase filter) where TQueryDescriptor : class
        {
            var queries = filter.ToQuery<TQueryDescriptor>();
            queries.AddMustExistsCriteria("measurementOrFacts");
            return queries;
        }

        /// <summary>
        /// Add signal search specific arguments
        /// </summary>
        /// <param name="queries"></param>
        /// <param name="extendedAuthorizations"></param>
        /// <param name="onlyAboveMyClearance"></param>
        public static void AddSignalSearchCriteria<TQueryDescriptor>(
            this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, IEnumerable<ExtendedAuthorizationAreaFilter> extendedAuthorizations, bool onlyAboveMyClearance) where TQueryDescriptor : class
        {
            var protectedQuerys = new List<Action<QueryDescriptor<TQueryDescriptor>>>();
            if (extendedAuthorizations?.Any() ?? false)
            {
                // Allow protected observations matching user extended authorization
                foreach (var extendedAuthorization in extendedAuthorizations)
                {
                    var protectedQuery = new List<Action<QueryDescriptor<TQueryDescriptor>>>();
                    if (onlyAboveMyClearance)
                    {
                        protectedQuery.TryAddNumericRangeCriteria("occurrence.sensitivityCategory", extendedAuthorization.MaxProtectionLevel, RangeTypes.GreaterThan);
                    }

                    TryAddGeographicalAreaFilter(protectedQuery, extendedAuthorization.GeographicAreas);

                    protectedQuerys.Add(q => q
                        .Bool(b => b
                            .Filter(protectedQuery.ToArray())
                        )
                    );
                }

                queries.Add(q => q
                    .Bool(b => b
                        .Should(protectedQuerys.ToArray())
                    )
                );
            }
        }

        /// <summary>
        ///  Create search filter
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skipSightingTypeFilters"></param>
        /// <param name="skipAuthorizationFilters"></param>
        /// <returns></returns>
        public static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> ToQuery<TQueryDescriptor>(
            this SearchFilterBase filter, bool skipSightingTypeFilters = false, bool skipAuthorizationFilters = false) where TQueryDescriptor : class
        {
            var queries = new List<Action<QueryDescriptor<TQueryDescriptor>>>();

            if (filter == null)
            {
                return queries;
            }

            if (skipAuthorizationFilters)
            {
                // A observation can exists in both public (as diffused) and in protected (as not diffused) index.
                // If we only get non diffused observations, we make sure we don't get a observation twice when searching both indexes
                //query.TryAddTermCriteria("diffusionStatus", 0);
            }
            else
            {
                queries.AddAuthorizationFilters(filter.ExtendedAuthorization);
            }
            // Only possible to use generalization filter when searching sensitive observations.
            if (filter.ExtendedAuthorization?.ProtectionFilter == ProtectionFilter.Public || filter.ExtendedAuthorization?.ProtectionFilter == ProtectionFilter.BothPublicAndSensitive)
            {
                filter.IncludeSensitiveGeneralizedObservations = false;
            }

            queries.TryAddDataStewardshipFilter(filter.DataStewardship);
            queries.TryAddEventFilter(filter.Event);
            queries.TryAddDeterminationFilters(filter);
            queries.TryAddLocationFilter(filter.Location);
            queries.TryAddModifiedDateFilter(filter.ModifiedDate);
            queries.TryAddNotRecoveredFilter(filter);
            queries.TryAddTaxonCriteria(filter.Taxa);
            queries.TryAddValidationStatusFilter(filter);
            queries.TryAddGeneralizationsCriteria(filter.IncludeSensitiveGeneralizedObservations, filter.IsPublicGeneralizedObservation);
            queries.TryAddNumericRangeCriteria("occurrence.birdNestActivityId", filter.BirdNestActivityLimit, RangeTypes.LessThanOrEquals);
            queries.TryAddTermsCriteria("dataProviderId", filter.DataProviderIds);
            queries.TryAddTermsCriteria("dataStewardship.datasetIdentifier", filter.DataStewardshipDatasetIds);
            queries.TryAddTermCriteria("occurrence.isPositiveObservation", filter.PositiveSightings);
            queries.TryAddTermsCriteria("occurrence.occurrenceId", filter.OccurrenceIds);
            queries.TryAddTermsCriteria("occurrence.sensitivityCategory", filter.SensitivityCategories);
            queries.TryAddTermsCriteria("projects.id", filter.ProjectIds);
            queries.TryAddTermsCriteria("taxon.kingdom", filter.Taxa?.Kingdoms); // Cos4Cloud specific
            queries.TryAddTermsCriteria("taxon.scientificName", filter.Taxa?.ScientificNames); // Cos4Cloud specific
            queries.TryAddTermsCriteria("license", filter.Licenses); // Cos4Cloud specific
            queries.TryAddTimeRangeFilters(filter.Date, "event.startDate");

            // If internal filter is "Use Period For All Year" we cannot apply date-range filter.
            if (!(filter is SearchFilterInternal filterInternal && filterInternal.UsePeriodForAllYears))
            {
                queries.TryAddDateRangeFilters(filter.Date, "event.startDate", "event.endDate");
            }

            if (filter.IsPartOfDataStewardshipDataset.GetValueOrDefault(false))
            {
                queries.AddExistsCriteria("dataStewardship");
            }

            var sightingTypeSearchGroupIds = queries.TryAddInternalFilters(filter);
            if (!skipSightingTypeFilters || (sightingTypeSearchGroupIds?.Any() ?? false))
            {
                queries.AddSightingTypeFilters(filter.TypeFilter, sightingTypeSearchGroupIds);
            }

            return queries;
        }

        /// <summary>
        /// Create a exclude query
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> ToExcludeQuery<TQueryDescriptor>(this SearchFilterBase filter) where TQueryDescriptor : class
        {
            var queries = new List<Action<QueryDescriptor<TQueryDescriptor>>>();

            if (filter == null)
            {
                return queries;
            }

            queries.TryAddTermsCriteria("occurrence.occurrenceId", filter.ExcludeFilter?.OccurrenceIds);
            queries.TryAddInternalExcludeFilters(filter);
     
            return queries;
        }

        /// <summary>
        /// Build a projection string
        /// </summary>
        /// <param name="includes"></param>
        /// <param name="isInternal"></param>        
        /// <returns></returns>
        public static SourceConfig ToProjection(
            this IEnumerable<string> includes,
            bool isInternal)
        {
            var excludes = new List<string>() {
                "defects",
                "event.endDayOfYear",
                "event.startDayOfYear",
                "event.endHistogramWeek",
                "event.startHistogramWeek",
                "location.attributes.isPrivate",
                "location.point",
                "location.pointLocation",
                "location.pointWithBuffer",
                "location.pointWithDisturbanceBuffer",
                "location.isInEconomicZoneOfSweden"
            };
            
            if (isInternal)
            {
                excludes.AddRange(new[] {
                    "artportalenInternal.activityCategoryId",
                    "artportalenInternal.triggeredObservationRuleUnspontaneous"
                });
            }
            else
            {
                excludes.Add("artportalenInternal");
            }

            return (includes, excludes).ToProjection();
        }

        /// <summary>
        /// Create source config
        /// </summary>        
        public static SourceConfig ToProjection(this (IEnumerable<string> Includes, IEnumerable<string> Excludes) sourceFields)
        {
            var excludes = (sourceFields.Excludes?.Count() ?? 0) == 0 ? null : sourceFields.Excludes?.Select(i => string.Join('.', i.Split('.').Select(f => f.ToCamelCase()))).ToArray();
            var includes = (sourceFields.Includes?.Count() ?? 0) == 0 ? null : sourceFields.Includes?.Select(i => string.Join('.', i.Split('.').Select(f => f.ToCamelCase()))).ToArray();
            
            return new SourceConfig(new SourceFilter
            {
                Excludes = Fields.FromStrings(excludes),
                Includes = Fields.FromStrings(includes)
            });
        }
    }
}