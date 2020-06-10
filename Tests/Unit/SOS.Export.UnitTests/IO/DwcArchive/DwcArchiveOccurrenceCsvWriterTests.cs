using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Export.IO.DwcArchive;
using SOS.Export.Managers;
using SOS.Export.MongoDb.Interfaces;
using SOS.Export.Repositories;
using SOS.Export.UnitTests.TestHelpers.Builders;
using SOS.Export.UnitTests.TestHelpers.Factories;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Search;
using Xunit;

namespace SOS.Export.UnitTests.IO.DwcArchive
{
    // todo - create DarwinCore class that is used for reading data where all properties is of string class? Suggested names: FlatDwcObservation, CsvDwcObservation, 
    public class DwcArchiveOccurrenceCsvWriterTests : TestBase
    {
        private List<dynamic> ReadCsvFile(MemoryStream memoryStream)
        {
            using var readMemoryStream = new MemoryStream(memoryStream.ToArray());
            using var streamReader = new StreamReader(readMemoryStream);
            using var csvReader = new CsvReader(streamReader, new CsvConfiguration(CultureInfo.InvariantCulture));
            SetCsvConfigurations(csvReader);
            var records = csvReader.GetRecords<dynamic>().ToList();
            return records;
        }

        private void SetCsvConfigurations(CsvReader csv)
        {
            csv.Configuration.HasHeaderRecord = true;
            csv.Configuration.Delimiter = "\t";
            csv.Configuration.Encoding = Encoding.UTF8;
            csv.Configuration.BadDataFound = x => { Console.WriteLine($"Bad data: <{x.RawRecord}>"); };
        }

        private DwcArchiveOccurrenceCsvWriter CreateDwcArchiveOccurrenceCsvWriter()
        {
            var writer = new DwcArchiveOccurrenceCsvWriter(
                ProcessedFieldMappingRepositoryStubFactory.Create().Object,
                new TaxonManager(
                    new ProcessedTaxonRepository(new Mock<IExportClient>().Object,
                        new Mock<ILogger<ProcessedTaxonRepository>>().Object),
                    new Mock<ILogger<TaxonManager>>().Object),
                new Mock<ILogger<DwcArchiveOccurrenceCsvWriter>>().Object);
            return writer;
        }


        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "DwcArchiveUnit")]
        public async Task CoordinateUncertaintyInMeters_property_is_never_zero()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcArchiveOccurrenceCsvWriter = CreateDwcArchiveOccurrenceCsvWriter();
            var memoryStream = new MemoryStream();
            var observationBuilder = new ProcessedObservationBuilder();
            var observation = observationBuilder
                .WithCoordinateUncertaintyInMeters(0)
                .Build();
            var processedDarwinCoreRepositoryStub = ProcessedDarwinCoreRepositoryStubFactory.Create(observation);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            await dwcArchiveOccurrenceCsvWriter.CreateOccurrenceCsvFileAsync(
                new SearchFilter(),
                memoryStream,
                FieldDescriptionHelper.GetDefaultDwcExportFieldDescriptions(),
                processedDarwinCoreRepositoryStub.Object,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            var record = ReadCsvFile(memoryStream).First();
            ((string) record.coordinateUncertaintyInMeters).Should().NotBe("0",
                "because Zero is not valid value for this term (https://dwc.tdwg.org/terms/#dwc:coordinateUncertaintyInMeters)");
            ((string) record.coordinateUncertaintyInMeters).Should()
                .Be("1", "because this is the closest natural number to 0");
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "DwcArchiveUnit")]
        public async Task Create_a_DwC_occurrence_csv_file_with_ten_observations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcArchiveOccurrenceCsvWriter = CreateDwcArchiveOccurrenceCsvWriter();
            var processedDarwinCoreRepositoryStub =
                ProcessedDarwinCoreRepositoryStubFactory.Create(@"Resources\TenProcessedTestObservations.json");
            var memoryStream = new MemoryStream();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await dwcArchiveOccurrenceCsvWriter.CreateOccurrenceCsvFileAsync(
                new SearchFilter(),
                memoryStream,
                FieldDescriptionHelper.GetDefaultDwcExportFieldDescriptions(),
                processedDarwinCoreRepositoryStub.Object,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            var records = ReadCsvFile(memoryStream).ToList();
            result.Should().BeTrue();
            records.Count.Should().Be(10);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "DwcArchiveUnit")]
        public async Task
            Creating_Occurrence_csv_file_with_subset_of_field_descriptions_is_using_exactly_the_provided_fields()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcArchiveOccurrenceCsvWriter = CreateDwcArchiveOccurrenceCsvWriter();
            var memoryStream = new MemoryStream();
            var observation = DarwinCoreObservationFactory.CreateDefaultObservation();
            var processedDarwinCoreRepositoryStub = ProcessedDarwinCoreRepositoryStubFactory.Create(observation);
            var fieldDescriptionIds = new List<FieldDescriptionId>
            {
                FieldDescriptionId.OccurrenceID,
                FieldDescriptionId.ScientificName,
                FieldDescriptionId.DecimalLongitude,
                FieldDescriptionId.DecimalLatitude
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            await dwcArchiveOccurrenceCsvWriter.CreateOccurrenceCsvFileAsync(
                new SearchFilter(),
                memoryStream,
                FieldDescriptionHelper.GetFieldDescriptions(fieldDescriptionIds),
                processedDarwinCoreRepositoryStub.Object,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            var record = ReadCsvFile(memoryStream).First();
            var recordDictionary = (IDictionary<string, object>) record;
            recordDictionary.Should()
                .ContainKey("occurrenceID", "because this field was provided as field description");
            recordDictionary.Should()
                .NotContainKey("basisOfRecord", "because this field was not provided as field description");
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "DwcArchiveUnit")]
        public async Task Longitude_and_latitude_is_rounded_to_five_decimals()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcArchiveOccurrenceCsvWriter = CreateDwcArchiveOccurrenceCsvWriter();
            var memoryStream = new MemoryStream();
            var observationBuilder = new ProcessedObservationBuilder();
            var observation = observationBuilder
                .WithDecimalLatitude(13.823392373018132)
                .WithDecimalLongitude(55.51071440795833)
                .Build();
            var processedDarwinCoreRepositoryStub = ProcessedDarwinCoreRepositoryStubFactory.Create(observation);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            await dwcArchiveOccurrenceCsvWriter.CreateOccurrenceCsvFileAsync(
                new SearchFilter(),
                memoryStream,
                FieldDescriptionHelper.GetDefaultDwcExportFieldDescriptions(),
                processedDarwinCoreRepositoryStub.Object,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            var record = ReadCsvFile(memoryStream).First();
            ((string) record.decimalLatitude).Should().Be("13.82339");
            ((string) record.decimalLongitude).Should().Be("55.51071");
        }


        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "DwcArchiveUnit")]
        public async Task OccurrenceRemarks_containing_NewLine_characters_is_replaced_by_space()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcArchiveOccurrenceCsvWriter = CreateDwcArchiveOccurrenceCsvWriter();
            var memoryStream = new MemoryStream();
            var observationBuilder = new ProcessedObservationBuilder();
            var observation = observationBuilder
                .WithOccurrenceRemarks("Sighting found in\r\nUppsala")
                .Build();
            var processedDarwinCoreRepositoryStub = ProcessedDarwinCoreRepositoryStubFactory.Create(observation);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            await dwcArchiveOccurrenceCsvWriter.CreateOccurrenceCsvFileAsync(
                new SearchFilter(),
                memoryStream,
                FieldDescriptionHelper.GetDefaultDwcExportFieldDescriptions(),
                processedDarwinCoreRepositoryStub.Object,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            var record = ReadCsvFile(memoryStream).First();
            ((string) record.occurrenceRemarks).Should().Be("Sighting found in Uppsala");
        }
    }
}