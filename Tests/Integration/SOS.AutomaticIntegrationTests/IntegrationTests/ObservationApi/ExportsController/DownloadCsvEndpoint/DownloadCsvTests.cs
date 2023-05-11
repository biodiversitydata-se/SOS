
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
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
            var csvFileResult = await _fixture.ExportsController.DownloadCsvAsync(
                null, 
                null,
                searchFilter,
                OutputFieldSet.Minimum,
                false,
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
                    .With(m => m.Activity = new MetadataWithCategory<int>((int)ActivityId.Incubating, 1))
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto
            {
                Output = new OutputFilterDto
                {
                    Fields = new List<string> { "Occurrence.OccurrenceId", "DatasetName", "Occurrence.RecordedBy", "Occurrence.Activity" }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var csvFileResult = await _fixture.ExportsController.DownloadCsvAsync(
                null,
                null,
                searchFilter,
                OutputFieldSet.Minimum,
                false,
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

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task DownloadCsvFile_TestDifferentPropertyLabels()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto
            {
                Output = new OutputFilterDto
                {
                    Fields = new List<string> { "Occurrence.OccurrenceId", "DatasetName", "Occurrence.RecordedBy", "Occurrence.Activity" }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var propertyNameFileResult = await _fixture.ExportsController.DownloadCsvAsync(
                null,
                null,
                searchFilter,
                OutputFieldSet.None,
                false,
                PropertyLabelType.PropertyName,
                "sv-SE",
                false);
            var propertyNameFile = (FileContentResult)propertyNameFileResult;

            var propertyPathFileResult = await _fixture.ExportsController.DownloadCsvAsync(
                null,
                null,
                searchFilter,
                OutputFieldSet.None,
                false,
                PropertyLabelType.PropertyPath,
                "sv-SE",
                false);
            var propertyPathFile = (FileContentResult)propertyPathFileResult;

            var swedishFileResult = await _fixture.ExportsController.DownloadCsvAsync(
                null,
                null,
                searchFilter,
                OutputFieldSet.None,
                false,
                PropertyLabelType.Swedish,
                "sv-SE",
                false);
            var swedishFile = (FileContentResult)swedishFileResult;

            var englishFileResult = await _fixture.ExportsController.DownloadCsvAsync(
                null,
                null,
                searchFilter,
                OutputFieldSet.None,
                false,
                PropertyLabelType.English,
                "sv-SE",
                false);
            var englishFile = (FileContentResult)englishFileResult;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            var propertyNameEntries = ReadCsvFile(propertyNameFile.FileContents);
            var propertyPathEntries = ReadCsvFile(propertyPathFile.FileContents);
            var swedishEntries = ReadCsvFile(swedishFile.FileContents);
            var englishEntries = ReadCsvFile(englishFile.FileContents);

            propertyNameEntries.First().Keys.Should()
                .BeEquivalentTo("OccurrenceId", "DatasetName", "RecordedBy", "Activity");
            propertyPathEntries.First().Keys.Should()
                .BeEquivalentTo("Occurrence.OccurrenceId", "DatasetName", "Occurrence.RecordedBy", "Occurrence.Activity");
            swedishEntries.First().Keys.Should()
                .BeEquivalentTo("Observation GUID", "Datakälla", "Observatör", "Aktivitet");
            englishEntries.First().Keys.Should()
                .BeEquivalentTo("Occurrence Id", "Dataset", "Recorded By", "Activity");
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task DownloadCsvFile_ExtendedFieldSet_with_project_parameters_and_media()
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
                    .HaveProjectInformation()
                    .HaveMediaInformation()
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto()
            {
                Output = new OutputFilterDto()
                {
                    FieldSet = OutputFieldSet.Extended,
                    Fields = new List<string> { "Occurrence.Media"}
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var csvFileResult = await _fixture.ExportsController.DownloadCsvAsync(
                null,
                null,
                searchFilter,
                OutputFieldSet.Minimum,
                false,
                PropertyLabelType.PropertyName,
                "sv-SE",
                false);

            var file = (FileContentResult)csvFileResult;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            file.FileContents.Length.Should().BeGreaterThan(0);
            var fileEntries = ReadCsvFile(file.FileContents);
            var fileEntry = fileEntries.Single(m => m["OccurrenceId"] == occurrenceId);
            fileEntry["Projects"].Should().NotBeNullOrEmpty();
            fileEntry["Media"].Should().NotBeNullOrEmpty();
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
