using FizzWare.NBuilder;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Helpers;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.IntegrationTests.Tests.Exports;

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
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto();

        // Act
        var response = await apiClient.PostAsync($"/exports/download/dwc?eventbased=true", JsonContent.Create(searchFilter));
        using var contentStream = await response.Content.ReadAsStreamAsync();
        var filePath = Path.GetTempFileName();
        using (var fileStream = File.Create(filePath))
        {
            await contentStream.CopyToAsync(fileStream);
        }
        var parsedDwcaFile = await DwcaHelper.ReadDwcaFileAsync(filePath, dataProvider);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        parsedDwcaFile.Occurrences.Count().Should().Be(15, because: "the DwC-A file contains 15 occurrences");
        parsedDwcaFile.Events.Count().Should().Be(7, because: "the DwC-A file contains 7 events");
    }

    [Fact]
    public async Task CreateOccurrenceCsvFile_ReturnsExpectedRowsAndColumns_WhenUsingNoFilter()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        var processedObservations = ProcessFixture.ProcessObservations(verbatimObservations).ToList();
        await ProcessFixture.AddObservationsToElasticsearchAsync(processedObservations, true, 0);
        var dwcArchiveOccurrenceCsvWriter = TestFixture.ApiFactory.Services.GetService<IDwcArchiveOccurrenceCsvWriter>();
        var processedObservationRepository = TestFixture.ApiFactory.Services.GetService<IProcessedObservationRepository>();
        var writeStream = new MemoryStream();
        var fieldDescriptions = FieldDescriptionHelper.GetAllDwcOccurrenceCoreFieldDescriptions();

        // Act
        var nrObservations = await dwcArchiveOccurrenceCsvWriter!.CreateOccurrenceCsvFileAsync(
            new Lib.Models.Search.Filters.SearchFilter(0),
            writeStream,
            fieldDescriptions,
            processedObservationRepository,
            null,
            true);

        // Assert
        List<Dictionary<string, string>> items = CsvHelper.ReadCsvFile(writeStream.ToArray());// ReadOccurrenceCsvFile(writeStream.ToArray());
        var csvObs = items[0];
        var processedObs = processedObservations.Single(m => m.Occurrence.OccurrenceId == csvObs["occurrenceID"]);
        csvObs["country"].Should().Be(processedObs.Location.Country.Value);
        // todo - add more asserts
    }
}