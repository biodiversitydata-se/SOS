using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos;
using SOS.Lib.Models.Shared;
using SOS.ContainerIntegrationTests.Setup;
using SOS.ContainerIntegrationTests.TestData.TestDataBuilder;
using SOS.ContainerIntegrationTests.Stubs;

namespace SOS.ContainerIntegrationTests.Tests.ObservationsEndpoints.ObservationsBySearchEndpoint;

[Collection(TestCollection.Name)]
public class MyObservationsTests : TestBase
{
    public MyObservationsTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByObservedByMe()
    {
        // Arrange
        const int userId = TestAuthHandler.DefaultTestUserId;
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(60)
                .With(m => m.ObserversInternal = m.ObserversInternal = new[] {
                    new UserInternal() { Id = userId, PersonId = userId, UserServiceUserId = userId, ViewAccess = true } })
                .With(m => m.Observers = "Tom Volgers")
                .With(m => m.ReportedByUserServiceUserId = userId)
                .With(m => m.ReportedBy = "Tom Volgers")
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { ObservedByMe = true };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 observations added to Elasticsearch are observed by UserId=15.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsNoObservations_WhenFilteringByObservedByMe_GivenNoObservationsObservedByMe()
    {
        // Arrange
        const int userId = TestAuthHandler.DefaultTestUserId;
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(60).With(m => m.ObserversInternal = null)
                         .With(m => m.Observers = "Via Tom Volgers")
                         .With(m => m.ReportedByUserServiceUserId = userId)
                         .With(m => m.ReportedBy = "Tom Volgers")
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { ObservedByMe = true };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(0,
            because: "0 observations added to Elasticsearch are observed by UserId=15.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByReportedByMe()
    {
        // Arrange
        const int userId = TestAuthHandler.DefaultTestUserId;
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(60).With(m => m.ObserversInternal = null)
                         .With(m => m.Observers = "Via Tom Volgers")
                         .With(m => m.ReportedByUserServiceUserId = userId)
                         .With(m => m.ReportedBy = "Tom Volgers")
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { ReportedByMe = true };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 observations added to Elasticsearch are reported by UserId=15.");
    }
}