using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchEndpoint;

[Collection(TestCollection.Name)]
public class OccurrenceStatusTests : TestBase
{
    public OccurrenceStatusTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsOnlyPresentObservations_WhenNoOccurrenceStatusFilterIsSet()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
             .TheFirst(60).With(o => o.NotPresent = false)
                          .With(o => o.NotRecovered = false)
             .TheNext(40).With(o => o.NotPresent = true)             
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            OccurrenceStatus = null
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 observations is present and only present observations should be returned if no filter is set.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsOnlyPresentObservations_WhenOccurrencePresentStatusFilterIsSet()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
             .TheFirst(60).With(o => o.NotPresent = false)
                          .With(o => o.NotRecovered = false)
             .TheNext(40).With(o => o.NotPresent = true)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 observations is present.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsOnlyPresentObservations_WhenOccurrenceAbsentStatusFilterIsSet()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
             .TheFirst(60).With(o => o.NotPresent = false)
                          .With(o => o.NotRecovered = false)
             .TheNext(40).With(o => o.NotPresent = true)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            OccurrenceStatus = OccurrenceStatusFilterValuesDto.Absent
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(40,
            because: "40 observations is absent.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsOnlyPresentObservations_WhenOccurrenceBothAbsentAndPresentStatusFilterIsSet()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
             .TheFirst(60).With(o => o.NotPresent = false)
                          .With(o => o.NotRecovered = false)
             .TheNext(40).With(o => o.NotPresent = true)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(100,
            because: "100 observations is present or absent.");
    }
}