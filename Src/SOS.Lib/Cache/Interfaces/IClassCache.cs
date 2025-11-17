using SOS.Lib.Models.Cache;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace SOS.Lib.Cache.Interfaces;

/// <summary>
/// Class holding a entity 
/// </summary>
/// <typeparam name="TClass"></typeparam>
public interface IClassCache<TClass>
{
    /// <summary>
    /// Event raised when cache is released
    /// </summary>
    event EventHandler CacheReleased;

    /// <summary>
    /// Event raised when cache soon expires
    /// </summary>
    event EventHandler CacheExpireSoon;

    /// <summary>
    /// Get cached entity
    /// </summary>
    /// <returns></returns>
    TClass Get();

    /// <summary>
    /// Set entity cache
    /// </summary>
    /// <param name="entity"></param>
    void Set(TClass entity);

    CacheEntry<T> CreateCacheEntry<T>(T item);
    T GetCacheEntryValue<T>(CacheEntry<T> entry);
    void CheckCacheSize<T>(Dictionary<string, CacheEntry<T>> dictionary);
    void CheckCacheSize<T>(ConcurrentDictionary<string, CacheEntry<T>> dictionary);
}
