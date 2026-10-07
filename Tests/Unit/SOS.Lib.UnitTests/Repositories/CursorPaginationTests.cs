using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Cluster;
using SOS.Api.Contracts.Observations.Requests;
using Elastic.Transport;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Lib.Cache;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SOS.Lib.UnitTests.Repositories;

public class CursorPaginationTests
{
    [Fact]
    public async Task OffsetSearchReusesTotalCountFromFirstPage()
    {
        using var fixture = new Fixture();
        var filter = CreateFilter();
        var first = await fixture.Repository.GetChunkAsync<Observation>(filter, 0, 100);
        fixture.Invoker.Total = 200;
        var second = await fixture.Repository.GetChunkAsync<Observation>(filter, 100, 100);

        Assert.Equal(100, first.TotalCount);
        Assert.Equal(100, second.TotalCount);
        Assert.True(fixture.Searches[0].GetProperty("track_total_hits").GetBoolean());
        Assert.False(fixture.Searches[1].GetProperty("track_total_hits").GetBoolean());
    }

    [Fact]
    public async Task UniqueSortFieldIsNotAddedTwice()
    {
        using var fixture = new Fixture();
        var filter = CreateFilter();
        filter.Output.SortOrders =
        [
            new SortOrderFilter
            {
                SortBy = "Occurrence.OccurrenceId",
                SortOrder = SearchSortOrder.Asc
            }
        ];

        await fixture.Repository.GetChunkAsync<Observation>(filter, 0, 100);

        var sort = Assert.Single(fixture.Searches).GetProperty("sort");
        Assert.Equal(JsonValueKind.Object, sort.ValueKind);
        Assert.True(sort.TryGetProperty("occurrence.occurrenceId", out _));
    }

    [Fact]
    public async Task ExactAndLimitedTotalCountsHaveSeparateCacheEntries()
    {
        using var fixture = new Fixture();
        var publicFilter = CreateFilter();
        await fixture.Repository.GetChunkAsync<Observation>(publicFilter, 0, 100);
        fixture.Invoker.Total = 200;
        var internalFilter = new SearchFilterInternal(publicFilter);
        internalFilter.RoleId = publicFilter.RoleId;
        internalFilter.AuthorizationApplicationIdentifier = publicFilter.AuthorizationApplicationIdentifier;
        var internalResult = await fixture.Repository.GetChunkAsync<Observation>(internalFilter, 0, 100);

        Assert.Equal(200, internalResult.TotalCount);
        Assert.Equal(10_000, fixture.Searches[1].GetProperty("track_total_hits").GetInt32());
    }

    [Fact]
    public async Task ContinuationReusesCountButFetchesFreshRecordsAndPreservesSortAndProjection()
    {
        using var fixture = new Fixture();
        var filter = CreateFilter();
        var first = await fixture.Repository.GetChunkBySearchAfterAsync<Observation>(filter, 100, null);
        fixture.Invoker.Total = 200;
        var second = await fixture.Repository.GetChunkBySearchAfterAsync<Observation>(filter, 50, first.SearchAfter);

        Assert.Equal(100, first.TotalCount);
        Assert.Equal(100, second.TotalCount);
        Assert.Equal("occ-2", Assert.Single(second.Records).Occurrence.OccurrenceId);
        var requests = fixture.Searches;
        Assert.Equal(2, requests.Count);
        Assert.True(requests[0].GetProperty("track_total_hits").GetBoolean());
        Assert.False(requests[1].GetProperty("track_total_hits").GetBoolean());
        Assert.Equal(50, requests[1].GetProperty("size").GetInt32());
        Assert.Equal("occ-1", requests[1].GetProperty("search_after")[1].GetString());
        Assert.Equal("desc", requests[1].GetProperty("sort")[0].GetProperty("modified").GetProperty("order").GetString());
        Assert.Contains("occurrence.occurrenceId", requests[1].GetProperty("sort").ToString());
        Assert.Contains("occurrence.occurrenceId", requests[1].GetProperty("_source").ToString());
        Assert.Equal(requests[0].GetProperty("query").ToString(), requests[1].GetProperty("query").ToString());
    }

    [Fact]
    public async Task CursorSearchCanReuseKnownTotalCount()
    {
        using var fixture = new Fixture();

        var result = await fixture.Repository.GetChunkBySearchAfterAsync<Observation>(CreateFilter(), 100, null,
            new SearchExecutionOptions { KnownTotalCount = 42 });

        Assert.Equal(42, result.TotalCount);
        Assert.False(Assert.Single(fixture.Searches).GetProperty("track_total_hits").GetBoolean());
    }

    [Fact]
    public async Task CacheExpiresAfterTwoMinutesEvenWithContinuationReads()
    {
        using var fixture = new Fixture();
        var filter = CreateFilter();
        var first = await fixture.Repository.GetChunkBySearchAfterAsync<Observation>(filter, 100, null);
        fixture.Clock.UtcNow += TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(59));
        await fixture.Repository.GetChunkBySearchAfterAsync<Observation>(filter, 100, first.SearchAfter);
        fixture.Clock.UtcNow += TimeSpan.FromSeconds(1);
        fixture.Invoker.Total = 200;
        var refreshed = await fixture.Repository.GetChunkBySearchAfterAsync<Observation>(filter, 100, first.SearchAfter);

        Assert.Equal(200, refreshed.TotalCount);
        Assert.True(fixture.Searches[2].GetProperty("track_total_hits").GetBoolean());
    }

    [Fact]
    public async Task LargeQueryDoesNotUseTheCountCache()
    {
        using var fixture = new Fixture();
        var filter = CreateFilter();
        filter.Taxa = new TaxonFilter { Ids = Enumerable.Range(1, 10_000) };

        var first = await fixture.Repository.GetChunkBySearchAfterAsync<Observation>(filter, 100, null);
        fixture.Invoker.Total = 200;
        var next = await fixture.Repository.GetChunkBySearchAfterAsync<Observation>(filter, 100, first.SearchAfter);

        Assert.Equal(200, next.TotalCount);
        Assert.True(fixture.Searches[1].GetProperty("track_total_hits").GetBoolean());
    }

    [Fact]
    public async Task RequestPolicyCanBypassTheCountCache()
    {
        using var fixture = new Fixture();
        var filter = CreateFilter();
        await fixture.Repository.GetChunkAsync<Observation>(filter, 0, 100);
        fixture.Invoker.Total = 200;

        var result = await fixture.Repository.GetChunkAsync<Observation>(filter, 100, 100,
            new SearchExecutionOptions { UseSearchTotalCountCache = false });

        Assert.Equal(200, result.TotalCount);
        Assert.True(fixture.Searches[1].GetProperty("track_total_hits").GetBoolean());
    }

    [Fact]
    public async Task RepeatedFirstPageReusesCachedCount()
    {
        using var fixture = new Fixture();
        var filter = CreateFilter();
        await fixture.Repository.GetChunkBySearchAfterAsync<Observation>(filter, 100, null);
        fixture.Invoker.Total = 200;
        var first = await fixture.Repository.GetChunkBySearchAfterAsync<Observation>(filter, 100, null);
        var next = await fixture.Repository.GetChunkBySearchAfterAsync<Observation>(filter, 100, first.SearchAfter);

        Assert.Equal(100, first.TotalCount);
        Assert.Equal(100, next.TotalCount);
        Assert.False(fixture.Searches[1].GetProperty("track_total_hits").GetBoolean());
        Assert.False(fixture.Searches[2].GetProperty("track_total_hits").GetBoolean());
    }

    [Theory]
    [InlineData("query")]
    [InlineData("user")]
    [InlineData("role")]
    [InlineData("application")]
    [InlineData("protection")]
    public async Task CountIsNotReusedAcrossDifferentQueriesOrAuthorizationContexts(string change)
    {
        using var fixture = new Fixture();
        var first = await fixture.Repository.GetChunkBySearchAfterAsync<Observation>(CreateFilter(), 100, null);
        var changed = CreateFilter();
        switch (change)
        {
            case "query": changed.DataProviderIds = new List<int> { 1 }; break;
            case "user": changed.ExtendedAuthorization.UserId = 456; break;
            case "role": changed.RoleId = 2; break;
            case "application": changed.AuthorizationApplicationIdentifier = "another-app"; break;
            case "protection": changed.ExtendedAuthorization.ProtectionFilter = ProtectionFilter.Sensitive; break;
        }
        fixture.Invoker.Total = 200;
        var next = await fixture.Repository.GetChunkBySearchAfterAsync<Observation>(changed, 100, first.SearchAfter);

        Assert.Equal(200, next.TotalCount);
        Assert.True(fixture.Searches[1].GetProperty("track_total_hits").GetBoolean());
    }

    [Fact]
    public async Task CursorFromAnotherInstanceRecountsWhenCacheIsEmpty()
    {
        using var firstInstance = new Fixture();
        using var secondInstance = new Fixture();
        var page = await firstInstance.Repository.GetChunkBySearchAfterAsync<Observation>(CreateFilter(), 100, null);
        secondInstance.Invoker.Total = 200;
        var next = await secondInstance.Repository.GetChunkBySearchAfterAsync<Observation>(CreateFilter(), 100, page.SearchAfter);

        Assert.Equal(200, next.TotalCount);
        Assert.True(Assert.Single(secondInstance.Searches).GetProperty("track_total_hits").GetBoolean());
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public async Task IncompleteSearchDoesNotPopulateCountCache(bool timedOut, int failedShards)
    {
        using var fixture = new Fixture();
        fixture.Invoker.TimedOut = timedOut;
        fixture.Invoker.FailedShards = failedShards;
        var first = await fixture.Repository.GetChunkBySearchAfterAsync<Observation>(CreateFilter(), 100, null);
        fixture.Invoker.TimedOut = false;
        fixture.Invoker.FailedShards = 0;
        fixture.Invoker.Total = 200;
        var next = await fixture.Repository.GetChunkBySearchAfterAsync<Observation>(CreateFilter(), 100, first.SearchAfter);

        Assert.Equal(200, next.TotalCount);
        Assert.True(fixture.Searches[1].GetProperty("track_total_hits").GetBoolean());
    }

    [Fact]
    public async Task ClearingCacheRecountsAfterDatasetReset()
    {
        using var fixture = new Fixture();
        await fixture.Repository.GetChunkAsync<Observation>(CreateFilter(), 0, 100);
        fixture.Invoker.Total = 200;
        fixture.ClearCountCache();

        var result = await fixture.Repository.GetChunkAsync<Observation>(CreateFilter(), 0, 100);

        Assert.Equal(200, result.TotalCount);
        Assert.True(fixture.Searches[1].GetProperty("track_total_hits").GetBoolean());
    }

    [Fact]
    public async Task RepositoryWithoutInjectedCacheDoesNotReuseCountsAcrossInstances()
    {
        using var firstInstance = new Fixture(enableCountCache: false);
        using var secondInstance = new Fixture(enableCountCache: false);
        await firstInstance.Repository.GetChunkAsync<Observation>(CreateFilter(), 0, 100);
        secondInstance.Invoker.Total = 200;

        var result = await secondInstance.Repository.GetChunkAsync<Observation>(CreateFilter(), 0, 100);

        Assert.Equal(200, result.TotalCount);
        Assert.True(Assert.Single(secondInstance.Searches).GetProperty("track_total_hits").GetBoolean());
    }

    private static SearchFilter CreateFilter()
    {
        var filter = new SearchFilter(123, ProtectionFilter.Public) { RoleId = 1, AuthorizationApplicationIdentifier = "app" };
        filter.Output.Fields = new[] { "occurrence.occurrenceId" };
        filter.Output.SortOrders = new[] { new SortOrderFilter { SortBy = "modified", SortOrder = SearchSortOrder.Desc } };
        return filter;
    }

    private sealed class Fixture : IDisposable
    {
        public TestClock Clock { get; } = new();
        public CursorRequestInvoker Invoker { get; } = new();
        public List<JsonElement> Searches { get; } = new();
        public ProcessedObservationCoreRepository Repository { get; }
        private readonly MemoryCache _cache;
        private readonly SearchTotalCountCache _totalCountCache;

        public void ClearCountCache() => _totalCountCache.Clear();

        public Fixture(bool enableCountCache = true)
        {
            _cache = new MemoryCache(new MemoryCacheOptions { Clock = Clock });
            _totalCountCache = new SearchTotalCountCache(new MemoryCacheOptions { Clock = Clock });
            var settings = new ElasticsearchClientSettings(new SingleNodePool(new Uri("http://localhost:9200")), Invoker)
                .DisableDirectStreaming()
                .OnRequestCompleted(call =>
                {
                    if (call.Uri.AbsolutePath.EndsWith("/_search"))
                        Searches.Add(JsonDocument.Parse(call.RequestBodyInBytes!).RootElement.Clone());
                });
            var clients = new Mock<IElasticClientManager>();
            clients.SetupGet(m => m.Clients).Returns(new[] { new ElasticsearchClient(settings) });
            Repository = new ProcessedObservationCoreRepository(clients.Object,
                new ElasticSearchConfiguration { IndexPrefix = "cursor-tests" },
                Mock.Of<ICache<string, ProcessedConfiguration>>(), Mock.Of<ITaxonManager>(),
                Mock.Of<IClassCache<ConcurrentDictionary<string, HealthResponse>>>(),
                _cache, NullLogger<ProcessedObservationCoreRepository>.Instance, enableCountCache ? _totalCountCache : null);
        }

        public void Dispose()
        {
            _cache.Dispose();
            _totalCountCache.Dispose();
        }
    }

    private sealed class TestClock : ISystemClock
    {
        public DateTimeOffset UtcNow { get; set; } = new(2026, 9, 7, 0, 0, 0, TimeSpan.Zero);
    }

    private sealed class CursorRequestInvoker() : InMemoryRequestInvoker(Array.Empty<byte>(), 200,
        headers: new Dictionary<string, IEnumerable<string>> { ["x-elastic-product"] = new[] { "Elasticsearch" } }), IRequestInvoker
    {
        public long Total { get; set; } = 100;
        public bool TimedOut { get; set; }
        public int FailedShards { get; set; }
        private int _page;

        public new Task<TResponse> RequestAsync<TResponse>(Endpoint endpoint, BoundConfiguration boundConfiguration,
            PostData postData, CancellationToken cancellationToken) where TResponse : TransportResponse, new()
        {
            var response = "{}";
            if (endpoint.Uri.AbsolutePath.EndsWith("/_search"))
            {
                var id = $"occ-{++_page}";
                response = JsonSerializer.Serialize(new
                {
                    took = 1, timed_out = TimedOut,
                    _shards = new { total = 2, successful = 2 - FailedShards, failed = FailedShards },
                    hits = new
                    {
                        total = new { value = Total, relation = "eq" },
                        hits = new[] { new { _index = "observations", _id = id,
                            _source = new { occurrence = new { occurrenceId = id } }, sort = new object[] { 123456789L, id } } }
                    }
                });
            }
            return BuildResponseAsync<TResponse>(endpoint, boundConfiguration, postData, cancellationToken,
                Encoding.UTF8.GetBytes(response), 200, "application/json");
        }
    }
}
