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
public class GeneralizationFilterTests : TestBase
{
    public GeneralizationFilterTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    private IList<ArtportalenObservationVerbatim> CreateTestData(int userId)
    {
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(11) // Observations that will be diffused in public index, and real coordinates in sensitive index.
                .IsDiffused(1000)
                .With(o => o.ReportedByUserServiceUserId = userId)
                .With(o => o.ProtectedBySystem = false)
            .TheNext(14) // Diffused observations that will only be in sensitive index
                .IsDiffused(1000)
                .With(o => o.ReportedByUserServiceUserId = userId)
                .With(o => o.ProtectedBySystem = true)
            .TheNext(18) // Observations that will be diffused in public index, and real coordinates in sensitive index. The user doesn't have access to sensitive obs.
                .IsDiffused(1000)
                .With(o => o.ReportedByUserServiceUserId = userId + 1)
                .With(o => o.ProtectedBySystem = false)
            .TheNext(7) // Diffused observations that will only be in sensitive index. The user doesn't have access to sensitive obs.
                .IsDiffused(1000)
                .With(o => o.ReportedByUserServiceUserId = userId + 1)
                .With(o => o.ProtectedBySystem = true)
            .TheNext(20) // Sensitive observations with user access
                .HaveTaxonSensitivityCategory(3)
                .With(o => o.ReportedByUserServiceUserId = userId)
            .TheNext(9) // Sensitive observations without user access
                .HaveTaxonSensitivityCategory(3)
                .With(o => o.ReportedByUserServiceUserId = userId + 1)
            .TheNext(21) // Public observations with no diffusion
                .With(o => o.ProtectedBySystem = false)
                .With(o => o.Site.DiffusionId = 0)
            .Build();

        return verbatimObservations;
    }

    [Fact]
    public async Task VerifyProcessingOfTestData()
    {
        // Arrange
        const int userId = TestAuthHandler.DefaultTestUserId;
        var verbatimObservations = CreateTestData(userId);

        // Act
        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearchUsingObservationProcessor(verbatimObservations);        

        // Assert - number of public and sensitive observations
        int expectedNumberOfPublicObserations = 11 + 18 + 21;
        int expectedNumberOfSensitiveObservations = 11 + 14 + 18 + 7 + 20 + 9;
        int numberOfPublicObservations = processedObservations.Count(m => !m.Sensitive);
        int numberOfSensitiveObservations = processedObservations.Count(m => m.Sensitive);
        numberOfPublicObservations.Should().Be(expectedNumberOfPublicObserations);
        numberOfSensitiveObservations.Should().Be(expectedNumberOfSensitiveObservations);

        // Assert - number of generalized observations.
        int expectedNumberOfGeneralizedObservations = 11 + 18;
        int numberOfGeneralizedObservations = processedObservations.Count(m => m.IsGeneralized);
        int numberOfPublicGeneralizedObservations = processedObservations.Count(m => m.IsGeneralized && !m.Sensitive);
        numberOfGeneralizedObservations.Should().Be(expectedNumberOfGeneralizedObservations);
        numberOfPublicGeneralizedObservations.Should().Be(expectedNumberOfGeneralizedObservations);

        // Assert - number of sensitve observations with generalized observation in other index.
        int expectedNumberOfObservationWithGeneralizedObsInOtherIndex = 11 + 18;
        int numberOfObservationWithGeneralizedObsInOtherIndex = processedObservations.Count(m => m.HasGeneralizedObservationInOtherIndex);
        int numberOfSensitiveObservationWithGeneralizedObsInOtherIndex = processedObservations.Count(m => m.HasGeneralizedObservationInOtherIndex && m.Sensitive);
        numberOfObservationWithGeneralizedObsInOtherIndex.Should().Be(expectedNumberOfObservationWithGeneralizedObsInOtherIndex);
        numberOfSensitiveObservationWithGeneralizedObsInOtherIndex.Should().Be(expectedNumberOfObservationWithGeneralizedObsInOtherIndex);
    }


    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenSearchingSensitiveObservationsWithUserAccess()
    {
        // Arrange
        const int userId = TestAuthHandler.DefaultTestUserId;
        var verbatimObservations = CreateTestData(userId);
        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearchUsingObservationProcessor(verbatimObservations);        
        var apiClientWithAccessToUser = TestFixture.CreateApiClientWithReplacedService(
            UserServiceStubFactory.CreateWithSightingAuthority(userId: userId, maxProtectionLevel: 1));
        
        // Get my sensitive observations, but not generalized observations
        var searchFilter = new SearchFilterInternalDto
        {
            ProtectionFilter = ProtectionFilterDto.Sensitive,
            Output = new OutputFilterExtendedDto { FieldSet = Lib.Enums.OutputFieldSet.AllWithValues }
        };
        var response = await apiClientWithAccessToUser.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();        
        result!.TotalCount.Should().Be(14 + 20);
        var generalizedObservationsCount = result.Records.Count(m => m.IsGeneralized);
        generalizedObservationsCount.Should().Be(0);

        // Get my sensitive observations and include generalized observations
        searchFilter = new SearchFilterInternalDto
        {
            ProtectionFilter = ProtectionFilterDto.Sensitive,
            Output = new OutputFilterExtendedDto { FieldSet = Lib.Enums.OutputFieldSet.AllWithValues },
            GeneralizationFilter = new GeneralizationFilterDto
            {
                SensitiveGeneralizationFilter = SensitiveGeneralizationFilterDto.IncludeGeneralizedObservations
            }            
        };
        response = await apiClientWithAccessToUser.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
        result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        result!.TotalCount.Should().Be(11 + 14 + 20);
        generalizedObservationsCount = result.Records.Count(m => m.IsGeneralized);
        generalizedObservationsCount.Should().Be(0);

        // Get only my generalized sensitive observations
        searchFilter = new SearchFilterInternalDto
        {
            ProtectionFilter = ProtectionFilterDto.Sensitive,
            Output = new OutputFilterExtendedDto { FieldSet = Lib.Enums.OutputFieldSet.AllWithValues },
            GeneralizationFilter = new GeneralizationFilterDto
            {
                SensitiveGeneralizationFilter = SensitiveGeneralizationFilterDto.OnlyGeneralizedObservations
            }            
        };
        response = await apiClientWithAccessToUser.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
        result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        result!.TotalCount.Should().Be(11);
        generalizedObservationsCount = result.Records.Count(m => m.IsGeneralized);
        generalizedObservationsCount.Should().Be(0);


        var apiClientWithAccessToLevel3 = TestFixture.CreateApiClientWithReplacedService(
            UserServiceStubFactory.CreateWithSightingAuthority(maxProtectionLevel: 3));

        // Get sensitive observations and without generalized observations
        searchFilter = new SearchFilterInternalDto
        {
            ProtectionFilter = ProtectionFilterDto.Sensitive,
            Output = new OutputFilterExtendedDto { FieldSet = Lib.Enums.OutputFieldSet.AllWithValues },
            GeneralizationFilter = new GeneralizationFilterDto
            {
                SensitiveGeneralizationFilter = SensitiveGeneralizationFilterDto.DontIncludeGeneralizedObservations
            }
        };
        response = await apiClientWithAccessToLevel3.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
        result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        result!.TotalCount.Should().Be(14 + 7 + 20 + 9);
        generalizedObservationsCount = result.Records.Count(m => m.IsGeneralized);
        generalizedObservationsCount.Should().Be(0);

        // Get sensitive observations and include generalized observations
        searchFilter = new SearchFilterInternalDto
        {
            ProtectionFilter = ProtectionFilterDto.Sensitive,
            Output = new OutputFilterExtendedDto { FieldSet = Lib.Enums.OutputFieldSet.AllWithValues },
            GeneralizationFilter = new GeneralizationFilterDto
            {
                SensitiveGeneralizationFilter = SensitiveGeneralizationFilterDto.IncludeGeneralizedObservations
            }
        };
        response = await apiClientWithAccessToLevel3.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
        result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        result!.TotalCount.Should().Be(11 + 14 + 18 + 7 + 20 + 9);
        generalizedObservationsCount = result.Records.Count(m => m.IsGeneralized);
        generalizedObservationsCount.Should().Be(0);        
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenSearchingSensitiveObservationsWithAccessToProtectionLevel3()
    {
        // Arrange
        const int userId = TestAuthHandler.DefaultTestUserId;
        var verbatimObservations = CreateTestData(userId);
        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearchUsingObservationProcessor(verbatimObservations);        
        var apiClientWithAccessToLevel3 = TestFixture.CreateApiClientWithReplacedService(
            UserServiceStubFactory.CreateWithSightingAuthority(maxProtectionLevel: 3));

        // Get sensitive observations and without generalized observations
        var searchFilter = new SearchFilterInternalDto
        {
            ProtectionFilter = ProtectionFilterDto.Sensitive,
            Output = new OutputFilterExtendedDto { FieldSet = Lib.Enums.OutputFieldSet.AllWithValues },
            GeneralizationFilter = new GeneralizationFilterDto
            {
                SensitiveGeneralizationFilter = SensitiveGeneralizationFilterDto.DontIncludeGeneralizedObservations
            }
        };
        var response = await apiClientWithAccessToLevel3.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        result!.TotalCount.Should().Be(14 + 7 + 20 + 9);
        var generalizedObservationsCount = result.Records.Count(m => m.IsGeneralized);
        generalizedObservationsCount.Should().Be(0);

        // Get sensitive observations and include generalized observations
        searchFilter = new SearchFilterInternalDto
        {
            ProtectionFilter = ProtectionFilterDto.Sensitive,
            Output = new OutputFilterExtendedDto { FieldSet = Lib.Enums.OutputFieldSet.AllWithValues },
            GeneralizationFilter = new GeneralizationFilterDto
            {
                SensitiveGeneralizationFilter = SensitiveGeneralizationFilterDto.IncludeGeneralizedObservations
            }
        };
        response = await apiClientWithAccessToLevel3.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
        result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        result!.TotalCount.Should().Be(11 + 14 + 18 + 7 + 20 + 9);
        generalizedObservationsCount = result.Records.Count(m => m.IsGeneralized);
        generalizedObservationsCount.Should().Be(0);
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenSearchingPublicObservations()
    {
        // Arrange
        const int userId = TestAuthHandler.DefaultTestUserId;
        var verbatimObservations = CreateTestData(userId);
        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearchUsingObservationProcessor(verbatimObservations);
        var apiClientWithAccessToUser = TestFixture.CreateApiClientWithReplacedService(
            UserServiceStubFactory.CreateWithSightingAuthority(userId: userId, maxProtectionLevel: 1));

        // Get only public observations
        var searchFilter = new SearchFilterInternalDto
        {
            ProtectionFilter = ProtectionFilterDto.Public,
            Output = new OutputFilterExtendedDto { FieldSet = Lib.Enums.OutputFieldSet.AllWithValues }
        };
        var response = await apiClientWithAccessToUser.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        result!.TotalCount.Should().Be(11 + 18 + 21);
        int generalizedObservationsCount = result.Records.Count(m => m.IsGeneralized);
        generalizedObservationsCount.Should().Be(11 + 18);

        // Get only public observations - try use filter (but the filter will only be used when searching only sensitive observations)
        searchFilter = new SearchFilterInternalDto
        {
            ProtectionFilter = ProtectionFilterDto.Public,
            Output = new OutputFilterExtendedDto { FieldSet = Lib.Enums.OutputFieldSet.AllWithValues },
            GeneralizationFilter = new GeneralizationFilterDto
            {
                SensitiveGeneralizationFilter = SensitiveGeneralizationFilterDto.OnlyGeneralizedObservations
            }
        };
        response = await apiClientWithAccessToUser.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
        result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        result!.TotalCount.Should().Be(11 + 18 + 21, because: "The generalization filter is only applied when searching sensitive observations");
        generalizedObservationsCount = result.Records.Count(m => m.IsGeneralized);
        generalizedObservationsCount.Should().Be(11 + 18);
        //result!.TotalCount.Should().Be(0); // todo - ska det vara så eller ska filtret bara appliceras om man söker på skyddade fynd?
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenSearchingBothPublicAndSensitiveObservations()
    {
        // Arrange
        const int userId = TestAuthHandler.DefaultTestUserId;
        var verbatimObservations = CreateTestData(userId);
        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearchUsingObservationProcessor(verbatimObservations);
        var apiClientWithAccessToUser = TestFixture.CreateApiClientWithReplacedService(
            UserServiceStubFactory.CreateWithSightingAuthority(userId: userId, maxProtectionLevel: 1));
        
        // Search both public and sensitive observations
        var searchFilter = new SearchFilterInternalDto
        {
            ProtectionFilter = ProtectionFilterDto.BothPublicAndSensitive,
            Output = new OutputFilterExtendedDto { FieldSet = Lib.Enums.OutputFieldSet.AllWithValues }
        };
        var response = await apiClientWithAccessToUser.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        result!.TotalCount.Should().Be(11 + 18 + 14 + 20 + 21, because: "Observations that is in both public and sensitive index, will only return observations from public index.");
        var generalizedObservationsCount = result.Records.Count(m => m.IsGeneralized);
        generalizedObservationsCount.Should().Be(11 + 18);
    }

    [Fact]
    public async Task Compare_generalized_observation_with_real_observation()
    {
        // Arrange
        const int userId = TestAuthHandler.DefaultTestUserId;
        var verbatimObservations = CreateTestData(userId);
        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearchUsingObservationProcessor(verbatimObservations);
        var apiClientWithAccessToUser = TestFixture.CreateApiClientWithReplacedService(
            UserServiceStubFactory.CreateWithSightingAuthority(userId: userId, maxProtectionLevel: 1));

        // Act, Assert

        // Compare generalized observation with real observation - get real observation first

        // 1. Get real observation
        var searchFilter = new SearchFilterInternalDto
        {
            ProtectionFilter = ProtectionFilterDto.Sensitive,
            Output = new OutputFilterExtendedDto { FieldSet = Lib.Enums.OutputFieldSet.AllWithValues },
            GeneralizationFilter = new GeneralizationFilterDto
            {
                SensitiveGeneralizationFilter = SensitiveGeneralizationFilterDto.OnlyGeneralizedObservations
            }
        };
        var response = await apiClientWithAccessToUser.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        var realObservation = result.Records.First();

        // 2. Get generalized observation
        searchFilter = new SearchFilterInternalDto
        {
            ProtectionFilter = ProtectionFilterDto.Public,
            Output = new OutputFilterExtendedDto { FieldSet = Lib.Enums.OutputFieldSet.AllWithValues }
        };
        response = await apiClientWithAccessToUser.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
        result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        var generalizedObservation = result.Records.First(m => m.Occurrence.OccurrenceId == realObservation.Occurrence.OccurrenceId);

        realObservation.Location.DecimalLongitude.Should().NotBe(generalizedObservation.Location.DecimalLongitude);
        realObservation.Location.CoordinateUncertaintyInMeters.Should().BeLessThan(generalizedObservation.Location.CoordinateUncertaintyInMeters!.Value);


        // Compare generalized observation with real observation - get generalized observation first

        // 1. Get generalized observation
        searchFilter = new SearchFilterInternalDto
        {
            ProtectionFilter = ProtectionFilterDto.Public,
            ReportedByMe = true,
            Output = new OutputFilterExtendedDto { FieldSet = Lib.Enums.OutputFieldSet.AllWithValues },
            GeneralizationFilter = new GeneralizationFilterDto
            {
                PublicGeneralizationFilter = PublicGeneralizationFilterDto.OnlyGeneralized
            }
        };
        response = await apiClientWithAccessToUser.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
        result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        generalizedObservation = result.Records.First();

        // 2. Get real observation
        searchFilter = new SearchFilterInternalDto
        {
            ProtectionFilter = ProtectionFilterDto.Sensitive,
            Output = new OutputFilterExtendedDto { FieldSet = Lib.Enums.OutputFieldSet.AllWithValues },
            GeneralizationFilter = new GeneralizationFilterDto
            {
                SensitiveGeneralizationFilter = SensitiveGeneralizationFilterDto.IncludeGeneralizedObservations
            }
        };
        response = await apiClientWithAccessToUser.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
        result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        realObservation = result.Records.First(m => m.Occurrence.OccurrenceId == realObservation.Occurrence.OccurrenceId);

        realObservation.Location.DecimalLongitude.Should().NotBe(generalizedObservation.Location.DecimalLongitude);
        realObservation.Location.CoordinateUncertaintyInMeters.Should().BeLessThan(generalizedObservation.Location.CoordinateUncertaintyInMeters!.Value);
    }


    //[Fact]
    //public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByDiffusionPublic()
    //{
    //    const int userId = TestAuthHandler.DefaultTestUserId;
    //    // Arrange
    //    var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
    //        .All().HaveValuesFromPredefinedObservations()
    //        .TheFirst(20)
    //            .IsDiffused(100)
    //        .TheNext(20)
    //            .IsDiffused(500)
    //        .TheNext(20)
    //            .IsDiffused(1000)
    //        .TheNext(40)
    //            .With(o => o.ProtectedBySystem = false)
    //            .With(o => o.Site.DiffusionId = 0)
    //        .Build();
    //    await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations, true);

    //    var searchFilter = new SearchFilterInternalDto { 
    //        ProtectionFilter = ProtectionFilterDto.Public,
    //        Output = new OutputFilterExtendedDto
    //        {
    //            Fields = new[] { "diffusionStatus" }
    //        }
    //    };
    //    var apiClient = TestFixture.CreateApiClient();

    //    // Act
    //    var response = await apiClient.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
    //    var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

    //    // Assert
    //    response.StatusCode.Should().Be(HttpStatusCode.OK);
    //    result!.TotalCount.Should().Be(100, because: "100 observations added to Elasticsearch are public");
    //    result!.Records.Count(o => o.DiffusionStatus != DiffusionStatus.NotDiffused).Should().Be(60, because: "60 observations added to Elasticsearch are diffused");
    //}

    //[Fact]
    //public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByDiffusionSensitive()
    //{
    //    const int userId = TestAuthHandler.DefaultTestUserId;
    //    // Arrange
    //    var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
    //        .All().HaveValuesFromPredefinedObservations()
    //        .TheFirst(20)
    //            .IsDiffused(100)
    //            .With(o => o.ReportedByUserServiceUserId = userId)
    //        .TheNext(20)
    //            .IsDiffused(500)
    //            .With(o => o.ReportedByUserServiceUserId = userId)
    //        .TheNext(20)
    //            .IsDiffused(1000)
    //            .With(o => o.ReportedByUserServiceUserId = userId)
    //        .TheNext(40)
    //            .With(o => o.ProtectedBySystem = false)
    //            .With(o => o.Site.DiffusionId = 0)
    //        .Build();
    //    await ProcessFixture.NewProcessAndAddObservationsToElasticSearch(verbatimObservations);

    //    //await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations, true);

    //    var searchFilter = new SearchFilterInternalDto
    //    {
    //        ProtectionFilter = ProtectionFilterDto.Sensitive,
    //        Output = new OutputFilterExtendedDto
    //        {
    //            Fields = new[] { "diffusionStatus" }
    //        }
    //    };
    //    var apiClient = TestFixture.CreateApiClient();

    //    // Act
    //    var response = await apiClient.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
    //    var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

    //    // Assert
    //    response.StatusCode.Should().Be(HttpStatusCode.OK);
    //    result!.TotalCount.Should().Be(60, because: "60 observations added to Elasticsearch are sensitive");
    //    result!.Records.Count(o => o.DiffusionStatus == DiffusionStatus.NotDiffused).Should().Be(60, because: "Sensitive observations added to Elasticsearch are not diffused");
    //}

    //[Fact]
    //public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByDiffusionPublicAndSensitive()
    //{
    //    const int userId = TestAuthHandler.DefaultTestUserId;
    //    // Arrange
    //    var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
    //        .All().HaveValuesFromPredefinedObservations()
    //        .TheFirst(20)
    //            .IsDiffused(100)
    //            .With(o => o.ReportedByUserServiceUserId = userId)
    //        .TheNext(20)
    //            .IsDiffused(500)
    //            .With(o => o.ReportedByUserServiceUserId = userId)
    //        .TheNext(20)
    //            .IsDiffused(1000)
    //            .With(o => o.ReportedByUserServiceUserId = userId)
    //        .TheNext(40)
    //            .With(o => o.ProtectedBySystem = false)
    //            .With(o => o.Site.DiffusionId = 0)
    //        .Build();
    //    await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations, true);

    //    var searchFilter = new SearchFilterInternalDto
    //    {
    //        ProtectionFilter = ProtectionFilterDto.BothPublicAndSensitive,
    //        Output = new OutputFilterExtendedDto
    //        {
    //            Fields = new[] { "diffusionStatus" }
    //        }
    //    };
    //    var apiClient = TestFixture.CreateApiClient();

    //    // Act
    //    var response = await apiClient.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
    //    var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

    //    // Assert
    //    response.StatusCode.Should().Be(HttpStatusCode.OK);
    //    result!.TotalCount.Should().Be(100, because: "60 observations added to Elasticsearch are sensitive and 40 are public");
    //    result!.Records.Count(o => o.DiffusionStatus == DiffusionStatus.NotDiffused).Should().Be(100, because: "Diffused observations are not return when quering both public and sensitive index");
    //}

    //[Fact]
    //public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByDiffusionPublicAndSensitiveNoAccess()
    //{
    //    const int userId = TestAuthHandler.DefaultTestUserId;
    //    // Arrange
    //    var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
    //        .All().HaveValuesFromPredefinedObservations()
    //        .TheFirst(20)
    //            .IsDiffused(100)
    //        .TheNext(20)
    //            .IsDiffused(500)
    //        .TheNext(20)
    //            .IsDiffused(1000)
    //        .TheNext(40)
    //            .With(o => o.ProtectedBySystem = false)
    //            .With(o => o.Site.DiffusionId = 0)
    //        .Build();
    //    await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations, true);

    //    var searchFilter = new SearchFilterInternalDto
    //    {
    //        ProtectionFilter = ProtectionFilterDto.BothPublicAndSensitive,
    //        Output = new OutputFilterExtendedDto
    //        {
    //            Fields = new[] { "diffusionStatus" }
    //        }
    //    };
    //    var apiClient = TestFixture.CreateApiClient();

    //    // Act
    //    var response = await apiClient.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
    //    var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

    //    // Assert
    //    response.StatusCode.Should().Be(HttpStatusCode.OK);
    //    result!.TotalCount.Should().Be(40, because: "40 are public and not diffused");
    //    result!.Records.Count(o => o.DiffusionStatus == DiffusionStatus.NotDiffused).Should().Be(40, because: "Diffused observations are not return when quering both public and sensitive index");
    //}

    //[Fact (Skip = "Suggested change to test above")]
    //public async Task ChangeSuggestion_ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByDiffusionPublicAndSensitiveNoAccess()
    //{
    //    const int userId = TestAuthHandler.DefaultTestUserId;
    //    // Arrange
    //    var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
    //        .All().HaveValuesFromPredefinedObservations()
    //        .TheFirst(20)
    //            .IsDiffused(100)
    //        .TheNext(20)
    //            .IsDiffused(500)
    //        .TheNext(20)
    //            .IsDiffused(1000)
    //        .TheNext(40)
    //            .With(o => o.ProtectedBySystem = false)
    //            .With(o => o.Site.DiffusionId = 0)
    //        .Build();
    //    await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations, true);

    //    var searchFilter = new SearchFilterInternalDto
    //    {
    //        ProtectionFilter = ProtectionFilterDto.BothPublicAndSensitive,
    //        Output = new OutputFilterExtendedDto
    //        {
    //            Fields = new[] { "diffusionStatus" }
    //        }
    //    };
    //    var apiClient = TestFixture.CreateApiClient();

    //    // Act
    //    var response = await apiClient.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
    //    var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

    //    // Assert
    //    response.StatusCode.Should().Be(HttpStatusCode.OK);
    //    result!.TotalCount.Should().Be(100);
    //    result.Records.Count(m => m.DiffusionStatus == DiffusionStatus.NotDiffused).Should().Be(40);
    //    result.Records.Count(m => m.DiffusionStatus == DiffusionStatus.DiffusedByProvider).Should().Be(60, because: "diffused observations should be prioritized");
    //}

    //[Fact]
    //public async Task ObservationsBySearchEndpoint_ReturnsCorrectDiffusion()
    //{
    //    const int userId = TestAuthHandler.DefaultTestUserId;
    //    // Arrange
    //    var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
    //        .All().HaveValuesFromPredefinedObservations()
    //        .TheFirst(20)
    //            .IsDiffused(100)
    //            .With(o => o.ReportedByUserServiceUserId = userId)
    //        .TheNext(20)
    //            .IsDiffused(500)
    //            .With(o => o.ReportedByUserServiceUserId = userId)
    //        .TheNext(20)
    //            .IsDiffused(1000)
    //            .With(o => o.ReportedByUserServiceUserId = userId)
    //        .TheNext(40)
    //            .With(o => o.ProtectedBySystem = false)
    //            .With(o => o.Site.DiffusionId = 0)
    //        .Build();
    //    await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations, true);

    //    var searchFilterSensitive = new SearchFilterInternalDto
    //    {
    //        ProtectionFilter = ProtectionFilterDto.Sensitive,
    //        Output = new OutputFilterExtendedDto
    //        {
    //            FieldSet = Lib.Enums.OutputFieldSet.AllWithValues
    //        }
    //    };
    //    var searchFilterPublic = new SearchFilterInternalDto
    //    {
    //        ProtectionFilter = ProtectionFilterDto.Public,
    //        Output = new OutputFilterExtendedDto
    //        {
    //            FieldSet = Lib.Enums.OutputFieldSet.AllWithValues
    //        }
    //    };
    //    var apiClient = TestFixture.CreateApiClient();

    //    // Act
    //    var responseSensitive = await apiClient.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilterSensitive));
    //    var resultSensitive = await responseSensitive.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
    //    var responsePublic = await apiClient.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilterPublic));
    //    var resultPublic = await responsePublic.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

    //    // Assert
    //    var sensitiveObs = resultSensitive.Records.First(m => m.Occurrence.OccurrenceId == "urn:lsid:artportalen.se:sighting:1");
    //    var publicObs = resultPublic.Records.First(m => m.Occurrence.OccurrenceId == "urn:lsid:artportalen.se:sighting:1");

    //    sensitiveObs.Location.DecimalLatitude.Should().NotBe(publicObs.Location.DecimalLatitude, because: "the observation is diffused in public index");
    //    sensitiveObs.DiffusionStatus.Should().Be(DiffusionStatus.NotDiffused);
    //    publicObs.DiffusionStatus.Should().Be(DiffusionStatus.DiffusedByProvider);
    //}
}