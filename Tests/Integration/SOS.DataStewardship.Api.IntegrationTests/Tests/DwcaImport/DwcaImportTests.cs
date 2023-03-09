using SOS.DataStewardship.Api.Contracts.Models;
using SOS.DataStewardship.Api.IntegrationTests.Core.Extensions;

namespace SOS.DataStewardship.Api.IntegrationTests.Tests.DwcaImport;

[Collection(Constants.IntegrationTestsCollectionName)]
public class DwcaImportTests : TestBase
{
    public DwcaImportTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task ImportDwcaFile_ShouldHaveExpectedRecords_WhenImportingDwcaContainingSingleDataset()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------        
        await ProcessFixture.ImportDwcaFileAsync(@"data\resources\dwca-datastewardship-single-dataset.zip", Output);

        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------

        // Get all datasets
        var datasetSearchFilter = new DatasetFilter {  };
        var datasetsBySearchPageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<Dataset>, DatasetFilter>(
            $"datastewardship/datasets", datasetSearchFilter, jsonSerializerOptions);

        // Get all events
        var eventsSearchFilter = new EventsFilter { };
        var eventsBySearchPageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<EventModel>, EventsFilter>(
            $"datastewardship/events", eventsSearchFilter, jsonSerializerOptions);

        // Get all occurrences
        var occurrenceSearchFilter = new OccurrenceFilter { };
        var occurrencesBySearchPageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<OccurrenceModel>, OccurrenceFilter>(
            $"datastewardship/occurrences", occurrenceSearchFilter, jsonSerializerOptions);

        //-----------------------------------------------------------------------------------------------------------
        // Assert
        //-----------------------------------------------------------------------------------------------------------
        string exptectedDataset = "ArtportalenDataHost - Dataset Bats (Hallaröd)";

        datasetsBySearchPageResult.TotalCount.Should().Be(1, "because the DwC-A file contains 1 datasets");
        datasetsBySearchPageResult.Records.First().Identifier.Should().Be(exptectedDataset);

        eventsBySearchPageResult.TotalCount.Should().Be(7, "because the DwC-A file contains 7 events");
        eventsBySearchPageResult.Records.Should().AllSatisfy(m => m.Dataset.Identifier.Should().Be(exptectedDataset));

        occurrencesBySearchPageResult.TotalCount.Should().Be(15, "because the DwC-A file contains 15 occurrences");
        occurrencesBySearchPageResult.Records.Should().AllSatisfy(m => m.Dataset.Identifier.Should().Be(exptectedDataset));
    }

    [Fact]
    public async Task ImportDwcaFile_ShouldHaveExpectedRecords_WhenImportingDwcaContainingMultipleDatasets()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------        
        await ProcessFixture.ImportDwcaFileAsync(@"data\resources\dwca-datastewardship-multiple-datasets.zip", Output);

        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------

        // Get all datasets
        var datasetSearchFilter = new DatasetFilter { };
        var datasetsBySearchPageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<Dataset>, DatasetFilter>(
            $"datastewardship/datasets", datasetSearchFilter, jsonSerializerOptions);

        // Get all events
        var eventsSearchFilter = new EventsFilter { };
        var eventsBySearchPageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<EventModel>, EventsFilter>(
            $"datastewardship/events", eventsSearchFilter, jsonSerializerOptions);

        // Get all occurrences
        var occurrenceSearchFilter = new OccurrenceFilter { };
        var occurrencesBySearchPageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<OccurrenceModel>, OccurrenceFilter>(
            $"datastewardship/occurrences", occurrenceSearchFilter, jsonSerializerOptions);

        //-----------------------------------------------------------------------------------------------------------
        // Assert
        //-----------------------------------------------------------------------------------------------------------
        string[] exptectedDatasets = new[] { 
            "ArtportalenDataHost - Dataset Bats (Hallaröd)", 
            "ArtportalenDataHost - Dataset Bats (Other)" 
        };
        
        datasetsBySearchPageResult.TotalCount.Should().Be(2, "because the DwC-A file contains 2 datasets");
        datasetsBySearchPageResult.Records.Select(m => m.Identifier).Should().BeEquivalentTo(exptectedDatasets);       

        eventsBySearchPageResult.TotalCount.Should().Be(7, "because the DwC-A file contains 7 events");
        eventsBySearchPageResult.Records.Should().AllSatisfy(m => m.Dataset.Identifier.Should().BeOneOf(exptectedDatasets));

        occurrencesBySearchPageResult.TotalCount.Should().Be(15, "because the DwC-A file contains 15 occurrences");
        occurrencesBySearchPageResult.Records.Should().AllSatisfy(m => m.Dataset.Identifier.Should().BeOneOf(exptectedDatasets));
    }
}