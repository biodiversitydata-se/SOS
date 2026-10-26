using SOS.Api.Contracts.Aggregations.Responses;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Aggregations.Requests;
using FizzWare.NBuilder;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using SOS.Api.Contracts.Analysis.Responses;
using FluentAssertions.Execution;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchEndpoint;

[Collection(TestCollection.Name)]
public class AggregateEndpointTests : ElasticsearchTestBase
{
    [Fact]
    public async Task AggregationField_IsCaseInsensitive()
    {
        var observations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(3)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).IsInDateSpan("2000-01-01T00:00:00", "2000-01-31T23:59:59")
            .TheNext(2).IsInDateSpan("2002-01-01T00:00:00", "2002-01-31T23:59:59")
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        using var client = TestFixture.CreateAnalysisApiClient();
        foreach (var endpoint in new[] { "/internal/aggregation", "/internal/aggregation_simple" })
        {
            string? expected = null;
            foreach (var field in new[] { "event.startYear", "Event.StartYear", "event.startyear", "EVENT.STARTYEAR", "EvEnT.StArTyEaR" })
            {
                using var response = await client.PostAsync($"{endpoint}?AGGREGATIONFIELD={field}&SORTORDER=KEYASCENDING",
                    JsonContent.Create(new SearchFilterAggregationInternalDto()), TestContext.Current.CancellationToken);

                response.StatusCode.Should().Be(HttpStatusCode.OK, $"endpoint={endpoint}, aggregationField={field}");
                using var scope = new AssertionScope($"endpoint={endpoint}, aggregationField={field}");
                var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
                using var document = JsonDocument.Parse(json);
                var records = endpoint.EndsWith("_simple", StringComparison.Ordinal)
                    ? document.RootElement : document.RootElement.GetProperty("records");
                records.GetArrayLength().Should().Be(2);
                expected ??= json;
                json.Should().Be(expected, "only the spelling of the field name changed");
            }
        }
    }

    [Fact]
    public async Task SimpleAggregation_PreservesNamedAndNumericSortOrderQueryValues()
    {
        var observations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(2).IsInDateSpan("2000-01-01T00:00:00", "2000-01-31T23:59:59")
            .TheNext(3).IsInDateSpan("2002-01-01T00:00:00", "2002-01-31T23:59:59")
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        using var client = TestFixture.CreateAnalysisApiClient();
        var cases = new[]
        {
            (SortOrder: "CountDescending", FirstKey: "2002"),
            (SortOrder: "CountAscending", FirstKey: "2000"),
            (SortOrder: "KeyAscending", FirstKey: "2000"),
            (SortOrder: "KeyDescending", FirstKey: "2002"),
            (SortOrder: "0", FirstKey: "2002")
        };

        foreach (var (sortOrder, firstKey) in cases)
        {
            using var response = await client.PostAsync($"/internal/aggregation_simple?aggregationField=event.startYear&sortOrder={sortOrder}",
                JsonContent.Create(new SearchFilterAggregationInternalDto()), TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK, $"sortOrder={sortOrder}");
            using var scope = new AssertionScope($"sortOrder={sortOrder}");
            var result = await response.Content.ReadFromJsonAsync<List<AggregationItemOrganismQuantityDto>>(TestContext.Current.CancellationToken);
            result.Should().HaveCount(2);
            result![0].AggregationKey.Should().Be(firstKey);
            result.Sum(item => item.DocCount).Should().Be(5);
        }
    }

    public AggregateEndpointTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task AggregationEndpoint_ReturnsExpectedAggregation_WhenAggregatingByYear()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).IsInDateSpan("2000-01-01T00:00:00", "2000-01-31T23:59:59")
             .TheNext(3).IsInDateSpan("2002-02-01T00:00:00", "2002-02-28T23:59:59")
             .TheNext(1).IsInDateSpan("2003-03-01T00:00:00", "2003-03-31T23:59:59")
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateAnalysisApiClient();
        var searchFilter = new SearchFilterInternalDto
        {
            
        };

        // Act
        var response = await apiClient.PostAsync($"/internal/aggregation?aggregationField=event.startYear", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);        
        var result = await response.Content.ReadFromJsonAsync<PagedAggregationResultDto<AggregationItemDto>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Records.Should().BeEquivalentTo(
            new List<AggregationItemDto>
            {
                new AggregationItemDto
                {
                    AggregationField = "2000",
                    Count = 1
                },
                new AggregationItemDto
                {
                    AggregationField = "2002",
                    Count = 3
                },
                new AggregationItemDto
                {
                    AggregationField = "2003",
                    Count = 1
                }
            },
            options => options.Excluding(m => m.UniqueTaxon)
        );
    }

    public class AggregationItemDto
    {
        /// <summary>
        /// Aggregated field
        /// </summary>
        public string AggregationField { get; set; }

        /// <summary>
        /// Document count
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Unique taxon count
        /// </summary>
        public int UniqueTaxon { get; set; }

        /// <summary>
        /// Organism quantity
        /// </summary>
        public int OrganismQuantity { get; set; }
    }
}
