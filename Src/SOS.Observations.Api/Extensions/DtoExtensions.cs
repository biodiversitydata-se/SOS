using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Models;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.UserService;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos.Vocabulary;
using static SOS.Observations.Api.Dtos.Filter.SearchFilterBaseDto;
using Location = SOS.Lib.Models.Processed.Observation.Location;

namespace SOS.Observations.Api.Extensions
{
    public static class DtoExtensions
    {
        public static StatusVerificationDto ToStatusVerification(this StatusValidationDto statusValidationDto)
        {
            switch (statusValidationDto)
            {
                case StatusValidationDto.NotValidated:
                    return StatusVerificationDto.NotVerified;
                case StatusValidationDto.Validated:
                    return StatusVerificationDto.Verified;
                case StatusValidationDto.BothValidatedAndNotValidated:
                    return StatusVerificationDto.BothVerifiedAndNotVerified;
                default: return StatusVerificationDto.BothVerifiedAndNotVerified;
            }
        }

        private static SearchFilterBase PopulateFilter(SearchFilterBaseDto searchFilterBaseDto, string translationCultureCode, bool sensitiveObservations)
        {
            if (searchFilterBaseDto == null) return default;

            var filter = searchFilterBaseDto is SearchFilterInternalBaseDto ? new SearchFilterInternal() : new SearchFilter();
            filter.Taxa = searchFilterBaseDto.Taxon?.ToTaxonFilter();
            filter.Date = PopulateDateFilter(searchFilterBaseDto.Date);
            filter.DataProviderIds = searchFilterBaseDto.DataProvider?.Ids;
            filter.FieldTranslationCultureCode = translationCultureCode;
            filter.NotRecoveredFilter = (SightingNotRecoveredFilter)searchFilterBaseDto.NotRecoveredFilter;
            filter.VerificationStatus = searchFilterBaseDto.ValidationStatus.HasValue ? (SearchFilterBase.StatusVerification)searchFilterBaseDto.ValidationStatus.Value.ToStatusVerification() : (SearchFilterBase.StatusVerification)searchFilterBaseDto.VerificationStatus;
            filter.ProjectIds = searchFilterBaseDto.ProjectIds;
            filter.BirdNestActivityLimit = searchFilterBaseDto.BirdNestActivityLimit;
            filter.Location = PopulateLocationFilter(searchFilterBaseDto.Geographics);
            filter.ModifiedDate = searchFilterBaseDto.ModifiedDate == null
                ? null
                : new ModifiedDateFilter
                {
                    From = searchFilterBaseDto.ModifiedDate.From,
                    To = searchFilterBaseDto.ModifiedDate.To
                };
            filter.ExtendedAuthorization.ProtectedObservations = sensitiveObservations;
            filter.ExtendedAuthorization.ObservedByMe = searchFilterBaseDto.ObservedByMe;
            filter.ExtendedAuthorization.ReportedByMe = searchFilterBaseDto.ReportedByMe;

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

            filter.DiffusionStatuses = searchFilterBaseDto.DiffusionStatuses?.Select(dsd => (DiffusionStatus)dsd)?.ToList();

            filter.DeterminationFilter = (SightingDeterminationFilter)searchFilterBaseDto.DeterminationFilter;

            if (searchFilterBaseDto is SearchFilterDto searchFilterDto)
            {
                filter.OutputFields = searchFilterDto.Output?.Fields?.ToList();
                filter.PopulateOutputFields(searchFilterDto.Output?.FieldSet);
            }

            if (searchFilterBaseDto is SearchFilterInternalBaseDto searchFilterInternalBaseDto)
            {
                var filterInternal = (SearchFilterInternal)filter;
                PopulateInternalBase(searchFilterInternalBaseDto, filterInternal);

                if (searchFilterBaseDto is SearchFilterInternalDto searchFilterInternalDto)
                {
                    filterInternal.IncludeRealCount = searchFilterInternalDto.IncludeRealCount;
                    filter.OutputFields = searchFilterInternalDto.Output?.Fields?.ToList();
                    filter.PopulateOutputFields(searchFilterInternalDto.Output?.FieldSet);
                }
            }

            return filter;
        }
        private static void PopulateInternalBase(SearchFilterInternalBaseDto searchFilterInternalDto, SearchFilterInternal internalFilter)
        {
            if (searchFilterInternalDto.ExtendedFilter != null)
            {
                internalFilter.CheckListId = searchFilterInternalDto.ExtendedFilter.CheckListId;
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
                internalFilter.MonthsComparison = (MonthsFilterComparison)searchFilterInternalDto.ExtendedFilter.MonthsComparison;
                internalFilter.DiscoveryMethodIds = searchFilterInternalDto.ExtendedFilter.DiscoveryMethodIds;
                internalFilter.LifeStageIds = searchFilterInternalDto.ExtendedFilter.LifeStageIds;
                internalFilter.ActivityIds = searchFilterInternalDto.ExtendedFilter.ActivityIds;
                internalFilter.HasTriggeredVerificationRule = searchFilterInternalDto.ExtendedFilter.HasTriggerdValidationRule ?? searchFilterInternalDto.ExtendedFilter.HasTriggeredVerificationRule;
                internalFilter.HasTriggeredVerificationRuleWithWarning =
                    searchFilterInternalDto.ExtendedFilter.HasTriggerdValidationRuleWithWarning ?? searchFilterInternalDto.ExtendedFilter.HasTriggeredVerificationRuleWithWarning;
                internalFilter.Length = searchFilterInternalDto.ExtendedFilter.Length;
                internalFilter.LengthOperator = searchFilterInternalDto.ExtendedFilter.LengthOperator;
                internalFilter.Weight = searchFilterInternalDto.ExtendedFilter.Weight;
                internalFilter.WeightOperator = searchFilterInternalDto.ExtendedFilter.WeightOperator;
                internalFilter.Quantity = searchFilterInternalDto.ExtendedFilter.Quantity;
                internalFilter.QuantityOperator = searchFilterInternalDto.ExtendedFilter.QuantityOperator;
                internalFilter.VerificationStatusIds = searchFilterInternalDto.ExtendedFilter.ValidationStatusIds != null && searchFilterInternalDto.ExtendedFilter.ValidationStatusIds.Any() ? searchFilterInternalDto.ExtendedFilter.ValidationStatusIds : searchFilterInternalDto.ExtendedFilter.VerificationStatusIds;
                internalFilter.ExcludeVerificationStatusIds = searchFilterInternalDto.ExtendedFilter.ExcludeValidationStatusIds != null && searchFilterInternalDto.ExtendedFilter.ExcludeValidationStatusIds.Any() ? searchFilterInternalDto.ExtendedFilter.ExcludeValidationStatusIds : searchFilterInternalDto.ExtendedFilter.ExcludeVerificationStatusIds;
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
                internalFilter.SiteIds = searchFilterInternalDto.ExtendedFilter.SiteIds;
                internalFilter.SpeciesFactsIds = searchFilterInternalDto.ExtendedFilter.SpeciesFactsIds;
                internalFilter.InstitutionId = searchFilterInternalDto.ExtendedFilter.InstitutionId;
                internalFilter.DatasourceIds = searchFilterInternalDto.ExtendedFilter.DatasourceIds;
                if (searchFilterInternalDto?.ExtendedFilter?.LocationNameFilter != null)
                {
                    if (internalFilter.Location == null) internalFilter.Location = new LocationFilter();
                    internalFilter.Location.NameFilter = searchFilterInternalDto.ExtendedFilter.LocationNameFilter;
                }

                if (searchFilterInternalDto.ExtendedFilter.SexIds?.Any() ?? false)
                {
                    internalFilter.Taxa ??= new TaxonFilter();
                    internalFilter.Taxa.SexIds = searchFilterInternalDto.ExtendedFilter.SexIds;
                }
            }
        }

        private static DateFilter PopulateDateFilter(DateFilterDto filter)
        {
            if (filter == null)
            {
                return null;
            }

            return new DateFilter
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                DateFilterType = (DateFilter.DateRangeFilterType)(filter?.DateFilterType).GetValueOrDefault(),
                TimeRanges = filter.TimeRanges?.Select(tr => (DateFilter.TimeRange)tr)
            };
        }

        private static LocationFilter PopulateLocationFilter(GeographicsFilterDto filter)
        {
            if (filter == null)
            {
                return null;
            }

            return new LocationFilter
            {
                Areas = filter.Areas?.Select(a => new AreaFilter
                { FeatureId = a.FeatureId, AreaType = (AreaType)a.AreaType }),
                Geometries = new GeographicsFilter
                {
                    BoundingBox = filter.BoundingBox?.ToLatLonBoundingBox(),
                    Geometries = filter.Geometries?.ToList(),
                    MaxDistanceFromPoint = filter.MaxDistanceFromPoint,
                    UseDisturbanceRadius = filter.ConsiderDisturbanceRadius,
                    UsePointAccuracy = filter.ConsiderObservationAccuracy,
                },
                MaxAccuracy = filter.MaxAccuracy
            };
        }

        private static TaxonFilter PopulateTaxa(TaxonFilterBaseDto filterDto)
        {
            if (filterDto == null)
            {
                return null;
            }

            var filter = new TaxonFilter
            {
                Ids = filterDto.Ids,
                IncludeUnderlyingTaxa = filterDto.IncludeUnderlyingTaxa,
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

        public static void OverrideBoundingBox(this SearchFilter filter, LatLonBoundingBox boundingbox)
        {
            filter ??= new SearchFilter();
            filter.Location ??= new LocationFilter();
            filter.Location.Geometries ??= new GeographicsFilter();
            filter.Location.Geometries.BoundingBox = boundingbox;
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

        public static GeoGridMetricResultDto ToDto(this GeoGridMetricResult geoGridMetricResult)
        {
            return new GeoGridMetricResultDto
            {
                BoundingBox = geoGridMetricResult.BoundingBox.ToLatLonBoundingBoxDto(),
                GridCellCount = geoGridMetricResult.GridCellCount,
                GridCellSizeInMeters = geoGridMetricResult.GridCellSizeInMeters,
                GridCells = geoGridMetricResult.GridCells.Select(cell => cell.ToGridCellDto(geoGridMetricResult.GridCellSizeInMeters)),
                Sweref99TmBoundingBox = geoGridMetricResult.BoundingBox.ToXYBoundingBoxDto()
            };
        }

        public static FeatureCollection ToGeoJson(this GeoGridMetricResultDto geoGridMetricResult)
        {
            var featureCollection = new FeatureCollection
            {
                BoundingBox = new Envelope(
                    geoGridMetricResult.BoundingBox.BottomRight.Longitude,
                    geoGridMetricResult.BoundingBox.TopLeft.Longitude,
                    geoGridMetricResult.BoundingBox.BottomRight.Latitude,
                    geoGridMetricResult.BoundingBox.TopLeft.Latitude)
            };

            foreach (var gridCell in geoGridMetricResult.GridCells)
            {                
                var polygon = new Polygon(new LinearRing(new[]
                    {
                        new Coordinate(gridCell.Sweref99TmBoundingBox.TopLeft.X, gridCell.Sweref99TmBoundingBox.BottomRight.Y),
                        new Coordinate(gridCell.Sweref99TmBoundingBox.BottomRight.X, gridCell.Sweref99TmBoundingBox.BottomRight.Y),
                        new Coordinate(gridCell.Sweref99TmBoundingBox.BottomRight.X, gridCell.Sweref99TmBoundingBox.TopLeft.Y),
                        new Coordinate(gridCell.Sweref99TmBoundingBox.TopLeft.X, gridCell.Sweref99TmBoundingBox.TopLeft.Y),
                        new Coordinate(gridCell.Sweref99TmBoundingBox.TopLeft.X, gridCell.Sweref99TmBoundingBox.BottomRight.Y)
                    }));
                var wgs84Polygon = polygon.Transform(CoordinateSys.SWEREF99_TM, CoordinateSys.WGS84);
                
                var feature = new Feature
                {                    
                    Attributes = new AttributesTable(new[]
                    {
                        new KeyValuePair<string, object>("id", gridCell.Id),
                        new KeyValuePair<string, object>("cellSizeInMeters", geoGridMetricResult.GridCellSizeInMeters),
                        new KeyValuePair<string, object>("observationsCount", gridCell.ObservationsCount),
                        new KeyValuePair<string, object>("taxaCount", gridCell.TaxaCount),
                        new KeyValuePair<string, object>("sweref99TmLeft", Convert.ToInt32(gridCell.Sweref99TmBoundingBox.TopLeft.X)),
                        new KeyValuePair<string, object>("sweref99TmTop", Convert.ToInt32(gridCell.Sweref99TmBoundingBox.TopLeft.Y)),
                        new KeyValuePair<string, object>("sweref99TmRight", Convert.ToInt32(gridCell.Sweref99TmBoundingBox.BottomRight.X)),
                        new KeyValuePair<string, object>("sweref99TmBottom", Convert.ToInt32(gridCell.Sweref99TmBoundingBox.BottomRight.Y))
                    }),                    
                    Geometry = wgs84Polygon,
                    BoundingBox = wgs84Polygon.EnvelopeInternal
                };
                featureCollection.Add(feature);
            }
            
            return featureCollection;
        }

        /// <summary>
        /// Cast lat lon bounding box dto 
        /// </summary>
        /// <param name="latLonBoundingBox"></param>
        /// <returns></returns>
        public static LatLonBoundingBox ToLatLonBoundingBox(this LatLonBoundingBoxDto latLonBoundingBox)
        {
            if (latLonBoundingBox?.TopLeft == null || latLonBoundingBox?.BottomRight == null)
            {
                return null;
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
        public static LatLonCoordinate ToLatLonCoordinate(this LatLonCoordinateDto latLonCoordinate)
        {
            if (latLonCoordinate == null)
            {
                return null;
            }

            return new LatLonCoordinate(latLonCoordinate.Latitude, latLonCoordinate.Longitude);
        }

        public static LatLonBoundingBoxDto ToLatLonBoundingBoxDto(this LatLonBoundingBox latLonBoundingBox)
        {
            return new LatLonBoundingBoxDto
            {
                TopLeft = latLonBoundingBox.TopLeft.ToLatLonCoordinateDto(),
                BottomRight = latLonBoundingBox.BottomRight.ToLatLonCoordinateDto()
            };
        }

        public static LatLonBoundingBoxDto ToLatLonBoundingBoxDto(this XYBoundingBox xyBoundingBox)
        {
            return new LatLonBoundingBoxDto
            {
                TopLeft = xyBoundingBox.TopLeft.ToLatLonCoordinateDto(),
                BottomRight = xyBoundingBox.BottomRight.ToLatLonCoordinateDto()
            };
        }

        public static XYBoundingBoxDto ToXYBoundingBoxDto(this LatLonBoundingBox latLonBoundingBox)
        {
            return new XYBoundingBoxDto
            {
                TopLeft = latLonBoundingBox.TopLeft.ToXYCoordinateDto(),
                BottomRight = latLonBoundingBox.BottomRight.ToXYCoordinateDto()
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

        public static LatLonCoordinateDto ToLatLonCoordinateDto(this XYCoordinate xyCoordinate)
        {
            var point = new Point(xyCoordinate.X, xyCoordinate.Y).Transform(CoordinateSys.SWEREF99_TM,
                CoordinateSys.WGS84, false) as Point;
            return new LatLonCoordinateDto
            {
                Latitude = point.Y,
                Longitude = point.X
            };
        }

        public static XYCoordinateDto ToXYCoordinateDto(this LatLonCoordinate latLonCoordinate)
        {
            var point = new Point(latLonCoordinate.Longitude, latLonCoordinate.Latitude).Transform(CoordinateSys.WGS84,
                CoordinateSys.SWEREF99_TM, false) as Point;
            return new XYCoordinateDto
            {
                X = point.X,
                Y = point.Y
            };
        }

        public static XYBoundingBoxDto ToXYBoundingBoxDto(this XYBoundingBox xyBoundingBox)
        {
            return new XYBoundingBoxDto
            {
                BottomRight = xyBoundingBox.BottomRight.ToXYCoordinateDto(),
                TopLeft = xyBoundingBox.TopLeft.ToXYCoordinateDto()
            };
        }

        public static XYCoordinateDto ToXYCoordinateDto(this XYCoordinate xyCoordinate)
        {
            return new XYCoordinateDto
            {
                X = xyCoordinate.X,
                Y = xyCoordinate.Y
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

        public static GridCellDto ToGridCellDto(this GridCell gridCell, int gridCellSizeInMeters)
        {
            return new GridCellDto
            {
                Id = GeoJsonHelper.GetGridCellId(gridCellSizeInMeters, Convert.ToInt32(gridCell.Sweref99TmBoundingBox.TopLeft.X), Convert.ToInt32(gridCell.Sweref99TmBoundingBox.BottomRight.Y)),
                BoundingBox = gridCell.Sweref99TmBoundingBox.ToLatLonBoundingBoxDto(),
                ObservationsCount = gridCell.ObservationsCount,
                Sweref99TmBoundingBox = gridCell.Sweref99TmBoundingBox.ToXYBoundingBoxDto(),
                TaxaCount = gridCell.TaxaCount
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

        public static PagedResultDto<TRecordDto> ToPagedResultDto<TRecord, TRecordDto>(
            this PagedResult<TRecord> pagedResult,
            IEnumerable<TRecordDto> records)
        {
            return new PagedResultDto<TRecordDto>
            {
                Records = records,
                Skip = pagedResult.Skip,
                Take = pagedResult.Take,
                TotalCount = pagedResult.TotalCount,
            };
        }

        public static GeoPagedResultDto<TRecordDto> ToGeoPagedResultDto<TRecord, TRecordDto>(
            this PagedResult<TRecord> pagedResult,
            IEnumerable<TRecordDto> records,
            OutputFormatDto outputFormat = OutputFormatDto.Json)
        {
            if (outputFormat == OutputFormatDto.Json)
            {
                return new GeoPagedResultDto<TRecordDto>
                {
                    Records = records,
                    Skip = pagedResult.Skip,
                    Take = pagedResult.Take,
                    TotalCount = pagedResult.TotalCount,
                };
            }

            var dictionaryRecords = records.Cast<IDictionary<string, object>>();
            bool flattenProperties = outputFormat == OutputFormatDto.GeoJsonFlat;
            string geoJson = GeoJsonHelper.GetFeatureCollectionString(dictionaryRecords, flattenProperties);
            return new GeoPagedResultDto<TRecordDto>
            {
                Skip = pagedResult.Skip,
                Take = pagedResult.Take,
                TotalCount = pagedResult.TotalCount,
                GeoJson = geoJson
            };
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

        public static ScrollResultDto<TRecordDto> ToScrollResultDto<TRecord, TRecordDto>(this ScrollResult<TRecord> scrollResult, IEnumerable<TRecordDto> records)
        {
            return new ScrollResultDto<TRecordDto>
            {
                Records = records,
                ScrollId = scrollResult.ScrollId,
                HasMorePages = scrollResult.ScrollId != null,
                Take = scrollResult.Take,
                TotalCount = scrollResult.TotalCount
            };
        }

        public static SearchFilter ToSearchFilter(this SearchFilterBaseDto searchFilterDto, string translationCultureCode, bool sensitiveObservations)
        {
            return (SearchFilter)PopulateFilter(searchFilterDto, translationCultureCode, sensitiveObservations);
        }

        public static SearchFilter ToSearchFilter(this SearchFilterDto searchFilterDto, string translationCultureCode, bool sensitiveObservations)
        {
            return (SearchFilter)PopulateFilter(searchFilterDto, translationCultureCode, sensitiveObservations);
        }

        public static SearchFilterInternal ToSearchFilterInternal(this SearchFilterInternalBaseDto searchFilterDto,
            string translationCultureCode, bool sensitiveObservations)
        {
            return (SearchFilterInternal)PopulateFilter(searchFilterDto, translationCultureCode, sensitiveObservations);
        }

        public static SearchFilterInternal ToSearchFilterInternal(this SearchFilterInternalDto searchFilterDto,
            string translationCultureCode, bool sensitiveObservations)
        {
            return (SearchFilterInternal)PopulateFilter(searchFilterDto, translationCultureCode, sensitiveObservations);
        }

        public static SearchFilter ToSearchFilter(this SearchFilterAggregationDto searchFilterDto, string translationCultureCode, bool sensitiveObservations)
        {
            return (SearchFilter)PopulateFilter(searchFilterDto, translationCultureCode, sensitiveObservations);
        }

        public static TaxonFilter ToTaxonFilterFilter(this TaxonFilterDto taxonFilterDto)
        {
            return PopulateTaxa(taxonFilterDto);            
        }

        public static SearchFilterInternal ToSearchFilterInternal(this SearchFilterAggregationInternalDto searchFilterDto, string translationCultureCode, bool sensitiveObservations)
        {
            return (SearchFilterInternal)PopulateFilter(searchFilterDto, translationCultureCode, sensitiveObservations);
        }

        public static SearchFilter ToSearchFilter(this ExportFilterDto searchFilterDto, string translationCultureCode, bool sensitiveObservations)
        {
            return (SearchFilter)PopulateFilter(searchFilterDto, translationCultureCode, sensitiveObservations);
        }

        public static (SearchFilter, CheckListSearchFilter) ToSearchFilters(this CalculateTrendFilterDto searchFilterDto)
        {
            var observationFilter = new SearchFilter
            {
                DataProviderIds = searchFilterDto.Observation?.DataProvider?.Ids,
                Date = PopulateDateFilter(searchFilterDto.Date),
                ProjectIds = searchFilterDto.ProjectIds,
                Taxa = new TaxonFilter { Ids = new[] { searchFilterDto.TaxonId } },
                VerificationStatus = (SearchFilterBase.StatusVerification)(searchFilterDto?.Observation?.VerificationStatus ?? StatusVerificationDto.BothVerifiedAndNotVerified)
            };
            observationFilter.Location = PopulateLocationFilter(searchFilterDto.Geographics);
            var checkListFilter = new CheckListSearchFilter
            {
                DataProviderIds = searchFilterDto.CheckList?.DataProvider?.Ids,
                Date = observationFilter.Date as CheckListDateFilter,
                Location = observationFilter.Location,
                Taxa = observationFilter.Taxa
            };

            if (TimeSpan.TryParse(searchFilterDto.CheckList?.MinEffortTime, out var minEffortTime))
            {
                checkListFilter.Date ??= new CheckListDateFilter();
                checkListFilter.Date.MinEffortTime = minEffortTime;
            }

            return (observationFilter, checkListFilter);
        }

        public static SearchFilterInternal ToSearchFilterInternal(this SignalFilterDto searchFilterDto)
        {
            if (searchFilterDto == null)
            {
                return null;
            }

            var searchFilter = new SearchFilterInternal
            {
                BirdNestActivityLimit = searchFilterDto.BirdNestActivityLimit,
                DataProviderIds = searchFilterDto.DataProvider?.Ids,
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
                        },
                    MaxAccuracy = searchFilterDto.Geographics?.MaxAccuracy
                },
                NotPresentFilter = SightingNotPresentFilter.DontIncludeNotPresent,
                NotRecoveredFilter = SightingNotRecoveredFilter.DontIncludeNotRecovered,
                PositiveSightings = true,
                ExtendedAuthorization = new ExtendedAuthorizationFilter { ProtectedObservations = true },
                Date = searchFilterDto.StartDate.HasValue ? new DateFilter
                {
                    StartDate = searchFilterDto.StartDate
                } : null,
                Taxa = searchFilterDto.Taxon?.ToTaxonFilter(),
                UnspontaneousFilter = SightingUnspontaneousFilter.NotUnspontaneous
            };
            searchFilter.ExtendedAuthorization.ProtectedObservations = true;

            return searchFilter;
        }

        public static IEnumerable<ProjectDto> ToProjectDtos(this IEnumerable<ProjectInfo> projectInfos)
        {
            return projectInfos.Select(vocabulary => vocabulary.ToProjectDto());
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
                return null;
            }

            var filter = new TaxonFilter
            {
                Ids = filterDto.Ids,
                IncludeUnderlyingTaxa = filterDto.IncludeUnderlyingTaxa,
                ListIds = filterDto.TaxonListIds,
                TaxonListOperator = TaxonFilter.TaxonListOp.Merge,
            };

            if (filterDto is TaxonFilterDto taxonFilterDto)
            {
                filter.RedListCategories = taxonFilterDto.RedListCategories;
                filter.TaxonListOperator =
                    (TaxonFilter.TaxonListOp)(taxonFilterDto?.TaxonListOperator).GetValueOrDefault();
                filter.TaxonCategories = taxonFilterDto.TaxonCategories;
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
                Id = (int)(VocabularyIdDto)vocabulary.Id,
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
                Id = (ExternalSystemIdDto)vocabularyExternalSystemsMapping.Id,
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

        /// <summary>
        /// Cast Location to locationDto
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public static LocationDto ToDto(this Location location)
        {
            if (location == null)
            {
                return null;
            }

            return new LocationDto
            {
                Continent = location.Continent == null
                    ? null
                    : new IdValueDto<int> { Id = location.Continent.Id, Value = location.Continent.Value },
                CoordinatePrecision = location.CoordinatePrecision,
                CoordinateUncertaintyInMeters = location.CoordinateUncertaintyInMeters,
                Country = location.Country == null
                    ? null
                    : new IdValueDto<int> { Id = location.Country.Id, Value = location.Country.Value },
                CountryCode = location.CountryCode,
                County = location.County == null
                    ? null
                    : new IdValueDto<string> { Id = location.County.FeatureId, Value = location.County.Name },
                DecimalLatitude = location.DecimalLatitude,
                DecimalLongitude = location.DecimalLongitude,
                Locality = location.Locality,
                LocationAccordingTo = location.LocationAccordingTo,
                LocationId = location.LocationId,
                LocationRemarks = location.LocationRemarks,
                Municipality = location.Municipality == null
                    ? null
                    : new IdValueDto<string> { Id = location.Municipality.FeatureId, Value = location.Municipality.Name },
                Province = location.Province == null
                    ? null
                    : new IdValueDto<string> { Id = location.Province.FeatureId, Value = location.Province.Name },
                Parish = location.Parish == null
                    ? null
                    : new IdValueDto<string> { Id = location.Parish.FeatureId, Value = location.Parish.Name },
                Point = location.Point,
                PointWithBuffer = location.PointWithBuffer,
                PointWithDisturbanceBuffer = location.PointWithDisturbanceBuffer
            };
        }

        public static List<PropertyFieldDescriptionDto> ToPropertyFieldDescriptionDtos(this IEnumerable<PropertyFieldDescription> fieldDescriptions)
        {
            return fieldDescriptions.Select(fieldDescription => fieldDescription.ToPropertyFieldDescriptionDto()).ToList();
        }

        public static PropertyFieldDescriptionDto ToPropertyFieldDescriptionDto(this PropertyFieldDescription fieldDescription)
        {
            if (fieldDescription == null)
            {
                return null;
            }

            return new PropertyFieldDescriptionDto
            {
                PropertyPath = fieldDescription.PropertyPath,
                DataType = fieldDescription.DataTypeEnum,
                DataTypeNullable = fieldDescription.DataTypeNullable.GetValueOrDefault(false),
                DwcIdentifier = fieldDescription.DwcIdentifier,
                DwcName = fieldDescription.DwcName,
                EnglishTitle = fieldDescription.GetEnglishTitle(),
                SwedishTitle = fieldDescription.GetSwedishTitle(),
                Name = fieldDescription.Name,
                FieldSet = fieldDescription.FieldSetEnum,
                PartOfFieldSets = fieldDescription.FieldSets
            };
        }


        public static UserInformationDto ToUserInformationDto(this UserInformation userInformation)
        {
            if (userInformation == null)
            {
                return null;
            }

            return new UserInformationDto
            {
                Id = userInformation.Id,
                UserName = userInformation.UserName,
                FirstName = userInformation.FirstName,
                LastName = userInformation.LastName,
                Email = userInformation.Email,
                HasSensitiveSpeciesAuthority = userInformation.HasSensitiveSpeciesAuthority,
                HasSightingIndicationAuthority = userInformation.HasSightingIndicationAuthority,
                Roles = userInformation.Roles.Select(role => role.ToUserRoleDto()).ToArray()
            };
        }

        public static UserRoleDto ToUserRoleDto(this UserRole userRole)
        {
            if (userRole == null) return null;

            return new UserRoleDto
            {
                Id = userRole.Id,
                Name = userRole.Name,
                ShortName = userRole.ShortName,
                Description = userRole.Description
            };
        }
    }
}