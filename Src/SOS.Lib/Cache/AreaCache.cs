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

        private IEnumerable<(AreaType AreaType, string FeatureId)> GetKeysMissingInCache(IEnumerable<(AreaType AreaType, string FeatureId)> keys) => keys?.Select(k => NormalizeKey(k)).Except(_geometryCache.Keys);

        private IDictionary<(AreaType AreaType, string FeatureId), IGeoShape> GetAreasFromCache(IEnumerable<(AreaType AreaType, string FeatureId)> keys) => _geometryCache.Where(gc => (keys?.Select(k => NormalizeKey(k))).Contains(gc.Key)).ToDictionary(gc => gc.Key, gc => gc.Value);

        private (AreaType AreaType, string FeatureId) NormalizeKey((AreaType AreaType, string FeatureId) key) => (key.AreaType, key.FeatureId?.ToLower());

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaRepository"></param>
        /// <param name="memoryCache"></param>
        /// <param name="logger"></param>
        public AreaCache(IAreaRepository areaRepository, IMemoryCache memoryCache, ILogger<CacheBase<string, Area>> logger) : base(areaRepository, memoryCache, logger)
        {
            _areaRepository = areaRepository;
            _geometryCache = new ConcurrentDictionary<(AreaType, string), Geometry>();
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
        public async Task<Geometry> GetGeometryAsync(AreaType areaType, string featureId)
        {
            if (_geometryCache.TryGetValue(NormalizeKey((areaType, featureId)), out var geometry))
            {
                return geometry;
            }

            geometry = (await _areaRepository.GetGeometryAsync(areaType, featureId));
            AddAreaToCache((areaType, featureId), geometry);
           
            return geometry;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Geometry>> GetGeometriesAsync(
            IEnumerable<(AreaType areaType, string featureId)> areaKeys)
        {
            if (!areaKeys?.Any() ?? true)
            {
                return null;
            }

            var missingInCache = GetKeysMissingInCache(areaKeys);
            if (missingInCache?.Any() ?? false)
            {
                await Task.WhenAll(missingInCache.Select(mic => GetGeometryAsync(mic.Item1, mic.Item2)));
            }

            return GetAreasFromCache(areaKeys)?.Values;
        }

        public async Task<IDictionary<(AreaType areaType, string featureId), Geometry>> GetBBoxGeometriesAsync(
           IEnumerable<(AreaType areaType, string featureId)> areaKeys)
        {
            if (!areaKeys?.Any() ?? true)
            {
                return null;
            }

            var missingInCache = GetKeysMissingInCache(areaKeys)?.ToArray();
            if (missingInCache?.Any() ?? false)
            {
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
                            var geometry = new Polygon(new LinearRing([
                                new Coordinate(area.BoundingBox.TopLeft.Longitude, area.BoundingBox.TopLeft.Latitude),
                                new Coordinate(area.BoundingBox.BottomRight.Longitude, area.BoundingBox.TopLeft.Latitude),
                                new Coordinate(area.BoundingBox.BottomRight.Longitude, area.BoundingBox.BottomRight.Latitude),
                                new Coordinate(area.BoundingBox.TopLeft.Longitude, area.BoundingBox.BottomRight.Latitude),
                                new Coordinate(area.BoundingBox.TopLeft.Longitude, area.BoundingBox.TopLeft.Latitude)
                            ]));
                            AddAreaToCache((area.AreaType, area.FeatureId), geometry);
                        }
                    }
                    fetchCount += batchSize;
                }
            }

            return GetAreasFromCache(areaKeys);
        }
    }
}
