using SOS.Api.Contracts.Observations.Requests;
using FizzWare.NBuilder;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Artportalen;
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
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(3)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { Output = new OutputFilterDto { FieldSet = SOS.Api.Contracts.Observations.Requests.OutputFieldSet.Minimum } };

        // Act
        var response = await apiClient.PostAsync($"/exports/download/geojson?gzip=false", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        byte[] contentBytes = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fileEntries = GeoJsonHelper.ReadGeoJsonFile(contentBytes);
        fileEntries.Count.Should().Be(3, because: "3 observations were added and no filter was used.");
    }


    [Fact (Skip = "Run on demand. Skipping test because of large number of observations")]
    public async Task DownloadGeoJsonFileEndpoint_ReturnsExpectedRows_WhenNoFilterWasUsedAndManyObservationsWasReturned()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(11000)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { Output = new OutputFilterDto { FieldSet = SOS.Api.Contracts.Observations.Requests.OutputFieldSet.Minimum } };

        // Act
        var response = await apiClient.PostAsync($"/exports/download/geojson?gzip=false", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        byte[] contentBytes = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fileEntries = GeoJsonHelper.ReadGeoJsonFile(contentBytes);
        fileEntries.Count.Should().Be(11000, because: "11000 observations were added and no filter was used.");
    }
}
