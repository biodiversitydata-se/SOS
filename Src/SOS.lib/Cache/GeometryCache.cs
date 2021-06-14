using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using SOS.Lib.Cache.Interfaces;

namespace SOS.Lib.Cache
{
    public class GeometryCache : IGeometryCache
    {
        private readonly IDistributedCache _distributedCache;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="distributedCache"></param>
        public GeometryCache(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        /// <inheritdoc />
        public async Task<bool> AddAsync(string key, Geometry geometry)
        {
            await _distributedCache.SetAsync(key, geometry.ToBinary());

            return true;
        }

        /// <inheritdoc />
        public async Task<Geometry> GetAsync(string key)
        {
            var geometryData = await _distributedCache.GetAsync(key, CancellationToken.None);
            if (geometryData == null)
            {
                return null;
            }
            var wkbReader = new WKBReader();
            return wkbReader.Read(geometryData); 
        }
    }
}
