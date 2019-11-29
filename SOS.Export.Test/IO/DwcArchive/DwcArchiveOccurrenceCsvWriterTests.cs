using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SOS.Export.Factories;
using SOS.Export.Helpers;
using SOS.Export.IO.DwcArchive;
using SOS.Export.Repositories.Interfaces;
using SOS.Export.Services;
using SOS.Export.Services.Interfaces;
using SOS.Export.Test.TestHelpers.JsonConverters;
using SOS.Export.Test.TestHelpers.Stubs;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Models.DarwinCore;
using Xunit;

namespace SOS.Export.Test.IO.DwcArchive
{
    // todo - create DarwinCore class that is used for reading data where all properties is of string class? Suggested names: FlatDwcObservation, CsvDwcObservation, 
    public class DwcArchiveOccurrenceCsvWriterTests : TestBase
    {
        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "DwcArchiveUnit")]
        public async Task CreateDwcOccurrenceCsvFile_LoadTenObservations_ShouldSucceed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var processedDarwinCoreRepositoryMock = CreateProcessedDarwinCoreRepositoryMock(@"Resources\TenProcessedTestObservations.json");
            var dwcArchiveOccurrenceCsvWriter = new DwcArchiveOccurrenceCsvWriter(
                new Mock<ILogger<DwcArchiveOccurrenceCsvWriter>>().Object);
            var memoryStream = new MemoryStream();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            bool result = await dwcArchiveOccurrenceCsvWriter.CreateOccurrenceCsvFileAsync(
                memoryStream, 
                FieldDescriptionHelper.GetDefaultDwcExportFieldDescriptions(),
                processedDarwinCoreRepositoryMock.Object, 
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            List<dynamic> records = ReadCsvFile(memoryStream).ToList();
            result.Should().BeTrue();
            records.Count.Should().Be(10);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "DwcArchiveUnit")]
        public async Task CreateDwcOccurrenceCsvFile_LongLatProperties_ShouldBeRoundedToFiveDecimals()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcArchiveOccurrenceCsvWriter = new DwcArchiveOccurrenceCsvWriter(
                new Mock<ILogger<DwcArchiveOccurrenceCsvWriter>>().Object);
            var memoryStream = new MemoryStream();
            var fieldDescriptions = FieldDescriptionHelper.GetDefaultDwcExportFieldDescriptions();
            
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Test data
            //-----------------------------------------------------------------------------------------------------------
            var observation = GetDefaultObservation();
            observation.Location.DecimalLatitude = 13.823392373018132;
            observation.Location.DecimalLongitude = 55.51071440795833;
            var processedDarwinCoreRepositoryMock = CreateProcessedDarwinCoreRepositoryMock(observation);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            await dwcArchiveOccurrenceCsvWriter.CreateOccurrenceCsvFileAsync(
                memoryStream,
                fieldDescriptions,
                processedDarwinCoreRepositoryMock.Object,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            dynamic record = ReadCsvFile(memoryStream).First();
            ((string)record.decimalLatitude).Should().Be("13.82339");
            ((string)record.decimalLongitude).Should().Be("55.51071");
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "DwcArchiveUnit")]
        public async Task CreateDwcOccurrenceCsvFile_OccurrenceRemarksWithNewLine_ShouldBeReplacedBySpace()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcArchiveOccurrenceCsvWriter = new DwcArchiveOccurrenceCsvWriter(
                new Mock<ILogger<DwcArchiveOccurrenceCsvWriter>>().Object);
            var memoryStream = new MemoryStream();
            var fieldDescriptions = FieldDescriptionHelper.GetDefaultDwcExportFieldDescriptions();

            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Test data
            //-----------------------------------------------------------------------------------------------------------
            var observation = GetDefaultObservation();
            observation.Occurrence.OccurrenceRemarks = "Sighting found in\r\nUppsala";
            var processedDarwinCoreRepositoryMock = CreateProcessedDarwinCoreRepositoryMock(observation);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            await dwcArchiveOccurrenceCsvWriter.CreateOccurrenceCsvFileAsync(
                memoryStream,
                fieldDescriptions,
                processedDarwinCoreRepositoryMock.Object,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            dynamic record = ReadCsvFile(memoryStream).First();
            ((string)record.occurrenceRemarks).Should().Be("Sighting found in Uppsala");
        }

        private static List<dynamic> ReadCsvFile(MemoryStream memoryStream)
        {
            using var readMemoryStream = new MemoryStream(memoryStream.ToArray());
            using var streamReader = new StreamReader(readMemoryStream);
            using var csvReader = new CsvReader(streamReader);
            SetCsvConfigurations(csvReader);
            var records = csvReader.GetRecords<dynamic>().ToList();
            return records;
        }


        private static void SetCsvConfigurations(CsvReader csv)
        {
            csv.Configuration.HasHeaderRecord = true;
            csv.Configuration.Delimiter = "\t";
            csv.Configuration.Encoding = System.Text.Encoding.UTF8;
            csv.Configuration.BadDataFound = x => { Console.WriteLine($"Bad data: <{x.RawRecord}>"); };
        }
    }
}