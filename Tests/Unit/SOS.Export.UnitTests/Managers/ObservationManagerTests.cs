using System;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Export.Managers;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Enums;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.IO.Excel.Interfaces;
using SOS.Lib.IO.GeoJson.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Services.Interfaces;
using Xunit;
using SOS.Lib.Models.Export;

namespace SOS.Export.UnitTests.Managers
{
    /// <summary>
    ///     Tests for observation manager
    /// </summary>
    public class ObservationManagerTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public ObservationManagerTests()
        {
            _dwcArchiveFileWriterMock = new Mock<IDwcArchiveFileWriter>();
            _excelFileWriter = new Mock<IExcelFileWriter>();
            _geoJsonFileWriter = new Mock<IGeoJsonFileWriter>();
            _csvFileWriter = new Mock<ICsvFileWriter>();
            _processedObservationRepositoryMock = new Mock<IProcessedObservationRepository>();
            _processInfoRepositoryMock = new Mock<IProcessInfoRepository>();
            _fileServiceMock = new Mock<IFileService>();
            _blobStorageServiceMock = new Mock<IBlobStorageService>();
            _zendToServiceMock = new Mock<IZendToService>();
            _filterManagerMock = new Mock<IFilterManager>();
            _loggerMock = new Mock<ILogger<ObservationManager>>();
        }

        private readonly Mock<IDwcArchiveFileWriter> _dwcArchiveFileWriterMock;
        private readonly Mock<IExcelFileWriter> _excelFileWriter;
        private readonly Mock<IGeoJsonFileWriter> _geoJsonFileWriter;
        private readonly Mock<ICsvFileWriter> _csvFileWriter;
        private readonly Mock<IProcessedObservationRepository> _processedObservationRepositoryMock;
        private readonly Mock<IProcessInfoRepository> _processInfoRepositoryMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly Mock<IBlobStorageService> _blobStorageServiceMock;
        private readonly Mock<IZendToService> _zendToServiceMock;
        private readonly Mock<IFilterManager> _filterManagerMock;
        private readonly Mock<ILogger<ObservationManager>> _loggerMock;

        /// <summary>
        ///     Return object to be tested
        /// </summary>
        private ObservationManager TestObject => new ObservationManager(
            _dwcArchiveFileWriterMock.Object,
            _excelFileWriter.Object,
            _geoJsonFileWriter.Object,
            _csvFileWriter.Object,
            _processedObservationRepositoryMock.Object,
            _processInfoRepositoryMock.Object,
            _fileServiceMock.Object,
            _blobStorageServiceMock.Object,
            _zendToServiceMock.Object,
            new FileDestination {Path = "test"},
            _filterManagerMock.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test export all fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExportAndSendAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _processInfoRepositoryMock.Setup(pir => pir.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new ProcessInfo("id", DateTime.Now));

            _dwcArchiveFileWriterMock.Setup(daf => daf.CreateDwcArchiveFileAsync(
                    DataProvider.FilterSubsetDataProvider,
                    It.IsAny<SearchFilterBase>(),
                    It.IsAny<string>(),
                    _processedObservationRepositoryMock.Object,
                    It.IsAny<ProcessInfo>(),
                    It.IsAny<string>(),
                    JobCancellationToken.Null
                )
            ).ReturnsAsync(new FileExportResult { FilePath = "filePath" });

            _zendToServiceMock.Setup(blss => blss.SendFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ExportFormat> ()))
                .ReturnsAsync(false);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.ExportAndSendAsync(It.IsAny<SearchFilter>(), It.IsAny<string>(), "", ExportFormat.DwC, "en-GB", false, OutputFieldSet.All, PropertyLabelType.PropertyPath, false,
                JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }

        /// <summary>
        ///     Test export all throws
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExportAndSendAsyncThrows()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _processInfoRepositoryMock.Setup(pir => pir.GetAsync(It.IsAny<string>()))
                .Throws(new Exception("Error"));

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.ExportAndSendAsync(It.IsAny<SearchFilter>(), It.IsAny<string>(), "", ExportFormat.DwC, "en-GB", false, OutputFieldSet.All, PropertyLabelType.PropertyPath, false,
                JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }

        /// <summary>
        ///     Test export all fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExportAndStoreAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _processInfoRepositoryMock.Setup(pir => pir.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new ProcessInfo("id", DateTime.Now));

            _dwcArchiveFileWriterMock.Setup(daf => daf.CreateDwcArchiveFileAsync(
                    DataProvider.FilterSubsetDataProvider,
                    It.IsAny<SearchFilterBase>(),
                    It.IsAny<string>(),
                    _processedObservationRepositoryMock.Object,
                    It.IsAny<ProcessInfo>(),
                    It.IsAny<string>(),
                    JobCancellationToken.Null
                )
            ).ReturnsAsync(new FileExportResult { FilePath = "filePath" });

            _blobStorageServiceMock.Setup(bss => bss.CreateContainerAsync(It.IsAny<string>()));
            _blobStorageServiceMock.Setup(bss => bss.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            _fileServiceMock.Setup(blss => blss.DeleteFile(It.IsAny<string>()));

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.ExportAndStoreAsync(null, It.IsAny<string>(), It.IsAny<string>(), "", 
                JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }             

        /// <summary>
        ///     Test export all throws
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExportAndStoreAsyncThrows()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _processInfoRepositoryMock.Setup(pir => pir.GetAsync(It.IsAny<string>()))
                .Throws(new Exception("Error"));

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.ExportAndStoreAsync(null, It.IsAny<string>(), It.IsAny<string>(), "", 
                JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }
    }
}