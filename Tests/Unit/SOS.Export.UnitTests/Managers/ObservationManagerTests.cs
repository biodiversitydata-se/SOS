using System;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Export.Managers;
using SOS.Export.Repositories.Interfaces;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using Xunit;

namespace SOS.Export.UnitTests.Managers
{
    /// <summary>
    /// Tests for observation manager
    /// </summary>
    public class ObservationManagerTests
    {
        private readonly Mock<IDwcArchiveFileWriter> _dwcArchiveFileWriterMock;
        private readonly Mock<IProcessedObservationRepository> _processedObservationRepositoryMock;
        private readonly Mock<IProcessInfoRepository> _processInfoRepositoryMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly Mock<IBlobStorageService> _blobStorageService;
        private readonly Mock<IZendToService> _zendToServiceMock;
        private readonly Mock<ILogger<ObservationManager>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public ObservationManagerTests()
        {
            _dwcArchiveFileWriterMock = new Mock<IDwcArchiveFileWriter>();
            _processedObservationRepositoryMock = new Mock<IProcessedObservationRepository>();
            _processInfoRepositoryMock = new Mock<IProcessInfoRepository>();
            _fileServiceMock = new Mock<IFileService>();
            _blobStorageService = new Mock<IBlobStorageService>();
            _zendToServiceMock = new Mock<IZendToService>();
            _loggerMock = new Mock<ILogger<ObservationManager>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public void ConstructorTest()
        {
            new ObservationManager(
                _dwcArchiveFileWriterMock.Object,
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _fileServiceMock.Object,
                _blobStorageService.Object,
                _zendToServiceMock.Object,
                new FileDestination { Path = "test" },
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new ObservationManager(
                null,
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _fileServiceMock.Object,
                _blobStorageService.Object,
                _zendToServiceMock.Object,
                new FileDestination { Path = "test" },
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("dwcArchiveFileWriter");

            create = () => new ObservationManager(
                _dwcArchiveFileWriterMock.Object,
                null,
                _processInfoRepositoryMock.Object,
                _fileServiceMock.Object,
                _blobStorageService.Object,
                _zendToServiceMock.Object,
                new FileDestination { Path = "test" },
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processedObservationRepository");

            create = () => new ObservationManager(
                _dwcArchiveFileWriterMock.Object,
                _processedObservationRepositoryMock.Object,
                null,
                _fileServiceMock.Object,
                _blobStorageService.Object,
                _zendToServiceMock.Object,
                new FileDestination { Path = "test" },
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processInfoRepository");

            create = () => new ObservationManager(
                _dwcArchiveFileWriterMock.Object,
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                null,
                _blobStorageService.Object,
                _zendToServiceMock.Object,
                new FileDestination { Path = "test" },
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("fileService");

            create = () => new ObservationManager(
                _dwcArchiveFileWriterMock.Object,
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _fileServiceMock.Object,
                null,
                _zendToServiceMock.Object,
                new FileDestination { Path = "test" },
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("blobStorageService");


            create = () => new ObservationManager(
                _dwcArchiveFileWriterMock.Object,
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _fileServiceMock.Object,
                _blobStorageService.Object,
                null,
                new FileDestination { Path = "test" },
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("zendToService");

            create = () => new ObservationManager(
                _dwcArchiveFileWriterMock.Object,
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _fileServiceMock.Object,
                _blobStorageService.Object,
                _zendToServiceMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("fileDestination");

            create = () => new ObservationManager(
                _dwcArchiveFileWriterMock.Object,
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _fileServiceMock.Object,
                _blobStorageService.Object,
                _zendToServiceMock.Object,
                new FileDestination { Path = "test" },
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Make a successful test of aggregation
        /// </summary>
        /// <returns></returns>

        /// <summary>
        /// Test aggregation fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExportAllAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _zendToServiceMock.Setup(blss => blss.SendFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var observationManager = new ObservationManager(
                _dwcArchiveFileWriterMock.Object,
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _fileServiceMock.Object,
                _blobStorageService.Object,
                _zendToServiceMock.Object,
                new FileDestination { Path = "test" },
                _loggerMock.Object);

            var result = await observationManager.ExportAllAsync(JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }
    }
}
