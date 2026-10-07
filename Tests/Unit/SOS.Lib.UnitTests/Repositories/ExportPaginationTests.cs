using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Cluster;
using SOS.Api.Contracts.Observations.Requests;
using Elastic.Transport;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
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

public class ExportPaginationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ObservationPagesPreserveAllRecordsAndForwardTheLastSortValues(bool withoutPit)
    {
        var requests = new List<(string Path, JsonElement Body)>();
        var invoker = new ExportRequestInvoker(10001);
        var repository = CreateRepository(invoker, requests);
        var filter = new SearchFilter { UseSearchAfterWithoutPit = withoutPit };
        filter.Output.Fields = new[] { "occurrence.occurrenceId" };
        var ids = new List<string>();
        string pit = null;
        ICollection<FieldValue> after = null;

        while (true)
        {
            var page = await repository.GetObservationsBySearchAfterAsync<Observation>(filter, pit, after);
            var observations = page.Records.ToArray();
            if (observations.Length == 0) break;
            ids.AddRange(observations.Select(o => o.Occurrence.OccurrenceId));
            pit = page.PointInTimeId;
            after = page.SearchAfter;
        }

        Assert.Equal(10001, ids.Count);
        Assert.Equal(ids.Count, ids.Distinct().Count());
        var searches = requests.Where(r => r.Path.EndsWith("/_search")).ToArray();
        Assert.Equal(6, searches.Length);
        Assert.False(searches[0].Body.TryGetProperty("search_after", out _));
        Assert.Equal("2026-01-01T00:41:39.0000000Z", searches[1].Body.GetProperty("search_after")[0].GetString());
        Assert.Equal("2026-01-01T01:23:19.0000000Z", searches[2].Body.GetProperty("search_after")[0].GetString());
        Assert.Equal("2026-01-01T02:04:59.0000000Z", searches[3].Body.GetProperty("search_after")[0].GetString());
        Assert.Equal("2026-01-01T02:46:39.0000000Z", searches[4].Body.GetProperty("search_after")[0].GetString());
        Assert.Equal("2026-01-01T02:46:40.0000000Z", searches[5].Body.GetProperty("search_after")[0].GetString());
        Assert.All(searches, request =>
        {
            Assert.Equal(2500, request.Body.GetProperty("size").GetInt32());
            Assert.False(request.Body.GetProperty("track_total_hits").GetBoolean());
            Assert.Equal(!withoutPit, request.Body.TryGetProperty("pit", out _));
            Assert.Contains("occurrence.occurrenceId", request.Body.GetProperty("_source").ToString());
            Assert.Equal(searches[0].Body.GetProperty("query").ToString(), request.Body.GetProperty("query").ToString());
        });
        if (withoutPit)
        {
            Assert.DoesNotContain(requests, r => r.Path.EndsWith("/_pit"));
            Assert.Contains("modified", searches[0].Body.GetProperty("sort").ToString());
            Assert.Contains("occurrence.occurrenceId", searches[0].Body.GetProperty("sort").ToString());
            Assert.Contains("_index", searches[0].Body.GetProperty("sort").ToString());
            Assert.Null(pit);
        }
        else
        {
            Assert.Equal(2, requests.Count(r => r.Path.EndsWith("/_pit")));
            Assert.Contains("_shard_doc", searches[0].Body.GetProperty("sort").ToString());
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task DwcExtensionReadsUseSelectedPagingAndForwardCursor(bool withoutPit, bool multimedia)
    {
        var requests = new List<(string Path, JsonElement Body)>();
        var repository = CreateRepository(new ExportRequestInvoker(0), requests);
        var filter = new SearchFilter { UseSearchAfterWithoutPit = withoutPit };
        FieldValue[] after = ["2026-01-01T00:00:00.0000000Z", "previous-occurrence", "previous-index"];
        if (multimedia)
            await repository.GetMultimediaBySearchAfterAsync(filter, withoutPit ? null : "existing-pit", after);
        else
            await repository.GetMeasurementOrFactsBySearchAfterAsync(filter, withoutPit ? null : "existing-pit", after);

        var search = Assert.Single(requests, r => r.Path.EndsWith("/_search")).Body;
        Assert.Equal(!withoutPit, search.TryGetProperty("pit", out _));
        Assert.Equal("2026-01-01T00:00:00.0000000Z", search.GetProperty("search_after")[0].GetString());
        Assert.Equal("previous-occurrence", search.GetProperty("search_after")[1].GetString());
        Assert.Equal("previous-index", search.GetProperty("search_after")[2].GetString());
        Assert.Contains(multimedia ? "media" : "measurementOrFacts", search.GetProperty("_source").ToString());
        if (withoutPit) Assert.DoesNotContain(requests, r => r.Path.EndsWith("/_pit"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ModifiedSortingAndProtectionFilterAreUsedWithoutPit(bool fastSearch)
    {
        var requests = new List<(string Path, JsonElement Body)>();
        var repository = CreateRepository(new ExportRequestInvoker(0), requests);
        var filter = new SearchFilter(123, ProtectionFilter.BothPublicAndSensitive) { UseSearchAfterWithoutPit = true };
        filter.Output.SortOrders = new[] { new SortOrderFilter { SortBy = "event.eventId", SortOrder = SearchSortOrder.Asc } };
        if (fastSearch)
            await repository.GetChunkAsync<Observation>(filter, 0, 2500,
                new SearchExecutionOptions { KnownTotalCount = 42 });
        else
            await repository.GetObservationsBySearchAfterAsync<Observation>(filter);

        var search = Assert.Single(requests, r => r.Path.EndsWith("/_search"));
        Assert.Contains("protected", search.Path);
        var sort = search.Body.GetProperty("sort");
        Assert.True(sort[0].TryGetProperty("modified", out _));
        Assert.True(sort[1].TryGetProperty("occurrence.occurrenceId", out _));
        Assert.True(sort[2].TryGetProperty("_index", out _));
        Assert.Equal("asc", sort[0].GetProperty("modified").GetProperty("order").GetString());
        Assert.False(search.Body.GetProperty("track_total_hits").GetBoolean());
        Assert.False(search.Body.TryGetProperty("pit", out _));
    }

    private static ProcessedObservationCoreRepository CreateRepository(
        ExportRequestInvoker invoker, List<(string Path, JsonElement Body)> requests)
    {
        var settings = new ElasticsearchClientSettings(new SingleNodePool(new Uri("http://localhost:9200")), invoker)
            .DisableDirectStreaming()
            .OnRequestCompleted(call => requests.Add((call.Uri.AbsolutePath,
                JsonDocument.Parse(call.RequestBodyInBytes ?? Encoding.UTF8.GetBytes("{}")).RootElement.Clone())));
        var clients = new Mock<IElasticClientManager>();
        clients.SetupGet(m => m.Clients).Returns(new[] { new ElasticsearchClient(settings) });
        return new ProcessedObservationCoreRepository(clients.Object,
            new ElasticSearchConfiguration { IndexPrefix = Guid.NewGuid().ToString("N") },
            Mock.Of<ICache<string, ProcessedConfiguration>>(), Mock.Of<ITaxonManager>(),
            Mock.Of<IClassCache<ConcurrentDictionary<string, HealthResponse>>>(),
            new MemoryCache(new MemoryCacheOptions()), NullLogger<ProcessedObservationCoreRepository>.Instance);
    }

    private sealed class ExportRequestInvoker(int total) : InMemoryRequestInvoker(Array.Empty<byte>(), 200,
        headers: new Dictionary<string, IEnumerable<string>> { ["x-elastic-product"] = new[] { "Elasticsearch" } }), IRequestInvoker
    {
        private int _offset;

        public new Task<TResponse> RequestAsync<TResponse>(Endpoint endpoint, BoundConfiguration boundConfiguration,
            PostData postData, CancellationToken cancellationToken) where TResponse : TransportResponse, new()
        {
            string response;
            if (endpoint.Uri.AbsolutePath.EndsWith("/_search"))
            {
                var count = Math.Min(2500, total - _offset);
                var hits = Enumerable.Range(_offset, count).Select(i => new
                {
                    _index = "observations", _id = $"occ-{i:D5}",
                    _source = new { occurrence = new { occurrenceId = $"occ-{i:D5}" } },
                    sort = new[] { new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(i).ToString("O"), $"occ-{i:D5}", "observations" }
                });
                response = JsonSerializer.Serialize(new
                {
                    took = 1, timed_out = false, pit_id = endpoint.Uri.AbsolutePath == "/_search" ? "test-pit" : null,
                    _shards = new { total = 1, successful = 1, failed = 0 },
                    hits = new { total = new { value = total, relation = "eq" }, hits = hits.ToArray() }
                });
                _offset += count;
            }
            else if (endpoint.Uri.AbsolutePath.EndsWith("/_pit"))
                response = "{\"id\":\"test-pit\",\"succeeded\":true,\"num_freed\":1}";
            else
                response = "{}";
            return BuildResponseAsync<TResponse>(endpoint, boundConfiguration, postData, cancellationToken,
                Encoding.UTF8.GetBytes(response), 200, "application/json");
        }
    }
}
