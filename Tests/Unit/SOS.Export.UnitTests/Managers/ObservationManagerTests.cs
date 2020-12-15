using System;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Export.Managers;
using SOS.Export.Managers.Interfaces;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Services.Interfaces;
using Xunit;

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
            _processedObservationRepositoryMock = new Mock<IProcessedObservationRepository>();
            _processInfoRepositoryMock = new Mock<IProcessInfoRepository>();
            _fileServiceMock = new Mock<IFileService>();
            _blobStorageServiceMock = new Mock<IBlobStorageService>();
            _zendToServiceMock = new Mock<IZendToService>();
            _filterManagerMock = new Mock<IFilterManager>();
            _loggerMock = new Mock<ILogger<ObservationManager>>();
        }

        private readonly Mock<IDwcArchiveFileWriter> _dwcArchiveFileWriterMock;
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
                    It.IsAny<FilterBase>(),
                    It.IsAny<string>(),
                    _processedObservationRepositoryMock.Object,
                    It.IsAny<ProcessInfo>(),
                    It.IsAny<string>(),
                    JobCancellationToken.Null
                )
            ).ReturnsAsync("filePath");

            _zendToServiceMock.Setup(blss => blss.SendFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.ExportAndSendAsync(It.IsAny<SearchFilter>(), It.IsAny<string>(),
                JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }

        /// <summary>
        ///     Make a successful test of export all
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExportAndSendAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _processInfoRepositoryMock.Setup(pir => pir.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new ProcessInfo("id", DateTime.Now));

            _dwcArchiveFileWriterMock.Setup(daf => daf.CreateDwcArchiveFileAsync(
                    DataProvider.FilterSubsetDataProvider, 
                    It.IsAny<FilterBase>(),
                    It.IsAny<string>(),
                    _processedObservationRepositoryMock.Object,
                    It.IsAny<ProcessInfo>(),
                    It.IsAny<string>(),
                    JobCancellationToken.Null
                )
            ).ReturnsAsync("filePath");

            _zendToServiceMock.Setup(blss => blss.SendFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.ExportAndSendAsync(It.IsAny<SearchFilter>(), It.IsAny<string>(),
                JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
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
            var result = await TestObject.ExportAndSendAsync(It.IsAny<SearchFilter>(), It.IsAny<string>(),
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
                    It.IsAny<FilterBase>(),
                    It.IsAny<string>(),
                    _processedObservationRepositoryMock.Object,
                    It.IsAny<ProcessInfo>(),
                    It.IsAny<string>(),
                    JobCancellationToken.Null
                )
            ).ReturnsAsync("filePath");

            _blobStorageServiceMock.Setup(bss => bss.CreateContainerAsync(It.IsAny<string>()));
            _blobStorageServiceMock.Setup(bss => bss.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            _fileServiceMock.Setup(blss => blss.DeleteFile(It.IsAny<string>()));

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.ExportAndStoreAsync(null, It.IsAny<string>(), It.IsAny<string>(), 
                JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }

        /// <summary>
        ///     Make a successful test of export all
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExportAndStoreAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _processInfoRepositoryMock.Setup(pir => pir.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new ProcessInfo("id", DateTime.Now));

            _dwcArchiveFileWriterMock.Setup(daf => daf.CreateDwcArchiveFileAsync(
                    DataProvider.FilterSubsetDataProvider, 
                    It.IsAny<FilterBase>(),
                    It.IsAny<string>(),
                    _processedObservationRepositoryMock.Object,
                    It.IsAny<ProcessInfo>(),
                    It.IsAny<string>(),
                    JobCancellationToken.Null
                )
            ).ReturnsAsync("filePath");

            _blobStorageServiceMock.Setup(bss => bss.CreateContainerAsync(It.IsAny<string>()));
            _blobStorageServiceMock.Setup(bss => bss.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _fileServiceMock.Setup(blss => blss.DeleteFile(It.IsAny<string>()));

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.ExportAndStoreAsync(null, It.IsAny<string>(), It.IsAny<string>(), JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
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
            var result = await TestObject.ExportAndStoreAsync(null, It.IsAny<string>(), It.IsAny<string>(), 
                JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }
    }
}