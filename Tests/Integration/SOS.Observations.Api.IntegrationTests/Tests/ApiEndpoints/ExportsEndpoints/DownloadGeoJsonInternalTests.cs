using SOS.Api.Contracts.Observations.Requests;
using FizzWare.NBuilder;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Helpers;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.Setup.Stubs;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ExportsEndpoints;

[Collection(TestCollection.Name)]
public class DownloadGeoJsonInternalTests : TestBase
{
    public DownloadGeoJsonInternalTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task DownloadGeoJsonFileEndpoint_ReturnsExpectedRows_WhenNoFilterWasUsed()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(3)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { Output = new OutputFilterDto { FieldSet = SOS.Api.Contracts.Observations.Requests.OutputFieldSet.Minimum } };

        // Act
        var response = await apiClient.PostAsync($"/exports/internal/download/geojson?gzip=false", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        byte[] contentBytes = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fileEntries = GeoJsonHelper.ReadGeoJsonFile(contentBytes);
        fileEntries.Count.Should().Be(3, because: "3 observations were added and no filter was used.");
    }

    [Fact]
    public async Task DownloadGeoJsonFileEndpoint_ReturnsHierarchicalJsonProperties_WhenFlatIsFalse()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(3)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { Output = new OutputFilterDto { FieldSet = SOS.Api.Contracts.Observations.Requests.OutputFieldSet.Minimum } };

        // Act
        var response = await apiClient.PostAsync($"/exports/internal/download/geojson?gzip=false&flat=false&excludeNullValues=true", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        byte[] contentBytes = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fileEntries = GeoJsonHelper.ReadGeoJsonFile(contentBytes);
        fileEntries.Count.Should().Be(3, because: "3 observations were added and no filter was used.");
    }

    [Fact]
    public async Task DownloadGeoJsonFileEndpoint_ReturnsExpectedObservations_WhenSearchingForSensitiveObservations_GivenTheUserHasAccessRights()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(3).HaveTaxonSensitivityCategory(3)
             .TheNext(2).HaveTaxonSensitivityCategory(4)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var userServiceStub = UserServiceStubFactory.CreateWithSightingAuthority(maxProtectionLevel: 3);
        var apiClient = TestFixture.CreateApiClientWithReplacedService(userServiceStub);
        var searchFilter = new SearchFilterInternalDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent };

        // Act
        var response = await apiClient.PostAsync($"/exports/internal/download/geojson?gzip=false&sensitiveObservations=true", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        byte[] contentBytes = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fileEntries = GeoJsonHelper.ReadGeoJsonFile(contentBytes);
        fileEntries.Count.Should().Be(3, because: "3 observations added to Elasticsearch have sensitivty category 3 and the user has max access rights to category 3.");
    }

    [Fact]
    public async Task DownloadGeoJsonFileEndpoint_ReturnsExpectedObservations_WhenSearchingForBothPublicAndSensitiveObservations_GivenTheUserHasAccessRights()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
                .TheFirst(3).HaveTaxonSensitivityCategory(3)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var userServiceStub = UserServiceStubFactory.CreateWithSightingAuthority(maxProtectionLevel: 6);
        var apiClient = TestFixture.CreateApiClientWithReplacedService(userServiceStub);
        var searchFilter = new SearchFilterInternalDto
        { 
            ProtectionFilter = ProtectionFilterDto.BothPublicAndSensitive,    
            OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent
        };

        // Act
        var response = await apiClient.PostAsync($"/exports/internal/download/geojson?gzip=false&sensitiveObservations=true", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        byte[] contentBytes = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fileEntries = GeoJsonHelper.ReadGeoJsonFile(contentBytes);
        fileEntries.Count.Should().Be(5, because: "5 observations were added and no filter was used.");
    }

}
