using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using SOS.Analysis.Api.Dtos.Filter;
using SOS.Analysis.Api.Dtos.Filter.Enums;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Extensions;
using Result = CSharpFunctionalExtensions.Result;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SOS.Analysis.Api.Controllers
{
    
    public class BaseController : ControllerBase
    {
        private readonly IAreaCache _areaCache;
        private readonly string _protectedScope;
        private readonly int _tilesLimit;

        /// <summary>
        /// Get bounding box
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="autoAdjustBoundingBox"></param>
        /// <returns></returns>
        private async Task<LatLonBoundingBoxDto> GetBoundingBoxAsync(
            GeographicsFilterDto filter,
            bool autoAdjustBoundingBox = true)
        {
            // Get geometry of sweden economic zone
            var swedenGeometry = await _areaCache.GetGeometryAsync(AreaType.EconomicZoneOfSweden, "1");

            // Get bounding box of swedish economic zone
            var swedenBoundingBox = swedenGeometry.ToGeometry().EnvelopeInternal;
            var userBoundingBox = new Envelope(new[]
            {
                new Coordinate(filter?.BoundingBox?.TopLeft?.Longitude ?? 0, filter?.BoundingBox?.TopLeft?.Latitude ?? 90),
                new Coordinate(filter?.BoundingBox?.BottomRight?.Longitude ?? 90, filter?.BoundingBox?.TopLeft?.Latitude ?? 90),
                new Coordinate(filter?.BoundingBox?.BottomRight?.Longitude ?? 90, filter?.BoundingBox?.BottomRight?.Latitude ?? 0),
                new Coordinate(filter?.BoundingBox?.TopLeft?.Longitude ?? 0, filter?.BoundingBox?.BottomRight?.Latitude ?? 0),
            });
            var boundingBox = swedenBoundingBox.Intersection(userBoundingBox);

            if (autoAdjustBoundingBox)
            {
                Geometry geometryUnion = null!;

                // If areas passed, adjust bounding box to them
                if (filter?.Areas?.Any() ?? false)
                {
                    var areas = await _areaCache.GetAreasAsync(filter.Areas.Select(a => ((AreaType)a.AreaType, a.FeatureId)));
                    var areaGeometries = areas?.Select(a => a.BoundingBox.GetPolygon().ToGeoShape());

                    var areaPolygons = areas?.Select(a => a.BoundingBox.GetPolygon());

                    foreach (var areaPolygon in areaPolygons!)
                    {
                        geometryUnion = geometryUnion == null ? areaPolygon : geometryUnion.Union(areaPolygon);
                    }
                }

                // If geometries passed, adjust bounding box to them
                if (filter?.Geometries?.Any() ?? false)
                {
                    foreach (var geoShape in filter.Geometries)
                    {
                        var geometry = geoShape.ToGeometry();
                        geometryUnion = geometryUnion == null ? geometry : geometryUnion.Union(geometry);
                    }
                }

                if (geometryUnion != null)
                {
                    boundingBox = boundingBox.AdjustByGeometry(geometryUnion, filter.MaxDistanceFromPoint);
                }
            }

            return LatLonBoundingBoxDto.Create(boundingBox);
        }

        protected void CheckAuthorization(bool sensitiveObservations)
        {
            if (sensitiveObservations && (!User?.HasAccessToScope(_protectedScope) ?? true))
            {
                throw new AuthenticationRequiredException("Not authorized");
            }
        }

        protected async Task<SearchFilterDto> InitializeSearchFilterAsync(SearchFilterDto filter) {

            filter ??= new SearchFilterDto();
            filter.Geographics ??= new GeographicsFilterDto();
            filter.Geographics.BoundingBox = await GetBoundingBoxAsync(filter.Geographics);
            return filter;
        }

        /// <summary>
        /// Get id of current user
        /// </summary>
        protected int UserId => int.Parse(User?.Claims?.FirstOrDefault(c => c.Type.Contains("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", StringComparison.CurrentCultureIgnoreCase))?.Value ?? "0");

        /// <summary>
        /// Validate areas
        /// </summary>
        /// <param name="areaIds"></param>
        /// <returns></returns>
        protected async Task<Result> ValidateAreasAsync(IEnumerable<AreaFilterDto> areaKeys)
        {
            if (!areaKeys?.Any() ?? true)
            {
                return Result.Success();
            }
           
            var existingAreaIds = (await _areaCache.GetAreasAsync(areaKeys.Select(a => ((AreaType)a.AreaType, a.FeatureId))))
                .Select(a => new AreaFilterDto { AreaType = (AreaTypeDto)a.AreaType, FeatureId = a.FeatureId });

            var missingAreas = areaKeys?
                .Where(aid => !existingAreaIds.Any(a => a.AreaType.Equals(aid.AreaType) && a.FeatureId.Equals(aid.FeatureId, StringComparison.CurrentCultureIgnoreCase)))
                .Select(aid => $"Area doesn't exist (AreaType: {aid.AreaType}, FeatureId: {aid.FeatureId})");

            return missingAreas?.Any() ?? false ?
                Result.Failure(string.Join(". ", missingAreas))
                :
                Result.Success();
        }

        /// <summary>
        /// Validate bounding box
        /// </summary>
        /// <param name="boundingBox"></param>
        /// <param name="mandatory"></param>
        /// <returns></returns>
        protected Result ValidateBoundingBox(
            LatLonBoundingBoxDto boundingBox,
            bool mandatory = false)
        {
            if (boundingBox == null)
            {
                return mandatory ? Result.Failure("Bounding box is missing.") : Result.Success();
            }

            if (boundingBox.TopLeft == null || boundingBox.BottomRight == null)
            {
                return Result.Failure("Bounding box is incomplete.");
            }

            if (boundingBox.TopLeft.Longitude >= boundingBox.BottomRight.Longitude)
            {
                return Result.Failure("Bounding box left longitude value is >= right longitude value.");
            }

            if (boundingBox.BottomRight.Latitude >= boundingBox.TopLeft.Latitude)
            {
                return Result.Failure("Bounding box bottom latitude value is >= top latitude value.");
            }

            return Result.Success();
        }

        protected Result ValidateInt(int value, int minLimit, int maxLimit, string paramName)
        {
            if (value < minLimit || value > maxLimit)
            {
                return Result.Failure($"{paramName} must be between {minLimit} and {maxLimit}");
            }

            return Result.Success();
        }

        protected Result ValidateDouble(double value, double minLimit, double maxLimit, string paramName)
        {
            if (value < minLimit || value > maxLimit)
            {
                return Result.Failure($"{paramName} must be between {minLimit} and {maxLimit}");
            }

            return Result.Success();
        }

        /// <summary>
        /// Validate metric tiles limit
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="gridCellSizeInMeters "></param>
        /// <returns></returns>
        protected Result ValidateMetricTilesLimit(
            Envelope envelope,
            int gridCellSizeInMeters)
        {

            if (envelope == null)
            {
                return Result.Success();
            }

            var maxLonTiles = Math.Ceiling((envelope.MaxX - envelope.MinX) / gridCellSizeInMeters);
            var maxLatTiles = Math.Ceiling((envelope.MaxY - envelope.MinY) / gridCellSizeInMeters);
            var maxTilesTot = maxLonTiles * maxLatTiles;

            if (maxTilesTot > _tilesLimit)
            {
                return Result.Failure($"The number of cells that can be returned is too large. The limit is {_tilesLimit} cells. Try using larger grid cell size or a smaller bounding box.");
            }

            return Result.Success();
        }

        /// <summary>
        /// Validate serach filter
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="bboxMandatory"></param>
        /// <returns></returns>
        protected Result ValidateSearchFilter(SearchFilterDto filter, bool bboxMandatory = false)
        {
            var errors = new List<string>();

            if (filter == null)
            {
                errors.Add("You must provide a filter");
            }

            var areaValidationResult = ValidateAreasAsync(filter?.Geographics?.Areas).Result;
            if (areaValidationResult.IsFailure)
            {
                errors.Add(areaValidationResult.Error);
            }

            var bboxResult = ValidateBoundingBox(filter?.Geographics?.BoundingBox, bboxMandatory);
            if (bboxResult.IsFailure)
            {
                errors.Add(bboxResult.Error);
            }

            if (filter?.ModifiedDate?.From > filter?.ModifiedDate?.To)
            {
                errors.Add("Modified from date can't be greater tha to date");
            }

            if (filter?.Taxon?.RedListCategories?.Any() ?? false)
            {
                errors.AddRange(filter.Taxon.RedListCategories
                    .Where(rc => !new[] { "DD", "EX", "RE", "CR", "EN", "VU", "NT", "LC", "NA", "NE" }.Contains(rc, StringComparer.CurrentCultureIgnoreCase))
                    .Select(rc => $"Red list category doesn't exist ({rc})"));
            }
            if (filter?.Date?.DateFilterType == DateFilterTypeDto.OnlyStartDate && (filter.Date?.StartDate == null || filter.Date?.EndDate == null))
            {
                errors.Add("When using OnlyStartDate as filter both StartDate and EndDate need to be specified");
            }
            if (filter?.Date?.DateFilterType == DateFilterTypeDto.OnlyEndDate && (filter.Date?.StartDate == null || filter.Date?.EndDate == null))
            {
                errors.Add("When using OnlyEndDate as filter both StartDate and EndDate need to be specified");
            }

            return errors.Any() ? Result.Failure(string.Join(". ", errors)) : Result.Success();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaCache"></param>
        /// <param name="protectedScope"></param>
        /// <param name="tilesLimit"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected BaseController(
            IAreaCache areaCache,
            string protectedScope,
            int tilesLimit)
        {
            _areaCache = areaCache ?? throw new ArgumentNullException(nameof(areaCache));
            _protectedScope = protectedScope ?? throw new ArgumentNullException(nameof(protectedScope));
            _tilesLimit = !tilesLimit.Equals(0) ? tilesLimit : throw new ArgumentNullException(nameof(tilesLimit));
        }
    }
}
