using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
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
using System.Threading;
using System.Threading.Tasks;

namespace SOS.Lib.Cache
{
    /// <summary>
    /// Area cache
    /// </summary>
    public class AreaCache : CacheBase<string, Area>, IAreaCache
    {
        private readonly IAreaRepository _areaRepository;
        private const int NumberOfEntriesCleanupLimit = 75000;
        private readonly ConcurrentDictionary<(AreaType AreaType, string FeatureId), Geometry> _geometryCache;
        private readonly SemaphoreSlim _semaphore;

        private void AddAreaToCache((AreaType AreaType, string FeatureId) key, Geometry geometry)
        {
            if (geometry != null)
            {
                if (_geometryCache.Count >= NumberOfEntriesCleanupLimit) // prevent too large geometry cache
                {
                    // Remove geoemtry being cached the longest time
                    _geometryCache.Remove(_geometryCache.Keys.First(), out var removedGeometry);
                }
                _geometryCache.TryAdd(NormalizeKey(key), geometry);
            }
        }

        private async Task AddAreasWithGridGeometryToCache((AreaType areaType, string featureId)[] missingInCache)
        {
            var getGeometryTasks = new List<Task<Geometry>>();
            var missingCount = missingInCache.Count();
            var batchSizeDefault = missingCount > 1000 ? 1000 : missingCount;
            var fetchCount = 0;
            for (int i = 0; i < missingCount; i += batchSizeDefault)
            {
                var notFetchCount = missingCount - fetchCount;
                var batchSize = notFetchCount < batchSizeDefault ? notFetchCount : batchSizeDefault;
                var batch = new (AreaType areaType, string featureId)[batchSize];
                Array.Copy(missingInCache, i, batch, 0, batchSize);
                var pagedAreas = await _areaRepository.GetAreasAsync(batch, 0, batchSize);
                if ((pagedAreas.Records?.Count() ?? 0) != 0)
                {
                    foreach (var area in pagedAreas.Records)
                    {
                        AddAreaToCache((area.AreaType, area.FeatureId), area.GridGeometry);
                    }
                }
                fetchCount += batchSize;
            }
        }

        private IDictionary<(AreaType AreaType, string FeatureId), Geometry> GetAreasFromCache(IEnumerable<(AreaType AreaType, string FeatureId)> keys) => _geometryCache.Where(gc => (keys?.Select(k => NormalizeKey(k))).Contains(gc.Key)).ToDictionary(gc => gc.Key, gc => gc.Value);

        private IEnumerable<(AreaType AreaType, string FeatureId)> GetKeysMissingInCache(IEnumerable<(AreaType AreaType, string FeatureId)> keys) => keys?.Select(k => NormalizeKey(k)).Except(_geometryCache.Keys);

        private (AreaType AreaType, string FeatureId) NormalizeKey((AreaType AreaType, string FeatureId) key) => (key.AreaType, key.FeatureId?.ToLower());

        public override TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaRepository"></param>
        /// <param name="logger"></param>
        public AreaCache(IAreaRepository areaRepository, ILogger<CacheBase<string, Area>> logger) : base(areaRepository, logger)
        {
            _areaRepository = areaRepository;
            _geometryCache = new ConcurrentDictionary<(AreaType, string), Geometry>();            
            var processorCount = Environment.ProcessorCount;
            _semaphore = new SemaphoreSlim(processorCount, processorCount);
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
            var cache = GetCache();
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
        public async Task<Geometry> GetGeometryAsync(AreaType areaType, string featureId)
        {
            if (_geometryCache.TryGetValue(NormalizeKey((areaType, featureId)), out var geometry))
            {
                return geometry;
            }
            try
            {
                await _semaphore.WaitAsync();
                geometry = (await _areaRepository.GetGeometryAsync(areaType, featureId));
            }
            finally
            {
                _semaphore.Release();
            }
            
            AddAreaToCache((areaType, featureId), geometry);
           
            return geometry;
        }

        /// <inheritdoc />
        public async Task<IDictionary<(AreaType areaType, string featureId), Geometry>> GetGeometriesAsync(
            IEnumerable<(AreaType areaType, string featureId)> areaKeys)
        {
            if (!areaKeys?.Any() ?? true)
            {
                return null;
            }

            var missingInCache = GetKeysMissingInCache(areaKeys);
            if (missingInCache?.Any() ?? false)
            {
                var missingWithGridGeometry = missingInCache.Where(a => a.AreaType.HasGridGeometry());
                if (missingWithGridGeometry.Count() != 0)
                {
                    await AddAreasWithGridGeometryToCache(missingWithGridGeometry.ToArray());
                }
                
                var missingWithOutGridGeometry = missingInCache.Where(a => !a.AreaType.HasGridGeometry());
                if (missingWithOutGridGeometry.Count() != 0)
                {
                    await Task.WhenAll(missingWithOutGridGeometry.Select(mic => GetGeometryAsync(mic.Item1, mic.Item2)));
                } 
            }

            return GetAreasFromCache(areaKeys);
        }
    }
}
