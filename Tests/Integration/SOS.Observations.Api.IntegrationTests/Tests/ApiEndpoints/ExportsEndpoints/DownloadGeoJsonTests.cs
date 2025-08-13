using FizzWare.NBuilder;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Helpers;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ExportsEndpoints;

[Collection(TestCollection.Name)]
public class DownloadGeoJsonTests : TestBase
{
    public DownloadGeoJsonTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
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
        var response = await apiClient.PostAsync($"/exports/download/geojson?gzip=false", JsonContent.Create(searchFilter));
        byte[] contentBytes = await response.Content.ReadAsByteArrayAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fileEntries = GeoJsonHelper.ReadGeoJsonFile(contentBytes);
        fileEntries.Count.Should().Be(100, because: "100 observations were added and no filter was used.");
    }


    [Fact]
    public async Task DownloadGeoJsonFileEndpoint_ReturnsExpectedRows_WhenNoFilterWasUsedAndManyObservationsWasReturned()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(11000)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { Output = new OutputFilterDto { FieldSet = OutputFieldSet.Minimum } };

        // Act
        var response = await apiClient.PostAsync($"/exports/download/geojson?gzip=false", JsonContent.Create(searchFilter));
        byte[] contentBytes = await response.Content.ReadAsByteArrayAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fileEntries = GeoJsonHelper.ReadGeoJsonFile(contentBytes);
        fileEntries.Count.Should().Be(11000, because: "11000 observations were added and no filter was used.");
    }
}