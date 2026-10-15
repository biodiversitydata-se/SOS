using FluentAssertions.Execution;
using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Observations.Requests;
using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchEndpoint;

[Collection(TestCollection.Name)]
public class TotalCountCacheTests : ElasticsearchTestBase
{
    public TotalCountCacheTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task ReplacingTestDataInvalidatesCachedTotalCount()
    {
        var observations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(3)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        using var apiClient = TestFixture.CreateApiClient();
        var filter = new SearchFilterInternalDto();

        foreach (var expectedCount in new[] { 1, 3 })
        {
            // Query both endpoints before replacing the data, so both must discard their old counts.
            await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations.Take(expectedCount));
            foreach (var endpoint in new[] { "/observations/search", "/observations/internal/search" })
            {
                using var response = await apiClient.PostAsync(endpoint, JsonContent.Create(filter), TestContext.Current.CancellationToken);
                response.StatusCode.Should().Be(HttpStatusCode.OK, $"endpoint={endpoint}, expectedCount={expectedCount}");
                using var scope = new AssertionScope($"endpoint={endpoint}, expectedCount={expectedCount}");
                var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);
                result!.TotalCount.Should().Be(expectedCount);
            }
        }
    }
}
