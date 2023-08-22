using SOS.ContainerIntegrationTests.Helpers;
using SOS.ContainerIntegrationTests.Setup;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Dtos.Filter;

namespace SOS.ContainerIntegrationTests.Tests.Exports;

/// <summary>
/// Integration tests for exporting to DwC-A file.
/// </summary>
[Collection(TestCollection.Name)]
public class DarwinCoreArchiveTests : TestBase
{
    public DarwinCoreArchiveTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }


    [Fact]
    public async Task ImportDwcaFile_ShouldHaveExpectedRecords_WhenImportingDwcaContainingSingleDataset()
    {
        // Arrange                 
        var dataProvider = new DataProvider { Id = 105, Identifier = "TestDataStewardshipBats", Type = DataProviderType.DwcA };
        await ProcessFixture.ImportDwcaFileUsingDwcArchiveReaderAsync(@"Resources/Dwca/dwca-datastewardship-single-dataset.zip", dataProvider, Output);
        //await ProcessFixture.ImportDwcaFileAsync(@"Resources/Dwca/dwca-datastewardship-single-dataset.zip", dataProvider, Output);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto();

        // Act
        var response = await apiClient.PostAsync($"/exports/download/dwc?eventbased=true", JsonContent.Create(searchFilter));
        using var contentStream = await response.Content.ReadAsStreamAsync();
        var filePath = Path.GetTempFileName();
        using (var fileStream = File.Create(filePath)) {
            await contentStream.CopyToAsync(fileStream);
        }
        var parsedDwcaFile = await DwcaHelper.ReadDwcaFileAsync(filePath, dataProvider);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        parsedDwcaFile.Occurrences.Count().Should().Be(15, because: "the DwC-A file contains 15 occurrences");
        parsedDwcaFile.Events.Count().Should().Be(7, because: "the DwC-A file contains 7 events");        
    }
}