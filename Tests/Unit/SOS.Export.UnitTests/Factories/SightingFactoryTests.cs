using System;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Export.Factories;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Export.Repositories.Interfaces;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using Xunit;

namespace SOS.Export.UnitTests.Factories
{
    /// <summary>
    /// Tests for sighting factory
    /// </summary>
    public class SightingFactoryTests
    {
        private readonly Mock<IDwcArchiveFileWriter> _dwcArchiveFileWriterMock;
        private readonly Mock<IProcessedObservationRepository> _processedObservationRepositoryMock;
        private readonly Mock<IProcessInfoRepository> _processInfoRepositoryMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly Mock<IBlobStorageService> _blobStorageService;
        private readonly Mock<IZendToService> _zendToServiceMock;
        private readonly Mock<ILogger<ObservationFactory>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public SightingFactoryTests()
        {
            _dwcArchiveFileWriterMock = new Mock<IDwcArchiveFileWriter>();
            _processedObservationRepositoryMock = new Mock<IProcessedObservationRepository>();
            _processInfoRepositoryMock = new Mock<IProcessInfoRepository>();
            _fileServiceMock = new Mock<IFileService>();
            _blobStorageService = new Mock<IBlobStorageService>();
            _zendToServiceMock = new Mock<IZendToService>();
            _loggerMock = new Mock<ILogger<ObservationFactory>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public void ConstructorTest()
        {
            new ObservationFactory(
                _dwcArchiveFileWriterMock.Object,
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _fileServiceMock.Object,
                _blobStorageService.Object,
                _zendToServiceMock.Object,
                new FileDestination { Path = "test" },
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new ObservationFactory(
                null,
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _fileServiceMock.Object,
                _blobStorageService.Object,
                _zendToServiceMock.Object,
                new FileDestination { Path = "test" },
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("dwcArchiveFileWriter");

            create = () => new ObservationFactory(
                _dwcArchiveFileWriterMock.Object,
                null,
                _processInfoRepositoryMock.Object,
                _fileServiceMock.Object,
                _blobStorageService.Object,
                _zendToServiceMock.Object,
                new FileDestination { Path = "test" },
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processedObservationRepository");

            create = () => new ObservationFactory(
                _dwcArchiveFileWriterMock.Object,
                _processedObservationRepositoryMock.Object,
                null,
                _fileServiceMock.Object,
                _blobStorageService.Object,
                _zendToServiceMock.Object,
                new FileDestination { Path = "test" },
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processInfoRepository");

            create = () => new ObservationFactory(
                _dwcArchiveFileWriterMock.Object,
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                null,
                _blobStorageService.Object,
                _zendToServiceMock.Object,
                new FileDestination { Path = "test" },
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("fileService");

            create = () => new ObservationFactory(
                _dwcArchiveFileWriterMock.Object,
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _fileServiceMock.Object,
                null,
                _zendToServiceMock.Object,
                new FileDestination { Path = "test" },
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("blobStorageService");


            create = () => new ObservationFactory(
                _dwcArchiveFileWriterMock.Object,
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _fileServiceMock.Object,
                _blobStorageService.Object,
                null,
                new FileDestination { Path = "test" },
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("zendToService");

            create = () => new ObservationFactory(
                _dwcArchiveFileWriterMock.Object,
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _fileServiceMock.Object,
                _blobStorageService.Object,
                _zendToServiceMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("fileDestination");

            create = () => new ObservationFactory(
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
            var sightingFactory = new ObservationFactory(
                _dwcArchiveFileWriterMock.Object,
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _fileServiceMock.Object,
                _blobStorageService.Object,
                _zendToServiceMock.Object,
                new FileDestination { Path = "test" },
                _loggerMock.Object);

            var result = await sightingFactory.ExportAllAsync(JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }
    }
}
