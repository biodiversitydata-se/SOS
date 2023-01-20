using SOS.DataStewardship.Api.IntegrationTests.Extensions;
using SOS.DataStewardship.Api.IntegrationTests.Helpers;
using SOS.Lib.Models.Shared;
using Xunit.Abstractions;

namespace SOS.DataStewardship.Api.IntegrationTests.IntegrationTests;

[Collection(Constants.IntegrationTestsCollectionName)]
public class DwcaImportTests : TestBase
{
    private readonly ProcessFixture _processFixture;
    private readonly ITestOutputHelper _output;    

    public DwcaImportTests(ApiWebApplicationFactory<Program> webApplicationFactory, 
        ITestOutputHelper output) : base(webApplicationFactory) 
    {
        this._output = output;
        using var scope = _factory.ServiceProvider.CreateScope();        
        _processFixture = scope.ServiceProvider.GetService<ProcessFixture>();
    }

    [Fact]
    public async Task Import_dwca_file_and_verify_observations()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------
        await ImportDwcaFileAsync(@"resources\dwca-datastewardship-bats-taxalists.zip");

        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------

        // Get by id
        var observationById1 = await Client.GetFromJsonAsync<OccurrenceModel>($"datastewardship/occurrences/test:bats:sighting:98571703", jsonSerializerOptions);
        var observationById2 = await Client.GetFromJsonAsync<OccurrenceModel>($"datastewardship/occurrences/test:bats:sighting:98571253", jsonSerializerOptions);

        // Get by search - Observations with Dataset "Bats (Hallaröd)"
        var searchFilter = new OccurrenceFilter { DatasetIds = new List<string> { "ArtportalenDataHost - Dataset Bats (Hallaröd)" } };        
        var pageResultHallarod = await Client.PostAsJsonAsync<PagedResult<OccurrenceModel>, OccurrenceFilter>($"datastewardship/occurrences", searchFilter, jsonSerializerOptions);

        // Get by search - Observations with Dataset "Bats (Other)"
        searchFilter = new OccurrenceFilter { DatasetIds = new List<string> { "ArtportalenDataHost - Dataset Bats (Other)" } };
        var pageResultOther = await Client.PostAsJsonAsync<PagedResult<OccurrenceModel>, OccurrenceFilter>($"datastewardship/occurrences", searchFilter, jsonSerializerOptions);

        //-----------------------------------------------------------------------------------------------------------
        // Assert
        //-----------------------------------------------------------------------------------------------------------
        observationById1.Should().NotBeNull();
        observationById2.Should().NotBeNull();

        pageResultHallarod.Records.Should().AllSatisfy(m =>
        {
            m.DatasetIdentifier.Should().Be("ArtportalenDataHost - Dataset Bats (Hallaröd)");
        });

        pageResultOther.Records.Should().AllSatisfy(m =>
        {
            m.DatasetIdentifier.Should().Be("ArtportalenDataHost - Dataset Bats (Other)");
        });
    }

    [Fact]
    public async Task Import_dwca_file_and_verify_events()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------        
        await ImportDwcaFileAsync(@"resources\dwca-datastewardship-bats-taxalists.zip");

        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------

        // Get by id
        var eventById1 = await Client.GetFromJsonAsync<EventModel>($"datastewardship/events/test:bats:event:12581041667877196608", jsonSerializerOptions);
        var eventById2 = await Client.GetFromJsonAsync<EventModel>($"datastewardship/events/test:bats:event:14009236676399444594", jsonSerializerOptions);

        // Get by search - Events with Dataset "Bats (Hallaröd)"
        var searchFilter = new EventsFilter { DatasetList = new List<string> { "ArtportalenDataHost - Dataset Bats (Hallaröd)" } };
        var pageResultHallarod = await Client.PostAsJsonAsync<PagedResult<EventModel>, EventsFilter>($"datastewardship/events", searchFilter, jsonSerializerOptions);

        // Get by search - Events with Dataset "Bats (Other)"
        searchFilter = new EventsFilter { DatasetList = new List<string> { "ArtportalenDataHost - Dataset Bats (Other)" } };
        var pageResultOther = await Client.PostAsJsonAsync<PagedResult<EventModel>, EventsFilter>($"datastewardship/events", searchFilter, jsonSerializerOptions);

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
        await ImportDwcaFileAsync(@"resources\dwca-datastewardship-bats-taxalists.zip");

        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------

        // Get by id
        var datasetById1 = await Client.GetFromJsonAsync<Dataset>($"datastewardship/datasets/ArtportalenDataHost - Dataset Bats (Hallaröd)", jsonSerializerOptions);
        var datasetById2 = await Client.GetFromJsonAsync<Dataset>($"datastewardship/datasets/ArtportalenDataHost - Dataset Bats (Other)", jsonSerializerOptions);

        // Get by search - Events with Dataset "Bats (Hallaröd)"
        var searchFilter = new DatasetFilter { DatasetList = new List<string> { "ArtportalenDataHost - Dataset Bats (Hallaröd)" } };
        var pageResultHallarod = await Client.PostAsJsonAsync<PagedResult<Dataset>, DatasetFilter>($"datastewardship/datasets", searchFilter, jsonSerializerOptions);

        // Get by search - Events with Dataset "Bats (Other)"
        searchFilter = new DatasetFilter { DatasetList = new List<string> { "ArtportalenDataHost - Dataset Bats (Other)" } };
        var pageResultOther = await Client.PostAsJsonAsync<PagedResult<Dataset>, DatasetFilter>($"datastewardship/datasets", searchFilter, jsonSerializerOptions);

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

    private async Task ImportDwcaFileAsync(string filePath)
    {
        var parsedDwcaFile = await DwcaHelper.ReadDwcaFileAsync(filePath);
        var observationFactory = _processFixture.GetDwcaObservationFactory(true);
        var eventFactory = _processFixture.GetDwcaEventFactory(true);
        var datasetFactory = _processFixture.GetDwcaDatasetFactory();
        var processedObservations = parsedDwcaFile
            .Occurrences
            .Select(m => observationFactory.CreateProcessedObservation(m, false))
            .ToList();
        await AddObservationsToElasticsearchAsync(processedObservations);
        _output.WriteLine($"Processed observations count= {processedObservations.Count}");

        var processedEvents = parsedDwcaFile
            .Events
            .Select(m => eventFactory.CreateEventObservation(m))
            .ToList();
        await AddEventsToElasticsearchAsync(processedEvents);
        _output.WriteLine($"Processed events count= {processedEvents.Count}");

        var processedDatasets = parsedDwcaFile
            .Datasets
            .Select(m => datasetFactory.CreateProcessedDataset(m))
            .ToList();
        await AddDatasetsToElasticsearchAsync(processedDatasets);
        _output.WriteLine($"Processed datasets count= {processedDatasets.Count}");
    }
}