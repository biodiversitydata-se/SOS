using FizzWare.NBuilder;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
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
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { Output = new OutputFilterDto { FieldSet = OutputFieldSet.Minimum } };

        // Act
        var response = await apiClient.PostAsync($"/exports/internal/download/geojson?gzip=false", JsonContent.Create(searchFilter));
        byte[] contentBytes = await response.Content.ReadAsByteArrayAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fileEntries = GeoJsonHelper.ReadGeoJsonFile(contentBytes);
        fileEntries.Count.Should().Be(100, because: "100 observations were added and no filter was used.");
    }

    [Fact]
    public async Task DownloadGeoJsonFileEndpoint_ReturnsExpectedObservations_WhenSearchingForSensitiveObservations_GivenTheUserHasAccessRights()
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
        var searchFilter = new SearchFilterInternalDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent };

        // Act
        var response = await apiClient.PostAsync($"/exports/internal/download/geojson?gzip=false&sensitiveObservations=true", JsonContent.Create(searchFilter));
        byte[] contentBytes = await response.Content.ReadAsByteArrayAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fileEntries = GeoJsonHelper.ReadGeoJsonFile(contentBytes);
        fileEntries.Count.Should().Be(60, because: "60 observations added to Elasticsearch have sensitivty category 3 and the user has max access rights to category 3.");
    }

    [Fact]
    public async Task DownloadGeoJsonFileEndpoint_ReturnsExpectedObservations_WhenSearchingForBothPublicAndSensitiveObservations_GivenTheUserHasAccessRights()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
                .TheFirst(60).HaveTaxonSensitivityCategory(3)             
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var userServiceStub = UserServiceStubFactory.CreateWithSightingAuthority(maxProtectionLevel: 6);
        var apiClient = TestFixture.CreateApiClientWithReplacedService(userServiceStub);
        var searchFilter = new SearchFilterInternalDto
        { 
            ProtectionFilter = Dtos.Enum.ProtectionFilterDto.BothPublicAndSensitive,    
            OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent 
        };

        // Act
        var response = await apiClient.PostAsync($"/exports/internal/download/geojson?gzip=false&sensitiveObservations=true", JsonContent.Create(searchFilter));
        byte[] contentBytes = await response.Content.ReadAsByteArrayAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fileEntries = GeoJsonHelper.ReadGeoJsonFile(contentBytes);
        fileEntries.Count.Should().Be(100, because: "100 observations were added and no filter was used.");
    }

}