using System;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Managers.Interfaces;

namespace SOS.Lib.Managers
{
    /// <summary>
    /// Manager for geometries
    /// </summary>
    public class GeometryManager : IGeometryManager
    {
        private readonly IGeometryCache _geometryCache;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="geometryCache"></param>
        public GeometryManager(IGeometryCache geometryCache)
        {
            _geometryCache = geometryCache ?? throw new ArgumentNullException(nameof(geometryCache));
        }

        /// <inheritdoc />
        public async Task<Geometry> GetCircleAsync(Point wgs84Point, int? accuracy)
        {
            if (wgs84Point?.Coordinate == null)
            {
                return null;
            }

            return wgs84Point.ToCircle(accuracy);

            // Create key
            var key = $"{Math.Round(wgs84Point.Coordinate.X, 5)}:{Math.Round(wgs84Point.Coordinate.Y, 5)}:{accuracy ?? 0}";

            // Try to get circle geometry from cache
            var geometry = await _geometryCache.GetAsync(key);

            // If circle geometry was found in cache, return it
            if (geometry != null)
            {
                return geometry;
            }

            // Create new circle and store it in cache
            geometry = wgs84Point.ToCircle(accuracy);

            await _geometryCache.AddAsync(key, geometry);

            return geometry;
        }
    }
}
