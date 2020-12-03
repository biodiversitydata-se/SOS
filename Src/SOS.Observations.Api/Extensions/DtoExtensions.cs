using System.Collections.Generic;
using System.Linq;
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

        public static SearchFilter ToSearchFilter(this SearchFilterDto searchFilterDto)
        {
            if (searchFilterDto == null) return null;

            var filter = new SearchFilter();
            filter.OutputFields = searchFilterDto.OutputFields;
            filter.StartDate = searchFilterDto.Date?.StartDate;
            filter.EndDate = searchFilterDto.Date?.EndDate;
            filter.DateFilterType = (FilterBase.DateRangeFilterType)(searchFilterDto.Date?.DateFilterType).GetValueOrDefault();
            filter.Areas = searchFilterDto.Areas?.Select(a => new AreaFilter {FeatureId = a.FeatureId, Type = a.Type});
            filter.AreaIds = searchFilterDto.AreaIds;
            filter.TaxonIds = searchFilterDto.Taxon?.TaxonIds;
            filter.IncludeUnderlyingTaxa = (searchFilterDto.Taxon?.IncludeUnderlyingTaxa).GetValueOrDefault();
            filter.RedListCategories = searchFilterDto.Taxon?.RedListCategories;
            filter.DataProviderIds = searchFilterDto.DataProviderIds;
            filter.FieldTranslationCultureCode = searchFilterDto.TranslationCultureCode;
            filter.OnlyValidated = searchFilterDto.OnlyValidated;
            filter.GeometryFilter = searchFilterDto.Geometry == null ? null : new GeometryFilter
            {
                Geometries = searchFilterDto.Geometry.Geometries,
                MaxDistanceFromPoint = searchFilterDto.Geometry.MaxDistanceFromPoint,
                UsePointAccuracy = searchFilterDto.Geometry.UsePointAccuracy
            };

            if (searchFilterDto.OccurrenceStatus != null)
            {
                switch (searchFilterDto.OccurrenceStatus)
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

            return filter;
        }

        public static SearchFilterInternal ToSearchFilterInternal(this SearchFilterInternalDto searchFilterDto)
        {
            if (searchFilterDto == null) return null;

            var filter = new SearchFilterInternal();
            filter.OutputFields = searchFilterDto.OutputFields;
            filter.StartDate = searchFilterDto.Date?.StartDate;
            filter.EndDate = searchFilterDto.Date?.EndDate;
            filter.DateFilterType = (FilterBase.DateRangeFilterType)(searchFilterDto.Date?.DateFilterType).GetValueOrDefault();
            filter.Areas = searchFilterDto.Areas?.Select(a => new AreaFilter { FeatureId = a.FeatureId, Type = a.Type });
            filter.AreaIds = searchFilterDto.AreaIds;
            filter.TaxonIds = searchFilterDto.Taxon?.TaxonIds;
            filter.IncludeUnderlyingTaxa = (searchFilterDto.Taxon?.IncludeUnderlyingTaxa).GetValueOrDefault();
            filter.RedListCategories = searchFilterDto.Taxon?.RedListCategories;
            filter.DataProviderIds = searchFilterDto.DataProviderIds;
            filter.FieldTranslationCultureCode = searchFilterDto.TranslationCultureCode;
            filter.OnlyValidated = searchFilterDto.OnlyValidated;
            filter.GeometryFilter = searchFilterDto.Geometry == null ? null : new GeometryFilter
            {
                Geometries = searchFilterDto.Geometry.Geometries,
                MaxDistanceFromPoint = searchFilterDto.Geometry.MaxDistanceFromPoint,
                UsePointAccuracy = searchFilterDto.Geometry.UsePointAccuracy
            };

            if (searchFilterDto.OccurrenceStatus != null)
            {
                switch (searchFilterDto.OccurrenceStatus)
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

            filter.IncludeRealCount = searchFilterDto.IncludeRealCount;
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
                filter.TypeFilter =
                    (SearchFilterInternal.SightingTypeFilter) searchFilterDto.ArtportalenFilter.TypeFilter;
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
                    (SearchFilterInternal.SightingDeterminationFilter) searchFilterDto.ArtportalenFilter
                        .DeterminationFilter;
                filter.UnspontaneousFilter =
                    (SearchFilterInternal.SightingUnspontaneousFilter) searchFilterDto.ArtportalenFilter
                        .UnspontaneousFilter;
                filter.NotRecoveredFilter =
                    (SearchFilterInternal.SightingNotRecoveredFilter) searchFilterDto.ArtportalenFilter
                        .NotRecoveredFilter;
                filter.SpeciesCollectionLabel = searchFilterDto.ArtportalenFilter.SpeciesCollectionLabel;
                filter.PublicCollection = searchFilterDto.ArtportalenFilter.PublicCollection;
                filter.PrivateCollection = searchFilterDto.ArtportalenFilter.PrivateCollection;
                filter.SubstrateSpeciesId = searchFilterDto.ArtportalenFilter.SubstrateSpeciesId;
                filter.SubstrateId = searchFilterDto.ArtportalenFilter.SubstrateId;
                filter.BiotopeId = searchFilterDto.ArtportalenFilter.BiotopeId;
                filter.NotPresentFilter =
                    (SearchFilterInternal.SightingNotPresentFilter) searchFilterDto.ArtportalenFilter.NotPresentFilter;
                filter.OnlySecondHandInformation = searchFilterDto.ArtportalenFilter.OnlySecondHandInformation;
                filter.PublishTypeIdsFilter = searchFilterDto.ArtportalenFilter.PublishTypeIdsFilter;
                filter.RegionalSightingStateIdsFilter =
                    searchFilterDto.ArtportalenFilter.RegionalSightingStateIdsFilter;
                filter.SiteIds = searchFilterDto.ArtportalenFilter.SiteIds;
                filter.SpeciesFactsIds = searchFilterDto.ArtportalenFilter.SpeciesFactsIds;
            }

            return filter;
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