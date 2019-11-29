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
            var processedDarwinCoreRepository = new ProcessedDarwinCoreRepository(
                new ExportClient(
                    exportConfiguration.MongoDbConfiguration.GetMongoDbSettings(),
                    exportConfiguration.MongoDbConfiguration.DatabaseName,
                    exportConfiguration.MongoDbConfiguration.BatchSize),
                new Mock<ILogger<ProcessedDarwinCoreRepository>>().Object);
            DwcArchiveFileWriter dwcArchiveFileWriter = new DwcArchiveFileWriter(
                new DwcArchiveOccurrenceCsvWriter(
                    new Mock<ILogger<DwcArchiveOccurrenceCsvWriter>>().Object), 
                    new FileService(), 
                    new Mock<ILogger<DwcArchiveFileWriter>>().Object);

            
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var zipFilePath = await dwcArchiveFileWriter.CreateDwcArchiveFileAsync(
                processedDarwinCoreRepository,
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
                new DwcArchiveOccurrenceCsvWriter(
                    new Mock<ILogger<DwcArchiveOccurrenceCsvWriter>>().Object),
                    new FileService(),
                    new Mock<ILogger<DwcArchiveFileWriter>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var zipFilePath = await dwcArchiveFileWriter.CreateDwcArchiveFileAsync(
                processedDarwinCoreRepositoryMock.Object,
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