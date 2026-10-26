using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Observations.Requests;
using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.Setup.Stubs;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchEndpoint;

[Collection(TestCollection.Name)]
public class SensitiveObservationsTests : ElasticsearchTestBase
{
    public SensitiveObservationsTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task SharedApiHost_KeepsUserServicesIsolatedBetweenClients()
    {
        var observations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(3)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(2).HaveTaxonSensitivityCategory(3)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        using var allowedClient = TestFixture.CreateApiClientWithUserService(
            UserServiceStubFactory.CreateWithSightingAuthority(maxProtectionLevel: 3));
        var hostCount = TestFixture.ApiFactory.Factories.Count;
        using var deniedClient = TestFixture.CreateApiClientWithUserService(
            UserServiceStubFactory.CreateWithSightingAuthority(maxProtectionLevel: 1));
        TestFixture.ApiFactory.Factories.Should().HaveCount(hostCount,
            "clients with different permissions should reuse the same API host");
        using var defaultClient = TestFixture.CreateApiClient();
        var sensitiveIds = observations.Take(2)
            .Select(observation => $"urn:lsid:artportalen.se:sighting:{observation.SightingId}").ToArray();
        var publicId = $"urn:lsid:artportalen.se:sighting:{observations[2].SightingId}";

        for (var iteration = 0; iteration < 3; iteration++)
        {
            await Task.WhenAll(
                AssertObservations(allowedClient, sensitive: true, sensitiveIds),
                AssertObservations(deniedClient, sensitive: true, Array.Empty<string>()),
                AssertObservations(defaultClient, sensitive: false, new[] { publicId }));
        }

        async Task AssertObservations(HttpClient client, bool sensitive, string[] expectedIds)
        {
            using var response = await client.PostAsync($"/observations/search?sensitiveObservations={sensitive}",
                JsonContent.Create(new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent }),
                TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);
            result!.TotalCount.Should().Be(expectedIds.Length);
            result.Records.Select(observation => observation.Occurrence.OccurrenceId).Should().BeEquivalentTo(expectedIds);
        }
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsNoObservations_WhenSearchingForPublicObservations_GivenOnlyProtectedExists()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(2)
            .All().HaveValuesFromPredefinedObservations()
                  .HaveTaxonSensitivityCategory(3)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent };

        // Act
        var response = await apiClient.PostAsync($"/observations/search?sensitiveObservations=false", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(0,
            because: "0 observations added to Elasticsearch are public observations.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsNoObservations_WhenSearchingForProtectedObservations_GivenTheUserHasNoAccessRights()
    {
        // Arrange            
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(2)
            .All().HaveValuesFromPredefinedObservations()
                  .HaveTaxonSensitivityCategory(3)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var userServiceStub = UserServiceStubFactory.CreateWithSightingAuthority(maxProtectionLevel: 1);
        using var apiClient = TestFixture.CreateApiClientWithUserService(userServiceStub);
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent };

        // Act
        var response = await apiClient.PostAsync($"/observations/search?sensitiveObservations=true", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(0,
            because: "All observations added to Elasticsearch is sensitive and the user has no access rights.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenSearchingForSensitiveObservations_GivenTheUserHasAccessRights()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(3).HaveTaxonSensitivityCategory(3)
             .TheNext(2).HaveTaxonSensitivityCategory(4)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var userServiceStub = UserServiceStubFactory.CreateWithSightingAuthority(maxProtectionLevel: 3);
        using var apiClient = TestFixture.CreateApiClientWithUserService(userServiceStub);
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent };

        // Act
        var response = await apiClient.PostAsync($"/observations/search?sensitiveObservations=true", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(3,
            because: "3 observations added to Elasticsearch have sensitivty category 3 and the user has max access rights to category 3.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenSearchingForSensitiveObservationsObservedByMe()
    {
        // Arrange
        const int userId = TestAuthHandler.DefaultTestUserId;
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
                  .HaveTaxonSensitivityCategory(3)
            .TheFirst(3).With(m => m.ObserversInternal = new[] {
                new UserInternal { Id = userId, PersonId = userId, UserServiceUserId = userId, ViewAccess = true } })
            .Build();

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var userServiceStub = UserServiceStubFactory.CreateWithSightingAuthority(maxProtectionLevel: 1);
        using var apiClient = TestFixture.CreateApiClientWithUserService(userServiceStub);
        apiClient.DefaultRequestHeaders.Add(TestAuthHandler.UserId, userId.ToString());
        var searchFilter = new SearchFilterDto { ObservedByMe = true };


        // Act
        var response = await apiClient.PostAsync($"/observations/search?sensitiveObservations=true", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(3,
            because: "3 observations added to Elasticsearch are observed by UserId=15. " +
                     "The user has no access rights but can view its own observed sightings.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenSearchingForSensitiveObservationsReportedByMe()
    {
        // Arrange
        const int userId = TestAuthHandler.DefaultTestUserId;
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
                  .HaveTaxonSensitivityCategory(3)
            .TheFirst(3).With(m => m.ReportedByUserServiceUserId = userId)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var userServiceStub = UserServiceStubFactory.CreateWithSightingAuthority(maxProtectionLevel: 1);
        using var apiClient = TestFixture.CreateApiClientWithUserService(userServiceStub);
        apiClient.DefaultRequestHeaders.Add(TestAuthHandler.UserId, userId.ToString());
        var searchFilter = new SearchFilterDto { ReportedByMe = true };

        // Act
        var response = await apiClient.PostAsync($"/observations/search?sensitiveObservations=true", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(3,
            because: "3 observations added to Elasticsearch are observed by UserId=15. " +
                     "The user has no access rights but can view its own reported sightings.");
    }
}
