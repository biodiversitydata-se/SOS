using SOS.Api.Contracts.Common.Responses;
using FizzWare.NBuilder;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchEndpoint;

[Collection(TestCollection.Name)]
public class ExcludeFilterTests : TestBase
{
    public ExcludeFilterTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenExcludingByOccurrenceIds()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        var excludeOccurrenceIds = verbatimObservations.Take(2).Select(m => $"urn:lsid:artportalen.se:sighting:{m.SightingId}").ToList();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            ExcludeFilter = new ExcludeFilterDto
            {
                OccurrenceIds = excludeOccurrenceIds
            }
        };

        // Act
        var response = await    apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(3,
            because: "2 of 5 observations were excluded.");
    }
}
