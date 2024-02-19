using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using SOS.Observations.Api.IntegrationTests.Setup.Stubs;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchEndpoint;

[Collection(TestCollection.Name)]
public class DiffusionFilterTests : TestBase
{
    public DiffusionFilterTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByDiffusionPublic()
    {
        const int userId = TestAuthHandler.DefaultTestUserId;
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(20)
                .IsDiffused(100)
            .TheNext(20)
                .IsDiffused(500)
            .TheNext(20)
                .IsDiffused(1000)
            .TheNext(40)
                .With(o => o.ProtectedBySystem = false)
                .With(o => o.Site.DiffusionId = 0)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations, true);
        
        var searchFilter = new SearchFilterInternalDto { 
            ProtectionFilter = ProtectionFilterDto.Public,
            Output = new OutputFilterExtendedDto
            {
                Fields = new[] { "diffusionStatus" }
            }
        };
        var apiClient = TestFixture.CreateApiClient();

        // Act
        var response = await apiClient.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(100, because: "100 observations added to Elasticsearch are public");
        result!.Records.Count(o => o.DiffusionStatus != DiffusionStatus.NotDiffused).Should().Be(60, because: "60 observations added to Elasticsearch are diffused");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByDiffusionSensitive()
    {
        const int userId = TestAuthHandler.DefaultTestUserId;
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(20)
                .IsDiffused(100)
                .With(o => o.ReportedByUserServiceUserId = userId)
            .TheNext(20)
                .IsDiffused(500)
                .With(o => o.ReportedByUserServiceUserId = userId)
            .TheNext(20)
                .IsDiffused(1000)
                .With(o => o.ReportedByUserServiceUserId = userId)
            .TheNext(40)
                .With(o => o.ProtectedBySystem = false)
                .With(o => o.Site.DiffusionId = 0)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations, true);

        var searchFilter = new SearchFilterInternalDto
        {
            ProtectionFilter = ProtectionFilterDto.Sensitive,
            Output = new OutputFilterExtendedDto
            {
                Fields = new[] { "diffusionStatus" }
            }
        };
        var apiClient = TestFixture.CreateApiClient();

        // Act
        var response = await apiClient.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60, because: "60 observations added to Elasticsearch are sensitive");
        result!.Records.Count(o => o.DiffusionStatus == DiffusionStatus.NotDiffused).Should().Be(60, because: "Sensitive observations added to Elasticsearch are not diffused");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByDiffusionPublicAndSensitive()
    {
        const int userId = TestAuthHandler.DefaultTestUserId;
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(20)
                .IsDiffused(100)
                .With(o => o.ReportedByUserServiceUserId = userId)
            .TheNext(20)
                .IsDiffused(500)
                .With(o => o.ReportedByUserServiceUserId = userId)
            .TheNext(20)
                .IsDiffused(1000)
                .With(o => o.ReportedByUserServiceUserId = userId)
            .TheNext(40)
                .With(o => o.ProtectedBySystem = false)
                .With(o => o.Site.DiffusionId = 0)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations, true);

        var searchFilter = new SearchFilterInternalDto
        {
            ProtectionFilter = ProtectionFilterDto.BothPublicAndSensitive,
            Output = new OutputFilterExtendedDto
            {
                Fields = new[] { "diffusionStatus" }
            }
        };
        var apiClient = TestFixture.CreateApiClient();

        // Act
        var response = await apiClient.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(100, because: "60 observations added to Elasticsearch are sensitive and 40 are public");
        result!.Records.Count(o => o.DiffusionStatus == DiffusionStatus.NotDiffused).Should().Be(100, because: "Diffused observations are not return when quering both public and sensitive index");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByDiffusionPublicAndSensitiveNoAccess()
    {
        const int userId = TestAuthHandler.DefaultTestUserId;
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(20)
                .IsDiffused(100)
            .TheNext(20)
                .IsDiffused(500)
            .TheNext(20)
                .IsDiffused(1000)
            .TheNext(40)
                .With(o => o.ProtectedBySystem = false)
                .With(o => o.Site.DiffusionId = 0)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations, true);

        var searchFilter = new SearchFilterInternalDto
        {
            ProtectionFilter = ProtectionFilterDto.BothPublicAndSensitive,
            Output = new OutputFilterExtendedDto
            {
                Fields = new[] { "diffusionStatus" }
            }
        };
        var apiClient = TestFixture.CreateApiClient();

        // Act
        var response = await apiClient.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(40, because: "40 are public and not diffused");
        result!.Records.Count(o => o.DiffusionStatus == DiffusionStatus.NotDiffused).Should().Be(40, because: "Diffused observations are not return when quering both public and sensitive index");
    }
}