using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Lib.Cache
{
    /// <summary>
    /// Area cache
    /// </summary>
    public class AreaCache : CacheBase<string, Area>, IAreaCache
    {
        private readonly IAreaRepository _areaRepository;

        protected readonly ConcurrentDictionary<(AreaType, string), IGeoShape> _geometryCache;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaRepository"></param>
        public AreaCache(IAreaRepository areaRepository) : base(areaRepository)
        {
            _areaRepository = areaRepository;
            _geometryCache = new ConcurrentDictionary<(AreaType, string), IGeoShape>();
        }

        public void Clear()
        {
            base.Clear();
            _geometryCache.Clear();
        }

        /// <inheritdoc />
        public async Task<Area> GetAsync(AreaType type, string featureId)
        {
            return await base.GetAsync(type.ToAreaId(featureId));
        }

        /// <inheritdoc />
        public async Task<PagedResult<Area>> GetAreasAsync(IEnumerable<AreaType> areaTypes, string searchString,
            int skip,
            int take)
        {
            return await _areaRepository.GetAreasAsync(areaTypes, searchString, skip, take);
        }

        /// <inheritdoc />
        public async Task<IGeoShape> GetGeometryAsync(AreaType areaType, string featureId)
        {
            if (_geometryCache.TryGetValue((areaType, featureId), out var geometry))
            {
                return geometry;
            }

            geometry = await _areaRepository.GetGeometryAsync(areaType, featureId);

            if (geometry != null)
            {
                _geometryCache.TryAdd((areaType, featureId), geometry);
            }

            return geometry;
        }
    }
}
