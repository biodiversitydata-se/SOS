using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Enums;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos.Vocabulary;

namespace SOS.Observations.Api.Extensions
{
    public static class DtoExtensions
    {
        private static FilterBase PopulateFilter(SearchFilterBaseDto searchFilterBaseDto, string translationCultureCode, bool protectedObservations)
        {
            if (searchFilterBaseDto == null) return default;

            var filter = searchFilterBaseDto is SearchFilterInternalBaseDto ? new SearchFilterInternal() : new SearchFilter();

            filter.StartDate = searchFilterBaseDto.Date?.StartDate;
            filter.EndDate = searchFilterBaseDto.Date?.EndDate;
            filter.DateFilterType = (FilterBase.DateRangeFilterType)(searchFilterBaseDto.Date?.DateFilterType).GetValueOrDefault();
            filter.TimeRanges = searchFilterBaseDto.Date?.TimeRanges?.Select(tr => (FilterBase.TimeRange)tr);
            filter.Areas = searchFilterBaseDto.Areas?.Select(a => new AreaFilter { FeatureId = a.FeatureId, AreaType = (AreaType)a.AreaType });
            filter.TaxonIds = searchFilterBaseDto.Taxon?.Ids;
            filter.IncludeUnderlyingTaxa = (searchFilterBaseDto.Taxon?.IncludeUnderlyingTaxa).GetValueOrDefault();
            filter.RedListCategories = searchFilterBaseDto.Taxon?.RedListCategories;
            filter.TaxonListIds = searchFilterBaseDto.Taxon?.TaxonListIds;
            filter.TaxonListOperator = (FilterBase.TaxonListOp)(searchFilterBaseDto.Taxon?.TaxonListOperator).GetValueOrDefault();
            filter.DataProviderIds = searchFilterBaseDto.DataProvider?.Ids;
            filter.FieldTranslationCultureCode = translationCultureCode;
            filter.MaxAccuracy = searchFilterBaseDto.Geometry?.MaxAccuracy;
            filter.OnlyValidated = searchFilterBaseDto.OnlyValidated;
            filter.ProtectedObservations = protectedObservations;
            filter.ProjectIds = searchFilterBaseDto.ProjectIds;
            filter.BirdNestActivityLimit = searchFilterBaseDto.BirdNestActivityLimit;
            filter.Geometries = searchFilterBaseDto.Geometry?.Geometries == null
                ? null
                : new GeometryFilter
                {
                    Geometries = searchFilterBaseDto.Geometry.Geometries,
                    MaxDistanceFromPoint = searchFilterBaseDto.Geometry.MaxDistanceFromPoint,
                    UsePointAccuracy = searchFilterBaseDto.Geometry.ConsiderObservationAccuracy
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

            filter.DiffusionStatuses = searchFilterBaseDto.DiffusionStatuses?.Select(dsd => (DiffusionStatus) dsd);

            filter.DeterminationFilter = (SightingDeterminationFilter)searchFilterBaseDto.DeterminationFilter;

            if (searchFilterBaseDto is SearchFilterDto searchFilterDto)
            {
                filter.OutputFields = searchFilterDto.OutputFields;
            }

            if (searchFilterBaseDto is SearchFilterInternalBaseDto searchFilterInternalBaseDto)
            {
                var filterInternal = (SearchFilterInternal)filter;
                PopulateInternalBase(searchFilterInternalBaseDto, filterInternal);

                if (searchFilterBaseDto is SearchFilterInternalDto searchFilterInternalDto)
                {
                    filterInternal.IncludeRealCount = searchFilterInternalDto.IncludeRealCount;
                    filter.OutputFields = searchFilterInternalDto.OutputFields;
                }
            }

            return filter;
        }

        private static void PopulateInternalBase(SearchFilterInternalBaseDto searchFilterInternalDto, SearchFilterInternal internalFilter)
        {
            if (searchFilterInternalDto.ExtendedFilter!= null)
            {
                internalFilter.ReportedByUserId = searchFilterInternalDto.ExtendedFilter.ReportedByUserId;
                internalFilter.ObservedByUserId = searchFilterInternalDto.ExtendedFilter.ObservedByUserId;
                internalFilter.BoundingBox = searchFilterInternalDto.ExtendedFilter.BoundingBox;
                internalFilter.OnlyWithMedia = searchFilterInternalDto.ExtendedFilter.OnlyWithMedia;
                internalFilter.OnlyWithNotes = searchFilterInternalDto.ExtendedFilter.OnlyWithNotes;
                internalFilter.OnlyWithNotesOfInterest = searchFilterInternalDto.ExtendedFilter.OnlyWithNotesOfInterest;
                internalFilter.OnlyWithBarcode = searchFilterInternalDto.ExtendedFilter.OnlyWithBarcode;
                internalFilter.ReportedDateFrom = searchFilterInternalDto.ExtendedFilter.ReportedDateFrom;
                internalFilter.ReportedDateTo = searchFilterInternalDto.ExtendedFilter.ReportedDateTo;
                internalFilter.TypeFilter = (SearchFilterInternal.SightingTypeFilter)searchFilterInternalDto.ExtendedFilter.TypeFilter;
                internalFilter.UsePeriodForAllYears = searchFilterInternalDto.ExtendedFilter.UsePeriodForAllYears;
                internalFilter.Months = searchFilterInternalDto.ExtendedFilter.Months;
                internalFilter.DiscoveryMethodIds = searchFilterInternalDto.ExtendedFilter.DiscoveryMethodIds;
                internalFilter.LifeStageIds = searchFilterInternalDto.ExtendedFilter.LifeStageIds;
                internalFilter.ActivityIds = searchFilterInternalDto.ExtendedFilter.ActivityIds;
                internalFilter.HasTriggerdValidationRule = searchFilterInternalDto.ExtendedFilter.HasTriggerdValidationRule;
                internalFilter.HasTriggerdValidationRuleWithWarning =
                    searchFilterInternalDto.ExtendedFilter.HasTriggerdValidationRuleWithWarning;
                internalFilter.Length = searchFilterInternalDto.ExtendedFilter.Length;
                internalFilter.LengthOperator = searchFilterInternalDto.ExtendedFilter.LengthOperator;
                internalFilter.Weight = searchFilterInternalDto.ExtendedFilter.Weight;
                internalFilter.WeightOperator = searchFilterInternalDto.ExtendedFilter.WeightOperator;
                internalFilter.Quantity = searchFilterInternalDto.ExtendedFilter.Quantity;
                internalFilter.QuantityOperator = searchFilterInternalDto.ExtendedFilter.QuantityOperator;
                internalFilter.ValidationStatusIds = searchFilterInternalDto.ExtendedFilter.ValidationStatusIds;
                internalFilter.ExcludeValidationStatusIds = searchFilterInternalDto.ExtendedFilter.ExcludeValidationStatusIds;
                internalFilter.UnspontaneousFilter =
                    (SightingUnspontaneousFilter)searchFilterInternalDto.ExtendedFilter
                        .UnspontaneousFilter;
                internalFilter.NotRecoveredFilter =
                    (SightingNotRecoveredFilter)searchFilterInternalDto.ExtendedFilter
                        .NotRecoveredFilter;
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
                internalFilter.SiteIds = searchFilterInternalDto.ExtendedFilter.SiteIds;
                internalFilter.SpeciesFactsIds = searchFilterInternalDto.ExtendedFilter.SpeciesFactsIds;
                internalFilter.SexIds = searchFilterInternalDto.ExtendedFilter.SexIds;
            }

        }

        public static GeoGridTileTaxonPageResultDto ToGeoGridTileTaxonPageResultDto(this GeoGridTileTaxonPageResult pageResult)
        {
            return new GeoGridTileTaxonPageResultDto
            {
                NextGeoTilePage = pageResult.NextGeoTilePage,
                NextTaxonIdPage = pageResult.NextTaxonIdPage,
                HasMorePages = pageResult.HasMorePages,
                GridCells = pageResult.GridCells.Select(m => m.ToGeoGridTileTaxaCellDto())
            };
        }

        public static GeoGridTileTaxaCellDto ToGeoGridTileTaxaCellDto(this GeoGridTileTaxaCell cell)
        {
            return new GeoGridTileTaxaCellDto
            {
                BoundingBox = ToLatLonBoundingBoxDto(cell.BoundingBox),
                GeoTile = cell.GeoTile,
                X = cell.X,
                Y = cell.Y,
                Zoom = cell.Zoom,
                Taxa = cell.Taxa.Select(m => new GeoGridTileTaxonObservationCountDto
                {
                    TaxonId = m.TaxonId,
                    ObservationCount = m.ObservationCount
                })
            };
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

        public static SearchFilter ToSearchFilter(this SearchFilterDto searchFilterDto, string translationCultureCode, bool protectedObservations)
        {
            return (SearchFilter) PopulateFilter(searchFilterDto, translationCultureCode, protectedObservations);
        }

        public static SearchFilterInternal ToSearchFilterInternal(this SearchFilterInternalDto searchFilterDto,
            string translationCultureCode, bool protectedObservations)
        {
            return (SearchFilterInternal)PopulateFilter(searchFilterDto, translationCultureCode, protectedObservations);
        }

        public static SearchFilter ToSearchFilter(this SearchFilterAggregationDto searchFilterDto, string translationCultureCode, bool protectedObservations)
        {
            return (SearchFilter)PopulateFilter(searchFilterDto, translationCultureCode, protectedObservations);
        }

        public static SearchFilterInternal ToSearchFilterInternal(this SearchFilterAggregationInternalDto searchFilterDto, string translationCultureCode, bool protectedObservations)
        {
            return (SearchFilterInternal)PopulateFilter(searchFilterDto, translationCultureCode, protectedObservations);
        }

        public static SearchFilter ToSearchFilter(this ExportFilterDto searchFilterDto, string translationCultureCode, bool protectedObservations)
        {
            return (SearchFilter)PopulateFilter(searchFilterDto, translationCultureCode, protectedObservations);
        }

        public static IEnumerable<ProjectDto> ToProjectDtos(this IEnumerable<ProjectInfo> projectInfos)
        {
            return projectInfos.Select(vocabulary => vocabulary.ToProjectDto());
        }

        public static ProjectDto ToProjectDto(this ProjectInfo projectInfo)
        {
            return new ProjectDto
            {
                Id = projectInfo.Id,
                Name = projectInfo.Name,
                StartDate = projectInfo.StartDate,
                EndDate = projectInfo.EndDate,
                Category = projectInfo.Category,
                CategorySwedish = projectInfo.CategorySwedish,
                Description = projectInfo.Description,
                IsPublic = projectInfo.IsPublic,
                Owner = projectInfo.Owner,
                ProjectURL = projectInfo.ProjectURL,
                SurveyMethod = projectInfo.SurveyMethod,
                SurveyMethodUrl = projectInfo.SurveyMethodUrl
            };
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

        public static TaxonListTaxonInformationDto ToTaxonListTaxonInformationDto(
            this TaxonListTaxonInformation taxonInformation)
        {
            return new TaxonListTaxonInformationDto
            {
                Id = taxonInformation.Id,
                ScientificName = taxonInformation.ScientificName,
                SwedishName = taxonInformation.SwedishName
            };
        }
    }
}