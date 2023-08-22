using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos;
using SOS.ContainerIntegrationTests.Setup;
using SOS.ContainerIntegrationTests.TestData.TestDataBuilder;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.ObservationsController.ObservationsBySearchEndpoint;

[Collection(TestCollection.Name)]
public class OccurrenceStatusFilterTests : TestBase
{
    public OccurrenceStatusFilterTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]    
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByPresentObservations()
    {            
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()                    
            .TheFirst(60).With(v => v.NotPresent = false)
                         .With(v => v.NotRecovered = false)
             .TheNext(20).With(v => v.NotPresent = true)
             .TheNext(20).With(v => v.NotRecovered = true)
            .Build();            
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        
        // Assert            
        response.StatusCode.Should().Be(HttpStatusCode.OK);            
        result!.TotalCount.Should().Be(60, because: "60 observations added to Elasticsearch are present observations");
    }
}