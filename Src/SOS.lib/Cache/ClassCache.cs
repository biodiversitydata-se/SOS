using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Cache;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace SOS.Lib.Cache
{
    /// <inheritdoc />
    public class ClassCache<TClass> : IClassCache<TClass>
    {
        private static readonly object InitLock = new object();
        private readonly IMemoryCache _memoryCache;
        private readonly string _cacheKey;
        protected readonly ILogger<ClassCache<TClass>> Logger;

        private JsonSerializerOptions _cacheKeyJsonSerializerOptions;
        private JsonSerializerOptions _cacheDataJsonSerializerOptions;
        private int _maxNumberOfItems = 50000;
        private Timer _renewalTimer;

        private void OnCacheEviction(object key, object value, EvictionReason reason, object state)
        {
            // Sometimes event is raised even if entity exists in cache.
            // In order to not bubble event when not needed, check if entity exists in cache
            if (CacheReleased == null || _memoryCache.TryGetValue(_cacheKey, out var entity))
            {
                return;
            }
            
            Logger.LogInformation($"Cache evicted. Key=\"{key}\", Reason={reason}");
            if (reason == EvictionReason.Expired || reason == EvictionReason.TokenExpired)
            {
                CacheReleased.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="memoryCache"></param>
        /// <param name="logger"></param>
        public ClassCache(IMemoryCache memoryCache, ILogger<ClassCache<TClass>> logger)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _cacheKey = typeof(TClass).GetFormattedName();
            Logger = logger;

            _cacheKeyJsonSerializerOptions = _cacheDataJsonSerializerOptions = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                Converters =
                {
                    new JsonStringEnumConverter(),
                    new GeoShapeConverter(),
                    new NetTopologySuite.IO.Converters.GeoJsonConverterFactory()
                }
            };
        }

        /// <summary>
        /// Cache duration.
        /// </summary>
        public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(10);
        public TimeSpan CacheExpireSoonTimeSpan { get; set; } = TimeSpan.FromMinutes(1);

        /// <inheritdoc />
        public event EventHandler CacheReleased;

        public event EventHandler CacheExpireSoon;

        /// <inheritdoc />
        public TClass Get()
        {
            _memoryCache.TryGetValue(_cacheKey, out var entity);
            return (TClass)entity;
        }

        /// <inheritdoc />
        public void Set(TClass entity)
        {
            lock (InitLock)
            {
                var expirationToken = new CancellationChangeToken(
                    new CancellationTokenSource(CacheDuration).Token);
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetPriority(CacheItemPriority.NeverRemove)
                    .AddExpirationToken(expirationToken)
                    .RegisterPostEvictionCallback(callback: OnCacheEviction, state: this);                
                _memoryCache.Set(_cacheKey, entity, cacheEntryOptions);
                Logger.LogInformation($"Cache set. Key=\"{_cacheKey}\"");
                var expirationTime = CacheDuration - CacheExpireSoonTimeSpan;
                _renewalTimer = new Timer(OnRenewalTimerElapsed, null, expirationTime, Timeout.InfiniteTimeSpan);
            }
        }

        private void OnRenewalTimerElapsed(object state)
        {
            if (CacheExpireSoon == null)
            {
                return;
            }

            Logger.LogInformation($"Cache expiration approaching. Key=\"{_cacheKey}\".");
            CacheExpireSoon.Invoke(this, EventArgs.Empty);
        }

        public void CheckCacheSize<T>(Dictionary<string, CacheEntry<T>> dictionary)
        {
            if (dictionary.Count >= _maxNumberOfItems)
            {
                RemoveLeastUsedItems(ref dictionary);
            }
        }

        private void RemoveLeastUsedItems<T>(ref Dictionary<string, CacheEntry<T>> dictionary)
        {
            var itemsToRemove = dictionary.OrderBy(entry => entry.Value.Count)
                                      .Take(Convert.ToInt32(_maxNumberOfItems * 0.1))
                                      .Select(entry => entry.Key)
                                      .ToList();

            dictionary = dictionary.Where(kvp => !itemsToRemove.Contains(kvp.Key))
                            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public CacheEntry<T> CreateCacheEntry<T>(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            using var memoryStream = new MemoryStream();
            JsonSerializer.Serialize(memoryStream, item, _cacheDataJsonSerializerOptions);
            byte[] byteArray = memoryStream.ToArray();
            return new CacheEntry<T>
            {
                Count = 1,
                Data = Compress(byteArray)
            };
        }

        public T GetCacheEntryValue<T>(CacheEntry<T> entry)
        {
            if (entry.Data == null)
                throw new ArgumentNullException(nameof(entry.Data));

            entry.Count++;
            var byteArray = Decompress(entry.Data);
            using var memoryStream = new MemoryStream(byteArray);
            return JsonSerializer.Deserialize<T>(memoryStream, _cacheDataJsonSerializerOptions);
        }

        private byte[] Compress(byte[] data)
        {
            using var output = new MemoryStream();
            using (var zip = new GZipStream(output, CompressionMode.Compress))
            {
                zip.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        private byte[] Decompress(byte[] compressedData)
        {
            using var input = new MemoryStream(compressedData);
            using var output = new MemoryStream();
            using (var zip = new GZipStream(input, CompressionMode.Decompress))
            {
                zip.CopyTo(output);
            }
            return output.ToArray();
        }
    }
}
