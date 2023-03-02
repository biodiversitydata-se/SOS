namespace SOS.DataStewardship.Api.IntegrationTests.Tests.DwcaImport;

[Collection(Constants.IntegrationTestsCollectionName)]
public class DwcaImportTests : TestBase
{
    public DwcaImportTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task Import_dwca_file_and_verify_observations()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------
        await ProcessFixture.ImportDwcaFileAsync(@"data\resources\dwca-datastewardship-bats-taxalists.zip", Output);

        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------

        // Get by id
        var observationById1 = await ApiClient.GetFromJsonAsync<OccurrenceModel>($"datastewardship/occurrences/test:bats:sighting:98571703", jsonSerializerOptions);
        var observationById2 = await ApiClient.GetFromJsonAsync<OccurrenceModel>($"datastewardship/occurrences/test:bats:sighting:98571253", jsonSerializerOptions);

        // Get by search - Observations with Dataset "Bats (Hallaröd)"
        var searchFilter = new OccurrenceFilter { DatasetIds = new List<string> { "ArtportalenDataHost - Dataset Bats (Hallaröd)" } };
        var pageResultHallarod = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<OccurrenceModel>, OccurrenceFilter>($"datastewardship/occurrences", searchFilter, jsonSerializerOptions);

        // Get by search - Observations with Dataset "Bats (Other)"
        searchFilter = new OccurrenceFilter { DatasetIds = new List<string> { "ArtportalenDataHost - Dataset Bats (Other)" } };
        var pageResultOther = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<OccurrenceModel>, OccurrenceFilter>($"datastewardship/occurrences", searchFilter, jsonSerializerOptions);

        //-----------------------------------------------------------------------------------------------------------
        // Assert
        //-----------------------------------------------------------------------------------------------------------
        observationById1.Should().NotBeNull();
        observationById2.Should().NotBeNull();

        pageResultHallarod.Records.Should().AllSatisfy(m =>
        {
            m.Dataset?.Identifier.Should().Be("ArtportalenDataHost - Dataset Bats (Hallaröd)");
        });

        pageResultOther.Records.Should().AllSatisfy(m =>
        {
            m.Dataset?.Identifier.Should().Be("ArtportalenDataHost - Dataset Bats (Other)");
        });
    }

    [Fact]
    public async Task Import_dwca_file_and_verify_events()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------        
        await ProcessFixture.ImportDwcaFileAsync(@"data\resources\dwca-datastewardship-bats-taxalists.zip", Output);

        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------

        // Get by id
        var eventById1 = await ApiClient.GetFromJsonAsync<EventModel>($"datastewardship/events/test:bats:event:12581041667877196608", jsonSerializerOptions);
        var eventById2 = await ApiClient.GetFromJsonAsync<EventModel>($"datastewardship/events/test:bats:event:14009236676399444594", jsonSerializerOptions);

        // Get by search - Events with Dataset "Bats (Hallaröd)"
        var searchFilter = new EventsFilter { DatasetIds = new List<string> { "ArtportalenDataHost - Dataset Bats (Hallaröd)" } };
        var pageResultHallarod = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<EventModel>, EventsFilter>($"datastewardship/events", searchFilter, jsonSerializerOptions);

        // Get by search - Events with Dataset "Bats (Other)"
        searchFilter = new EventsFilter { DatasetIds = new List<string> { "ArtportalenDataHost - Dataset Bats (Other)" } };
        var pageResultOther = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<EventModel>, EventsFilter>($"datastewardship/events", searchFilter, jsonSerializerOptions);

        //-----------------------------------------------------------------------------------------------------------
        // Assert
        //-----------------------------------------------------------------------------------------------------------
        eventById1.Should().NotBeNull();
        eventById2.Should().NotBeNull();

        pageResultHallarod.Records.Should().AllSatisfy(m =>
        {
            m.Dataset.Identifier.Should().Be("ArtportalenDataHost - Dataset Bats (Hallaröd)");
        });

        pageResultOther.Records.Should().AllSatisfy(m =>
        {
            m.Dataset.Identifier.Should().Be("ArtportalenDataHost - Dataset Bats (Other)");
        });
    }

    [Fact]
    public async Task Import_dwca_file_and_verify_datasets()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------        
        await ProcessFixture.ImportDwcaFileAsync(@"data\resources\dwca-datastewardship-bats-taxalists.zip", Output);

        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------
        // Get by id        
        var datasetById1 = await ApiClient.GetFromJsonAsync<Dataset>($"datastewardship/datasets/ArtportalenDataHost - Dataset Bats (Hallaröd)", jsonSerializerOptions);
        var datasetById2 = await ApiClient.GetFromJsonAsync<Dataset>($"datastewardship/datasets/ArtportalenDataHost - Dataset Bats (Other)", jsonSerializerOptions);

        // Get by search - Events with Dataset "Bats (Hallaröd)"
        var searchFilter = new DatasetFilter { DatasetIds = new List<string> { "ArtportalenDataHost - Dataset Bats (Hallaröd)" } };
        var pageResultHallarod = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<Dataset>, DatasetFilter>($"datastewardship/datasets", searchFilter, jsonSerializerOptions);

        // Get by search - Events with Dataset "Bats (Other)"
        searchFilter = new DatasetFilter { DatasetIds = new List<string> { "ArtportalenDataHost - Dataset Bats (Other)" } };
        var pageResultOther = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<Dataset>, DatasetFilter>($"datastewardship/datasets", searchFilter, jsonSerializerOptions);

        //-----------------------------------------------------------------------------------------------------------
        // Assert
        //-----------------------------------------------------------------------------------------------------------
        datasetById1.Should().NotBeNull();
        datasetById2.Should().NotBeNull();

        pageResultHallarod.Records.Should().AllSatisfy(m =>
        {
            m.Identifier.Should().Be("ArtportalenDataHost - Dataset Bats (Hallaröd)");
        });

        pageResultOther.Records.Should().AllSatisfy(m =>
        {
            m.Identifier.Should().Be("ArtportalenDataHost - Dataset Bats (Other)");
        });
    }
}