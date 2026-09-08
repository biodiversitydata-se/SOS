using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Lib.Cache;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using System.Xml.Linq;
using Xunit;

namespace SOS.Lib.UnitTests.Cache;

public class DataProviderCacheTests
{
    [Fact]
    public async Task EmlExpiresOneHourAfterFetchEvenWhenReadInBetween()
    {
        var clock = new TestClock();
        using var memory = new MemoryCache(new MemoryCacheOptions { Clock = clock });
        var repository = new Mock<IDataProviderRepository>();
        repository.Setup(r => r.GetEmlAsync(1)).ReturnsAsync(XDocument.Parse("<eml />"));
        var cache = CreateCache(repository, memory);

        await cache.GetEmlAsync(1);
        clock.UtcNow += TimeSpan.FromMinutes(59);
        await cache.GetEmlAsync(1);
        repository.Verify(r => r.GetEmlAsync(1), Times.Once);
        clock.UtcNow += TimeSpan.FromMinutes(1);
        await cache.GetEmlAsync(1);
        repository.Verify(r => r.GetEmlAsync(1), Times.Exactly(2));
    }

    [Fact]
    public async Task ConcurrentReadsShareFetchAndReturnIndependentDocuments()
    {
        using var memory = new MemoryCache(new MemoryCacheOptions());
        var repository = new Mock<IDataProviderRepository>();
        var fetch = new TaskCompletionSource<XDocument>(TaskCreationOptions.RunContinuationsAsynchronously);
        repository.Setup(r => r.GetEmlAsync(1)).Returns(fetch.Task);
        repository.Setup(r => r.GetEmlAsync(2)).ReturnsAsync(XDocument.Parse("<other />"));
        var cache = CreateCache(repository, memory);
        var reads = Enumerable.Range(0, 10).Select(_ => cache.GetEmlAsync(1)).ToArray();
        // A pending read for one provider must not block another provider.
        Assert.Equal("other", (await cache.GetEmlAsync(2)).Root.Name.LocalName);
        var original = XDocument.Parse("<eml>original</eml>");
        fetch.SetResult(original);
        var results = await Task.WhenAll(reads);
        original.Root.Value = "changed at source";
        results[0].Root.Value = "changed by caller";

        Assert.Equal("original", results[1].Root.Value);
        Assert.Equal("original", (await cache.GetEmlAsync(1)).Root.Value);
        repository.Verify(r => r.GetEmlAsync(1), Times.Once);
        repository.Verify(r => r.GetEmlAsync(2), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SuccessfulStoreInvalidatesCachedEml(bool succeeds)
    {
        using var memory = new MemoryCache(new MemoryCacheOptions());
        var repository = new Mock<IDataProviderRepository>();
        var updated = XDocument.Parse("<eml>updated</eml>");
        repository.SetupSequence(r => r.GetEmlAsync(1))
            .ReturnsAsync(XDocument.Parse("<eml>original</eml>"))
            .ReturnsAsync(updated);
        repository.Setup(r => r.StoreEmlAsync(1, updated)).ReturnsAsync(succeeds);
        var cache = CreateCache(repository, memory);
        await cache.GetEmlAsync(1);

        Assert.Equal(succeeds, await cache.StoreEmlAsync(1, updated));
        Assert.Equal(succeeds ? "updated" : "original", (await cache.GetEmlAsync(1)).Root.Value);
        repository.Verify(r => r.GetEmlAsync(1), Times.Exactly(succeeds ? 2 : 1));
    }

    [Fact]
    public async Task MissingOrFailedReadsAreRetried()
    {
        using var memory = new MemoryCache(new MemoryCacheOptions());
        var repository = new Mock<IDataProviderRepository>();
        repository.SetupSequence(r => r.GetEmlAsync(1))
            .ReturnsAsync((XDocument)null)
            .ThrowsAsync(new InvalidOperationException("Read failed"))
            .ReturnsAsync(XDocument.Parse("<eml />"));
        var cache = CreateCache(repository, memory);

        Assert.Null(await cache.GetEmlAsync(1));
        await Assert.ThrowsAsync<InvalidOperationException>(() => cache.GetEmlAsync(1));
        Assert.NotNull(await cache.GetEmlAsync(1));
        Assert.NotNull(await cache.GetEmlAsync(1));
        repository.Verify(r => r.GetEmlAsync(1), Times.Exactly(3));
    }

    private static DataProviderCache CreateCache(Mock<IDataProviderRepository> repository, IMemoryCache memory) =>
        new(repository.Object, NullLogger<CacheBase<int, DataProvider>>.Instance, memory);

    private sealed class TestClock : ISystemClock
    {
        public DateTimeOffset UtcNow { get; set; } = new(2026, 9, 7, 0, 0, 0, TimeSpan.Zero);
    }
}
