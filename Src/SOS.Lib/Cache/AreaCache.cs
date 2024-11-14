using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Nest;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Lib.Cache
{
    /// <summary>
    /// Area cache
    /// </summary>
    public class AreaCache : CacheBase<string, Area>, IAreaCache
    {
        private readonly IAreaRepository _areaRepository;

        protected readonly ConcurrentDictionary<(AreaType, string), IGeoShape> _geometryCache;
        private const int NumberOfEntriesCleanupLimit = 50000;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaRepository"></param>
        /// <param name="memoryCache"></param>
        /// <param name="logger"></param>
        public AreaCache(IAreaRepository areaRepository, IMemoryCache memoryCache, ILogger<CacheBase<string, Area>> logger) : base(areaRepository, memoryCache, logger)
        {
            _areaRepository = areaRepository;
            _geometryCache = new ConcurrentDictionary<(AreaType, string), IGeoShape>();
            CacheDuration = TimeSpan.FromMinutes(10);
        }

        /// <summary>
        /// Clear cache
        /// </summary>
        public new void Clear()
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
        public async Task<IEnumerable<Area>> GetAreasAsync(
            IEnumerable<(AreaType areaType, string featureId)> areaKeys)
        {
            var cache = await GetCacheAsync();
            if (!areaKeys?.Any() ?? true)
            {
                return null;
            }

            var missingInCache = areaKeys.Select(k => k.areaType.ToAreaId(k.featureId)).Except(cache.Keys);

            if (missingInCache?.Any() ?? false)
            {
                var areas = await _areaRepository.GetAsync(missingInCache);

                if (areas != null)
                {
                    foreach (var area in areas)
                    {
                        cache.TryAdd(area.Id, area);
                    }
                }
            }

            return cache.Where(gc => areaKeys.Select(k => k.areaType.ToAreaId(k.featureId)).Contains(gc.Key)).Select(gc => gc.Value);
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

            geometry = (await _areaRepository.GetGeometryAsync(areaType, featureId))?.ToGeoShape();

            if (geometry != null)
            {
                if (_geometryCache.Count >= NumberOfEntriesCleanupLimit) // prevent too large geometry cache
                {
                    // Remove geoemtry being cached the longest time
                    _geometryCache.Remove(_geometryCache.Keys.First(), out var removedGeometry);
                    // _geometryCache.Clear();
                }
                _geometryCache.TryAdd((areaType, featureId), geometry);
            }

            return geometry;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IGeoShape>> GetGeometriesAsync(
            IEnumerable<(AreaType areaType, string featureId)> areaKeys)
        {
            if (!areaKeys?.Any() ?? true)
            {
                return null;
            }

            var missingInCache = areaKeys.Except(_geometryCache.Keys);

            if (missingInCache?.Any() ?? false)
            {
                await Task.WhenAll(missingInCache.Select(mic => GetGeometryAsync(mic.Item1, mic.Item2)));
            }

            return _geometryCache.Where(gc => areaKeys.Contains(gc.Key)).Select(gc => gc.Value);
        }
    }
}
