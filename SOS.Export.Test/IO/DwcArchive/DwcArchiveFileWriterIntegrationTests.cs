using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Export.Factories;
using SOS.Export.IO.DwcArchive;
using SOS.Export.MongoDb;
using SOS.Export.Repositories;
using SOS.Export.Services;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Search;
using Xunit;

namespace SOS.Export.Test.IO.DwcArchive
{
    public class DwcArchiveFileWriterIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category","Integration")]
        [Trait("Category", "DwcArchiveIntegration")]
        public async Task CreateDwcArchiveFile_ForAllProcessedObservationsInMongoDb()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var exportConfiguration = GetExportConfiguration();
            string exportFolderPath = exportConfiguration.FileDestination.Path;
            var exportClient = new ExportClient(
                exportConfiguration.MongoDbConfiguration.GetMongoDbSettings(),
                exportConfiguration.MongoDbConfiguration.DatabaseName,
                exportConfiguration.MongoDbConfiguration.BatchSize);
            var processedDarwinCoreRepository = new ProcessedDarwinCoreRepository(
                exportClient,
                new Mock<ILogger<ProcessedDarwinCoreRepository>>().Object);
            DwcArchiveFileWriter dwcArchiveFileWriter = new DwcArchiveFileWriter(
                new DwcArchiveOccurrenceCsvWriter(new Mock<ILogger<DwcArchiveOccurrenceCsvWriter>>().Object),
                new ExtendedMeasurementOrFactCsvWriter(new Mock<ILogger<ExtendedMeasurementOrFactCsvWriter>>().Object),
                new FileService(), 
                new Mock<ILogger<DwcArchiveFileWriter>>().Object);
            var processInfoRepository = new ProcessInfoRepository(exportClient, new Mock<ILogger<ProcessInfoRepository>>().Object);
            var processInfo = await processInfoRepository.GetAsync(processInfoRepository.ActiveInstance);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var zipFilePath = await dwcArchiveFileWriter.CreateDwcArchiveFileAsync(
                new AdvancedFilter(),
                "fileName",
                processedDarwinCoreRepository,
                processInfo,
                exportFolderPath,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            bool fileExists = System.IO.File.Exists(zipFilePath);
            fileExists.Should().BeTrue("because the zip file should have been generated");
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Category", "DwcArchiveIntegration")]
        public async Task CreateDwcArchiveFile_ForTenTestObservations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var exportConfiguration = GetExportConfiguration();
            string exportFolderPath = exportConfiguration.FileDestination.Path;
            var processedDarwinCoreRepositoryMock = CreateProcessedDarwinCoreRepositoryMock(@"Resources\TenProcessedTestObservations.json");
            DwcArchiveFileWriter dwcArchiveFileWriter = new DwcArchiveFileWriter(
                new DwcArchiveOccurrenceCsvWriter(new Mock<ILogger<DwcArchiveOccurrenceCsvWriter>>().Object),
                new ExtendedMeasurementOrFactCsvWriter(new Mock<ILogger<ExtendedMeasurementOrFactCsvWriter>>().Object),
                new FileService(),
                new Mock<ILogger<DwcArchiveFileWriter>>().Object);
            
            var exportClient = new ExportClient(
                exportConfiguration.MongoDbConfiguration.GetMongoDbSettings(),
                exportConfiguration.MongoDbConfiguration.DatabaseName,
                exportConfiguration.MongoDbConfiguration.BatchSize);
            var processInfoRepository = new ProcessInfoRepository(exportClient, new Mock<ILogger<ProcessInfoRepository>>().Object);
            var processInfo = await processInfoRepository.GetAsync(processInfoRepository.ActiveInstance);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var zipFilePath = await dwcArchiveFileWriter.CreateDwcArchiveFileAsync(
                new AdvancedFilter(),
                "fileName",
                processedDarwinCoreRepositoryMock.Object,
                processInfo,
                exportFolderPath,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            bool fileExists = System.IO.File.Exists(zipFilePath);
            fileExists.Should().BeTrue("because the zip file should have been generated");
        }
    }
}