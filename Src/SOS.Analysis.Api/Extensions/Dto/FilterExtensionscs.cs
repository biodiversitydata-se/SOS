using SOS.Analysis.Api.Dtos.Filter;
using SOS.Analysis.Api.Dtos.Filter.Enums;
using SOS.Lib.Enums;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;

namespace SOS.Analysis.Api.Extensions.Dto
{
    public static class FilterExtensions
    {
        private static SearchFilterBase PopulateFilter(SearchFilterDto searchFilterDto, int userId, string translationCultureCode)
        {
            if (searchFilterDto == null) return default!;

            var filter = new SearchFilterInternal(userId, (ProtectionFilter)searchFilterDto.ProtectionFilter);
            filter.Taxa = searchFilterDto.Taxon?.ToTaxonFilter();
            filter.DataStewardship = ToDataStewardshipFilter(searchFilterDto.DataStewardship);
            filter.Date = ToDateFilter(searchFilterDto.Date!);
            filter.DataProviderIds = searchFilterDto.DataProvider?.Ids?.ToList();
            filter.Event = ToEventFilter(searchFilterDto.Event);
            filter.FieldTranslationCultureCode = translationCultureCode;
            filter.NotRecoveredFilter = (SightingNotRecoveredFilter)searchFilterDto.NotRecoveredFilter;
            filter.VerificationStatus = (SearchFilterBase.StatusVerification)searchFilterDto.VerificationStatus;
            filter.ProjectIds = searchFilterDto.ProjectIds?.ToList();
            filter.BirdNestActivityLimit = searchFilterDto.BirdNestActivityLimit;
            filter.Location = ToLocationFilter(searchFilterDto.Geographics!);
            filter.ModifiedDate = searchFilterDto.ModifiedDate == null
                ? null
                : new ModifiedDateFilter
                {
                    From = searchFilterDto.ModifiedDate.From,
                    To = searchFilterDto.ModifiedDate.To
                };
            filter.ExtendedAuthorization.ObservedByMe = searchFilterDto.ObservedByMe ?? false;
            filter.ExtendedAuthorization.ReportedByMe = searchFilterDto.ReportedByMe ?? false;

            if (searchFilterDto.OccurrenceStatus.HasValue)
            {
                filter.PositiveSightings = searchFilterDto.OccurrenceStatus.Value switch
                {
                    OccurrenceStatusFilterValuesDto.Present => true,
                    OccurrenceStatusFilterValuesDto.Absent => false,
                    _ => null
                };
            }

            filter.DiffusionStatuses = searchFilterDto.DiffusionStatuses?.Select(dsd => (DiffusionStatus)dsd)?.ToList();
            filter.DeterminationFilter = (SightingDeterminationFilter)searchFilterDto.DeterminationFilter;

            if (searchFilterDto is SearchFilterInternalDto searchFilterInternalDto)
            {
                PopulateInternalBase(searchFilterInternalDto, filter);
            }

            if (searchFilterDto.ExcludeFilter != null)
            {
                filter.ExcludeFilter = new ExcludeFilter();
                filter.ExcludeFilter.OccurrenceIds = searchFilterDto.ExcludeFilter.OccurrenceIds;
            }

            return filter;
        }

        private static void PopulateInternalBase(SearchFilterInternalDto searchFilterInternalDto, SearchFilterInternal internalFilter)
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
                internalFilter.Length = searchFilterInternalDto.ExtendedFilter.Length;
                internalFilter.LengthOperator = searchFilterInternalDto.ExtendedFilter.LengthOperator;
                internalFilter.Weight = searchFilterInternalDto.ExtendedFilter.Weight;
                internalFilter.WeightOperator = searchFilterInternalDto.ExtendedFilter.WeightOperator;
                internalFilter.Quantity = searchFilterInternalDto.ExtendedFilter.Quantity;
                internalFilter.QuantityOperator = searchFilterInternalDto.ExtendedFilter.QuantityOperator;
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

                if (searchFilterInternalDto.ExtendedFilter.SexIds?.Any() ?? false)
                {
                    internalFilter.Taxa ??= new TaxonFilter();
                    internalFilter.Taxa.SexIds = searchFilterInternalDto.ExtendedFilter.SexIds;
                }
                internalFilter.Years = searchFilterInternalDto.ExtendedFilter.Years;
                internalFilter.YearsComparison = (DateFilterComparison)searchFilterInternalDto.ExtendedFilter.YearsComparison;
            }
        }

        private static DataStewardshipFilter? ToDataStewardshipFilter(DataStewardshipFilterDto? filter)
        {
            return filter == null ? null :
                new DataStewardshipFilter
                {
                    DatasetIdentifiers = filter.DatasetIdentifiers
                };
        }

        private static DateFilter ToDateFilter(DateFilterDto filter)
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
                TimeRanges = filter!.TimeRanges?.Select(tr => (DateFilter.TimeRange)tr)
            };
        }

        private static EventFilter? ToEventFilter(EventFilterDto? filter)
        {
            return filter == null ? null :
                new EventFilter
                {
                    Ids = filter.Ids
                };
        }

        /// <summary>
        /// Cast lat lon bounding box dto 
        /// </summary>
        /// <param name="latLonBoundingBox"></param>
        /// <returns></returns>
        private static LatLonBoundingBox ToLatLonBoundingBox(this LatLonBoundingBoxDto latLonBoundingBox)
        {
            if (latLonBoundingBox?.TopLeft == null || latLonBoundingBox?.BottomRight == null)
            {
                return null!;
            }

            return new LatLonBoundingBox
            {
                TopLeft = latLonBoundingBox.TopLeft.ToLatLonCoordinate(),
                BottomRight = latLonBoundingBox.BottomRight.ToLatLonCoordinate()
            };
        }

        /// <summary>
        /// Cast dto Coordinate
        /// </summary>
        /// <param name="latLonCoordinate"></param>
        /// <returns></returns>
        private static LatLonCoordinate ToLatLonCoordinate(this LatLonCoordinateDto latLonCoordinate)
        {
            if (latLonCoordinate == null)
            {
                return null!;
            }

            return new LatLonCoordinate(latLonCoordinate.Latitude, latLonCoordinate.Longitude);
        }

        private static LocationFilter ToLocationFilter(GeographicsFilterDto filter)
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
                },
                LocationIds = filter.LocationIds,
                NameFilter = filter.LocationNameFilter,
                MaxAccuracy = filter.MaxAccuracy
            };
        }

        private static TaxonFilter ToTaxonFilter(this TaxonFilterDto filterDto)
        {
            if (filterDto == null)
            {
                return null!;
            }

            return new TaxonFilter
            {
                Ids = filterDto.Ids,
                IncludeUnderlyingTaxa = filterDto.IncludeUnderlyingTaxa,
                ListIds = filterDto.TaxonListIds,
                RedListCategories = filterDto.RedListCategories,
                TaxonCategories = filterDto!.TaxonCategories,
                TaxonListOperator =
                    (TaxonFilter.TaxonListOp)(filterDto?.TaxonListOperator).GetValueOrDefault()
            };
        }

        public static SearchFilterInternal ToSearchFilter(this SearchFilterInternalDto searchFilterDto, int userId, string translationCultureCode)
        {
            return (SearchFilterInternal)PopulateFilter(searchFilterDto, userId, translationCultureCode);
        }
    }
}
