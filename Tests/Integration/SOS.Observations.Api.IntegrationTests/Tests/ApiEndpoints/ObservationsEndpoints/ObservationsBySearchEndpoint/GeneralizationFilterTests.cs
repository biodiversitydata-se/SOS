using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using SOS.Observations.Api.IntegrationTests.Setup.Stubs;
using SOS.Lib.Enums;
using NetTopologySuite.Geometries;
using SOS.Lib.Extensions;
using SOS.Observations.Api.IntegrationTests.Helpers;
using SOS.Harvest.Processors.Artportalen;

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
                .HaveTaxonSensitivityCategory(3)
                .With(o => o.ReportedByUserServiceUserId = userId)
                .With(o => o.ProtectedBySystem = true)
            .TheNext(18) // Observations that will be diffused in public index, and real coordinates in sensitive index. The user doesn't have access to sensitive obs.
                .IsDiffused(1000)
                .With(o => o.ReportedByUserServiceUserId = userId + 1)
                .With(o => o.ProtectedBySystem = false)
            .TheNext(7) // Diffused observations that will only be in sensitive index. The user doesn't have access to sensitive obs.
                .IsDiffused(1000)
                .HaveTaxonSensitivityCategory(3)
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
        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);        

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
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservationsWithCorrectCoordinates_WhenSearchingSensitiveObservationsWithUserAccess()
    {
        // Arrange
        const int userId = TestAuthHandler.DefaultTestUserId;
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1) // Observations that will be diffused in public index, and real coordinates in sensitive index.
                .IsDiffused(1000)
                .With(o => o.ReportedByUserServiceUserId = userId)
                .With(o => o.ProtectedBySystem = false)
            .TheNext(1) // Observations that will be diffused in public index, and real coordinates in sensitive index. The user doesn't have access to sensitive obs.
                .IsDiffused(1000)
                .With(o => o.ReportedByUserServiceUserId = userId + 1)
                .With(o => o.ProtectedBySystem = false)
            .TheNext(1) // Sensitive observations with user access
                .HaveTaxonSensitivityCategory(3)
                .With(o => o.ReportedByUserServiceUserId = userId)
            .TheNext(1) // Sensitive observations without user access
                .HaveTaxonSensitivityCategory(3)
                .With(o => o.ReportedByUserServiceUserId = userId + 1)
            .TheNext(1) // Public observations with no diffusion
                .With(o => o.ProtectedBySystem = false)
                .With(o => o.Site.DiffusionId = 0)
            .Build();

        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClientWithAccessToUser = TestFixture.CreateApiClientWithReplacedService(
            UserServiceStubFactory.CreateWithSightingAuthority(userId: userId, maxProtectionLevel: 1));

        // Act - Get public observations (with user access to one observation)
        var searchFilter = new SearchFilterInternalDto
        {
            ProtectionFilter = ProtectionFilterDto.BothPublicAndSensitive,
            Output = new OutputFilterExtendedDto { FieldSet = OutputFieldSet.AllWithValues }
        };
        var response = await apiClientWithAccessToUser.PostAsync($"/observations/internal/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        var diffusedObsWithAccess = (Verbatim: verbatimObservations.ElementAt(0), Processed: result.Records.First(m => m.Occurrence.OccurrenceId == $"urn:lsid:artportalen.se:sighting:{verbatimObservations.ElementAt(0).SightingId}"));
        var diffusedObsWithoutAccess = (Verbatim: verbatimObservations.ElementAt(1), Processed: result.Records.First(m => m.Occurrence.OccurrenceId == $"urn:lsid:artportalen.se:sighting:{verbatimObservations.ElementAt(1).SightingId}"));
        var publicObservation = (Verbatim: verbatimObservations.ElementAt(4), Processed: result.Records.First(m => m.Occurrence.OccurrenceId == $"urn:lsid:artportalen.se:sighting:{verbatimObservations.ElementAt(4).SightingId}"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.TotalCount.Should().Be(4);

        // Assert - diffused observation without user access
        diffusedObsWithoutAccess.Processed.IsGeneralized.Should().BeTrue();
        diffusedObsWithoutAccess.Processed.Location.CoordinateUncertaintyInMeters.Should().Be(ArtportalenObservationFactory.GetDiffusionCoordinateUncertainty(diffusedObsWithoutAccess.Verbatim.Site.DiffusionId), because: "the user has not access to the real coordinates");
        var point = new Point(diffusedObsWithoutAccess.Verbatim.Site.XCoordDiffused.Value, diffusedObsWithoutAccess.Verbatim.Site.YCoordDiffused.Value);
        var transformedPoint = point.Transform(CoordinateSys.WebMercator, CoordinateSys.WGS84);
        diffusedObsWithoutAccess.Processed.Location.DecimalLongitude.Should().BeApproximately(transformedPoint.X, 0.1);
        diffusedObsWithoutAccess.Processed.Location.DecimalLatitude.Should().BeApproximately(transformedPoint.Y, 0.1);

        // Assert - diffused observation with user access
        diffusedObsWithAccess.Processed.IsGeneralized.Should().BeFalse();
        diffusedObsWithAccess.Processed.Location.CoordinateUncertaintyInMeters.Should().Be(diffusedObsWithAccess.Verbatim.Site.Accuracy, because: "the user has access to the real coordinates");
        point = new Point(diffusedObsWithAccess.Verbatim.Site.XCoord, diffusedObsWithAccess.Verbatim.Site.YCoord);
        transformedPoint = point.Transform(CoordinateSys.WebMercator, CoordinateSys.WGS84);
        diffusedObsWithAccess.Processed.Location.DecimalLongitude.Should().BeApproximately(transformedPoint.X, 0.1);
        diffusedObsWithAccess.Processed.Location.DecimalLatitude.Should().BeApproximately(transformedPoint.Y, 0.1);

        // Assert - public observation
        publicObservation.Processed.IsGeneralized.Should().BeFalse();
        publicObservation.Processed.Location.CoordinateUncertaintyInMeters.Should().Be(publicObservation.Verbatim.Site.Accuracy);
    }

    [Fact]
    public async Task ObservationByIdEndpoint_ReturnsExpectedObservationsWithCorrectCoordinates_WhenFetchingSensitiveObservationWithUserAccess()
    {
        // Arrange
        const int userId = TestAuthHandler.DefaultTestUserId;
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1) // Observations that will be diffused in public index, and real coordinates in sensitive index.
                .IsDiffused(1000)
                .With(o => o.ReportedByUserServiceUserId = userId)
                .With(o => o.ProtectedBySystem = false)
            .TheNext(1) // Observations that will be diffused in public index, and real coordinates in sensitive index. The user doesn't have access to sensitive obs.
                .IsDiffused(1000)
                .With(o => o.ReportedByUserServiceUserId = userId + 1)
                .With(o => o.ProtectedBySystem = false)
            .TheNext(1) // Sensitive observations with user access
                .HaveTaxonSensitivityCategory(3)
                .With(o => o.ReportedByUserServiceUserId = userId)
            .TheNext(1) // Sensitive observations without user access
                .HaveTaxonSensitivityCategory(3)
                .With(o => o.ReportedByUserServiceUserId = userId + 1)
            .TheNext(1) // Public observations with no diffusion
                .With(o => o.ProtectedBySystem = false)
                .With(o => o.Site.DiffusionId = 0)
            .Build();

        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClientWithAccessToUser = TestFixture.CreateApiClientWithReplacedService(
            UserServiceStubFactory.CreateWithSightingAuthority(userId: userId, maxProtectionLevel: 1));

        // Act, Assert - Get public observations (with user access to one observation)
        var verbatimDiffusedObsWithAccess = verbatimObservations.ElementAt(0);
        var response = await apiClientWithAccessToUser.GetAsync($"/observations/internal?occurrenceId=urn:lsid:artportalen.se:sighting:{verbatimDiffusedObsWithAccess.SightingId}&resolveGeneralizedObservations=true");
        var diffusedObsWithAccess = await response.Content.ReadFromJsonAsync<Observation>();
        diffusedObsWithAccess!.IsGeneralized.Should().BeFalse();
        diffusedObsWithAccess.Location.CoordinateUncertaintyInMeters.Should().Be(verbatimDiffusedObsWithAccess.Site.Accuracy, because: "the user has access to the real coordinates");
        var point = new Point(verbatimDiffusedObsWithAccess.Site.XCoord, verbatimDiffusedObsWithAccess.Site.YCoord);
        var transformedPoint = point.Transform(CoordinateSys.WebMercator, CoordinateSys.WGS84);
        diffusedObsWithAccess.Location.DecimalLongitude.Should().BeApproximately(transformedPoint.X, 0.1);
        diffusedObsWithAccess.Location.DecimalLatitude.Should().BeApproximately(transformedPoint.Y, 0.1);

        // Act, Assert - Get public observations (with user access to one observation) - no resolve
        response = await apiClientWithAccessToUser.GetAsync($"/observations/internal?occurrenceId=urn:lsid:artportalen.se:sighting:{verbatimDiffusedObsWithAccess.SightingId}&resolveGeneralizedObservations=false");
        var diffusedObsWithAccessNoResolve = await response.Content.ReadFromJsonAsync<Observation>();
        diffusedObsWithAccessNoResolve!.IsGeneralized.Should().BeFalse();
        diffusedObsWithAccessNoResolve.Location.CoordinateUncertaintyInMeters.Should().Be(ArtportalenObservationFactory.GetDiffusionCoordinateUncertainty(verbatimDiffusedObsWithAccess.Site.DiffusionId), because: "the user has access to the real coordinates");
        point = new Point(verbatimDiffusedObsWithAccess.Site.XCoordDiffused!.Value, verbatimDiffusedObsWithAccess.Site.YCoordDiffused!.Value);
        transformedPoint = point.Transform(CoordinateSys.WebMercator, CoordinateSys.WGS84);
        diffusedObsWithAccessNoResolve.Location.DecimalLongitude.Should().BeApproximately(transformedPoint.X, 0.1);
        diffusedObsWithAccessNoResolve.Location.DecimalLatitude.Should().BeApproximately(transformedPoint.Y, 0.1);

        // Act, Assert - diffused observation without user access
        var verbatimDiffusedObsWithoutAccess = verbatimObservations.ElementAt(1);
        response = await apiClientWithAccessToUser.GetAsync($"/observations/internal?occurrenceId=urn:lsid:artportalen.se:sighting:{verbatimDiffusedObsWithoutAccess.SightingId}&resolveGeneralizedObservations=true");
        var diffusedObsWithoutAccess = await response.Content.ReadFromJsonAsync<Observation>();
        diffusedObsWithoutAccess!.IsGeneralized.Should().BeTrue();
        diffusedObsWithoutAccess.Location.CoordinateUncertaintyInMeters.Should().Be(ArtportalenObservationFactory.GetDiffusionCoordinateUncertainty(verbatimDiffusedObsWithoutAccess.Site.DiffusionId), because: "the user has not access to the real coordinates");
        point = new Point(verbatimDiffusedObsWithoutAccess.Site.XCoordDiffused!.Value, verbatimDiffusedObsWithoutAccess.Site.YCoordDiffused!.Value);
        transformedPoint = point.Transform(CoordinateSys.WebMercator, CoordinateSys.WGS84);
        diffusedObsWithoutAccess.Location.DecimalLongitude.Should().BeApproximately(transformedPoint.X, 0.1);
        diffusedObsWithoutAccess.Location.DecimalLatitude.Should().BeApproximately(transformedPoint.Y, 0.1);
    }

    [Fact]
    public async Task DownloadCsvFileEndpoint_ReturnsExpectedObservationsWithCorrectCoordinates_WhenSearchingSensitiveObservationsWithUserAccess()
    {
        // Arrange
        const int userId = TestAuthHandler.DefaultTestUserId;
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1) // Observations that will be diffused in public index, and real coordinates in sensitive index.
                .IsDiffused(1000)
                .With(o => o.ReportedByUserServiceUserId = userId)
                .With(o => o.ProtectedBySystem = false)
            .TheNext(1) // Observations that will be diffused in public index, and real coordinates in sensitive index. The user doesn't have access to sensitive obs.
                .IsDiffused(1000)
                .With(o => o.ReportedByUserServiceUserId = userId + 1)
                .With(o => o.ProtectedBySystem = false)
            .TheNext(1) // Sensitive observations with user access
                .HaveTaxonSensitivityCategory(3)
                .With(o => o.ReportedByUserServiceUserId = userId)
            .TheNext(1) // Sensitive observations without user access
                .HaveTaxonSensitivityCategory(3)
                .With(o => o.ReportedByUserServiceUserId = userId + 1)
            .TheNext(1) // Public observations with no diffusion
                .With(o => o.ProtectedBySystem = false)
                .With(o => o.Site.DiffusionId = 0)
            .Build();

        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClientWithAccessToUser = TestFixture.CreateApiClientWithReplacedService(
            UserServiceStubFactory.CreateWithSightingAuthority(userId: userId, maxProtectionLevel: 1));
        var searchFilter = new SearchFilterInternalDto
        {
            ProtectionFilter = ProtectionFilterDto.BothPublicAndSensitive,
            Output = new OutputFilterExtendedDto { FieldSet = OutputFieldSet.AllWithValues }
        };       

        // Act
        var response = await apiClientWithAccessToUser.PostAsync($"/exports/internal/download/csv?gzip=false", JsonContent.Create(searchFilter));
        byte[] contentBytes = await response.Content.ReadAsByteArrayAsync();

        // Assert
        var verbatimDiffusedObsWithAccess = verbatimObservations.ElementAt(0);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fileEntries = CsvHelper.ReadCsvFile(contentBytes);
        fileEntries.Count.Should().Be(4);
        var fileEntry = fileEntries.Single(m => m["OccurrenceId"] == $"urn:lsid:artportalen.se:sighting:{verbatimDiffusedObsWithAccess.SightingId}");
        fileEntry["CoordinateUncertaintyInMeters"].Should().Be(verbatimDiffusedObsWithAccess.Site.Accuracy.ToString());  
    }


    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenSearchingSensitiveObservationsWithUserAccess()
    {
        // Arrange
        const int userId = TestAuthHandler.DefaultTestUserId;
        var verbatimObservations = CreateTestData(userId);
        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);        
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
        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
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
        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
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
        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
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
        generalizedObservationsCount.Should().Be(18, because: "11 generalized observations will be resolved and 18 not");
    }

    [Fact]
    public async Task Compare_generalized_observation_with_real_observation()
    {
        // Arrange
        const int userId = TestAuthHandler.DefaultTestUserId;
        var verbatimObservations = CreateTestData(userId);
        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
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
}