using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace SOS.Lib.Cache.Interfaces
{
    public interface IGeometryCache
    {
        /// <summary>
        /// Add geometry to cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="geometry"></param>
        /// <returns></returns>
        Task<bool> AddAsync(string key, Geometry geometry);

        /// <summary>
        /// Try to get cached geometry
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<Geometry> GetAsync(string key);
    }
}
