
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Filter;
using Xunit;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.ExportsController.DownloadDwCEndpoint
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class DownloadDwCTests
    {
        private readonly IntegrationTestFixture _fixture;

        private List<Dictionary<string, string>> ReadDwCFile(byte[] file)
        {
            var dwcFileName = @"C:\\Temp\DwC_test.zip";
            try
            {
                // Save dwc file temporary
                using var stream = File.Create(dwcFileName);
                stream.Write(file, 0, file.Length);
                stream.Close();
                using var zipArchive = ZipFile.OpenRead(dwcFileName);

                var items = new List<Dictionary<string, string>>();
                using (var csvStream = zipArchive.GetEntry("occurrence.txt").Open())
                {
                    using (var streamRdr = new StreamReader(csvStream))
                    {
                        var dwCReader = new NReco.Csv.CsvReader(streamRdr, "\t");
                        var columnIdByHeader = new Dictionary<string, int>();
                        var headerByColumnId = new Dictionary<int, string>();

                        // Read header
                        dwCReader.Read();
                        for (int i = 0; i < dwCReader.FieldsCount; i++)
                        {
                            string val = dwCReader[i];
                            columnIdByHeader.Add(val, i);
                            headerByColumnId.Add(i, val);
                        }

                        // Read data
                        while (dwCReader.Read())
                        {
                            var item = new Dictionary<string, string>();
                            for (int i = 0; i < dwCReader.FieldsCount; i++)
                            {
                                string val = dwCReader[i];
                                item.Add(headerByColumnId[i], val);
                            }

                            items.Add(item);
                        }
                    }
                    csvStream.Close();
                }
                return items;
            }
            finally
            {
                File.Delete(dwcFileName);
            }
        }

        public DownloadDwCTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }
        
        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task DownloadDwCFile_MinimumFieldSet()
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
            var dwCFileResult = await _fixture.ExportsController.DownloadDwC(
                searchFilter);

            var file = (FileContentResult)dwCFileResult;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            file.FileContents.Length.Should().BeGreaterThan(0);
            var fileEntries = ReadDwCFile(file.FileContents);
            fileEntries.Count.Should().Be(100);
        }
    }
}
