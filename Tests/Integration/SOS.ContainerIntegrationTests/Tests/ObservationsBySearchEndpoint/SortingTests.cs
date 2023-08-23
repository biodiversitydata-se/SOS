using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos;
using SOS.ContainerIntegrationTests.Setup;
using SOS.ContainerIntegrationTests.TestData.TestDataBuilder;

namespace SOS.ContainerIntegrationTests.Tests.ObservationsBySearchEndpoint;

/// <summary>
/// Integration tests for the ObservationsBySearch endpoint with sorting observations scenarios.
/// </summary>
[Collection(TestCollection.Name)]
public class SortingTests : TestBase
{
    public SortingTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsObservationsInCorrectOrder_WhenOrderingObservationsByTaxonIdAscendingAndDescending()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present };

        // Act
        var responseAsc = await apiClient.PostAsync($"/observations/search?sortBy=taxon.id&sortOrder=asc", JsonContent.Create(searchFilter));
        var resultAsc = await responseAsc.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        var responseDesc = await apiClient.PostAsync($"/observations/search?sortBy=taxon.id&sortOrder=desc", JsonContent.Create(searchFilter));
        var resultDesc = await responseDesc.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        responseAsc.StatusCode.Should().Be(HttpStatusCode.OK);
        responseDesc.StatusCode.Should().Be(HttpStatusCode.OK);
        resultAsc!.Records.Select(m => m.Id).Should().BeInAscendingOrder();
        resultDesc!.Records.Select(m => m.Id).Should().BeInDescendingOrder();
    }
}