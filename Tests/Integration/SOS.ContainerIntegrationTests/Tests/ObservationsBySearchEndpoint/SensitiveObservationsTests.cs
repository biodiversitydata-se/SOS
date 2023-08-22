using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos;
using SOS.Lib.Models.Shared;
using SOS.ContainerIntegrationTests.Setup;
using SOS.ContainerIntegrationTests.TestData.TestDataBuilder;
using SOS.TestHelpers.Helpers.Builders;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.ObservationsController.ObservationsBySearchEndpoint;

[Collection(TestCollection.Name)]
public class SensitiveObservationsTests : TestBase
{
    public SensitiveObservationsTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]        
    public async Task ObservationsBySearchEndpoint_ReturnsNoObservations_WhenSearchingForPublicObservationsButOnlyProtectedExists()
    {            
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations().HaveTaxonSensitivityCategory(3)
            .Build();

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/search?sensitiveObservations=false", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(0, because: "All observations should be added to the protected index");
    }

    //[Fact]
    //[Trait("Category", "AutomaticIntegrationTest")]
    //public async Task Search_sensitive_observations_without_permissions_returns_no_observations()
    //{            
    //    // Arrange            
    //    var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
    //        .All()
    //            .HaveValuesFromPredefinedObservations()
    //            .HaveTaxonSensitivityCategory(3)
    //        .Build();
    //    var authorityBuilder = new UserAuthorizationTestBuilder();
    //    var authority = authorityBuilder
    //        .WithAuthorityIdentity("Sighting")
    //        .WithMaxProtectionLevel(1)
    //        .Build();
    //    ProcessFixture.UseMockUserService(15, authority);
    //    ProcessFixture.UseMockUser(_fixture.ObservationsController, 15, "user@test.xx");

    //    await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
    //    var searchFilter = new SearchFilterDto
    //    {
    //        OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent
    //    };
        
    //    // Act
    //    var response = await ProcessFixture.ObservationsController.ObservationsBySearch(
    //        null,
    //        null,
    //        searchFilter,
    //        0,
    //        100,
    //        sensitiveObservations: true);
    //    var result = response.GetResult<PagedResultDto<Observation>>();
        
    //    // Assert
    //    result.Should().NotBeNull();
    //    result.TotalCount.Should().Be(0);
    //    ProcessFixture.RestoreUserService();
    //}

    //[Fact]
    //[Trait("Category", "AutomaticIntegrationTest")]
    //public async Task Search_sensitive_observations_with_permission_to_category3()
    //{
    //    //-----------------------------------------------------------------------------------------------------------
    //    // Arrange
    //    //-----------------------------------------------------------------------------------------------------------            
    //    var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
    //        .All()
    //            .HaveValuesFromPredefinedObservations()
    //        .TheFirst(60)
    //            .HaveTaxonSensitivityCategory(3)
    //        .TheNext(40)
    //            .HaveTaxonSensitivityCategory(4)
    //        .Build();

    //    var authorityBuilder = new UserAuthorizationTestBuilder();
    //    var authority = authorityBuilder
    //        .WithAuthorityIdentity("Sighting")
    //        .WithMaxProtectionLevel(3)                
    //        .Build();
    //    _fixture.UseMockUserService(15, authority);
    //    _fixture.UseMockUser(_fixture.ObservationsController, 15, "user@test.xx");

    //    await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
    //    var searchFilter = new SearchFilterDto
    //    {
    //        OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent
    //    };

    //    //-----------------------------------------------------------------------------------------------------------
    //    // Act
    //    //-----------------------------------------------------------------------------------------------------------
    //    var response = await _fixture.ObservationsController.ObservationsBySearch(
    //        null,
    //        null,
    //        searchFilter,
    //        0,
    //        100,
    //        sensitiveObservations: true);
    //    var result = response.GetResult<PagedResultDto<Observation>>();

    //    //-----------------------------------------------------------------------------------------------------------
    //    // Assert
    //    //-----------------------------------------------------------------------------------------------------------            
    //    result.Should().NotBeNull();
    //    result.TotalCount.Should().Be(60);
    //    _fixture.RestoreUserService();
    //}

    //[Fact]
    //[Trait("Category", "AutomaticIntegrationTest")]
    //public async Task Search_sensitive_observations_without_permissions_returns_my_observed_observations()
    //{
    //    //-----------------------------------------------------------------------------------------------------------
    //    // Arrange
    //    //-----------------------------------------------------------------------------------------------------------            
    //    const int userId = 15;
    //    var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
    //        .All()
    //            .HaveValuesFromPredefinedObservations()
    //            .HaveTaxonSensitivityCategory(3)
    //        .TheFirst(60)
    //            .With(m => m.ObserversInternal = new[] { new UserInternal() { Id = userId, PersonId = userId, UserServiceUserId = userId, ViewAccess = true } })
    //        .Build();

    //    var authorityBuilder = new UserAuthorizationTestBuilder();
    //    var authority = authorityBuilder
    //        .WithAuthorityIdentity("Sighting")
    //        .WithMaxProtectionLevel(1)
    //        .Build();
    //    _fixture.UseMockUserService(userId, authority);
    //    _fixture.UseMockUser(_fixture.ObservationsController, userId, "user@test.xx");

    //    await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
    //    var searchFilter = new SearchFilterDto
    //    {
    //        ObservedByMe = true
    //    };

    //    //-----------------------------------------------------------------------------------------------------------
    //    // Act
    //    //-----------------------------------------------------------------------------------------------------------
    //    var response = await _fixture.ObservationsController.ObservationsBySearch(
    //        null,
    //        null,
    //        searchFilter,
    //        0,
    //        100,
    //        sensitiveObservations: true);
    //    var result = response.GetResult<PagedResultDto<Observation>>();

    //    //-----------------------------------------------------------------------------------------------------------
    //    // Assert
    //    //-----------------------------------------------------------------------------------------------------------            
    //    result.Should().NotBeNull();
    //    result.TotalCount.Should().Be(60);
    //    _fixture.RestoreUserService();
    //}

    //[Fact]
    //[Trait("Category", "AutomaticIntegrationTest")]
    //public async Task Search_sensitive_observations_without_permissions_returns_my_reported_observations()
    //{
    //    //-----------------------------------------------------------------------------------------------------------
    //    // Arrange
    //    //-----------------------------------------------------------------------------------------------------------            
    //    const int userId = 15;
    //    var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
    //        .All()
    //            .HaveValuesFromPredefinedObservations()
    //            .HaveTaxonSensitivityCategory(3)
    //        .TheFirst(60)
    //            .With(m => m.ReportedByUserServiceUserId = userId)
    //        .Build();

    //    var authorityBuilder = new UserAuthorizationTestBuilder();
    //    var authority = authorityBuilder
    //        .WithAuthorityIdentity("Sighting")
    //        .WithMaxProtectionLevel(1)
    //        .Build();
    //    _fixture.UseMockUserService(userId, authority);
    //    _fixture.UseMockUser(_fixture.ObservationsController, userId, "user@test.xx");

    //    await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
    //    var searchFilter = new SearchFilterDto
    //    {
    //        ReportedByMe = true
    //    };

    //    //-----------------------------------------------------------------------------------------------------------
    //    // Act
    //    //-----------------------------------------------------------------------------------------------------------
    //    var response = await _fixture.ObservationsController.ObservationsBySearch(
    //        null,
    //        null,
    //        searchFilter,
    //        0,
    //        100,
    //        sensitiveObservations: true);
    //    var result = response.GetResult<PagedResultDto<Observation>>();

    //    //-----------------------------------------------------------------------------------------------------------
    //    // Assert
    //    //-----------------------------------------------------------------------------------------------------------            
    //    result.Should().NotBeNull();
    //    result.TotalCount.Should().Be(60);
    //    _fixture.RestoreUserService();
    //}
}