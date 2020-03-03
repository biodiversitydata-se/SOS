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
        private readonly Mock<IProcessedSightingRepository> _processedSightingRepositoryMock;
        private readonly Mock<IProcessInfoRepository> _processInfoRepositoryMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly Mock<IBlobStorageService> _blobStorageServiceMock;
        private readonly Mock<ILogger<SightingFactory>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public SightingFactoryTests()
        {
            _dwcArchiveFileWriterMock = new Mock<IDwcArchiveFileWriter>();
            _processedSightingRepositoryMock = new Mock<IProcessedSightingRepository>();
            _processInfoRepositoryMock = new Mock<IProcessInfoRepository>();
            _fileServiceMock = new Mock<IFileService>();
            _blobStorageServiceMock = new Mock<IBlobStorageService>();
            _loggerMock = new Mock<ILogger<SightingFactory>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public void ConstructorTest()
        {
            new SightingFactory(
                _dwcArchiveFileWriterMock.Object,
                _processedSightingRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _fileServiceMock.Object,
                _blobStorageServiceMock.Object,
                new FileDestination { Path = "test" },
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new SightingFactory(
                null,
                _processedSightingRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _fileServiceMock.Object,
                _blobStorageServiceMock.Object,
                new FileDestination { Path = "test" },
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("dwcArchiveFileWriter");

            create = () => new SightingFactory(
                _dwcArchiveFileWriterMock.Object,
                null,
                _processInfoRepositoryMock.Object,
                _fileServiceMock.Object,
                _blobStorageServiceMock.Object,
                new FileDestination { Path = "test" },
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processedSightingRepository");

            create = () => new SightingFactory(
                _dwcArchiveFileWriterMock.Object,
                _processedSightingRepositoryMock.Object,
                null,
                _fileServiceMock.Object,
                _blobStorageServiceMock.Object,
                new FileDestination { Path = "test" },
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processInfoRepository");

            create = () => new SightingFactory(
                _dwcArchiveFileWriterMock.Object,
                _processedSightingRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                null,
                _blobStorageServiceMock.Object,
                new FileDestination { Path = "test" },
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("fileService");

            create = () => new SightingFactory(
                _dwcArchiveFileWriterMock.Object,
                _processedSightingRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _fileServiceMock.Object,
                null,
                new FileDestination { Path = "test" },
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("blobStorageService");

            create = () => new SightingFactory(
                _dwcArchiveFileWriterMock.Object,
                _processedSightingRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _fileServiceMock.Object,
                _blobStorageServiceMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("fileDestination");

            create = () => new SightingFactory(
                _dwcArchiveFileWriterMock.Object,
                _processedSightingRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _fileServiceMock.Object,
                _blobStorageServiceMock.Object,
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
            _blobStorageServiceMock.Setup(blss => blss.CreateContainerAsync(It.IsAny<string>())).Throws(new Exception());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var sightingFactory = new SightingFactory(
                _dwcArchiveFileWriterMock.Object,
                _processedSightingRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _fileServiceMock.Object,
                _blobStorageServiceMock.Object,
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
