using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Models;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.UserService;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Enum;
using SOS.Observations.Api.Dtos.Export;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos.Location;
using SOS.Observations.Api.Dtos.Observation;
using SOS.Observations.Api.Dtos.Vocabulary;
using Location = SOS.Lib.Models.Processed.Observation.Location;

namespace SOS.Observations.Api.Extensions
{
    public static class DtoExtensions
    {
      /*  public static StatusVerificationDto ToStatusVerification(this StatusValidationDto statusValidationDto)
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
        }*/

        private static SearchFilterBase PopulateFilter(SearchFilterBaseDto searchFilterBaseDto, int userId, ProtectionFilterDto? protectionFilter, string translationCultureCode)
        {
            if (searchFilterBaseDto == null) return default;

            var filter = searchFilterBaseDto is SearchFilterInternalBaseDto ? 
                new SearchFilterInternal(userId, protectionFilter.ToFilter()) : 
                new SearchFilter(userId, protectionFilter.ToFilter());
            filter.Taxa = searchFilterBaseDto.Taxon?.ToTaxonFilter();
            filter.Date = PopulateDateFilter(searchFilterBaseDto.Date);
            filter.DataProviderIds = searchFilterBaseDto.DataProvider?.Ids;
            filter.FieldTranslationCultureCode = translationCultureCode;
            filter.NotRecoveredFilter = (SightingNotRecoveredFilter)searchFilterBaseDto.NotRecoveredFilter;
            //filter.VerificationStatus = searchFilterBaseDto.ValidationStatus.HasValue ? (SearchFilterBase.StatusVerification)searchFilterBaseDto.ValidationStatus.Value.ToStatusVerification() : (SearchFilterBase.StatusVerification)searchFilterBaseDto.VerificationStatus;
            filter.VerificationStatus = (SearchFilterBase.StatusVerification)searchFilterBaseDto.VerificationStatus;
            filter.ProjectIds = searchFilterBaseDto.ProjectIds;
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

            filter.Output = new OutputFilter();
            if (searchFilterBaseDto is SearchFilterDto searchFilterDto)
            {
                filter.Output.Fields = searchFilterDto.Output?.Fields?.ToArray();
                filter.Output.PopulateFields(searchFilterDto.Output?.FieldSet);
            }

            if (searchFilterBaseDto is SearchFilterInternalBaseDto searchFilterInternalBaseDto)
            {
                var filterInternal = (SearchFilterInternal)filter;
                PopulateInternalBase(searchFilterInternalBaseDto, filterInternal);

                if (searchFilterBaseDto is SearchFilterInternalDto searchFilterInternalDto)
                {
                    filterInternal.IncludeRealCount = searchFilterInternalDto.IncludeRealCount;
                    filterInternal.Output.Fields = searchFilterInternalDto.Output?.Fields?.ToArray();
                    filterInternal.Output.PopulateFields(searchFilterInternalDto.Output?.FieldSet);
                    filterInternal.Output.SortOrders = searchFilterInternalDto.Output?.SortOrders?.Select(so => new SortOrderFilter { SortBy = so.SortBy, SortOrder = so.SortOrder });
                }
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
                internalFilter.Years = searchFilterInternalDto.ExtendedFilter.Years;
                internalFilter.YearsComparison = (DateFilterComparison)searchFilterInternalDto.ExtendedFilter.YearsComparison;
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
                TimeRanges = filter.TimeRanges?.Select(tr => (DateFilter.TimeRange)tr).ToList()
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

        private static ProtectionFilter ToFilter(this ProtectionFilterDto? protectionFilter)
        {
            return protectionFilter switch
            {
                ProtectionFilterDto.BothPublicAndSensitive => ProtectionFilter.BothPublicAndSensitive,
                ProtectionFilterDto.Sensitive => ProtectionFilter.Sensitive,
                _ => ProtectionFilter.Public
            };
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

        public static GeoGridResultDto ToGeoGridResultDto(this GeoGridTileResult geoGridTileResult, long totalGridCellCount)
        {
            return new GeoGridResultDto
            {
                Zoom = geoGridTileResult.Zoom,
                GridCellCount = geoGridTileResult.GridCellTileCount,
                BoundingBox = geoGridTileResult.BoundingBox.ToLatLonBoundingBoxDto(),
                GridCells = geoGridTileResult.GridCellTiles.Select(cell => cell.ToGeoGridCellDto()),
                TotalGridCellCount = geoGridTileResult.GridCellTileCount > totalGridCellCount ? geoGridTileResult.GridCellTileCount : totalGridCellCount
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

        public static FeatureCollection ToGeoJson(this GeoGridMetricResultDto geoGridMetricResult, MetricCoordinateSys metricCoordinateSys)
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
                        new Coordinate(gridCell.MetricBoundingBox.TopLeft.X, gridCell.MetricBoundingBox.BottomRight.Y),
                        new Coordinate(gridCell.MetricBoundingBox.BottomRight.X, gridCell.MetricBoundingBox.BottomRight.Y),
                        new Coordinate(gridCell.MetricBoundingBox.BottomRight.X, gridCell.MetricBoundingBox.TopLeft.Y),
                        new Coordinate(gridCell.MetricBoundingBox.TopLeft.X, gridCell.MetricBoundingBox.TopLeft.Y),
                        new Coordinate(gridCell.MetricBoundingBox.TopLeft.X, gridCell.MetricBoundingBox.BottomRight.Y)
                    }));

                var wgs84Polygon = polygon.Transform((CoordinateSys)metricCoordinateSys, CoordinateSys.WGS84);
                
                var feature = new Feature
                {                    
                    Attributes = new AttributesTable(new[]
                    {
                        new KeyValuePair<string, object>("id", gridCell.Id),
                        new KeyValuePair<string, object>("cellSizeInMeters", geoGridMetricResult.GridCellSizeInMeters),
                        new KeyValuePair<string, object>("observationsCount", gridCell.ObservationsCount),
                        new KeyValuePair<string, object>("taxaCount", gridCell.TaxaCount),
                        new KeyValuePair<string, object>("metricCRS", metricCoordinateSys.ToString()),
                        new KeyValuePair<string, object>("metricLeft", Convert.ToInt32(gridCell.MetricBoundingBox.TopLeft.X)),
                        new KeyValuePair<string, object>("metricTop", Convert.ToInt32(gridCell.MetricBoundingBox.TopLeft.Y)),
                        new KeyValuePair<string, object>("metricRight", Convert.ToInt32(gridCell.MetricBoundingBox.BottomRight.X)),
                        new KeyValuePair<string, object>("metricBottom", Convert.ToInt32(gridCell.MetricBoundingBox.BottomRight.Y))
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
                Id = GeoJsonHelper.GetGridCellId(gridCellSizeInMeters, Convert.ToInt32(gridCell.MetricBoundingBox.TopLeft.X), Convert.ToInt32(gridCell.MetricBoundingBox.BottomRight.Y)),
                BoundingBox = gridCell.MetricBoundingBox.ToLatLonBoundingBoxDto(),
                ObservationsCount = gridCell.ObservationsCount,
                MetricBoundingBox = gridCell.MetricBoundingBox.ToXYBoundingBoxDto(),
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
                FirstSighting = taxonAggregationItem.FirstSighting,
                LastSighting = taxonAggregationItem.LastSighting,
                TaxonId = taxonAggregationItem.TaxonId,
                ObservationCount = taxonAggregationItem.ObservationCount
            };
        }

        public static IEnumerable<YearCountResultDto> ToDtos(this IEnumerable<YearCountResult> yearMonthResults)
        {
            return yearMonthResults.Select(item => item.ToDto());
        }

        public static YearCountResultDto ToDto(this YearCountResult yearCountResult)
        {
            return new YearCountResultDto
            {
                Count = yearCountResult.Count,
                TaxonCount = yearCountResult.TaxonCount,
                Year = yearCountResult.Year
            };
        }

        public static IEnumerable<YearMonthCountResultDto> ToDtos(this IEnumerable<YearMonthCountResult> yearMonthCountResults)
        {
            return yearMonthCountResults.Select(item => item.ToDto());
        }

        public static YearMonthCountResultDto ToDto(this YearMonthCountResult yearMonthCountResult)
        {
            return new YearMonthCountResultDto
            {
                Count = yearMonthCountResult.Count,
                Month = yearMonthCountResult.Month,
                TaxonCount = yearMonthCountResult.TaxonCount,
                Year = yearMonthCountResult.Year
            };
        }

        public static IEnumerable<YearMonthDayCountResultDto> ToDtos(this IEnumerable<YearMonthDayCountResult> yearMonthDayCountResults)
        {
            return yearMonthDayCountResults.Select(item => item.ToDto());
        }

        public static YearMonthDayCountResultDto ToDto(this YearMonthDayCountResult yearMonthDayCountResult)
        {
            return new YearMonthDayCountResultDto
            {
                Count = yearMonthDayCountResult.Count,
                Day = yearMonthDayCountResult.Day,
                Month = yearMonthDayCountResult.Month,
                Localities = yearMonthDayCountResult.Localities?.ToDtos(),
                TaxonCount = yearMonthDayCountResult.TaxonCount,
                Year = yearMonthDayCountResult.Year
            };
        }

        public static IEnumerable<IdNameDto<T>> ToDtos<T>(this IEnumerable<IdName<T>> idNames)
        {
            return idNames.Select(item => item.ToDto());
        }

        public static IdNameDto<T> ToDto<T>(this IdName<T> idName)
        {
            return new IdNameDto<T>
            {
                Id = idName.Id,
                Name = idName.Name
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
        public static DarwinCoreOccurrenceDto ToDto(this Observation observation)
        {
            if (observation == null)
            {
                return null;
            }

            var dto = new DarwinCoreOccurrenceDto
            {
                AccessRights = observation.AccessRights?.Value,
                BasisOfRecord = observation.BasisOfRecord?.Value,
                Bed = null,
                BibliographicCitation = observation.BibliographicCitation,
                CollectionCode = observation.CollectionCode,
                CollectionID = observation.CollectionId,
                CultivarEpithet = null,
                DataGeneralizations = null,
                DatasetID = observation.DatasetId,
                DatasetName = observation.DatasetName,
                DegreeOfEstablishment = null,
                Disposition = null,
                DynamicProperties = observation.DynamicProperties,
                EarliestAgeOrLowestStage = null,
                EarliestEonOrLowestEonothem = null,
                EarliestEpochOrLowestSeries = null,
                EarliestEraOrLowestErathem = null,
                EarliestPeriodOrLowestSystem = null,
                EstablishmentMeans = null,
                FieldNotes = null,
                FieldNumber = null,
                Formation = null,
                GenericName = null,
                Group = null,
                HighestBiostratigraphicZone = null,
                IdentifiedByID = null,
                InformationWithheld = observation.InformationWithheld,
                InstitutionCode = observation.InstitutionCode?.Value,
                InstitutionID = observation.InstitutionId,
                Language = observation.Language,
                LatestAgeOrHighestStage = null,
                LatestEonOrHighestEonothem = null,
                LatestEpochOrHighestSeries = null,
                LatestEraOrHighestErathem = null,
                LatestPeriodOrHighestSystem = null,
                License = observation.License,
                LithostratigraphicTerms = null,
                LowestBiostratigraphicZone = null,
                Member = null,
                Modified = observation.Modified,
                NomenclaturalCode = null,
                NomenclaturalStatus = null,
                OwnerInstitutionCode = observation.OwnerInstitutionCode,
                Pathway = null,
                Preparations = null,
                RecordedByID = null,
                References = observation.References,
                RightsHolder = observation.RightsHolder,
            };

            if (observation.Event != null)
            {
                dto.Day = observation.Event.StartDay;
                dto.EndDayOfYear = observation.Event.EndDay;
                dto.EventDate = DwcFormatter.CreateDateIntervalString(observation.Event.StartDate, observation.Event.EndDate);
                dto.EventID = observation.Event.EventId;
                dto.EventRemarks = observation.Event.EventRemarks;
                dto.EventTime = observation.Event.PlainStartTime;
                dto.Habitat = observation.Event.Habitat;
                dto.Month = observation.Event.StartMonth;
                dto.ParentEventID = observation.Event.ParentEventId;
                dto.SampleSizeUnit = observation.Event.SampleSizeUnit;
                dto.SampleSizeValue = observation.Event.SampleSizeValue;
                dto.SamplingEffort = observation.Event.SamplingEffort;
                dto.SamplingProtocol = observation.Event.SamplingProtocol;
                dto.StartDayOfYear = observation.Event.StartDay;
            }

            if (observation.Identification != null)
            {
                dto.DateIdentified = observation.Identification.DateIdentified;
                dto.IdentificationID = observation.Identification.IdentificationId;
                dto.IdentificationQualifier = observation.Identification.IdentificationQualifier;
                dto.IdentificationReferences = observation.Identification.IdentificationReferences;
                dto.IdentificationRemarks = observation.Identification.IdentificationRemarks;
                dto.IdentificationVerificationStatus = observation.Identification.VerificationStatus?.Value;
                dto.IdentifiedBy = observation.Identification.IdentifiedBy;
            }

            if (observation.Location != null)
            {
                dto.Continent = observation.Location.Continent?.Value;
                dto.CoordinatePrecision = observation.Location.CoordinatePrecision;
                dto.CoordinateUncertaintyInMeters = observation.Location.CoordinateUncertaintyInMeters;
                dto.Country = observation.Location.Country?.Value;
                dto.CountryRegion = observation.Location.CountryRegion?.Name;
                dto.CountryCode = observation.Location.CountryCode;
                dto.County = observation.Location.County?.Name;
                dto.DecimalLatitude = observation.Location.DecimalLatitude;
                dto.DecimalLongitude = observation.Location.DecimalLongitude;
                dto.FootprintSpatialFit = observation.Location.FootprintSpatialFit;
                dto.FootprintSRS = observation.Location.FootprintSRS;
                dto.FootprintWKT = observation.Location.FootprintWKT;
                dto.GeodeticDatum = observation.Location.GeodeticDatum;
                dto.GeoreferencedBy = observation.Location.GeoreferencedBy;
                dto.GeoreferencedDate = observation.Location.GeoreferencedDate;
                dto.GeoreferenceProtocol = observation.Location.GeoreferenceProtocol;
                dto.GeoreferenceRemarks = observation.Location.GeoreferenceRemarks;
                dto.GeoreferenceSources = observation.Location.GeoreferenceSources;
                dto.GeoreferenceVerificationStatus = observation.Location.GeoreferenceVerificationStatus;
                dto.HigherGeography = observation.Location.HigherGeography;
                dto.HigherGeographyID = observation.Location.HigherGeographyId;
                dto.IslandGroup = observation.Location.IslandGroup;
                dto.Locality = observation.Location.Locality;
                dto.LocationAccordingTo = observation.Location.LocationAccordingTo;
                dto.LocationID = observation.Location.LocationId;
                dto.LocationRemarks = observation.Location.LocationRemarks;
                dto.MaximumDepthInMeters = observation.Location.MaximumDepthInMeters;
                dto.MaximumDistanceAboveSurfaceInMeters = observation.Location.MaximumDistanceAboveSurfaceInMeters;
                dto.MaximumElevationInMeters = observation.Location.MaximumElevationInMeters;
                dto.MinimumDepthInMeters = observation.Location.MinimumDepthInMeters;
                dto.MinimumDistanceAboveSurfaceInMeters = observation.Location.MinimumDistanceAboveSurfaceInMeters;
                dto.MinimumElevationInMeters = observation.Location.MinimumElevationInMeters;
                dto.Municipality = observation.Location.Municipality?.Name;
                dto.PointRadiusSpatialFit = observation.Location.PointRadiusSpatialFit;
                dto.StateProvince = observation.Location.Province?.Name;
                dto.VerbatimLatitude = observation.Location.VerbatimLatitude;
                dto.VerbatimLongitude = observation.Location.VerbatimLongitude;
                dto.VerbatimCoordinateSystem = observation.Location.VerbatimCoordinateSystem;
                dto.VerbatimSRS = observation.Location.VerbatimSRS;
            }

            if (observation.MaterialSample != null)
            {
                dto.MaterialSampleID = observation.MaterialSample.MaterialSampleId;
            }

            if (observation.Occurrence != null)
            {
                dto.AssociatedMedia = observation.Occurrence.AssociatedMedia;
                dto.AssociatedOccurrences = observation.Occurrence.AssociatedOccurrences;
                dto.AssociatedReferences = observation.Occurrence.AssociatedReferences;
                dto.AssociatedSequences = observation.Occurrence.AssociatedSequences;
                dto.AssociatedTaxa = observation.Occurrence.AssociatedTaxa;
                dto.Behavior = observation.Occurrence.Behavior?.Value;
                dto.CatalogNumber = observation.Occurrence.CatalogNumber;
                dto.IndividualCount = observation.Occurrence.IndividualCount;
                dto.LifeStage = observation.Occurrence.LifeStage?.Value;
                dto.Media = observation.Occurrence.Media;
                dto.OccurrenceID = observation.Occurrence.OccurrenceId;
                dto.OccurrenceRemarks = observation.Occurrence.OccurrenceRemarks;
                dto.OccurrenceStatus = observation.Occurrence.OccurrenceStatus?.Value;
                dto.OrganismQuantity = observation.Occurrence.OrganismQuantity;
                dto.OrganismQuantityType = observation.Occurrence.OrganismQuantityUnit?.Value;
                dto.OtherCatalogNumbers = observation.Occurrence.OtherCatalogNumbers;
                dto.RecordedBy = observation.Occurrence.RecordedBy;
                dto.RecordNumber = observation.Occurrence.RecordNumber;
                dto.ReproductiveCondition = observation.Occurrence.ReproductiveCondition?.Value;
                dto.Sex = observation.Occurrence.Sex?.Value;
            }

            if (observation.Organism != null)
            {
                dto.AssociatedOrganisms = observation.Organism.AssociatedOrganisms;
                dto.OrganismID = observation.Organism.OrganismId;
                dto.OrganismName = observation.Organism.OrganismName;
                dto.OrganismRemarks = observation.Organism.OrganismRemarks;
                dto.OrganismScope = observation.Organism.OrganismScope;
                dto.PreviousIdentifications = observation.Organism.PreviousIdentifications;
            }

            if (observation.Taxon != null)
            {
                dto.AcceptedNameUsage = observation.Taxon.AcceptedNameUsage;
                dto.AcceptedNameUsageID = observation.Taxon.AcceptedNameUsageId;
                dto.Class = observation.Taxon.Class;
                dto.Family = observation.Taxon.Family;
                dto.Genus = observation.Taxon?.Genus;
                dto.HigherClassification = observation.Taxon.HigherClassification;
                dto.InfragenericEpithet = observation.Taxon.InfraspecificEpithet;
                dto.InfraspecificEpithet = observation.Taxon.InfraspecificEpithet;
                dto.Kingdom = observation.Taxon.Kingdom;
                dto.NameAccordingTo = observation.Taxon.NameAccordingTo;
                dto.NameAccordingToID = observation.Taxon.NameAccordingToId;
                dto.NamePublishedIn = observation.Taxon.NamePublishedIn;
                dto.NamePublishedInID = observation.Taxon.NamePublishedInId;
                dto.NamePublishedInYear = observation.Taxon.NamePublishedInYear;
                dto.Order = observation.Taxon.Order;
                dto.OriginalNameUsage = observation.Taxon.OriginalNameUsage;
                dto.OriginalNameUsageID = observation.Taxon.OriginalNameUsageId;
                dto.ParentNameUsage = observation.Taxon.ParentNameUsage;
                dto.ParentNameUsageID = observation.Taxon.ParentNameUsageId;
                dto.Phylum = observation.Taxon.Phylum;
                dto.ScientificName = observation.Taxon.ScientificName;
                dto.ScientificNameAuthorship = observation.Taxon.ScientificNameAuthorship;
                dto.ScientificNameID = observation.Taxon.ScientificNameId;
                dto.SpecificEpithet = observation.Taxon.SpecificEpithet;
                dto.VernacularName = observation.Taxon.VernacularName;
                dto.TaxonRank = observation.Taxon.TaxonRank;
                dto.TaxonomicStatus = observation.Taxon.TaxonomicStatus;
                dto.TaxonRemarks = observation.Taxon.TaxonRemarks;
            }

            return dto;
        }

        public static ProjectDto ToDto(this ProjectInfo projectInfo)
        {
            if (projectInfo == null)
            {
                return null;
            }

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

        public static SearchFilter ToSearchFilter(this SearchFilterBaseDto searchFilterDto, int userId, bool sensitiveObservations, string translationCultureCode)
        {
            return (SearchFilter)PopulateFilter(searchFilterDto, userId, sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public, translationCultureCode);
        }

        public static SearchFilter ToSearchFilter(this SearchFilterDto searchFilterDto, int userId, bool sensitiveObservations, string translationCultureCode,
            string sortBy, SearchSortOrder sortOrder)
        {
            var searchFilter = (SearchFilter)PopulateFilter(searchFilterDto, userId, sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public, translationCultureCode);

            if (!string.IsNullOrEmpty(sortBy))
            {
                searchFilter ??= new SearchFilter(userId, sensitiveObservations ? ProtectionFilter.Sensitive : ProtectionFilter.Public);
                searchFilter.Output ??= new OutputFilter();
                searchFilter.Output.SortOrders = new[] { new SortOrderFilter { SortBy = sortBy, SortOrder = sortOrder } };
            }
            return searchFilter;
        }

        public static SearchFilterInternal ToSearchFilterInternal(this SearchFilterInternalBaseDto searchFilterDto,
            int userId, bool sensitiveObservations, string translationCultureCode)
        {
            return (SearchFilterInternal)PopulateFilter(searchFilterDto, userId, searchFilterDto.ProtectionFilter ?? (sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public), translationCultureCode);
        }

        public static SearchFilterInternal ToSearchFilterInternal(this SearchFilterInternalDto searchFilterDto,
            int userId, bool sensitiveObservations, string translationCultureCode, string sortBy, SearchSortOrder sortOrder)
        {
            // User sort order passed the old way if not any other sort order is passed 
            if (!string.IsNullOrEmpty(sortBy) && (!searchFilterDto?.Output?.SortOrders?.Any() ?? true))
            {
                searchFilterDto ??= new SearchFilterInternalDto();
                searchFilterDto.Output ??= new OutputFilterExtendedDto();
                searchFilterDto.Output.SortOrders = new[] { new SortOrderDto { SortBy = sortBy, SortOrder = sortOrder } };
            }
 
            return (SearchFilterInternal)PopulateFilter(searchFilterDto, userId, searchFilterDto.ProtectionFilter ?? (sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public), translationCultureCode);
        }

        public static SearchFilter ToSearchFilter(this SearchFilterAggregationDto searchFilterDto, int userId, bool sensitiveObservations, string translationCultureCode)
        {
            return (SearchFilter)PopulateFilter(searchFilterDto, userId, sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public, translationCultureCode);
        }

        public static TaxonFilter ToTaxonFilterFilter(this TaxonFilterDto taxonFilterDto)
        {
            return PopulateTaxa(taxonFilterDto);            
        }

        public static SearchFilterInternal ToSearchFilterInternal(this SearchFilterAggregationInternalDto searchFilterDto, int userId, bool sensitiveObservations, string translationCultureCode)
        {
            return (SearchFilterInternal)PopulateFilter(searchFilterDto, userId, searchFilterDto.ProtectionFilter ?? (sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public), translationCultureCode);
        }

        public static SearchFilter ToSearchFilter(this ExportFilterDto searchFilterDto, int userId, bool sensitiveObservations, string translationCultureCode)
        {
            return (SearchFilter)PopulateFilter(searchFilterDto, userId, sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public, translationCultureCode);
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

            return (null, checklistFilter);
            //return (observationFilter, checklistFilter);
        }

        public static SearchFilterInternal ToSearchFilterInternal(this SignalFilterDto searchFilterDto, int userId, bool sensitiveObservations)
        {
            if (searchFilterDto == null)
            {
                return null;
            }

            var searchFilter = new SearchFilterInternal(userId, sensitiveObservations ? ProtectionFilter.Sensitive : ProtectionFilter.Public)
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
                Date = searchFilterDto.StartDate.HasValue ? new DateFilter
                {
                    StartDate = searchFilterDto.StartDate
                } : null,
                Taxa = searchFilterDto.Taxon?.ToTaxonFilter(),
                UnspontaneousFilter = SightingUnspontaneousFilter.NotUnspontaneous
            };

            return searchFilter;
        }

        public static IEnumerable<ProjectDto> ToProjectDtos(this IEnumerable<ProjectInfo> projectInfos)
        {
            return projectInfos.Select(vocabulary => vocabulary.ToDto());
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
                CountryRegion = location.CountryRegion == null
                    ? null
                    : new IdValueDto<string> { Id = location.CountryRegion.FeatureId, Value = location.CountryRegion.Name },
                County = location.County == null
                    ? null
                    : new IdValueDto<string> { Id = location.County.FeatureId, Value = location.County.Name },
                DecimalLatitude = location.DecimalLatitude,
                DecimalLongitude = location.DecimalLongitude,
                ExternalId = location.Attributes?.ExternalId,
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
                PointWithDisturbanceBuffer = location.PointWithDisturbanceBuffer,
                ProjectId = location.Attributes?.ProjectId
            };
        }

        public static LocationSearchResultDto ToDto(this LocationSearchResult locationSearchResult)
        {
            if (locationSearchResult == null)
            {
                return null;
            }

            return new LocationSearchResultDto
            {
                County = locationSearchResult.County,
                Id = locationSearchResult.Id,
                Latitude = locationSearchResult.Latitude,
                Longitude = locationSearchResult.Longitude,
                Municipality = locationSearchResult.Municipality,
                Name = locationSearchResult.Name,
                Parish = locationSearchResult.Parish
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
                DataTypeNullable = fieldDescription.DataTypeIsNullable.GetValueOrDefault(false),
                DwcIdentifier = fieldDescription.DwcIdentifier,
                DwcName = fieldDescription.DwcName,
                EnglishTitle = fieldDescription.GetEnglishTitle(),
                SwedishTitle = fieldDescription.GetSwedishTitle(),
                Name = fieldDescription.PropertyName,
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
                Description = userRole.Description,
                HasSensitiveSpeciesAuthority = userRole.HasSensitiveSpeciesAuthority,
                HasSightingIndicationAuthority = userRole.HasSightingIndicationAuthority,                
                Authorities = userRole.Authorities?.Select(a => a.ToUserAuthorityDto()).ToList()
            };
        }

        public static UserAuthorityDto ToUserAuthorityDto(this UserAuthority userAuthority)
        {
            if (userAuthority == null) return null;

            return new UserAuthorityDto
            {
                Id = userAuthority.Id,
                Name = userAuthority.Name,
                Areas = userAuthority.Areas?.Select(a => a.ToUserAreaDto()).ToList()
            };
        }

        public static UserAreaDto ToUserAreaDto(this UserArea userArea)
        {
            if (userArea == null) return null;

            return new UserAreaDto
            {
                AreaType = (AreaTypeDto)userArea.AreaType,
                FeatureId = userArea.FeatureId,
                Name = userArea.Name
            };
        }

        public static ExportJobInfoDto ToDto(this ExportJobInfo exportJobInfo)
        {
            if (exportJobInfo == null)
            {
                return null!;
            }
            return new ExportJobInfoDto
            {
                CreatedDate = exportJobInfo.CreatedDate,
                Description = exportJobInfo.Description,
                ExpireDate = exportJobInfo.ProcessEndDate.HasValue ? exportJobInfo.ProcessEndDate.Value.AddDays(exportJobInfo.LifetimeDays) : null,
                Format = exportJobInfo.Format,
                Id = exportJobInfo.Id,
                NumberOfObservations = exportJobInfo.NumberOfObservations,
                OutputFieldSet = exportJobInfo.OutputFieldSet,
                PickUpUrl = exportJobInfo.PickUpUrl,
                ProcessEndDate = exportJobInfo.ProcessEndDate,
                ProcessStartDate = exportJobInfo.ProcessStartDate,
                Status = exportJobInfo.Status
            };
        }
    }
}