using SOS.Analysis.Api.Dtos.Filter;
using SOS.Analysis.Api.Dtos.Filter.Enums;
using SOS.Lib.Enums;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;

namespace SOS.Analysis.Api.Extensions.Dto
{
    public static class FilterExtensionscs
    {
        public static SearchFilter ToSearchFilter(this SearchFilterDto searchFilterDto, int userId, bool sensitiveObservations, string translationCultureCode)
        {
            if (searchFilterDto == null) return default!;

            var filter = new SearchFilter(userId, sensitiveObservations);
            filter.Taxa = searchFilterDto.Taxon?.ToTaxonFilter();
            filter.Date = ToDateFilter(searchFilterDto.Date);
            filter.DataProviderIds = searchFilterDto.DataProvider?.Ids?.ToList();
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
            filter.ExtendedAuthorization.ProtectedObservations = sensitiveObservations;
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

            return filter;
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
    }
}
