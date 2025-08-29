using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;

namespace SOS.Shared.Api.Extensions.Dto
{
    public static class FilterExtensions
    {
        private static SearchFilterBase PopulateFilter(SearchFilterBaseDto searchFilterBaseDto, int userId, ProtectionFilterDto? protectionFilter, string translationCultureCode)
        {
            if (searchFilterBaseDto == null)
            {
                return default!;
            }
            protectionFilter ??= ProtectionFilterDto.Public;

             var filter = searchFilterBaseDto is SearchFilterInternalBaseDto ?
                new SearchFilterInternal(userId, protectionFilter.ToFilter()) :
                new SearchFilter(userId, protectionFilter.ToFilter());
            filter.Taxa = searchFilterBaseDto.Taxon?.ToTaxonFilter();
            filter.DataStewardship = PopulateDataStewardshipFilter(searchFilterBaseDto.DataStewardship);
            filter.Date = PopulateDateFilter(searchFilterBaseDto.Date);
            filter.DataProviderIds = searchFilterBaseDto.DataProvider?.Ids?.ToList();
            filter.Event = PopulateEventFilter(searchFilterBaseDto.Event!);
            filter.FieldTranslationCultureCode = translationCultureCode;
            filter.NotRecoveredFilter = (SightingNotRecoveredFilter)(searchFilterBaseDto.NotRecoveredFilter ?? SightingNotRecoveredFilterDto.NoFilter);
            //filter.VerificationStatus = searchFilterBaseDto.ValidationStatus.HasValue ? (SearchFilterBase.StatusVerification)searchFilterBaseDto.ValidationStatus.Value.ToStatusVerification() : (SearchFilterBase.StatusVerification)searchFilterBaseDto.VerificationStatus;
            filter.VerificationStatus = (SearchFilterBase.StatusVerification)(searchFilterBaseDto.VerificationStatus ?? StatusVerificationDto.BothVerifiedAndNotVerified);
            filter.ProjectIds = searchFilterBaseDto.ProjectIds;
            filter.ProjectIds = searchFilterBaseDto.ProjectIds;
            filter.BirdNestActivityLimit = searchFilterBaseDto.BirdNestActivityLimit;
            filter.Location = PopulateLocationFilter(searchFilterBaseDto.Geographics!);
            filter.ModifiedDate = searchFilterBaseDto.ModifiedDate == null
                ? null
                : new ModifiedDateFilter
                {
                    From = searchFilterBaseDto.ModifiedDate.From,
                    To = searchFilterBaseDto.ModifiedDate.To
                };
            filter.ExtendedAuthorization.ObservedByMe = searchFilterBaseDto.ObservedByMe;
            filter.ExtendedAuthorization.ReportedByMe = searchFilterBaseDto.ReportedByMe;
            bool isOnlyNotPresentOrRecoveredFilterSet = false;
            if (searchFilterBaseDto.NotRecoveredFilter == SightingNotRecoveredFilterDto.OnlyNotRecovered ||
                searchFilterBaseDto.NotRecoveredFilter == SightingNotRecoveredFilterDto.IncludeNotRecovered)
            {
                isOnlyNotPresentOrRecoveredFilterSet = true;
            }
            
            filter.DiffusionStatuses = searchFilterBaseDto.DiffusionStatuses?.Select(dsd => (DiffusionStatus)dsd)?.ToList();
            filter.DeterminationFilter = (SightingDeterminationFilter)(searchFilterBaseDto.DeterminationFilter ?? SightingDeterminationFilterDto.NoFilter);
            filter.Output = new OutputFilter();
            if (searchFilterBaseDto is SearchFilterDto searchFilterDto)
            {
                filter.Output.Fields = searchFilterDto.Output?.Fields?.ToList();
                filter.Output.PopulateFields(searchFilterDto.Output?.FieldSet);
            }

            if (searchFilterBaseDto is SearchFilterInternalBaseDto searchFilterInternalBaseDto)
            {
                var filterInternal = (SearchFilterInternal)filter;
                PopulateInternalBase(searchFilterInternalBaseDto, filterInternal);
                if (filterInternal.NotPresentFilter == SightingNotPresentFilter.OnlyNotPresent || 
                    filterInternal.NotPresentFilter == SightingNotPresentFilter.IncludeNotPresent)
                {
                    isOnlyNotPresentOrRecoveredFilterSet = true;
                }

                if (searchFilterBaseDto is SearchFilterInternalDto searchFilterInternalDto)
                {
                    filterInternal.IncludeRealCount = searchFilterInternalDto.IncludeRealCount;
                    filterInternal.Output.Fields = searchFilterInternalDto.Output?.Fields?.ToList();
                    filterInternal.Output.PopulateFields(searchFilterInternalDto.Output?.FieldSet);
                    filterInternal.Output.SortOrders = searchFilterInternalDto.Output?.SortOrders?.Select(so => new SortOrderFilter { SortBy = so.SortBy, SortOrder = so.SortOrder });
                }

                filter.IncludeSensitiveGeneralizedObservations = searchFilterInternalBaseDto?.GeneralizationFilter?.SensitiveGeneralizationFilter switch
                {
                    SensitiveGeneralizationFilterDto.DontIncludeGeneralizedObservations => false,
                    SensitiveGeneralizationFilterDto.OnlyGeneralizedObservations => true,
                    SensitiveGeneralizationFilterDto.IncludeGeneralizedObservations => null,
                    null => false,
                    _ => false
                };

                filter.IsPublicGeneralizedObservation = searchFilterInternalBaseDto?.GeneralizationFilter?.PublicGeneralizationFilter switch
                {
                    PublicGeneralizationFilterDto.DontIncludeGeneralized => false,
                    PublicGeneralizationFilterDto.OnlyGeneralized => true,
                    PublicGeneralizationFilterDto.NoFilter => null,
                    null => null,
                    _ => null
                };                
            }

            if (searchFilterBaseDto.OccurrenceStatus != null)
            {
                switch (searchFilterBaseDto.OccurrenceStatus)
                {
                    case OccurrenceStatusFilterValuesDto.Present:
                        filter.PositiveSightings = true;
                        break;
                    case OccurrenceStatusFilterValuesDto.Absent:
                        filter.PositiveSightings = false;
                        break;
                    case OccurrenceStatusFilterValuesDto.BothPresentAndAbsent:
                        filter.PositiveSightings = null;
                        break;
                }
            }
            else if (!isOnlyNotPresentOrRecoveredFilterSet)
            {
                filter.PositiveSightings = true;
            }

            if (searchFilterBaseDto.ExcludeFilter != null)
            {
                filter.ExcludeFilter = new ExcludeFilter();
                filter.ExcludeFilter.OccurrenceIds = searchFilterBaseDto.ExcludeFilter.OccurrenceIds;
            }

            return filter;
        }

        private static void PopulateInternalBase(SearchFilterInternalBaseDto searchFilterInternalDto, SearchFilterInternal internalFilter)
        {
            if (searchFilterInternalDto.ExtendedFilter != null)
            {
                internalFilter.ChecklistId = searchFilterInternalDto.ExtendedFilter.ChecklistId;
                internalFilter.FieldDiaryGroupIds = searchFilterInternalDto.ExtendedFilter.FieldDiaryGroupIds;
                internalFilter.ReportedByUserId = searchFilterInternalDto.ExtendedFilter.ReportedByUserId;
                internalFilter.ObservedByUserId = searchFilterInternalDto.ExtendedFilter.ObservedByUserId;
                internalFilter.ReportedByUserServiceUserId = searchFilterInternalDto.ExtendedFilter.ReportedByUserServiceUserId;
                internalFilter.ObservedByUserServiceUserId = searchFilterInternalDto.ExtendedFilter.ObservedByUserServiceUserId;
                internalFilter.OnlyWithMedia = searchFilterInternalDto.ExtendedFilter.OnlyWithMedia;
                internalFilter.OnlyWithNotes = searchFilterInternalDto.ExtendedFilter.OnlyWithNotes;
                internalFilter.OnlyWithNotesOfInterest = searchFilterInternalDto.ExtendedFilter.OnlyWithNotesOfInterest;
                internalFilter.OnlyWithUserComments = searchFilterInternalDto.ExtendedFilter.OnlyWithUserComments;
                internalFilter.OnlyWithBarcode = searchFilterInternalDto.ExtendedFilter.OnlyWithBarcode;
                internalFilter.ReportedDateFrom = searchFilterInternalDto.ExtendedFilter.ReportedDateFrom;
                internalFilter.ReportedDateTo = searchFilterInternalDto.ExtendedFilter.ReportedDateTo;
                internalFilter.TypeFilter = (SearchFilterInternal.SightingTypeFilter)searchFilterInternalDto.ExtendedFilter.TypeFilter;
                internalFilter.UsePeriodForAllYears = searchFilterInternalDto.ExtendedFilter.UsePeriodForAllYears;
                internalFilter.Months = searchFilterInternalDto.ExtendedFilter.Months;
                internalFilter.MonthsComparison = (DateFilterComparison)searchFilterInternalDto.ExtendedFilter.MonthsComparison;
                internalFilter.DiscoveryMethodIds = searchFilterInternalDto.ExtendedFilter.DiscoveryMethodIds;
                internalFilter.LifeStageIds = searchFilterInternalDto.ExtendedFilter.LifeStageIds;
                internalFilter.ActivityIds = searchFilterInternalDto.ExtendedFilter.ActivityIds;
                //internalFilter.HasTriggeredVerificationRule = searchFilterInternalDto.ExtendedFilter.HasTriggerdValidationRule ?? searchFilterInternalDto.ExtendedFilter.HasTriggeredVerificationRule;
                internalFilter.HasTriggeredVerificationRule = searchFilterInternalDto.ExtendedFilter.HasTriggeredVerificationRule;
                /*internalFilter.HasTriggeredVerificationRuleWithWarning =
                    searchFilterInternalDto.ExtendedFilter.HasTriggerdValidationRuleWithWarning ?? searchFilterInternalDto.ExtendedFilter.HasTriggeredVerificationRuleWithWarning;*/
                internalFilter.HasTriggeredVerificationRuleWithWarning = searchFilterInternalDto.ExtendedFilter.HasTriggeredVerificationRuleWithWarning;
                internalFilter.Length = searchFilterInternalDto.ExtendedFilter.Length;
                internalFilter.LengthOperator = searchFilterInternalDto.ExtendedFilter.LengthOperator;
                internalFilter.Weight = searchFilterInternalDto.ExtendedFilter.Weight;
                internalFilter.WeightOperator = searchFilterInternalDto.ExtendedFilter.WeightOperator;
                internalFilter.Quantity = searchFilterInternalDto.ExtendedFilter.Quantity;
                internalFilter.QuantityOperator = searchFilterInternalDto.ExtendedFilter.QuantityOperator;
                //internalFilter.VerificationStatusIds = searchFilterInternalDto.ExtendedFilter.ValidationStatusIds != null && searchFilterInternalDto.ExtendedFilter.ValidationStatusIds.Any() ? searchFilterInternalDto.ExtendedFilter.ValidationStatusIds : searchFilterInternalDto.ExtendedFilter.VerificationStatusIds;
                internalFilter.VerificationStatusIds = searchFilterInternalDto.ExtendedFilter.VerificationStatusIds;
                //internalFilter.ExcludeVerificationStatusIds = searchFilterInternalDto.ExtendedFilter.ExcludeValidationStatusIds != null && searchFilterInternalDto.ExtendedFilter.ExcludeValidationStatusIds.Any() ? searchFilterInternalDto.ExtendedFilter.ExcludeValidationStatusIds : searchFilterInternalDto.ExtendedFilter.ExcludeVerificationStatusIds;
                internalFilter.ExcludeVerificationStatusIds = searchFilterInternalDto.ExtendedFilter.ExcludeVerificationStatusIds;
                internalFilter.UnspontaneousFilter =
                    (SightingUnspontaneousFilter)searchFilterInternalDto.ExtendedFilter
                        .UnspontaneousFilter;
                internalFilter.SpeciesCollectionLabel = searchFilterInternalDto.ExtendedFilter.SpeciesCollectionLabel;
                internalFilter.PublicCollection = searchFilterInternalDto.ExtendedFilter.PublicCollection;
                internalFilter.PrivateCollection = searchFilterInternalDto.ExtendedFilter.PrivateCollection;
                internalFilter.SubstrateSpeciesId = searchFilterInternalDto.ExtendedFilter.SubstrateSpeciesId;
                internalFilter.SubstrateId = searchFilterInternalDto.ExtendedFilter.SubstrateId;
                internalFilter.BiotopeId = searchFilterInternalDto.ExtendedFilter.BiotopeId;
                internalFilter.NotPresentFilter =
                    (SightingNotPresentFilter)searchFilterInternalDto.ExtendedFilter.NotPresentFilter;
                internalFilter.OnlySecondHandInformation = searchFilterInternalDto.ExtendedFilter.OnlySecondHandInformation;
                internalFilter.PublishTypeIdsFilter = searchFilterInternalDto.ExtendedFilter.PublishTypeIdsFilter;
                internalFilter.RegionalSightingStateIdsFilter =
                    searchFilterInternalDto.ExtendedFilter.RegionalSightingStateIdsFilter;
                internalFilter.TriggeredObservationRuleFrequencyIds =
                    searchFilterInternalDto.ExtendedFilter.TriggeredObservationRuleFrequencyIds;
                internalFilter.TriggeredObservationRuleReproductionIds =
                    searchFilterInternalDto.ExtendedFilter.TriggeredObservationRuleReproductionIds;
                internalFilter.SightingTypeSearchGroupIds = searchFilterInternalDto.ExtendedFilter.SightingTypeSearchGroupIds;
                internalFilter.SiteIds = searchFilterInternalDto.ExtendedFilter.SiteIds;
                internalFilter.SiteProjectIds = searchFilterInternalDto.ExtendedFilter.SiteProjectIds;
                internalFilter.SpeciesFactsIds = searchFilterInternalDto.ExtendedFilter.SpeciesFactsIds;
                internalFilter.InstitutionId = searchFilterInternalDto.ExtendedFilter.InstitutionId;
                internalFilter.DatasourceIds = searchFilterInternalDto.ExtendedFilter.DatasourceIds;
                /*if (searchFilterInternalDto?.ExtendedFilter?.LocationNameFilter != null)
                {
                    if (internalFilter.Location == null) internalFilter.Location = new LocationFilter();
                    internalFilter.Location.NameFilter = searchFilterInternalDto.ExtendedFilter.LocationNameFilter;
                }*/

                if (searchFilterInternalDto.ExtendedFilter.SexIds?.Any() ?? false)
                {
                    internalFilter.Taxa ??= new TaxonFilter();
                    internalFilter.Taxa.SexIds = searchFilterInternalDto.ExtendedFilter.SexIds;
                }
                internalFilter.InvasiveSpeciesTreatmentIds = searchFilterInternalDto.ExtendedFilter.InvasiveSpeciesTreatmentIds;
                internalFilter.Years = searchFilterInternalDto.ExtendedFilter.Years;
                internalFilter.YearsComparison = (DateFilterComparison)searchFilterInternalDto.ExtendedFilter.YearsComparison;
            }
        }

        private static DataStewardshipFilter PopulateDataStewardshipFilter(DataStewardshipFilterDto filter)
        {
            return filter == null ? null! :
                new DataStewardshipFilter
                {
                    DatasetIdentifiers = filter.DatasetIdentifiers
                };
        }

        private static DateFilter PopulateDateFilter(DateFilterDto filter)
        {
            if (filter == null)
            {
                return null!;
            }

            return new DateFilter
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                DateFilterType = (DateFilter.DateRangeFilterType)(filter?.DateFilterType).GetValueOrDefault(),
                TimeRanges = filter?.TimeRanges?.Select(tr => (DateFilter.TimeRange)tr).ToList()
            };
        }

        private static EventFilter PopulateEventFilter(EventFilterDto filter)
        {
            return filter == null ? null! :
                new EventFilter
                {
                    Ids = filter.Ids
                };
        }

        private static LocationFilter PopulateLocationFilter(GeographicsFilterDto filter)
        {
            if (filter == null)
            {
                return null!;
            }

            return new LocationFilter
            {
                Areas = filter.Areas?.Select(a => new AreaFilter
                { FeatureId = a.FeatureId, AreaType = (AreaType)a.AreaType }).ToList(),
                Geometries = new GeographicsFilter
                {
                    BoundingBox = filter.BoundingBox?.ToLatLonBoundingBox(),
                    Geometries = filter.Geometries?.ToList(),
                    MaxDistanceFromPoint = filter.MaxDistanceFromPoint,
                    UseDisturbanceRadius = filter.ConsiderDisturbanceRadius,
                    UsePointAccuracy = filter.ConsiderObservationAccuracy,
                    UseAuthorizationBuffer = filter.ConsiderAuthorizationBuffer
                },
                LocationIds = filter.LocationIds,
                NameFilter = filter.LocationNameFilter,
                MaxAccuracy = filter.MaxAccuracy
            };
        }
        
        private static TaxonFilter PopulateTaxa(TaxonFilterBaseDto filterDto)
        {
            if (filterDto == null)
            {
                return null!;
            }

            var filter = new TaxonFilter
            {
                Ids = filterDto.Ids,
                IncludeUnderlyingTaxa = filterDto.IncludeUnderlyingTaxa ?? false,
                ListIds = filterDto.TaxonListIds,
                TaxonListOperator = TaxonFilter.TaxonListOp.Merge
            };

            if (filterDto is TaxonFilterDto taxonFilterDto)
            {
                filter.RedListCategories = taxonFilterDto.RedListCategories;
                filter.TaxonCategories = taxonFilterDto.TaxonCategories;
                filter.TaxonListOperator =
                    (TaxonFilter.TaxonListOp)(taxonFilterDto?.TaxonListOperator).GetValueOrDefault();
            }

            return filter;
        }

        private static ProtectionFilter ToFilter(this ProtectionFilterDto? protectionFilter)
        {
            return protectionFilter switch
            {
                ProtectionFilterDto.BothPublicAndSensitive => ProtectionFilter.BothPublicAndSensitive,
                ProtectionFilterDto.Sensitive => ProtectionFilter.Sensitive,
                _ => ProtectionFilter.Public
            };
        }

        public static SearchFilter ToSearchFilter(this SearchFilterBaseDto searchFilterDto, int userId, ProtectionFilterDto? protectionFilterDto, string translationCultureCode)
        {
            return (SearchFilter)PopulateFilter(searchFilterDto, userId, protectionFilterDto, translationCultureCode);
        }

        public static SearchFilter ToSearchFilter(this SearchFilterDto searchFilterDto, int userId, ProtectionFilterDto? protectionFilterDto, string translationCultureCode,
            string sortBy, SearchSortOrder sortOrder)
        {
            var searchFilter = (SearchFilter)PopulateFilter(searchFilterDto, userId, protectionFilterDto, translationCultureCode);

            if (!string.IsNullOrEmpty(sortBy))
            {
                searchFilter ??= new SearchFilter(userId, (ProtectionFilter)protectionFilterDto!);
                searchFilter.Output ??= new OutputFilter();
                searchFilter.Output.SortOrders = new[] { new SortOrderFilter { SortBy = sortBy, SortOrder = sortOrder } };
            }
            return searchFilter;
        }

        public static SearchFilterInternal ToSearchFilterInternal(this SearchFilterInternalBaseDto searchFilterDto,
            int userId, string translationCultureCode)
        {
            return (SearchFilterInternal)PopulateFilter(searchFilterDto, userId, searchFilterDto.ProtectionFilter, translationCultureCode);
        }

        public static SearchFilterInternal ToSearchFilterInternal(this SearchFilterInternalDto searchFilterDto,
            int userId, string translationCultureCode, string sortBy, SearchSortOrder sortOrder)
        {
            // User sort order passed the old way if not any other sort order is passed 
            if (!string.IsNullOrEmpty(sortBy) && (!searchFilterDto?.Output?.SortOrders?.Any() ?? true))
            {
                searchFilterDto ??= new SearchFilterInternalDto();
                searchFilterDto.Output ??= new OutputFilterExtendedDto();
                searchFilterDto.Output.SortOrders = new[] { new SortOrderDto { SortBy = sortBy, SortOrder = sortOrder } };
            }
 
            return (SearchFilterInternal)PopulateFilter(searchFilterDto!, userId, searchFilterDto?.ProtectionFilter, translationCultureCode);
        }

        public static SearchFilter ToSearchFilter(this SearchFilterAggregationDto searchFilterDto, int userId, ProtectionFilterDto protectionFilterDto, string translationCultureCode)
        {
            return (SearchFilter)PopulateFilter(searchFilterDto, userId, protectionFilterDto, translationCultureCode);
        }

        public static TaxonFilter ToTaxonFilterFilter(this TaxonFilterDto taxonFilterDto)
        {
            return PopulateTaxa(taxonFilterDto);            
        }

        public static SearchFilterInternal ToSearchFilterInternal(this SearchFilterAggregationInternalDto searchFilterDto, int userId, string translationCultureCode)
        {

            return (SearchFilterInternal)PopulateFilter(searchFilterDto, userId, searchFilterDto.ProtectionFilter, translationCultureCode);
        }

        public static SearchFilter ToSearchFilter(this ExportFilterDto searchFilterDto, int userId, ProtectionFilterDto protectionFilterDto, string translationCultureCode)
        {
            return (SearchFilter)PopulateFilter(searchFilterDto, userId, protectionFilterDto, translationCultureCode);
        }

        public static (SearchFilter, ChecklistSearchFilter) ToSearchFilters(this CalculateTrendFilterDto searchFilterDto)
        {
            var dateFilter = PopulateDateFilter(searchFilterDto.Date);
            var taxonFilter = new TaxonFilter { Ids = new[] { searchFilterDto.TaxonId } };
            var locationFilter = PopulateLocationFilter(searchFilterDto.Geographics);
            // todo - add this in next version
            //var observationFilter = new SearchFilter
            //{
            //    DataProviderIds = searchFilterDto.Observation?.DataProvider?.Ids,
            //    Date = PopulateDateFilter(searchFilterDto.Date),
            //    Taxa = new TaxonFilter { Ids = new[] { searchFilterDto.TaxonId } },
            //    VerificationStatus = (SearchFilterBase.StatusVerification)(searchFilterDto?.Observation?.VerificationStatus ?? StatusVerificationDto.BothVerifiedAndNotVerified)
            //};
            //observationFilter.Location = PopulateLocationFilter(searchFilterDto.Geographics);
            var checklistFilter = new ChecklistSearchFilter
            {
                DataProviderIds = searchFilterDto.Checklist?.DataProvider?.Ids,
                Date = dateFilter as ChecklistDateFilter,
                Location = locationFilter,
                Taxa = taxonFilter
            };

            if (TimeSpan.TryParse(searchFilterDto.Checklist?.MinEffortTime, out var minEffortTime))
            {
                checklistFilter.Date ??= new ChecklistDateFilter();
                checklistFilter.Date.MinEffortTime = minEffortTime;
            }

            return (null!, checklistFilter);
            //return (observationFilter, checklistFilter);
        }

        public static SearchFilterInternal ToSearchFilterInternal(this SignalFilterDto searchFilterDto, int userId, bool sensitiveObservations)
        {
            if (searchFilterDto == null)
            {
                return null!;
            }

            var searchFilter = new SearchFilterInternal(userId, sensitiveObservations ? ProtectionFilter.Sensitive : ProtectionFilter.Public)
            {
                BirdNestActivityLimit = searchFilterDto.BirdNestActivityLimit,
                DataProviderIds = searchFilterDto.DataProvider?.Ids?.ToList(),
                Location = new LocationFilter
                {
                    Areas = searchFilterDto.Geographics?.Areas?.Select(a => new AreaFilter { FeatureId = a.FeatureId, AreaType = (AreaType)a.AreaType }).ToList(),
                    Geometries = searchFilterDto.Geographics == null
                        ? null
                        : new GeographicsFilter
                        {
                            BoundingBox = searchFilterDto.Geographics.BoundingBox?.ToLatLonBoundingBox(),
                            Geometries = searchFilterDto.Geographics.Geometries?.ToList(),
                            MaxDistanceFromPoint = searchFilterDto.Geographics.MaxDistanceFromPoint,
                            UseDisturbanceRadius = searchFilterDto.Geographics.ConsiderDisturbanceRadius,
                            UsePointAccuracy = searchFilterDto.Geographics.ConsiderObservationAccuracy,
                            UseAuthorizationBuffer = searchFilterDto.Geographics.ConsiderAuthorizationBuffer
                        },
                    MaxAccuracy = searchFilterDto.Geographics?.MaxAccuracy
                },
                NotPresentFilter = SightingNotPresentFilter.DontIncludeNotPresent,
                NotRecoveredFilter = SightingNotRecoveredFilter.DontIncludeNotRecovered,
                PositiveSightings = true,
                Date = searchFilterDto.StartDate.HasValue ? new DateFilter
                {
                    StartDate = searchFilterDto.StartDate
                } : null,
                Taxa = searchFilterDto.Taxon?.ToTaxonFilter(),
                UnspontaneousFilter = SightingUnspontaneousFilter.NotUnspontaneous
            };

            return searchFilter;
        }

        /// <summary>
        /// Cast taxon filter dto to taxon filter
        /// </summary>
        /// <param name="filterDto"></param>
        /// <returns></returns>
        public static TaxonFilter ToTaxonFilter(this TaxonFilterBaseDto filterDto)
        {
            if (filterDto == null)
            {
                return null!;
            }

            var filter = new TaxonFilter
            {
                Ids = filterDto.Ids,
                IncludeUnderlyingTaxa = filterDto.IncludeUnderlyingTaxa ?? false,
                IsInvasiveInSweden = filterDto.IsInvasiveInSweden,
                ListIds = filterDto.TaxonListIds,
                TaxonListOperator = TaxonFilter.TaxonListOp.Merge,
            };

            if (filterDto is TaxonFilterDto taxonFilterDto)
            {
                filter.RedListCategories = taxonFilterDto.RedListCategories;
                filter.TaxonListOperator =
                    (TaxonFilter.TaxonListOp)(taxonFilterDto?.TaxonListOperator).GetValueOrDefault();
                filter.TaxonCategories = taxonFilterDto?.TaxonCategories!;
            }

            return filter;
        }

    }
}