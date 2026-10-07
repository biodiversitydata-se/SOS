using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Lib.Cache;
using SOS.Lib.Models.Cache;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Models.Search.Filters;
using System.Collections.Concurrent;
using System.IO.Compression;
using Xunit;

namespace SOS.Lib.UnitTests.Cache;

public class ClassCacheTests
{
    private const int MaxNumberOfItems = 50_000;

    [Theory]
    [InlineData(SearchSortOrder.Asc, "Asc")]
    [InlineData(SearchSortOrder.Desc, "Desc")]
    public void SortOrder_PreservesCachedJsonAndRoundTripsWithSystemTextJson(SearchSortOrder order, string name)
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cache = new ClassCache<SortOrderFilter>(memoryCache, NullLogger<ClassCache<SortOrderFilter>>.Instance);
        var entry = cache.CreateCacheEntry(new SortOrderFilter { SortBy = "modified", SortOrder = order });

        using var compressed = new MemoryStream(entry.Data);
        using var gzip = new GZipStream(compressed, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip);
        Assert.Equal($"{{\"SortBy\":\"modified\",\"SortOrder\":\"{name}\"}}", reader.ReadToEnd());

        var restored = cache.GetCacheEntryValue(entry);
        Assert.Equal("modified", restored.SortBy);
        Assert.Equal(order, restored.SortOrder);
    }

    [Fact]
    public void CheckCacheSize_RemovesLeastUsedItemsFromConcurrentDictionary()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cache = new ClassCache<object>(memoryCache, NullLogger<ClassCache<object>>.Instance);
        var entries = new ConcurrentDictionary<string, CacheEntry<int>>(
            Enumerable.Range(0, MaxNumberOfItems)
                .Select(index => new KeyValuePair<string, CacheEntry<int>>(
                    index.ToString(),
                    new CacheEntry<int> { Count = index })));

        cache.CheckCacheSize(entries);

        Assert.Equal(45_000, entries.Count);
        Assert.DoesNotContain("0", entries.Keys);
        Assert.Contains("49999", entries.Keys);
    }

    [Fact]
    public void CheckCacheSize_RemovesLeastUsedItemsFromDictionary()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cache = new ClassCache<object>(memoryCache, NullLogger<ClassCache<object>>.Instance);
        var entries = Enumerable.Range(0, MaxNumberOfItems)
            .ToDictionary(
                index => index.ToString(),
                index => new CacheEntry<int> { Count = index });

        cache.CheckCacheSize(entries);

        Assert.Equal(45_000, entries.Count);
        Assert.DoesNotContain("0", entries.Keys);
        Assert.Contains("49999", entries.Keys);
    }
}
