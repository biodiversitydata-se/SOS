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
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
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
        public async Task DownloadCsvFile_MinimumFieldSet()
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

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task DownloadCsvFile_SpecificFields()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            const int sightingId = 123456;
            const string occurrenceId = "urn:lsid:artportalen.se:sighting:123456";
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(1)
                    .With(m => m.SightingId = sightingId)
                    .With(m => m.DatasourceId = 1)
                    .With(m => m.Observers = "Tom Volgers")
                    .With(m => m.Activity = new MetadataWithCategory((int)ActivityId.Incubating, 1))
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto
            {
                Output = new OutputFilterDto
                {
                    Fields = new List<string> { "Occurrence.OccurrenceId", "DatasetName", "Occurrence.RecordedBy", "Occurrence.Activity.Value" }
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

            var file = (FileContentResult)csvFileResult;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            file.FileContents.Length.Should().BeGreaterThan(0);
            var fileEntries = ReadCsvFile(file.FileContents);
            fileEntries.Count.Should().Be(100);
            var fileEntry = fileEntries.Single(m => m["OccurrenceId"] == occurrenceId);
            fileEntry["DatasetName"].Should().Be("Artportalen");
            fileEntry["RecordedBy"].Should().Be("Tom Volgers");
            fileEntry["Activity"].Should().Be("ruvande");
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
