using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;

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
            filter.SearchOnlyBetweenDates = (searchFilterDto.Date?.SearchOnlyBetweenDates).GetValueOrDefault();
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
            filter.SearchOnlyBetweenDates = (searchFilterDto.Date?.SearchOnlyBetweenDates).GetValueOrDefault();
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
    }
}
