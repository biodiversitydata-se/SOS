using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SOS.AutomaticIntegrationTests.Extensions;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using Xunit;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.ExportsController.DownloadCsvEndpoint
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class DownloadCsvTests
    {
        private readonly IntegrationTestFixture _fixture;

        public DownloadCsvTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }
        
        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task DownloadCsvFile()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto()
            {
                Output = new OutputFilterDto()
                {
                    //Fields = new List<string> { "Occurrence.OccurrenceId", "Event.StartDate", "Location.DecimalLatitude"}
                    FieldSet = OutputFieldSet.Minimum
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var csvFileResult = await _fixture.ExportsController.DownloadCsv(
                searchFilter,
                PropertyLabelType.PropertyName,
                "sv-SE",
                false);

            var file = (FileContentResult) csvFileResult;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            file.FileContents.Length.Should().BeGreaterThan(0);
            var fileEntries = ReadCsvFile(file.FileContents);
            fileEntries.Count.Should().Be(100);
        }

        private List<Dictionary<string, string>> ReadCsvFile(byte[] file)
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
