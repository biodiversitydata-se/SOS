using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Cluster;
using Elastic.Transport;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
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
    public async Task CacheExpiresAfterOneMinuteEvenWithContinuationReads()
    {
        using var fixture = new Fixture();
        var filter = CreateFilter();
        var first = await fixture.Repository.GetChunkBySearchAfterAsync<Observation>(filter, 100, null);
        fixture.Clock.UtcNow += TimeSpan.FromSeconds(59);
        await fixture.Repository.GetChunkBySearchAfterAsync<Observation>(filter, 100, first.SearchAfter);
        fixture.Clock.UtcNow += TimeSpan.FromSeconds(1);
        fixture.Invoker.Total = 200;
        var refreshed = await fixture.Repository.GetChunkBySearchAfterAsync<Observation>(filter, 100, first.SearchAfter);

        Assert.Equal(200, refreshed.TotalCount);
        Assert.True(fixture.Searches[2].GetProperty("track_total_hits").GetBoolean());
    }

    [Fact]
    public async Task FirstPageAlwaysRecountsAndRefreshesCachedCount()
    {
        using var fixture = new Fixture();
        var filter = CreateFilter();
        await fixture.Repository.GetChunkBySearchAfterAsync<Observation>(filter, 100, null);
        fixture.Invoker.Total = 200;
        var first = await fixture.Repository.GetChunkBySearchAfterAsync<Observation>(filter, 100, null);
        var next = await fixture.Repository.GetChunkBySearchAfterAsync<Observation>(filter, 100, first.SearchAfter);

        Assert.Equal(200, first.TotalCount);
        Assert.Equal(200, next.TotalCount);
        Assert.True(fixture.Searches[1].GetProperty("track_total_hits").GetBoolean());
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

        public Fixture()
        {
            _cache = new MemoryCache(new MemoryCacheOptions { Clock = Clock });
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
                _cache, NullLogger<ProcessedObservationCoreRepository>.Instance);
        }

        public void Dispose() => _cache.Dispose();
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
