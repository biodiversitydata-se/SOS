using NetTopologySuite.Geometries;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Shared.Api.Utilities.Objects.Interfaces;

namespace SOS.Shared.Api.Utilities.Objects
{
    public class SearchFilterUtility : ISearchFilterUtility
    {
        private readonly IAreaCache _areaCache;

        /// <summary>
        /// Get bounding box
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="autoAdjustBoundingBox"></param>
        /// <returns></returns>
        private async Task<LatLonBoundingBoxDto?> GetBoundingBoxAsync(
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
                    boundingBox = boundingBox.AdjustByGeometry(geometryUnion, filter?.MaxDistanceFromPoint);
                }
            }

            return LatLonBoundingBoxDto.Create(boundingBox);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaCache"></param>
        public SearchFilterUtility(
            IAreaCache areaCache
        )
        {
            _areaCache = areaCache ?? throw new ArgumentNullException(nameof(areaCache));
        }

        /// <inheritdoc/>
        public async Task<T> InitializeSearchFilterAsync<T>(T? filter) where T : SearchFilterBaseDto
        {
            filter ??= new SearchFilterBaseDto() as T;
            filter.Geographics ??= new GeographicsFilterDto();
            filter.Geographics.BoundingBox = await GetBoundingBoxAsync(filter.Geographics);
            return filter;
        }

        /// <inheritdoc/>
        public async Task<SearchFilterInternalDto> InitializeSearchFilterAsync(SearchFilterInternalDto filter)
        {
            filter ??= new SearchFilterInternalDto();
            filter.Geographics ??= new GeographicsFilterDto();
            filter.Geographics.BoundingBox = await GetBoundingBoxAsync(filter.Geographics);
            return filter;
        }
    }
}
