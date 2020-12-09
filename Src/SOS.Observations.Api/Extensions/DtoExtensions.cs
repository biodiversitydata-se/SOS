using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Enums;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos.Vocabulary;

namespace SOS.Observations.Api.Extensions
{
    public static class DtoExtensions
    {
        private static FilterBase PopulateFilter(SearchFilterBaseDto searchFilterBaseDto, string translationCultureCode)
        {
            if (searchFilterBaseDto == null) return default;

            var isInternalFilter = searchFilterBaseDto is SearchFilterInternalBaseDto;

            var filter = isInternalFilter ? new SearchFilterInternal() : new SearchFilter();
            filter.StartDate = searchFilterBaseDto.Date?.StartDate;
            filter.EndDate = searchFilterBaseDto.Date?.EndDate;
            filter.DateFilterType = (FilterBase.DateRangeFilterType)(searchFilterBaseDto.Date?.DateFilterType).GetValueOrDefault();
            filter.Areas = searchFilterBaseDto.Areas?.Select(a => new AreaFilter { FeatureId = a.FeatureId, Type = a.Type });
            filter.TaxonIds = searchFilterBaseDto.Taxon?.TaxonIds;
            filter.IncludeUnderlyingTaxa = (searchFilterBaseDto.Taxon?.IncludeUnderlyingTaxa).GetValueOrDefault();
            filter.RedListCategories = searchFilterBaseDto.Taxon?.RedListCategories;
            filter.DataProviderIds = searchFilterBaseDto.DataProviderIds;
            filter.FieldTranslationCultureCode = translationCultureCode;
            filter.OnlyValidated = searchFilterBaseDto.OnlyValidated;
            filter.GeometryFilter = searchFilterBaseDto.Geometry == null ? null : new GeometryFilter
            {
                Geometries = searchFilterBaseDto.Geometry.Geometries,
                MaxDistanceFromPoint = searchFilterBaseDto.Geometry.MaxDistanceFromPoint,
                UsePointAccuracy = searchFilterBaseDto.Geometry.UsePointAccuracy
            };

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

            if (searchFilterBaseDto is SearchFilterInternalBaseDto searchFilterInternalBaseDto)
            {
                PopulateInternalBase(searchFilterInternalBaseDto, filter as SearchFilterInternal);

                if (searchFilterBaseDto is SearchFilterInternalDto searchFilterInternalDto)
                {
                    (filter as SearchFilterInternal).IncludeRealCount = searchFilterInternalDto.IncludeRealCount;
                    filter.OutputFields = searchFilterInternalDto.OutputFields;
                }
            } 
            
            if (searchFilterBaseDto is SearchFilterDto searchFilterDto)
            {
                filter.OutputFields = searchFilterDto.OutputFields;
            }

            
            return filter;
        }

        private static void PopulateInternalBase(SearchFilterInternalBaseDto searchFilterInternalDto, SearchFilterInternal internalFilter)
        {
            if (searchFilterInternalDto.ArtportalenFilter != null)
            {
                internalFilter.ReportedByUserId = searchFilterInternalDto.ArtportalenFilter.ReportedByUserId;
                internalFilter.ObservedByUserId = searchFilterInternalDto.ArtportalenFilter.ObservedByUserId;
                internalFilter.ProjectIds = searchFilterInternalDto.ArtportalenFilter.ProjectIds;
                internalFilter.BoundingBox = searchFilterInternalDto.ArtportalenFilter.BoundingBox;
                internalFilter.OnlyWithMedia = searchFilterInternalDto.ArtportalenFilter.OnlyWithMedia;
                internalFilter.OnlyWithNotes = searchFilterInternalDto.ArtportalenFilter.OnlyWithNotes;
                internalFilter.OnlyWithNotesOfInterest = searchFilterInternalDto.ArtportalenFilter.OnlyWithNotesOfInterest;
                internalFilter.OnlyWithBarcode = searchFilterInternalDto.ArtportalenFilter.OnlyWithBarcode;
                internalFilter.ReportedDateFrom = searchFilterInternalDto.ArtportalenFilter.ReportedDateFrom;
                internalFilter.ReportedDateTo = searchFilterInternalDto.ArtportalenFilter.ReportedDateTo;
                internalFilter.TypeFilter = (SearchFilterInternal.SightingTypeFilter)searchFilterInternalDto.ArtportalenFilter.TypeFilter;
                internalFilter.MaxAccuracy = searchFilterInternalDto.ArtportalenFilter.MaxAccuracy;
                internalFilter.UsePeriodForAllYears = searchFilterInternalDto.ArtportalenFilter.UsePeriodForAllYears;
                internalFilter.Months = searchFilterInternalDto.ArtportalenFilter.Months;
                internalFilter.DiscoveryMethodIds = searchFilterInternalDto.ArtportalenFilter.DiscoveryMethodIds;
                internalFilter.LifeStageIds = searchFilterInternalDto.ArtportalenFilter.LifeStageIds;
                internalFilter.ActivityIds = searchFilterInternalDto.ArtportalenFilter.ActivityIds;
                internalFilter.HasTriggerdValidationRule = searchFilterInternalDto.ArtportalenFilter.HasTriggerdValidationRule;
                internalFilter.HasTriggerdValidationRuleWithWarning =
                    searchFilterInternalDto.ArtportalenFilter.HasTriggerdValidationRuleWithWarning;
                internalFilter.Length = searchFilterInternalDto.ArtportalenFilter.Length;
                internalFilter.LengthOperator = searchFilterInternalDto.ArtportalenFilter.LengthOperator;
                internalFilter.Weight = searchFilterInternalDto.ArtportalenFilter.Weight;
                internalFilter.WeightOperator = searchFilterInternalDto.ArtportalenFilter.WeightOperator;
                internalFilter.Quantity = searchFilterInternalDto.ArtportalenFilter.Quantity;
                internalFilter.QuantityOperator = searchFilterInternalDto.ArtportalenFilter.QuantityOperator;
                internalFilter.ValidationStatusIds = searchFilterInternalDto.ArtportalenFilter.ValidationStatusIds;
                internalFilter.ExcludeValidationStatusIds = searchFilterInternalDto.ArtportalenFilter.ExcludeValidationStatusIds;
                internalFilter.DeterminationFilter =
                    (SightingDeterminationFilter)searchFilterInternalDto.ArtportalenFilter
                        .DeterminationFilter;
                internalFilter.UnspontaneousFilter =
                    (SightingUnspontaneousFilter)searchFilterInternalDto.ArtportalenFilter
                        .UnspontaneousFilter;
                internalFilter.NotRecoveredFilter =
                    (SightingNotRecoveredFilter)searchFilterInternalDto.ArtportalenFilter
                        .NotRecoveredFilter;
                internalFilter.SpeciesCollectionLabel = searchFilterInternalDto.ArtportalenFilter.SpeciesCollectionLabel;
                internalFilter.PublicCollection = searchFilterInternalDto.ArtportalenFilter.PublicCollection;
                internalFilter.PrivateCollection = searchFilterInternalDto.ArtportalenFilter.PrivateCollection;
                internalFilter.SubstrateSpeciesId = searchFilterInternalDto.ArtportalenFilter.SubstrateSpeciesId;
                internalFilter.SubstrateId = searchFilterInternalDto.ArtportalenFilter.SubstrateId;
                internalFilter.BiotopeId = searchFilterInternalDto.ArtportalenFilter.BiotopeId;
                internalFilter.NotPresentFilter =
                    (SightingNotPresentFilter)searchFilterInternalDto.ArtportalenFilter.NotPresentFilter;
                internalFilter.OnlySecondHandInformation = searchFilterInternalDto.ArtportalenFilter.OnlySecondHandInformation;
                internalFilter.PublishTypeIdsFilter = searchFilterInternalDto.ArtportalenFilter.PublishTypeIdsFilter;
                internalFilter.RegionalSightingStateIdsFilter =
                    searchFilterInternalDto.ArtportalenFilter.RegionalSightingStateIdsFilter;
                internalFilter.SiteIds = searchFilterInternalDto.ArtportalenFilter.SiteIds;
                internalFilter.SpeciesFactsIds = searchFilterInternalDto.ArtportalenFilter.SpeciesFactsIds;
            }

        }

        public static GeoGridResultDto ToGeoGridResultDto(this GeoGridTileResult geoGridTileResult)
        {
            return new GeoGridResultDto
            {
                Zoom = geoGridTileResult.Zoom,
                GridCellCount = geoGridTileResult.GridCellTileCount,
                BoundingBox = geoGridTileResult.BoundingBox.ToLatLonBoundingBoxDto(),
                GridCells = geoGridTileResult.GridCellTiles.Select(cell => cell.ToGeoGridCellDto())
            };
        }

        public static LatLonBoundingBoxDto ToLatLonBoundingBoxDto(this LatLonBoundingBox latLonBoundingBox)
        {
            return new LatLonBoundingBoxDto
            {
                TopLeft = latLonBoundingBox.TopLeft.ToLatLonCoordinateDto(),
                BottomRight = latLonBoundingBox.BottomRight.ToLatLonCoordinateDto()
            };
        }

        public static LatLonCoordinateDto ToLatLonCoordinateDto(this LatLonCoordinate latLonCoordinate)
        {
            return new LatLonCoordinateDto
            {
                Latitude = latLonCoordinate.Latitude,
                Longitude = latLonCoordinate.Longitude
            };
        }

        public static GeoGridCellDto ToGeoGridCellDto(this GridCellTile gridCellTile)
        {
            return new GeoGridCellDto
            {
                X = gridCellTile.X,
                Y = gridCellTile.Y,
                TaxaCount = gridCellTile.TaxaCount,
                ObservationsCount = gridCellTile.ObservationsCount,
                Zoom = gridCellTile.Zoom,
                BoundingBox = gridCellTile.BoundingBox.ToLatLonBoundingBoxDto()
            };
        }

        public static IEnumerable<TaxonAggregationItemDto> ToTaxonAggregationItemDtos(this IEnumerable<TaxonAggregationItem> taxonAggregationItems)
        {
            return taxonAggregationItems.Select(item => item.ToTaxonAggregationItemDto());
        }

        public static TaxonAggregationItemDto ToTaxonAggregationItemDto(this TaxonAggregationItem taxonAggregationItem)
        {
            return new TaxonAggregationItemDto
            {
                TaxonId = taxonAggregationItem.TaxonId,
                ObservationCount = taxonAggregationItem.ObservationCount
            };
        }

        public static PagedResultDto<TRecordDto> ToPagedResultDto<TRecord, TRecordDto>(this PagedResult<TRecord> pagedResult, IEnumerable<TRecordDto> records)
        {
            return new PagedResultDto<TRecordDto>
            {
                Records = records,
                Skip = pagedResult.Skip,
                Take = pagedResult.Take,
                TotalCount = pagedResult.TotalCount
            };
        }

       /* public static FilterBase ToSearchFilter(this SearchFilterBaseDto searchFilterDto, string translationCultureCode)
        {
            return PopulateFilter(searchFilterDto, translationCultureCode);
        }

        public static SearchFilterInternal ToSearchFilterInternal(this SearchFilterInternalBaseDto searchFilterDto, string translationCultureCode)
        {
            if (searchFilterDto == null) return null;


            var x = (searchFilterDto as SearchFilterInternalDto).ToSearchFilter(translationCultureCode);

            var filter = new SearchFilterInternal((searchFilterDto as SearchFilterInternalDto).ToSearchFilter(translationCultureCode));

            if (searchFilterDto.ArtportalenFilter != null)
            {
                filter.ReportedByUserId = searchFilterDto.ArtportalenFilter.ReportedByUserId;
                filter.ObservedByUserId = searchFilterDto.ArtportalenFilter.ObservedByUserId;
                filter.ProjectIds = searchFilterDto.ArtportalenFilter.ProjectIds;
                filter.BoundingBox = searchFilterDto.ArtportalenFilter.BoundingBox;
                filter.OnlyWithMedia = searchFilterDto.ArtportalenFilter.OnlyWithMedia;
                filter.OnlyWithNotes = searchFilterDto.ArtportalenFilter.OnlyWithNotes;
                filter.OnlyWithNotesOfInterest = searchFilterDto.ArtportalenFilter.OnlyWithNotesOfInterest;
                filter.OnlyWithBarcode = searchFilterDto.ArtportalenFilter.OnlyWithBarcode;
                filter.ReportedDateFrom = searchFilterDto.ArtportalenFilter.ReportedDateFrom;
                filter.ReportedDateTo = searchFilterDto.ArtportalenFilter.ReportedDateTo;
                filter.TypeFilter = (SearchFilterInternal.SightingTypeFilter)searchFilterDto.ArtportalenFilter.TypeFilter;
                filter.MaxAccuracy = searchFilterDto.ArtportalenFilter.MaxAccuracy;
                filter.UsePeriodForAllYears = searchFilterDto.ArtportalenFilter.UsePeriodForAllYears;
                filter.Months = searchFilterDto.ArtportalenFilter.Months;
                filter.DiscoveryMethodIds = searchFilterDto.ArtportalenFilter.DiscoveryMethodIds;
                filter.LifeStageIds = searchFilterDto.ArtportalenFilter.LifeStageIds;
                filter.ActivityIds = searchFilterDto.ArtportalenFilter.ActivityIds;
                filter.HasTriggerdValidationRule = searchFilterDto.ArtportalenFilter.HasTriggerdValidationRule;
                filter.HasTriggerdValidationRuleWithWarning =
                    searchFilterDto.ArtportalenFilter.HasTriggerdValidationRuleWithWarning;
                filter.Length = searchFilterDto.ArtportalenFilter.Length;
                filter.LengthOperator = searchFilterDto.ArtportalenFilter.LengthOperator;
                filter.Weight = searchFilterDto.ArtportalenFilter.Weight;
                filter.WeightOperator = searchFilterDto.ArtportalenFilter.WeightOperator;
                filter.Quantity = searchFilterDto.ArtportalenFilter.Quantity;
                filter.QuantityOperator = searchFilterDto.ArtportalenFilter.QuantityOperator;
                filter.ValidationStatusIds = searchFilterDto.ArtportalenFilter.ValidationStatusIds;
                filter.ExcludeValidationStatusIds = searchFilterDto.ArtportalenFilter.ExcludeValidationStatusIds;
                filter.DeterminationFilter =
                    (SightingDeterminationFilter)searchFilterDto.ArtportalenFilter
                        .DeterminationFilter;
                filter.UnspontaneousFilter =
                    (SightingUnspontaneousFilter)searchFilterDto.ArtportalenFilter
                        .UnspontaneousFilter;
                filter.NotRecoveredFilter =
                    (SightingNotRecoveredFilter)searchFilterDto.ArtportalenFilter
                        .NotRecoveredFilter;
                filter.SpeciesCollectionLabel = searchFilterDto.ArtportalenFilter.SpeciesCollectionLabel;
                filter.PublicCollection = searchFilterDto.ArtportalenFilter.PublicCollection;
                filter.PrivateCollection = searchFilterDto.ArtportalenFilter.PrivateCollection;
                filter.SubstrateSpeciesId = searchFilterDto.ArtportalenFilter.SubstrateSpeciesId;
                filter.SubstrateId = searchFilterDto.ArtportalenFilter.SubstrateId;
                filter.BiotopeId = searchFilterDto.ArtportalenFilter.BiotopeId;
                filter.NotPresentFilter =
                    (SightingNotPresentFilter)searchFilterDto.ArtportalenFilter.NotPresentFilter;
                filter.OnlySecondHandInformation = searchFilterDto.ArtportalenFilter.OnlySecondHandInformation;
                filter.PublishTypeIdsFilter = searchFilterDto.ArtportalenFilter.PublishTypeIdsFilter;
                filter.RegionalSightingStateIdsFilter =
                    searchFilterDto.ArtportalenFilter.RegionalSightingStateIdsFilter;
                filter.SiteIds = searchFilterDto.ArtportalenFilter.SiteIds;
                filter.SpeciesFactsIds = searchFilterDto.ArtportalenFilter.SpeciesFactsIds;
            }

            return filter;
        }
       */
        public static SearchFilter ToSearchFilter(this SearchFilterDto searchFilterDto, string translationCultureCode)
        {
            return (SearchFilter) PopulateFilter(searchFilterDto, translationCultureCode);
           /* if (searchFilterDto == null) return null;

            var filter = (SearchFilter)(searchFilterDto as SearchFilterBaseDto).ToSearchFilter(translationCultureCode);
            filter.OutputFields = searchFilterDto.OutputFields;

            return filter;*/
        }

        public static SearchFilterInternal ToSearchFilterInternal(this SearchFilterInternalDto searchFilterDto,
            string translationCultureCode)
        {
            return (SearchFilterInternal)PopulateFilter(searchFilterDto, translationCultureCode);
            /*if (searchFilterDto == null) return null;

            var filter = (searchFilterDto as SearchFilterInternalBaseDto).ToSearchFilterInternal(translationCultureCode);
            filter.OutputFields = searchFilterDto.OutputFields;
            filter.IncludeRealCount = searchFilterDto.IncludeRealCount;
           
            return filter;*/
        }

        public static SearchFilter ToSearchFilter(this SearchFilterAggregationDto searchFilterDto, string translationCultureCode)
        {
            return (SearchFilter)PopulateFilter(searchFilterDto, translationCultureCode);

         //   return (SearchFilter)(searchFilterDto as SearchFilterBaseDto)?.ToSearchFilter(translationCultureCode);
        }

        public static SearchFilterInternal ToSearchFilterInternal(this SearchFilterAggregationInternalDto searchFilterDto, string translationCultureCode)
        {
            return (SearchFilterInternal)PopulateFilter(searchFilterDto, translationCultureCode);
            //return (searchFilterDto as SearchFilterInternalBaseDto)?.ToSearchFilterInternal(translationCultureCode);
        }

        public static ExportFilter ToExportFilter(this ExportFilterDto searchFilterDto, string translationCultureCode)
        {
            return (ExportFilter)PopulateFilter(searchFilterDto, translationCultureCode);
            /* if (searchFilterDto == null) return null;

             var filter = (SearchFilter)(searchFilterDto as SearchFilterBaseDto).ToSearchFilter(translationCultureCode);
             filter.OutputFields = searchFilterDto.OutputFields;

             return filter;*/
        }

        public static IEnumerable<VocabularyDto> ToVocabularyDtos(this IEnumerable<Vocabulary> vocabularies, bool includeSystemMappings = true)
        {
            return vocabularies.Select(vocabulary => vocabulary.ToVocabularyDto(includeSystemMappings));
        }

        public static VocabularyDto ToVocabularyDto(this Vocabulary vocabulary, bool includeSystemMappings = true)
        {
            return new VocabularyDto
            {
                Id = (int)(VocabularyIdDto) vocabulary.Id,
                EnumId = (VocabularyIdDto)vocabulary.Id,
                Name = vocabulary.Name,
                Description = vocabulary.Description,
                Localized = vocabulary.Localized,
                Values = vocabulary.Values.Select(val => val.ToVocabularyValueInfoDto()).ToList(),
                ExternalSystemsMapping = includeSystemMappings == false || vocabulary.ExternalSystemsMapping == null ? 
                    null : 
                    vocabulary.ExternalSystemsMapping.Select(m => m.ToExternalSystemMappingDto()).ToList()
            };
        }

        private static VocabularyValueInfoDto ToVocabularyValueInfoDto(this VocabularyValueInfo vocabularyValue)
        {
            return new VocabularyValueInfoDto
            {
                Id = vocabularyValue.Id,
                Value = vocabularyValue.Value,
                Description = vocabularyValue.Description,
                Localized = vocabularyValue.Localized,
                Category = vocabularyValue.Category == null
                    ? null
                    : new VocabularyValueInfoCategoryDto
                    {
                        Id = vocabularyValue.Category.Id,
                        Name = vocabularyValue.Category.Name,
                        Description = vocabularyValue.Category.Description,
                        Localized = vocabularyValue.Category.Localized,
                        Translations = vocabularyValue.Category.Translations?.Select(
                            vocabularyValueCategoryTranslation => new VocabularyValueTranslationDto
                            {
                                CultureCode = vocabularyValueCategoryTranslation.CultureCode,
                                Value = vocabularyValueCategoryTranslation.Value
                            }).ToList()
                    },
                Translations = vocabularyValue.Translations?.Select(vocabularyValueTranslation =>
                    new VocabularyValueTranslationDto
                    {
                        CultureCode = vocabularyValueTranslation.CultureCode,
                        Value = vocabularyValueTranslation.Value
                    }).ToList()
            };
        }

        private static ExternalSystemMappingDto ToExternalSystemMappingDto(
            this ExternalSystemMapping vocabularyExternalSystemsMapping)
        {
            return new ExternalSystemMappingDto
            {
                Id = (ExternalSystemIdDto) vocabularyExternalSystemsMapping.Id,
                Name = vocabularyExternalSystemsMapping.Name,
                Description = vocabularyExternalSystemsMapping.Description,
                Mappings = vocabularyExternalSystemsMapping.Mappings?.Select(vocabularyExternalSystemsMappingMapping =>
                    new ExternalSystemMappingFieldDto
                    {
                        Key = vocabularyExternalSystemsMappingMapping.Key,
                        Description = vocabularyExternalSystemsMappingMapping.Description,
                        Values = vocabularyExternalSystemsMappingMapping.Values?.Select(
                            vocabularyExternalSystemsMappingMappingValue => new ExternalSystemMappingValueDto
                            {
                                Value = vocabularyExternalSystemsMappingMappingValue.Value,
                                SosId = vocabularyExternalSystemsMappingMappingValue.SosId
                            }).ToList()
                    }).ToList()
            };
        }
    }
}