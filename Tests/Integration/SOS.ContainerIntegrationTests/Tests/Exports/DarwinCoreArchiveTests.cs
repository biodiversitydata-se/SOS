using FizzWare.NBuilder;
using SOS.ContainerIntegrationTests.Setup;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Verbatim.Artportalen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.ContainerIntegrationTests.Tests.Exports;

/// <summary>
/// Integration tests for exporting to DwC-A file.
/// </summary>
[Collection(IntegrationTestsCollection.Name)]
public class DarwinCoreArchiveTests : IntegrationTestsBase
{
    public DarwinCoreArchiveTests(IntegrationTestsFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    //[Fact]
    //public async Task CreateOccurrenceCsvFile()
    //{
    //    //-----------------------------------------------------------------------------------------------------------
    //    // Arrange - Create verbatim observations
    //    //-----------------------------------------------------------------------------------------------------------            
    //    var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
    //        .All()
    //            .HaveValuesFromPredefinedObservations()
    //        .Build();

    //    var processedObservations = ProcessFixture.ProcessObservations(verbatimObservations).ToList();
    //    await ProcessFixture.AddObservationsToElasticsearchAsync(processedObservations);

    //    //-----------------------------------------------------------------------------------------------------------
    //    // Arrange
    //    //-----------------------------------------------------------------------------------------------------------
    //    var writeStream = new MemoryStream();
    //    var fieldDescriptions = FieldDescriptionHelper.GetAllDwcOccurrenceCoreFieldDescriptions();

    //    //-----------------------------------------------------------------------------------------------------------
    //    // Act
    //    //-----------------------------------------------------------------------------------------------------------
    //    var nrObservations = await _fixture.DwcArchiveOccurrenceCsvWriter.CreateOccurrenceCsvFileAsync(
    //        new Lib.Models.Search.Filters.SearchFilter(0),
    //        writeStream,
    //        fieldDescriptions,
    //        _fixture.ProcessedObservationRepository,
    //        null,
    //        true);

    //    //-----------------------------------------------------------------------------------------------------------
    //    // Assert
    //    //-----------------------------------------------------------------------------------------------------------
    //    List<Dictionary<string, string>> items = ReadOccurrenceCsvFile(writeStream.ToArray());
    //    var csvObs = items[0];
    //    var processedObs = processedObservations.Single(m => m.Occurrence.OccurrenceId == csvObs["occurrenceID"]);
    //    csvObs["country"].Should().Be(processedObs.Location.Country.Value);
    //    // todo - add more asserts
    //}

    private List<Dictionary<string, string>> ReadOccurrenceCsvFile(byte[] file)
    {
        var items = new List<Dictionary<string, string>>();
        using (var readMemoryStream = new MemoryStream(file))
        {
            using (var streamRdr = new StreamReader(readMemoryStream))
            {
                var csvReader = new NReco.Csv.CsvReader(streamRdr, "\t");
                var columnIdByHeader = new Dictionary<string, int>();
                var headerByColumnId = new Dictionary<int, string>();

                // Read header
                csvReader.Read();
                for (int i = 0; i < csvReader.FieldsCount; i++)
                {
                    string val = csvReader[i];
                    columnIdByHeader.Add(val, i);
                    headerByColumnId.Add(i, val);
                }

                // Read data
                while (csvReader.Read())
                {
                    var item = new Dictionary<string, string>();
                    for (int i = 0; i < csvReader.FieldsCount; i++)
                    {
                        string val = csvReader[i];
                        item.Add(headerByColumnId[i], val);
                    }

                    items.Add(item);
                }
            }

            return items;
        }
    }
}