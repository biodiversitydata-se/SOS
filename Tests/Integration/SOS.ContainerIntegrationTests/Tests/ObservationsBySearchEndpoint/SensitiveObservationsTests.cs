using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos;
using SOS.Lib.Models.Shared;
using SOS.ContainerIntegrationTests.Setup;
using SOS.ContainerIntegrationTests.TestData.TestDataBuilder;
using SOS.ContainerIntegrationTests.Stubs;

namespace SOS.ContainerIntegrationTests.Tests.ObservationsBySearchEndpoint;

[Collection(TestCollection.Name)]
public class SensitiveObservationsTests : TestBase
{
    public SensitiveObservationsTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsNoObservations_WhenSearchingForPublicObservations_GivenOnlyProtectedExists()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
                  .HaveTaxonSensitivityCategory(3)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent };

        // Act
        var response = await apiClient.PostAsync($"/observations/search?sensitiveObservations=false", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(0,
            because: "0 observations added to Elasticsearch are public observations.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsNoObservations_WhenSearchingForProtectedObservations_GivenTheUserHasNoAccessRights()
    {
        // Arrange            
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
                  .HaveTaxonSensitivityCategory(3)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var userServiceStub = UserServiceStubFactory.CreateWithSightingAuthority(maxProtectionLevel: 1);
        var apiClient = TestFixture.CreateApiClientWithReplacedService(userServiceStub);
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent };

        // Act
        var response = await apiClient.PostAsync($"/observations/search?sensitiveObservations=true", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(0,
            because: "All observations added to Elasticsearch is sensitive and the user has no access rights.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenSearchingForSensitiveObservations_GivenTheUserHasAccessRights()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(60).HaveTaxonSensitivityCategory(3)
             .TheNext(40).HaveTaxonSensitivityCategory(4)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var userServiceStub = UserServiceStubFactory.CreateWithSightingAuthority(maxProtectionLevel: 3);
        var apiClient = TestFixture.CreateApiClientWithReplacedService(userServiceStub);
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent };

        // Act
        var response = await apiClient.PostAsync($"/observations/search?sensitiveObservations=true", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 observations added to Elasticsearch have sensitivty category 3 and the user has max access rights to category 3.");
    }

    [Fact]
    [Trait("Category", "AutomaticIntegrationTest")]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenSearchingForSensitiveObservationsObservedByMe()
    {
        // Arrange
        const int userId = TestAuthHandler.DefaultTestUserId;
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
                  .HaveTaxonSensitivityCategory(3)
            .TheFirst(60).With(m => m.ObserversInternal = new[] {
                new UserInternal { Id = userId, PersonId = userId, UserServiceUserId = userId, ViewAccess = true } })
            .Build();

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var userServiceStub = UserServiceStubFactory.CreateWithSightingAuthority(maxProtectionLevel: 1);
        var apiClient = TestFixture.CreateApiClientWithReplacedService(userServiceStub);
        apiClient.DefaultRequestHeaders.Add(TestAuthHandler.UserId, userId.ToString());
        var searchFilter = new SearchFilterDto { ObservedByMe = true };


        // Act
        var response = await apiClient.PostAsync($"/observations/search?sensitiveObservations=true", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 observations added to Elasticsearch are observed by UserId=15. " +
                     "The user has no access rights but can view its own observed sightings.");
    }

    [Fact]
    [Trait("Category", "AutomaticIntegrationTest")]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenSearchingForSensitiveObservationsReportedByMe()
    {
        // Arrange
        const int userId = TestAuthHandler.DefaultTestUserId;
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
                  .HaveTaxonSensitivityCategory(3)
            .TheFirst(60).With(m => m.ReportedByUserServiceUserId = userId)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var userServiceStub = UserServiceStubFactory.CreateWithSightingAuthority(maxProtectionLevel: 1);
        var apiClient = TestFixture.CreateApiClientWithReplacedService(userServiceStub);
        apiClient.DefaultRequestHeaders.Add(TestAuthHandler.UserId, userId.ToString());
        var searchFilter = new SearchFilterDto { ReportedByMe = true };

        // Act
        var response = await apiClient.PostAsync($"/observations/search?sensitiveObservations=true", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 observations added to Elasticsearch are observed by UserId=15. " +
                     "The user has no access rights but can view its own reported sightings.");
    }
}