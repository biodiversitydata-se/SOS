using FizzWare.NBuilder;
using SOS.Lib.Dtos;
using SOS.Lib.Dtos.Filter;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchEndpoint;

[Collection(TestCollection.Name)]
public class TotalCountCacheTests : TestBase
{
    public TotalCountCacheTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Theory]
    [InlineData("/observations/search")]
    [InlineData("/observations/internal/search")]
    public async Task ReplacingTestDataInvalidatesCachedTotalCount(string endpoint)
    {
        var observations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(3)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        var apiClient = TestFixture.CreateApiClient();
        var filter = new SearchFilterInternalDto();

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations.Take(1));
        using var firstResponse = await apiClient.PostAsync(endpoint, JsonContent.Create(filter), TestContext.Current.CancellationToken);
        firstResponse.EnsureSuccessStatusCode();
        var first = await firstResponse.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);
        first!.TotalCount.Should().Be(1);

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        using var secondResponse = await apiClient.PostAsync(endpoint, JsonContent.Create(filter), TestContext.Current.CancellationToken);
        secondResponse.EnsureSuccessStatusCode();
        var second = await secondResponse.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);
        second!.TotalCount.Should().Be(3);
    }
}
