using DwC_A;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Harvest.DarwinCore;
using SOS.Harvest.DarwinCore.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using System.Reflection;

namespace SOS.DataStewardship.Api.IntegrationTests.IntegrationTests;

[Collection(Constants.IntegrationTestsCollectionName)]
public class DwcaImportTests : TestBase
{
    private ProcessFixture _processFixture;
    public DwcaImportTests(DataStewardshipApiWebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory) 
    {
        //using var scope = _factory.ServiceProvider.CreateScope();        
        //_processFixture = scope.ServiceProvider.GetService<ProcessFixture2>();
    }
   
    [Fact]
    public async Task Test_import_dwca_file()
    {
        // Arrange
        //const string fileName = "./resources/dwca-datastewardship-bats-taxalists.zip";
        const string fileName = "/resources/dwca-datastewardship-bats-taxalists.zip";
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var filePath = Path.Combine(assemblyPath, @"resources\dwca-datastewardship-bats-taxalists.zip");
        var parsedDwcaFile = await ReadDwcaFile(filePath);

        using var scope = _factory.ServiceProvider.CreateScope();
        var processFixture2 = scope.ServiceProvider.GetService<ProcessFixture>();
        var dwcFactory = processFixture2.GetDarwinCoreFactory(false);
        
        var processedObservations = parsedDwcaFile.Occurrences.Select(m => dwcFactory.CreateProcessedObservation(m, false));
        await base.AddObservationsToElasticsearchAsync(processedObservations);

        // Act

        // Get by id
        var observation1 = await Client.GetFromJsonAsync<OccurrenceModel>($"datastewardship/occurrences/test:bats:sighting:98571703", jsonSerializerOptions);
        var observation2 = await Client.GetFromJsonAsync<OccurrenceModel>($"datastewardship/occurrences/test:bats:sighting:98571253", jsonSerializerOptions);


        // Get by search
        var searchFilter = new OccurrenceFilter { DatasetIds = new List<string> { "ArtportalenDataHost - Dataset Bats (Hallaröd)" } };        
        var getBySearchResponse = await Client.PostAsJsonAsync($"datastewardship/occurrences", searchFilter, jsonSerializerOptions);
        var pageResult = await getBySearchResponse.Content.ReadFromJsonAsync<PagedResult<OccurrenceModel>>(jsonSerializerOptions);


        // Get by search2
        var searchFilter2 = new OccurrenceFilter { DatasetIds = new List<string> { "ArtportalenDataHost - Dataset Bats (Other)" } };
        var getBySearchResponse2 = await Client.PostAsJsonAsync($"datastewardship/occurrences", searchFilter2, jsonSerializerOptions);
        var pageResult2 = await getBySearchResponse2.Content.ReadFromJsonAsync<PagedResult<OccurrenceModel>>(jsonSerializerOptions);

        // Assert
        observation1.Should().NotBeNull();
        observation2.Should().NotBeNull();

        pageResult.Records.Should().AllSatisfy(m =>
        {
            m.DatasetIdentifier.Should().Be("ArtportalenDataHost - Dataset Bats (Hallaröd)");
        });

        pageResult2.Records.Should().AllSatisfy(m =>
        {
            m.DatasetIdentifier.Should().Be("ArtportalenDataHost - Dataset Bats (Other)");
        });
    }

    private async Task<DwcaComposite> ReadDwcaFile(string filepath)
    {
        //const string archivePath = "./resources/dwca/dwca-datastewardship-bats-taxalists.zip";
        var dataProvider = new DataProvider { Id = 105, Identifier = "TestDataStewardshipBats", Type = DataProviderType.DwcA };
        IDwcArchiveReader dwcArchiveReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());
        string outputPath = Path.GetTempPath();
        using var archiveReader = new ArchiveReader(filepath, outputPath); // @"C:\Temp\DwcaImport");
        var archiveReaderContext = ArchiveReaderContext.Create(archiveReader, dataProvider);

        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------
        var datasets = await dwcArchiveReader.ReadDatasetsAsync(archiveReaderContext);
        var occurrences = await dwcArchiveReader.ReadOccurrencesAsync(archiveReaderContext);
        var events = await dwcArchiveReader.ReadEventsAsync(archiveReaderContext);

        return new  DwcaComposite
        {
            Datasets = datasets,
            Events = events,
            Occurrences = occurrences
        };
    }

    public class DwcaComposite
    {
        public List<DwcVerbatimObservationDataset> Datasets { get; set; }
        public IEnumerable<SOS.Lib.Models.Verbatim.DarwinCore.DwcEventOccurrenceVerbatim> Events { get; set; }
        public IEnumerable<SOS.Lib.Models.Verbatim.DarwinCore.DwcObservationVerbatim> Occurrences { get; set; }
    }
}