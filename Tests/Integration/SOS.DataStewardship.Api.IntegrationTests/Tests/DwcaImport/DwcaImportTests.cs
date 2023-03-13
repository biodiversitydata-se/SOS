using SOS.DataStewardship.Api.Contracts.Models;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

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
        var dataProvider = new DataProvider { Id = 105, Identifier = "TestDataStewardshipBats", Type = DataProviderType.DwcA };
        await ProcessFixture.ImportDwcaFileAsync(@"data\resources\dwca-datastewardship-single-dataset.zip", dataProvider, Output);

        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------

        // Get all datasets
        var datasetSearchFilter = new DatasetFilter {  };
        var datasetsBySearchPageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<Contracts.Models.Dataset>, DatasetFilter>(
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
        datasetsBySearchPageResult.Records.First().EventIds.Should().NotBeEmpty("because the dataset have events");

        eventsBySearchPageResult.TotalCount.Should().Be(7, "because the DwC-A file contains 7 events");
        eventsBySearchPageResult.Records.Should().AllSatisfy(m => m.Dataset.Identifier.Should().Be(exptectedDataset));
        eventsBySearchPageResult.Records.Should().AllSatisfy(m => m.OccurrenceIds.Should().NotBeEmpty(), "because the event have occurrences");

        occurrencesBySearchPageResult.TotalCount.Should().Be(15, "because the DwC-A file contains 15 occurrences");
        occurrencesBySearchPageResult.Records.Should().AllSatisfy(m => m.Dataset.Identifier.Should().Be(exptectedDataset));
    }

    [Fact]
    public async Task ImportDwcaFile_ShouldHaveExpectedRecords_WhenImportingDwcaContainingMultipleDatasets()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------        
        var dataProvider = new DataProvider { Id = 105, Identifier = "TestDataStewardshipBats", Type = DataProviderType.DwcA };
        await ProcessFixture.ImportDwcaFileAsync(@"data\resources\dwca-datastewardship-multiple-datasets.zip", dataProvider, Output);

        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------

        // Get all datasets
        var datasetSearchFilter = new DatasetFilter { };
        var datasetsBySearchPageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<Contracts.Models.Dataset>, DatasetFilter>(
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
        datasetsBySearchPageResult.Records.Should().AllSatisfy(m => m.EventIds.Should().NotBeEmpty(), "because the datasets have events");

        eventsBySearchPageResult.TotalCount.Should().Be(7, "because the DwC-A file contains 7 events");
        eventsBySearchPageResult.Records.Should().AllSatisfy(m => m.Dataset.Identifier.Should().BeOneOf(exptectedDatasets));
        eventsBySearchPageResult.Records.Should().AllSatisfy(m => m.OccurrenceIds.Should().NotBeEmpty(), "because the events have occurrences");

        occurrencesBySearchPageResult.TotalCount.Should().Be(15, "because the DwC-A file contains 15 occurrences");
        occurrencesBySearchPageResult.Records.Should().AllSatisfy(m => m.Dataset.Identifier.Should().BeOneOf(exptectedDatasets));
    }

    [Fact]
    public async Task ImportMultipleDwcaFiles_ShouldHaveExpectedRecords_WhenImportingDwcaContainingMultipleDatasets()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------        
        var files = new List<(string filePath, DataProvider dataProvider)>()
        {
            (@"data\resources\dwca-datastewardship-single-dataset.zip", new DataProvider { Id = 105, Identifier = "TestDataStewardshipBats", Type = DataProviderType.DwcA }),
            (@"data\resources\dwca-datastewardship-single-dataset-with-other-dataset-identifier.zip", new DataProvider { Id = 106, Identifier = "TestDataStewardshipBats (other name)", Type = DataProviderType.DwcA })
        };
        await ProcessFixture.ImportDwcaFilesAsync(files, Output);

        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------

        // Get all datasets
        var datasetSearchFilter = new DatasetFilter { };
        var datasetsBySearchPageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<Contracts.Models.Dataset>, DatasetFilter>(
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
            "Dataset Bats (Hallaröd)"
        };

        datasetsBySearchPageResult.TotalCount.Should().Be(2, "because each DwC-A file contains 1 datasets");
        datasetsBySearchPageResult.Records.Select(m => m.Identifier).Should().BeEquivalentTo(exptectedDatasets);
        datasetsBySearchPageResult.Records.Should().AllSatisfy(m => m.EventIds.Should().NotBeEmpty(), "because the datasets have events");

        eventsBySearchPageResult.TotalCount.Should().Be(14, "because each DwC-A file contains 7 events");
        eventsBySearchPageResult.Records.Should().AllSatisfy(m => m.Dataset.Identifier.Should().BeOneOf(exptectedDatasets));
        eventsBySearchPageResult.Records.Should().AllSatisfy(m => m.OccurrenceIds.Should().NotBeEmpty(), "because the events have occurrences");

        occurrencesBySearchPageResult.TotalCount.Should().Be(30, "because each DwC-A file contains 15 occurrences");
        occurrencesBySearchPageResult.Records.Should().AllSatisfy(m => m.Dataset.Identifier.Should().BeOneOf(exptectedDatasets));
    }

    [Fact]
    public async Task ImportDwcaFile_ShouldHaveExpectedRecords_WhenImportingDwcaContainingSingleDatasetWithTaxaList()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------        
        var dataProvider = new DataProvider { Id = 105, Identifier = "TestDataStewardshipBats", Type = DataProviderType.DwcA };
        await ProcessFixture.ImportDwcaFileAsync(@"data\resources\dwca-datastewardship-single-dataset-with-taxalist.zip", dataProvider, Output);

        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------

        // Get all datasets
        var datasetSearchFilter = new DatasetFilter { };
        var datasetsBySearchPageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<Contracts.Models.Dataset>, DatasetFilter>(
            $"datastewardship/datasets", datasetSearchFilter, jsonSerializerOptions);

        // Get all events
        var eventsSearchFilter = new EventsFilter { };
        var eventsBySearchPageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<EventModel>, EventsFilter>(
            $"datastewardship/events", eventsSearchFilter, jsonSerializerOptions);

        // Get all occurrences
        var occurrenceSearchFilter = new OccurrenceFilter { };
        var occurrencesBySearchPageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<OccurrenceModel>, OccurrenceFilter>(
            $"datastewardship/occurrences?skip=0&take=100", occurrenceSearchFilter, jsonSerializerOptions);

        //-----------------------------------------------------------------------------------------------------------
        // Assert
        //-----------------------------------------------------------------------------------------------------------
        string exptectedDataset = "ArtportalenDataHost - Dataset Bats (Hallaröd)";

        datasetsBySearchPageResult.TotalCount.Should().Be(1, "because the DwC-A file contains 1 datasets");
        datasetsBySearchPageResult.Records.First().Identifier.Should().Be(exptectedDataset);
        datasetsBySearchPageResult.Records.First().EventIds.Should().NotBeEmpty("because the dataset have events");

        eventsBySearchPageResult.TotalCount.Should().Be(7, "because the DwC-A file contains 7 events");
        eventsBySearchPageResult.Records.Should().AllSatisfy(m => m.Dataset.Identifier.Should().Be(exptectedDataset));
        eventsBySearchPageResult.Records.Should().AllSatisfy(m => m.OccurrenceIds.Should().NotBeEmpty(), "because the event have occurrences");

        occurrencesBySearchPageResult.TotalCount
            .Should().Be(40, "because the DwC-A file contains 40 occurrences, when absent occurrences is generated");

        occurrencesBySearchPageResult.Records
            .Where(m => m.OccurrenceStatus == Contracts.Enums.OccurrenceStatus.Observerad).Count()
            .Should().Be(15, "because the DwC-A file contains 15 occurrences with present status");

        occurrencesBySearchPageResult.Records
            .Where(m => m.OccurrenceStatus == Contracts.Enums.OccurrenceStatus.InteObserverad).Count()
            .Should().Be(25, "because the DwC-A file contains 25 occurrences with absent status");
    }
}