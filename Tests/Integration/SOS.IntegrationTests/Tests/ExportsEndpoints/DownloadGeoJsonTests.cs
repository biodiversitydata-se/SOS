using FizzWare.NBuilder;
using SOS.IntegrationTests.Helpers;
using SOS.IntegrationTests.Setup;
using SOS.IntegrationTests.TestData.TestDataBuilder;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Filter;

namespace SOS.IntegrationTests.Tests.ExportsEndpoints;

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
}