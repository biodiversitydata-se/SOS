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
        /// <summary>
        /// Add filter to limit response to only show observations user is allowed to see
        /// </summary>
        /// <param name="queries"></param>
        /// <param name="filter"></param>
        private static void AddAuthorizationFilters<TQueryDescriptor>(this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, ExtendedAuthorizationFilter filter) where TQueryDescriptor : class
        {
            if (filter.ReportedByMe ?? false)
            {
                queries.Add(q => q
                    .TryAddTermCriteria("artportalenInternal.reportedByUserServiceUserId", filter.UserId)
                );
            }

            if (filter.ObservedByMe ?? false)
            {
                queries.Add(q => q
                    .TryAddTermCriteria($"artportalenInternal.occurrenceRecordedByInternal.userServiceUserId", filter.UserId)
                );
                queries.Add(q => q
                    .TryAddTermCriteria($"artportalenInternal.occurrenceRecordedByInternal.viewAccess", true)
                );
            }

            if (filter.ProtectionFilter.Equals(ProtectionFilter.Public))
            {
                // Just to be sure since we can query both public and protected index... Only public observations
                queries.Add(q => q
                    .TryAddTermCriteria("sensitive", false)
                );
                return;
            }

            // A observation can exists in both public (as diffused) and in protected (as not diffused) index.
            // If we only get non diffused observations, we make sure we don't get a observation twice when searching both indexes
            //query.TryAddTermCriteria("diffusionStatus", 0);

            // At least on of the sub queries in authorized querys must match
            var restrictionFilter = new List<Action<QueryDescriptor<TQueryDescriptor>>>() {
                q => q.TryAddTermCriteria("sensitive", false)  // Match all public observations
            };

            // Match user specific areas and taxa
            if (filter.ExtendedAreas?.Any() ?? false)
            {
                foreach (var extendedAuthorization in filter.ExtendedAreas)
                {
                    var protectedQuery = new List<Action<QueryDescriptor<TQueryDescriptor>>>();
                    protectedQuery.TryAddGeographicalAreaFilter(extendedAuthorization.GeographicAreas);

                    protectedQuery.Add(q => q
                        .TryAddTermCriteria("sensitive", true)
                        .TryAddNumericRangeCriteria("occurrence.sensitivityCategory", extendedAuthorization.MaxProtectionLevel, RangeTypes.LessThanOrEquals)
                        .TryAddTermsCriteria("taxon.id", extendedAuthorization.TaxonIds)
                    );
                    
                    restrictionFilter.Add(q => q
                        .Bool(b => b
                            .Filter(protectedQuery.ToArray())
                        )
                    );
                }
            }

            // Match observations sighted or reported by requesting user
            if (filter.UserId != 0)
            {
                // Add autorization to a users 'own' observations 
                var observedByMeQuery = new QueryDescriptor<TQueryDescriptor>();
                observedByMeQuery.TryAddTermCriteria("artportalenInternal.reportedByUserServiceUserId",
                    filter.UserId);

                restrictionFilter.Add(q => q
                    .Bool(b => b
                        .Filter(observedByMeQuery)
                    )
                );

                var reportedByMeQuery = new QueryDescriptor<TQueryDescriptor>();
                reportedByMeQuery.TryAddTermCriteria($"artportalenInternal.occurrenceRecordedByInternal.userServiceUserId", filter.UserId);
                reportedByMeQuery.TryAddTermCriteria($"artportalenInternal.occurrenceRecordedByInternal.viewAccess", true);

                restrictionFilter.Add(q => q
                    .Bool(b => b
                        .Filter(reportedByMeQuery)
                    )
                );
            }

            queries.Add(q => q
                .Bool(b => b
                    .Should(restrictionFilter.ToArray())
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
                queries.Add(q => q
                    .TryAddTermCriteria("artportalenInternal.checklistId", internalFilter.ChecklistId)
                    .TryAddTermsCriteria("artportalenInternal.fieldDiaryGroupId", internalFilter.FieldDiaryGroupIds)
                    .TryAddTermsCriteria("artportalenInternal.datasourceId", internalFilter.DatasourceIds)
                    .TryAddTermCriteria("artportalenInternal.hasTriggeredVerificationRules", internalFilter.HasTriggeredVerificationRule, true)
                    .TryAddTermCriteria("artportalenInternal.hasAnyTriggeredVerificationRuleWithWarning", internalFilter.HasTriggeredVerificationRuleWithWarning, true)
                    .TryAddTermCriteria("artportalenInternal.hasUserComments", internalFilter.OnlyWithUserComments, true)
                    .TryAddTermCriteria("artportalenInternal.noteOfInterest", internalFilter.OnlyWithNotesOfInterest, true)
                    .TryAddTermCriteria("artportalenInternal.occurrenceRecordedByInternal.id", internalFilter.ObservedByUserId)
                    .TryAddTermCriteria("artportalenInternal.occurrenceRecordedByInternal.userServiceUserId", internalFilter.ObservedByUserServiceUserId)
                    .TryAddTermsCriteria("artportalenInternal.regionalSightingStateId", internalFilter.RegionalSightingStateIdsFilter)
                    .TryAddTermCriteria("artportalenInternal.reportedByUserId", internalFilter.ReportedByUserId)
                    .TryAddTermCriteria("artportalenInternal.reportedByUserServiceUserId", internalFilter.ReportedByUserServiceUserId)
                    .TryAddTermsCriteria("artportalenInternal.sightingPublishTypeIds", internalFilter.PublishTypeIdsFilter)
                    .TryAddTermsCriteria("artportalenInternal.triggeredObservationRuleFrequencyId", internalFilter.TriggeredObservationRuleFrequencyIds)
                    .TryAddTermsCriteria("artportalenInternal.triggeredObservationRuleReproductionId", internalFilter.TriggeredObservationRuleReproductionIds)
                    .TryAddTermsCriteria("event.discoveryMethod.id", internalFilter.DiscoveryMethodIds)
                    .TryAddTermsCriteria("identification.verificationStatus.id", internalFilter.VerificationStatusIds)
                    .TryAddTermCriteria("institutionId", internalFilter.InstitutionId)
                    .TryAddTermsCriteria("location.attributes.projectId", internalFilter.SiteProjectIds)
                    .TryAddTermsCriteria("occurrence.activity.id", internalFilter.ActivityIds)
                    .TryAddTermCriteria("occurrence.biotope.id", internalFilter.BiotopeId)
                    .TryAddTermsCriteria("occurrence.lifeStage.id", internalFilter.LifeStageIds)
                    .TryAddDateRangeCriteria("occurrence.reportedDate", internalFilter.ReportedDateFrom, RangeTypes.GreaterThanOrEquals)
                    .TryAddDateRangeCriteria("occurrence.reportedDate", internalFilter.ReportedDateTo, RangeTypes.LessThanOrEquals)
                    .TryAddTermCriteria("occurrence.substrate.id", internalFilter.SubstrateId)
                    .TryAddTermCriteria("occurrence.substrate.speciesId", internalFilter.SubstrateSpeciesId)
                    .TryAddTermCriteria("privateCollection", internalFilter.PrivateCollection)
                    .TryAddTermCriteria("publicCollection", internalFilter.PublicCollection)
                    .TryAddTermCriteria("speciesCollectionLabel", internalFilter.SpeciesCollectionLabel)
                );

                switch (internalFilter.UnspontaneousFilter)
                {
                    case SightingUnspontaneousFilter.NotUnspontaneous:
                        queries.Add(q => q.TryAddTermCriteria("occurrence.isNaturalOccurrence", true));
                        break;
                    case SightingUnspontaneousFilter.Unspontaneous:
                        queries.Add(q => q.TryAddTermCriteria("occurrence.isNaturalOccurrence", false));
                        break;
                }

                //search by locationId, but include child-locations observations aswell
                var siteTerms = internalFilter?.SiteIds?.Select(s => $"urn:lsid:artportalen.se:site:{s}");
                if (siteTerms?.Any() ?? false)
                {
                    queries.Add(q => q.Bool(p => p
                        .Should(s => s
                            .TryAddTermsCriteria("location.locationId", siteTerms),
                            s => s
                            .TryAddTermsCriteria("artportalenInternal.parentLocationId", internalFilter.SiteIds)
                        )
                    ));
                }

                if (internalFilter.OnlySecondHandInformation)
                {
                    queries.Add(q => q.TryAddTermCriteria("artportalenInternal.secondHandInformation", true));
                }

                if (internalFilter.OnlyWithBarcode)
                {
                    queries.Add(q => q.AddMustExistsCriteria("artportalenInternal.sightingBarcodeURL"));
                }

                if (internalFilter.SpeciesFactsIds?.Any() ?? false)
                {
                    var speciesFactQuerys = new List<Action<QueryDescriptor<TQueryDescriptor>>>();
                    foreach (var factsId in internalFilter.SpeciesFactsIds)
                    {
                        speciesFactQuerys.Add(a => a.TryAddTermCriteria("artportalenInternal.speciesFactsIds", factsId));
                    }
                    queries.Add(q => q.Bool(b => b
                        .Should(speciesFactQuerys.ToArray())
                    ));
                }

                if (internalFilter.OnlyWithMedia)
                {
                    var mediaQueries = new Action<QueryDescriptor<TQueryDescriptor>>[] { 
                        q => q.AddMustExistsCriteria("occurrence.associatedMedia"),
                        q => q.AddMustExistsCriteria("artportalenInternal.associatedMedia")
                    };
                    queries.Add(q => q
                        .Bool(b => b
                            .Should(mediaQueries.ToArray())
                        )
                    );
                }

                switch (internalFilter.NotPresentFilter)
                {
                    case SightingNotPresentFilter.DontIncludeNotPresent:
                        queries.Add(q => q.TryAddTermCriteria("occurrence.isNeverFoundObservation", false));
                        break;
                    case SightingNotPresentFilter.OnlyNotPresent:
                        queries.Add(q => q.TryAddTermCriteria("occurrence.isNeverFoundObservation", true));
                        break;
                }

                if (internalFilter.Length.HasValue && !string.IsNullOrWhiteSpace(internalFilter.LengthOperator))
                {
                    queries.Add(q => q.AddNumericFilterWithRelationalOperator("occurrence.length", internalFilter.Length.Value, internalFilter.LengthOperator));
                }
                if (internalFilter.OnlyWithNotes)
                {
                    queries.Add(q => q.AddMustExistsCriteria("occurrence.occurrenceRemarks"));
                }
                if (internalFilter.Quantity.HasValue && !string.IsNullOrWhiteSpace(internalFilter.QuantityOperator))
                {
                    queries.Add(q => q.AddNumericFilterWithRelationalOperator("occurrence.organismQuantityInt", internalFilter.Quantity.Value, internalFilter.QuantityOperator));
                }
                if (internalFilter.QuantityOperator?.ToLower() == "missing")
                {
                    queries.Add(q => q.AddNotExistsCriteria("occurrence.organismQuantityInt"));
                }
                if (internalFilter.Weight.HasValue && !string.IsNullOrWhiteSpace(internalFilter.WeightOperator))
                {
                    queries.Add(q => q.AddNumericFilterWithRelationalOperator("occurrence.weight", internalFilter.Weight.Value, internalFilter.WeightOperator));
                }
                if (internalFilter.Months?.Any() ?? false)
                {
                    switch (internalFilter.MonthsComparison)
                    {
                        case DateFilterComparison.BothStartDateAndEndDate:
                            queries.Add(q => q
                                .TryAddTermsCriteria("event.startMonth", internalFilter.Months)
                                .TryAddTermsCriteria("event.endMonth", internalFilter.Months)
                            );
                            break;
                        case DateFilterComparison.EndDate:
                            queries.Add(q => q.TryAddTermsCriteria("event.endMonth", internalFilter.Months));
                            break;
                        case DateFilterComparison.StartDateEndDateMonthRange:
                            queries.Add(q => q.TryAddTermsCriteria("artportalenInternal.eventMonths", internalFilter.Months));
                            break;
                        default:
                            queries.Add(q => q.TryAddTermsCriteria("event.startMonth", internalFilter.Months));
                            break;
                    }
                }

                if (internalFilter.Years?.Any() ?? false)
                {
                    switch (internalFilter.YearsComparison)
                    {
                        case DateFilterComparison.BothStartDateAndEndDate:
                            queries.Add(q => q
                                .TryAddTermsCriteria("event.startYear", internalFilter.Years)
                                .TryAddTermsCriteria("event.endYear", internalFilter.Years)
                            );
                            break;
                        case DateFilterComparison.EndDate:
                            queries.Add(q => q.TryAddTermsCriteria("event.endYear", internalFilter.Years));
                            break;
                        default:
                            queries.Add(q => q.TryAddTermsCriteria("event.startYear", internalFilter.Years));
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

                    queries.Add(q => q
                        .TryAddScript($@"
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

                            return false;
                        ")
                    );
                }

                return internalFilter.SightingTypeSearchGroupIds;
            }

            return null;
        }

        private static void PopulateDaysInInterval(ref HashSet<int> daysOfYear, int startYear, DateTime startDate, DateTime endDate)
        {
            var filterStartDate = CreateDate(startYear, startDate.Month,startDate.Day);
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
        private static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> TryAddInternalExcludeFilters<TQueryDescriptor>(this
            ICollection<Action<QueryDescriptor<TQueryDescriptor>>> excludeQueries, SearchFilterBase filter) where TQueryDescriptor : class
        {
            if (filter is SearchFilterInternal internalFilter)
            {
                excludeQueries.Add(q => q.TryAddTermsCriteria("identification.verificationStatus.id", internalFilter.ExcludeVerificationStatusIds));
            }
            return excludeQueries;
        }

        private static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> TryAddDataStewardshipFilter<TQueryDescriptor>(this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, DataStewardshipFilter filter) where TQueryDescriptor : class
        {
            if (filter != null)
            {
                queries.Add(q => q.TryAddTermsCriteria("dataStewardship.datasetIdentifier", filter.DatasetIdentifiers));
            }

            return queries;
        }

        /// <summary>
        /// Add determination filters
        /// </summary>
        /// <param name="queries"></param>
        /// <param name="filter"></param>
        private static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> TryAddDeterminationFilters<TQueryDescriptor>(this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, SearchFilterBase filter) where TQueryDescriptor : class
        {
            switch (filter.DeterminationFilter)
            {
                case SightingDeterminationFilter.NotUnsureDetermination:
                    queries.Add(q => q.TryAddTermCriteria("identification.uncertainIdentification", false));
                    break;
                case SightingDeterminationFilter.OnlyUnsureDetermination:
                    queries.Add(q => q.TryAddTermCriteria("identification.uncertainIdentification", true));
                    break;
            }
            return queries;
        }

        private static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> TryAddEventFilter<TQueryDescriptor>(this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, EventFilter filter) where TQueryDescriptor : class
        {
            if (filter != null)
            {
                queries.Add(q => q.TryAddTermsCriteria("event.eventId", filter.Ids));
            }
            return queries;
        }


        /// <summary>
        /// Add geometry filter to query
        /// </summary>
        /// <param name="queries"></param>
        /// <param name="geographicsFilter"></param>
        private static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> TryAddGeometryFilters<TQueryDescriptor>(
            this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries,
            GeographicsFilter geographicsFilter) where TQueryDescriptor : class
        {
            if (geographicsFilter != null)
            {
                var boundingBoxContainers = new List<Action<QueryDescriptor<TQueryDescriptor>>>();

                if (!(!geographicsFilter.UsePointAccuracy && geographicsFilter.UseDisturbanceRadius))
                {
                    boundingBoxContainers.Add(q => q.TryAddBoundingBoxCriteria(
                        $"location.{(geographicsFilter.UsePointAccuracy ? "pointWithBuffer" : "point")}",
                        geographicsFilter.BoundingBox));
                }

                if (geographicsFilter.UseDisturbanceRadius)
                {
                    // Add both point and pointWithDisturbanceBuffer, since pointWithDisturbanceBuffer can be null if no dist buffer exists
                    boundingBoxContainers.Add(q => q
                        .TryAddBoundingBoxCriteria("location.point", geographicsFilter.BoundingBox)
                    );
                    boundingBoxContainers.Add(q => q
                        .TryAddBoundingBoxCriteria("location.pointWithDisturbanceBuffer", geographicsFilter.BoundingBox)
                    );
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
                                geometryContainers.Add(q => q
                                    .AddGeoDistanceCriteria($"location.{(geographicsFilter.UsePointAccuracy ? "pointWithBuffer" : "point")}", geom as Point, GeoDistanceType.Arc, geographicsFilter.MaxDistanceFromPoint ?? 0)
                                );

                                if (!geographicsFilter.UseDisturbanceRadius)
                                {
                                    continue;
                                }
                                geometryContainers.Add(q => q
                                    .AddGeoDistanceCriteria("location.pointWithDisturbanceBuffer", geom as Point, GeoDistanceType.Arc, geographicsFilter.MaxDistanceFromPoint ?? 0)
                                );
                                break;
                            case OgcGeometryType.Polygon:
                            case OgcGeometryType.MultiPolygon:
                                var vaildGeometry = geom.TryMakeValid();
                                geometryContainers.Add(q => q
                                    .AddGeoShapeCriteria($"location.{(geographicsFilter.UsePointAccuracy ? "pointWithBuffer" : "point")}", vaildGeometry, geographicsFilter.UsePointAccuracy ? GeoShapeRelation.Intersects : GeoShapeRelation.Within)
                                );
                                if (!geographicsFilter.UseDisturbanceRadius)
                                {
                                    continue;
                                }
                                geometryContainers.Add(q => q  
                                    .AddGeoShapeCriteria("location.pointWithDisturbanceBuffer", vaildGeometry, GeoShapeRelation.Intersects)
                                );
                                break;
                        }
                    }

                    queries.Add(q => q.Bool(b => b
                            .Should(geometryContainers.ToArray())
                        )
                    );
                }
            }

            return queries;
        }

        private static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> TryAddModifiedDateFilter<TQueryDescriptor>(this
                ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, ModifiedDateFilter filter) where TQueryDescriptor : class
        {
            if (filter != null)
            {
                queries.Add(q => q
                    .TryAddDateRangeCriteria("modified", filter.From, RangeTypes.GreaterThanOrEquals)
                    .TryAddDateRangeCriteria("modified", filter.To, RangeTypes.LessThanOrEquals)
                );
            }

            return queries;
        }

        /// <summary>
        /// Try to add geographic filter
        /// </summary>
        /// <param name="queries"></param>
        /// <param name="geographicAreasFilter"></param>
        private static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> TryAddGeographicalAreaFilter<TQueryDescriptor>(
            this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries,
            GeographicAreasFilter geographicAreasFilter) where TQueryDescriptor : class
        {
            if (geographicAreasFilter != null)
            {
                queries.TryAddGeometryFilters(geographicAreasFilter.GeometryFilter);
                queries.Add(q => q
                    .TryAddTermsCriteria("artportalenInternal.birdValidationAreaIds", geographicAreasFilter.BirdValidationAreaIds)
                    .TryAddTermsCriteria("location.countryRegion.featureId", geographicAreasFilter.CountryRegionIds)
                    .TryAddTermsCriteria("location.county.featureId", geographicAreasFilter.CountyIds)
                    .TryAddTermsCriteria("location.municipality.featureId", geographicAreasFilter.MunicipalityIds)
                    .TryAddTermsCriteria("location.parish.featureId", geographicAreasFilter.ParishIds)
                    .TryAddTermsCriteria("location.province.featureId", geographicAreasFilter.ProvinceIds)
                );
            }

            return queries;
                   
        }

        private static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> TryAddLocationFilter<TQueryDescriptor>(
            this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries,
            LocationFilter filter) where TQueryDescriptor : class
        {
            if (filter != null)
            {
                queries.TryAddGeographicalAreaFilter(filter.AreaGeographic);
                queries.TryAddGeometryFilters(filter.Geometries);
                queries.Add(q => q
                    .TryAddTermsCriteria("location.locationId", filter.LocationIds)
                    .TryAddWildcardCriteria("location.locality", filter.NameFilter)

                    .TryAddNumericRangeCriteria("location.coordinateUncertaintyInMeters", filter.MaxAccuracy, RangeTypes.LessThanOrEquals)
                );
                
            }
            return queries;
        }

        /// <summary>
        /// Try to add not recovered filter
        /// </summary>
        /// <param name="queries"></param>
        /// <param name="filter"></param>
        private static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> TryAddNotRecoveredFilter<TQueryDescriptor>(
            this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, SearchFilterBase filter) where TQueryDescriptor : class
        {
            if (filter != null)
            {
                switch (filter.NotRecoveredFilter)
                {
                    case SightingNotRecoveredFilter.DontIncludeNotRecovered:
                        queries.Add(q => q.TryAddTermCriteria("occurrence.isNotRediscoveredObservation", false));
                        break;
                    case SightingNotRecoveredFilter.OnlyNotRecovered:
                        queries.Add(q => q.TryAddTermCriteria("occurrence.isNotRediscoveredObservation", true));
                        break;
                }

            }
            return queries;
        }

        /// <summary>
        /// Try to add taxon search criteria
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="queries"></param>
        /// <param name="filter"></param>
        private static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> TryAddTaxonCriteria<TQueryDescriptor>(
            this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, TaxonFilter filter) where TQueryDescriptor : class
        {
            if (filter != null)
            {
                queries.Add(q => q
                    .TryAddTermsCriteria("taxon.attributes.redlistCategoryDerived", filter.RedListCategories?.Select(m => m.ToUpper()))
                    .TryAddTermsCriteria("taxon.id", filter.Ids)
                    .TryAddTermsCriteria("occurrence.sex.id", filter.SexIds)
                );
            }

            return queries;
        }

        private static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> TryAddValidationStatusFilter<TQueryDescriptor>(this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, SearchFilterBase filter) where TQueryDescriptor : class
        {
            if (filter != null)
            {
                switch (filter.VerificationStatus)
                {
                    case SearchFilterBase.StatusVerification.Verified:
                        queries.Add(q => q.TryAddTermCriteria("identification.verified", true, true));
                        break;
                    case SearchFilterBase.StatusVerification.NotVerified:
                        queries.Add(q => q.TryAddTermCriteria("identification.verified", false, false));
                        break;
                }
            }

            return queries;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aggregationType"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static void AddAggregationFilter<TQueryDescriptor>(this QueryDescriptor<TQueryDescriptor> query, AggregationType aggregationType) where TQueryDescriptor : class
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

                query.TryAddScript($@"ChronoUnit.DAYS.between(doc['event.startDate'].value, doc['event.endDate'].value) <= {maxDuration}");
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
                        _ => new[] { // 1, 4, 16, 32, 128 Default DoNotShowMerged
                        (int)SightingTypeSearchGroup.Ordinary,
                        (int)SightingTypeSearchGroup.Aggregated,
                        (int)SightingTypeSearchGroup.AssessmentChild,
                        (int)SightingTypeSearchGroup.Replacement,
                        (int)SightingTypeSearchGroup.OwnBreedingAssessment }
                    };

            var sightingTypeQuery = new QueryDescriptor<TQueryDescriptor>();
            sightingTypeQuery.TryAddTermsCriteria("artportalenInternal.sightingTypeSearchGroupId", sightingTypeSearchGroupFilter);

            // If not only Assessment is selected
            if (!(sightingTypeSearchGroupFilter.Count().Equals(1) && sightingTypeSearchGroupFilter.First().Equals(2)))
            {
                // Get observations from other than Artportalen too
                sightingTypeQuery.AddNotExistsCriteria("artportalenInternal.sightingTypeSearchGroupId");
            }

            queries.Add(q => q
                .Bool(b => b
                    .Should(sightingTypeQuery)
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
            queries.Add(q => q.AddMustExistsCriteria("occurrence.media"));
            return queries;
        }

        public static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> ToMeasurementOrFactsQuery<TQueryDescriptor>(
            this SearchFilterBase filter) where TQueryDescriptor : class
        {
            var queries = filter.ToQuery<TQueryDescriptor>();
            queries.Add(q => q.AddMustExistsCriteria("measurementOrFacts"));
            return queries;
        }

        /// <summary>
        /// Add signal search specific arguments
        /// </summary>
        /// <param name="queries"></param>
        /// <param name="extendedAuthorizations"></param>
        /// <param name="onlyAboveMyClearance"></param>
        public static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> AddSignalSearchCriteria<TQueryDescriptor>(
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
                        protectedQuery.Add(q => q.TryAddNumericRangeCriteria("occurrence.sensitivityCategory", extendedAuthorization.MaxProtectionLevel, RangeTypes.GreaterThan));
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

            return queries;
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

            queries.TryAddDataStewardshipFilter(filter.DataStewardship)
                .TryAddEventFilter(filter.Event)
                .TryAddDeterminationFilters(filter)
                .TryAddLocationFilter(filter.Location)
                .TryAddModifiedDateFilter(filter.ModifiedDate)
                .TryAddNotRecoveredFilter(filter)
                .TryAddTaxonCriteria(filter.Taxa)
                .TryAddValidationStatusFilter(filter);
            queries.Add(q => q
                .TryAddGeneralizationsCriteria(filter.IncludeSensitiveGeneralizedObservations, filter.IsPublicGeneralizedObservation)
                .TryAddNumericRangeCriteria("occurrence.birdNestActivityId", filter.BirdNestActivityLimit, RangeTypes.LessThanOrEquals)
                .TryAddTermsCriteria("dataProviderId", filter.DataProviderIds)
                .TryAddTermsCriteria("dataStewardship.datasetIdentifier", filter.DataStewardshipDatasetIds)
                .TryAddTermCriteria("occurrence.isPositiveObservation", filter.PositiveSightings)
                .TryAddTermsCriteria("occurrence.occurrenceId", filter.OccurrenceIds)
                .TryAddTermsCriteria("occurrence.sensitivityCategory", filter.SensitivityCategories)
                .TryAddTermsCriteria("projects.id", filter.ProjectIds)
                .TryAddTermsCriteria("taxon.kingdom", filter.Taxa?.Kingdoms)  // Cos4Cloud specific
                .TryAddTermsCriteria("taxon.scientificName", filter.Taxa?.ScientificNames) // Cos4Cloud specific
                .TryAddTermsCriteria("license", filter.Licenses) // Cos4Cloud specific
                .TryAddTimeRangeFilters(filter.Date, "event.startDate")
            );

            // If internal filter is "Use Period For All Year" we cannot apply date-range filter.
            if (!(filter is SearchFilterInternal filterInternal && filterInternal.UsePeriodForAllYears))
            {
                queries.Add(q => q.TryAddDateRangeFilters(filter.Date, "event.startDate", "event.endDate"));
            }

            if (filter.IsPartOfDataStewardshipDataset.GetValueOrDefault(false))
            {
                queries.Add(q => q.AddExistsCriteria("dataStewardship"));
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
                if (filter.Location?.AreaGeographic?.GeometryFilter?.IsValid ?? false)
                {
                    foreach (var geom in filter.Location.AreaGeographic?.GeometryFilter.Geometries)
                    {
                        switch (geom.OgcGeometryType)
                        {
                            case OgcGeometryType.Polygon:
                            case OgcGeometryType.MultiPolygon:
                                queries.Add(q => q.AddGeoShapeCriteria($"location.{(filter.Location.AreaGeographic.GeometryFilter.UsePointAccuracy ? "pointWithBuffer" : "point")}", geom, GeoShapeRelation.Intersects));
                                if (!filter.Location.AreaGeographic.GeometryFilter.UseDisturbanceRadius) // Not sure this should be used here
                                {
                                    continue;
                                }
                                queries.Add(q => q.AddGeoShapeCriteria("location.pointWithDisturbanceBuffer", geom, GeoShapeRelation.Intersects));

                                break;
                        }
                    }
                }

                queries.Add(q => q.TryAddTermsCriteria("occurrence.occurrenceId", filter.ExcludeFilter?.OccurrenceIds));
                queries.TryAddInternalExcludeFilters(filter);
            }

            return queries;
        }

        /// <summary>
        /// Build a projection string
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="isInternal"></param>        
        /// <returns></returns>
        public static SourceConfig ToProjection(
            this IEnumerable<string> properties,
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

            return new SourceConfig(new SourceFilter
            {
                Excludes = Fields.FromStrings(excludes.ToArray()),
                Includes = (properties?.Count() ?? 0) == 0 ? null : Fields.FromStrings(properties.ToArray())
            });
        }

        /// <summary>
        /// Create source config
        /// </summary>        
        public static SourceConfig ToProjection(this (IEnumerable<string> Includes, IEnumerable<string> Excludes) sourceFields)
        {
            return new SourceConfig(new SourceFilter
            {
                Excludes = (sourceFields.Excludes?.Count() ?? 0) == 0 ? null : Fields.FromStrings(sourceFields.Excludes.ToArray()),
                Includes = (sourceFields.Includes?.Count() ?? 0) == 0 ? null : Fields.FromStrings(sourceFields.Includes.ToArray())
            });
        }
    }
}