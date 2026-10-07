using SOS.Api.Contracts.Observations;
using CSharpFunctionalExtensions;
using NetTopologySuite.Geometries;
using SOS.Api.Contracts.Checklists.Requests;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Input;
using static SOS.Lib.Models.Search.Filters.SearchFilterBase;

namespace SOS.Lib.Extensions.Dto;

/// <summary>
/// Converts API filter DTOs to search models and handles their geometry serialization boundary.
/// </summary>
public static class FilterExtensions
{
    /// <summary>
    /// Gets the geometries used by the search, reusing server geometries when available.
    /// For a GeoJSON-based DTO, converts its geometries to NetTopologySuite instead.
    /// </summary>
    /// <exception cref="ArgumentException">A supplied geometry could not be converted.</exception>
    public static List<Geometry>? GetSearchGeometries(this GeographicsFilterDto? source)
    {
        var result = source.TryGetSearchGeometries();
        if (result.IsFailure) throw new ArgumentException(result.Error, nameof(source));
        return result.Value;
    }

    /// <summary>Gets search geometries or a conversion error, never a partially converted collection.</summary>
    public static Result<List<Geometry>?> TryGetSearchGeometries(this GeographicsFilterDto? source)
    {
        if (source is GeographicsFilterInputModel input)
        {
            return input.GeometryConversionError is { } error
                ? Result.Failure<List<Geometry>?>(error)
                : Result.Success(input.ServerGeometries);
        }

        try
        {
            return Result.Success(source?.Geometries?
                .Select(geometry => geometry.ToSearchGeometry())
                .Where(geometry => geometry is not null)
                .Cast<Geometry>()
                .ToList());
        }
        catch (ArgumentException)
        {
            return Result.Failure<List<Geometry>?>("Invalid geometry");
        }
    }

    /// <summary>Checks whether search geometries were supplied without converting GeoJSON objects.</summary>
    public static bool HasSearchGeometries(this GeographicsFilterDto? source) =>
        source is GeographicsFilterInputModel input
            ? input.GeometryConversionError is not null || input.ServerGeometries is { Count: > 0 }
            : source?.Geometries?.Any(geometry => geometry is not null) == true;

    /// <summary>Checks for geometry conversion errors, not geometric validity.</summary>
    public static bool HasGeometryConversionError(this GeographicsFilterDto? source) =>
        source.TryGetSearchGeometries().IsFailure;

    /// <summary>Creates GeoJSON geometries when a filter must be serialized.</summary>
    public static void MaterializeGeoJsonGeometriesForSerialization(this SearchFilterBaseDto filter)
    {
        if (filter.Geographics is GeographicsFilterInputModel input)
        {
            filter.Geographics = input.ToSerializableGeographicsFilterDto();
        }
    }

    private static SearchFilterBase PopulateFilter(SearchFilterBaseDto inputFilter, int userId, ProtectionFilterDto? protectionFilter, string translationCultureCode)
    {
        if (inputFilter == null)
        {
            return default!;
        }
        protectionFilter ??= ProtectionFilterDto.Public;

        var filter = inputFilter is SearchFilterInternalBaseDto ?
            new SearchFilterInternal(userId, protectionFilter.ToFilter()) :
            new SearchFilter(userId, protectionFilter.ToFilter());
        filter.Taxa = inputFilter.Taxon?.ToTaxonFilter();
        filter.DataStewardship = PopulateDataStewardshipFilter(inputFilter.DataStewardship);
        filter.Date = PopulateDateFilter(inputFilter.Date);
        filter.DataProviderIds = inputFilter.DataProvider?.Ids?.ToList();
        filter.Event = PopulateEventFilter(inputFilter.Event!);
        filter.FieldTranslationCultureCode = translationCultureCode;
        filter.NotRecoveredFilter = (SightingNotRecoveredFilter)(inputFilter.NotRecoveredFilter ?? SightingNotRecoveredFilterDto.NoFilter);
        //filter.VerificationStatus = inputFilter.ValidationStatus.HasValue ? (SearchFilterBase.StatusVerification)inputFilter.ValidationStatus.Value.ToStatusVerification() : (SearchFilterBase.StatusVerification)inputFilter.VerificationStatus;
        filter.VerificationStatus = (SearchFilterBase.StatusVerification)(inputFilter.VerificationStatus ?? StatusVerificationDto.BothVerifiedAndNotVerified);
        filter.ProjectIds = inputFilter.ProjectIds;
        filter.BirdNestActivityLimit = inputFilter.BirdNestActivityLimit;
        filter.TypeFilter = (SightingTypeFilter)(inputFilter.Artportalen?.TypeFilter ??
            SightingTypeFilterDto.DoNotShowMerged);
        filter.Location = PopulateLocationFilter(inputFilter.Geographics!);
        filter.ModifiedDate = inputFilter.ModifiedDate == null
            ? null
            : new ModifiedDateFilter
            {
                From = inputFilter.ModifiedDate.From?.UtcDateTime,
                To = inputFilter.ModifiedDate.To?.UtcDateTime
            };
        filter.ExtendedAuthorization.ObservedByMe = inputFilter.ObservedByMe;
        filter.ExtendedAuthorization.ReportedByMe = inputFilter.ReportedByMe;
        bool isOnlyNotPresentOrRecoveredFilterSet = false;
        if (inputFilter.NotRecoveredFilter == SightingNotRecoveredFilterDto.OnlyNotRecovered ||
            inputFilter.NotRecoveredFilter == SightingNotRecoveredFilterDto.IncludeNotRecovered)
        {
            isOnlyNotPresentOrRecoveredFilterSet = true;
        }
        
        filter.DiffusionStatuses = inputFilter.DiffusionStatuses?.ToList();
        filter.DeterminationFilter = (SightingDeterminationFilter)(inputFilter.DeterminationFilter ?? SightingDeterminationFilterDto.NoFilter);
        filter.Output = new OutputFilter();
        if (inputFilter is SearchFilterDto publicInput)
        {
            filter.Output.Fields = publicInput.Output?.Fields?.ToList();
            filter.Output.PopulateFields(publicInput.Output?.FieldSet);
        }
        if (inputFilter is SearchFilterInternalBaseDto internalBaseInput)
        {
            var filterInternal = (SearchFilterInternal)filter;
            PopulateInternalBase(internalBaseInput, filterInternal);
            if (filterInternal.NotPresentFilter == SightingNotPresentFilter.OnlyNotPresent || 
                filterInternal.NotPresentFilter == SightingNotPresentFilter.IncludeNotPresent)
            {
                isOnlyNotPresentOrRecoveredFilterSet = true;
            }

            if (inputFilter is SearchFilterInternalDto internalInput)
            {
                filterInternal.IncludeRealCount = internalInput.IncludeRealCount;
                filterInternal.Output.Fields = internalInput.Output?.Fields?.ToList();
                filterInternal.Output.PopulateFields(internalInput.Output?.FieldSet);
                filterInternal.Output.SortOrders = internalInput.Output?.SortOrders?.Select(so => new SortOrderFilter
                {
                    SortBy = so.SortBy,
                    SortOrder = so.SortOrder
                });
            }

            filter.IncludeSensitiveGeneralizedObservations = internalBaseInput?.GeneralizationFilter?.SensitiveGeneralizationFilter switch
            {
                SensitiveGeneralizationFilterDto.DontIncludeGeneralizedObservations => false,
                SensitiveGeneralizationFilterDto.OnlyGeneralizedObservations => true,
                SensitiveGeneralizationFilterDto.IncludeGeneralizedObservations => null,
                null => false,
                _ => false
            };

            filter.IsPublicGeneralizedObservation = internalBaseInput?.GeneralizationFilter?.PublicGeneralizationFilter switch
            {
                PublicGeneralizationFilterDto.DontIncludeGeneralized => false,
                PublicGeneralizationFilterDto.OnlyGeneralized => true,
                PublicGeneralizationFilterDto.NoFilter => null,
                null => null,
                _ => null
            };                
        }

        if (inputFilter.OccurrenceStatus != null)
        {
            switch (inputFilter.OccurrenceStatus)
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

        if (inputFilter.ExcludeFilter != null)
        {
            filter.ExcludeFilter = new ExcludeFilter();
            filter.ExcludeFilter.OccurrenceIds = inputFilter.ExcludeFilter.OccurrenceIds;
        }

        if (inputFilter.IncludeFilter != null)
        {
            filter.IncludeFilter = new IncludeFilter();
            filter.IncludeFilter.OccurrenceIds = inputFilter.IncludeFilter.OccurrenceIds;
        }

        return filter;
    }

    private static void PopulateInternalBase(SearchFilterInternalBaseDto internalInput, SearchFilterInternal internalFilter)
    {
        if (internalInput.ExtendedFilter != null)
        {
            internalFilter.ChecklistId = internalInput.ExtendedFilter.ChecklistId;
            internalFilter.FieldDiaryGroupIds = internalInput.ExtendedFilter.FieldDiaryGroupIds;
            internalFilter.ReportedByUserId = internalInput.ExtendedFilter.ReportedByUserId;
            internalFilter.ObservedByUserId = internalInput.ExtendedFilter.ObservedByUserId;
            internalFilter.ReportedByUserServiceUserId = internalInput.ExtendedFilter.ReportedByUserServiceUserId;
            internalFilter.ObservedByUserServiceUserId = internalInput.ExtendedFilter.ObservedByUserServiceUserId;
            internalFilter.OnlyWithMedia = internalInput.ExtendedFilter.OnlyWithMedia;
            internalFilter.OnlyWithNotes = internalInput.ExtendedFilter.OnlyWithNotes;
            internalFilter.OnlyWithNotesOfInterest = internalInput.ExtendedFilter.OnlyWithNotesOfInterest;
            internalFilter.OnlyWithUserComments = internalInput.ExtendedFilter.OnlyWithUserComments;
            internalFilter.OnlyWithBarcode = internalInput.ExtendedFilter.OnlyWithBarcode;
            internalFilter.ReportedDateFrom = internalInput.ExtendedFilter.ReportedDateFrom?.UtcDateTime;
            internalFilter.ReportedDateTo = internalInput.ExtendedFilter.ReportedDateTo?.UtcDateTime;
            internalFilter.TypeFilter = (SearchFilterInternal.SightingTypeFilter)internalInput.ExtendedFilter.TypeFilter;
            internalFilter.UsePeriodForAllYears = internalInput.ExtendedFilter.UsePeriodForAllYears;
            internalFilter.Months = internalInput.ExtendedFilter.Months;
            internalFilter.MonthsComparison = (DateFilterComparison)internalInput.ExtendedFilter.MonthsComparison;
            internalFilter.DiscoveryMethodIds = internalInput.ExtendedFilter.DiscoveryMethodIds;
            internalFilter.LifeStageIds = internalInput.ExtendedFilter.LifeStageIds;
            internalFilter.ActivityIds = internalInput.ExtendedFilter.ActivityIds;
            //internalFilter.HasTriggeredVerificationRule = internalInput.ExtendedFilter.HasTriggerdValidationRule ?? internalInput.ExtendedFilter.HasTriggeredVerificationRule;
            internalFilter.HasTriggeredVerificationRule = internalInput.ExtendedFilter.HasTriggeredVerificationRule;
            /*internalFilter.HasTriggeredVerificationRuleWithWarning =
                internalInput.ExtendedFilter.HasTriggerdValidationRuleWithWarning ?? internalInput.ExtendedFilter.HasTriggeredVerificationRuleWithWarning;*/
            internalFilter.HasTriggeredVerificationRuleWithWarning = internalInput.ExtendedFilter.HasTriggeredVerificationRuleWithWarning;
            internalFilter.Length = internalInput.ExtendedFilter.Length;
            internalFilter.LengthOperator = internalInput.ExtendedFilter.LengthOperator;
            internalFilter.Weight = internalInput.ExtendedFilter.Weight;
            internalFilter.WeightOperator = internalInput.ExtendedFilter.WeightOperator;
            internalFilter.Quantity = internalInput.ExtendedFilter.Quantity;
            internalFilter.QuantityOperator = internalInput.ExtendedFilter.QuantityOperator;
            //internalFilter.VerificationStatusIds = internalInput.ExtendedFilter.ValidationStatusIds != null && internalInput.ExtendedFilter.ValidationStatusIds.Any() ? internalInput.ExtendedFilter.ValidationStatusIds : internalInput.ExtendedFilter.VerificationStatusIds;
            internalFilter.VerificationStatusIds = internalInput.ExtendedFilter.VerificationStatusIds;
            //internalFilter.ExcludeVerificationStatusIds = internalInput.ExtendedFilter.ExcludeValidationStatusIds != null && internalInput.ExtendedFilter.ExcludeValidationStatusIds.Any() ? internalInput.ExtendedFilter.ExcludeValidationStatusIds : internalInput.ExtendedFilter.ExcludeVerificationStatusIds;
            internalFilter.ExcludeVerificationStatusIds = internalInput.ExtendedFilter.ExcludeVerificationStatusIds;
            internalFilter.UnspontaneousFilter =
                (SightingUnspontaneousFilter)internalInput.ExtendedFilter
                    .UnspontaneousFilter;
            internalFilter.SpeciesCollectionLabel = internalInput.ExtendedFilter.SpeciesCollectionLabel;
            internalFilter.PublicCollection = internalInput.ExtendedFilter.PublicCollection;
            internalFilter.PrivateCollection = internalInput.ExtendedFilter.PrivateCollection;
            internalFilter.SubstrateSpeciesId = internalInput.ExtendedFilter.SubstrateSpeciesId;
            internalFilter.SubstrateId = internalInput.ExtendedFilter.SubstrateId;
            internalFilter.BiotopeId = internalInput.ExtendedFilter.BiotopeId;
            internalFilter.NotPresentFilter =
                (SightingNotPresentFilter)internalInput.ExtendedFilter.NotPresentFilter;
            internalFilter.OnlySecondHandInformation = internalInput.ExtendedFilter.OnlySecondHandInformation;
            internalFilter.PublishTypeIdsFilter = internalInput.ExtendedFilter.PublishTypeIdsFilter;
            internalFilter.TriggeredObservationRuleFrequencyIds =
                internalInput.ExtendedFilter.TriggeredObservationRuleFrequencyIds;
            internalFilter.TriggeredObservationRuleReproductionIds =
                internalInput.ExtendedFilter.TriggeredObservationRuleReproductionIds;
            internalFilter.SightingTypeSearchGroupIds = internalInput.ExtendedFilter.SightingTypeSearchGroupIds;
            internalFilter.SiteIds = internalInput.ExtendedFilter.SiteIds;
            internalFilter.SiteProjectIds = internalInput.ExtendedFilter.SiteProjectIds;
            internalFilter.SpeciesFactsIds = internalInput.ExtendedFilter.SpeciesFactsIds;
            internalFilter.InstitutionId = internalInput.ExtendedFilter.InstitutionId;
            internalFilter.DatasourceIds = internalInput.ExtendedFilter.DatasourceIds;
            /*if (internalInput?.ExtendedFilter?.LocationNameFilter != null)
            {
                if (internalFilter.Location == null) internalFilter.Location = new LocationFilter();
                internalFilter.Location.NameFilter = internalInput.ExtendedFilter.LocationNameFilter;
            }*/

            if (internalInput.ExtendedFilter.SexIds?.Any() ?? false)
            {
                internalFilter.Taxa ??= new TaxonFilter();
                internalFilter.Taxa.SexIds = internalInput.ExtendedFilter.SexIds;
            }
            internalFilter.InvasiveSpeciesTreatmentIds = internalInput.ExtendedFilter.InvasiveSpeciesTreatmentIds;
            internalFilter.HasInvasiveSpeciesTreatment = internalInput.ExtendedFilter.HasInvasiveSpeciesTreatment;
            internalFilter.Years = internalInput.ExtendedFilter.Years;
            internalFilter.YearsComparison = (DateFilterComparison)internalInput.ExtendedFilter.YearsComparison;
        }
    }

    private static DataStewardshipFilter PopulateDataStewardshipFilter(DataStewardshipFilterDto filter)
    {
        return filter == null ? null! :
            new DataStewardshipFilter
            {
                DatasetIdentifiers = filter.DatasetIdentifiers,
                DatasetTitleFilter = filter.DatasetTitleFilter
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
            StartDate = filter.StartDate?.UtcDateTime,
            EndDate = filter.EndDate?.UtcDateTime,
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
            Geometries = PopulateGeographicsFilter(filter),
            IncludeObservationsOutsideSweden = filter.IncludeObservationsOutsideSweden,
            LocationIds = filter.LocationIds,
            NameFilter = filter.LocationNameFilter,
            MaxAccuracy = filter.MaxAccuracy
        };
    }

    private static GeographicsFilter? PopulateGeographicsFilter(GeographicsFilterDto? filter) =>
        filter == null ? null : new GeographicsFilter
        {
            BoundingBox = filter.BoundingBox.GetCompleteBoundingBox(),
            Geometries = filter.GetSearchGeometries(),
            MaxDistanceFromPoint = filter.MaxDistanceFromPoint,
            UseDisturbanceRadius = filter.ConsiderDisturbanceRadius,
            UsePointAccuracy = filter.ConsiderObservationAccuracy,
            UseAuthorizationBuffer = filter.ConsiderAuthorizationBuffer
        };
    
    extension(ProtectionFilterDto? protectionFilter)
    {
        private ProtectionFilter ToFilter()
        {
            return protectionFilter switch
            {
                ProtectionFilterDto.BothPublicAndSensitive => ProtectionFilter.BothPublicAndSensitive,
                ProtectionFilterDto.Sensitive => ProtectionFilter.Sensitive,
                _ => ProtectionFilter.Public
            };
        }
    }

    public static SearchFilter ToSearchFilter(
        this SearchFilterDto source,
        int userId,
        ProtectionFilterDto? protectionFilter,
        string translationCultureCode,
        string sortBy,
        SearchSortOrder sortOrder)
    {
        var filter = source.ToSearchFilter(userId, protectionFilter, translationCultureCode);
        if (!string.IsNullOrEmpty(sortBy))
        {
            filter.Output.SortOrders = [new SortOrderFilter
            {
                SortBy = sortBy,
                SortOrder = sortOrder
            }];
        }
        return filter;
    }

    extension(SearchFilterBaseDto inputFilter)
    {
        public SearchFilter ToSearchFilter(int userId, ProtectionFilterDto? protectionFilterDto, string translationCultureCode)
        {
            return (SearchFilter)PopulateFilter(inputFilter, userId, protectionFilterDto, translationCultureCode);
        }
    }

    extension(SearchFilterInternalBaseDto inputFilter)
    {
        public SearchFilterInternal ToSearchFilterInternal(int userId, string translationCultureCode)
        {
            return (SearchFilterInternal)PopulateFilter(inputFilter, userId, inputFilter.ProtectionFilter, translationCultureCode);
        }
    }

    extension(SearchFilterInternalDto inputFilter)
    {
        public SearchFilterInternal ToSearchFilterInternal(int userId, string translationCultureCode, string sortBy, SearchSortOrder sortOrder)
        {
            // User sort order passed the old way if not any other sort order is passed 
            if (!string.IsNullOrEmpty(sortBy) && (!inputFilter?.Output?.SortOrders?.Any() ?? true))
            {
                inputFilter ??= new SearchFilterInternalDto();
                inputFilter.Output ??= new OutputFilterExtendedDto();
                inputFilter.Output.SortOrders = new[]
                {
                    new SortOrderDto { SortBy = sortBy, SortOrder = sortOrder }
                };
            }

            return (SearchFilterInternal)PopulateFilter(inputFilter!, userId, inputFilter?.ProtectionFilter, translationCultureCode);
        }
    }

    extension(CalculateTrendFilterDto searchFilterDto)
    {
        public (SearchFilter, ChecklistSearchFilter) ToSearchFilters()
        {
            var dateFilter = PopulateDateFilter(searchFilterDto.Date!);
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
                Date = dateFilter is null ? null : new ChecklistDateFilter
                {
                    StartDate = dateFilter.StartDate,
                    EndDate = dateFilter.EndDate,
                    DateFilterType = dateFilter.DateFilterType,
                    TimeRanges = dateFilter.TimeRanges
                },
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
    }

    extension(SignalFilterDto searchFilterDto)
    {
        public SearchFilterInternal ToSearchFilterInternal(int userId, bool sensitiveObservations)
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
                    Geometries = PopulateGeographicsFilter(searchFilterDto.Geographics),
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
                UnspontaneousFilter = SightingUnspontaneousFilter.NotUnspontaneous,
                TypeFilter = (SightingTypeFilter)searchFilterDto.ArtportalenTypeFilter
            };

            return searchFilter;
        }
    }

    public static TaxonFilter ToTaxonFilter(this TaxonFilterDto source)
    {
        return new TaxonFilter
        {
            Ids = source.Ids,
            IncludeUnderlyingTaxa = source.IncludeUnderlyingTaxa ?? false,
            IsInvasiveInSweden = source.IsInvasiveInSweden,
            ListIds = source.TaxonListIds,
            RedListCategories = source.RedListCategories,
            TaxonCategories = source.TaxonCategories,
            TaxonListOperator = (TaxonFilter.TaxonListOp)source.TaxonListOperator,
            TaxonomyType = source.TaxonomyType
        };
    }

    extension(TaxonFilterBaseDto filterDto)
    {
        /// <summary>
        /// Cast taxon filter dto to taxon filter
        /// </summary>
        /// <returns></returns>
        public TaxonFilter ToTaxonFilter()
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

            return filter;
        }
    }
}
