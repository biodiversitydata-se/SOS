using FizzWare.NBuilder;
using FluentAssertions;
using SOS.Lib.Models.Processed.Observation;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos;
using System.Linq;
using LinqStatistics;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.Extensions;
using System.ComponentModel;
using SOS.Lib.Models.Verbatim.DarwinCore;
using System.IO;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationProcessing.DarwinCore
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class DarwinCoreArchiveTests
    {
        private readonly IntegrationTestFixture _fixture;

        public DarwinCoreArchiveTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]        
        public async Task CreateOccurrenceCsvFile()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .Build();

            var processedObservations = _fixture.ProcessObservations(verbatimObservations).ToList();
            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);            
            await _fixture.AddObservationsToElasticsearchAsync(processedObservations);

            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var writeStream = new MemoryStream();
            var fieldDescriptions = FieldDescriptionHelper.GetAllDwcOccurrenceCoreFieldDescriptions();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var nrObservations = await _fixture.DwcArchiveOccurrenceCsvWriter.CreateOccurrenceCsvFileAsync(
                new Lib.Models.Search.SearchFilter(),
                writeStream,
                fieldDescriptions,
                _fixture.CustomProcessedObservationRepository,
                null,
                true);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            List<Dictionary<string, string>> items = ReadOccurrenceCsvFile(writeStream.ToArray());
            var csvObs = items[0];            
            var processedObs = processedObservations.Single(m => m.Occurrence.OccurrenceId == csvObs["occurrenceID"]);
            csvObs["country"].Should().Be(processedObs.Location.Country.Value);
            // todo - add more asserts
        }

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
}