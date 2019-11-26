using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Export.Factories;
using SOS.Export.Mappings;
using SOS.Export.Models.DarwinCore;
using SOS.Export.Repositories.Interfaces;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Models.Processed.DarwinCore;
using Xunit;

namespace SOS.Export.Test.Factories
{
    /// <summary>
    /// Tests for sighting factory
    /// </summary>
    public class SightingFactoryTests
    {
        private readonly Mock<IProcessedDarwinCoreRepository> _processedDarwinCoreRepositoryMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly Mock<IBlobStorageService> _blobStorageServiceMock;
        private readonly Mock<ILogger<SightingFactory>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public SightingFactoryTests()
        {
            _processedDarwinCoreRepositoryMock = new Mock<IProcessedDarwinCoreRepository>();
            _fileServiceMock = new Mock<IFileService>();
            _blobStorageServiceMock = new Mock<IBlobStorageService>();
            _loggerMock = new Mock<ILogger<SightingFactory>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new SightingFactory(
                _processedDarwinCoreRepositoryMock.Object,
                _fileServiceMock.Object,
                _blobStorageServiceMock.Object,
                new FileDestination{Path = "test"},
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new SightingFactory(
                null,
                _fileServiceMock.Object,
                _blobStorageServiceMock.Object,
                new FileDestination { Path = "test" },
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processedDarwinCoreRepository");

            create = () => new SightingFactory(
                _processedDarwinCoreRepositoryMock.Object,
                null,
                _blobStorageServiceMock.Object,
                new FileDestination { Path = "test" },
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("fileService");

            create = () => new SightingFactory(
                _processedDarwinCoreRepositoryMock.Object,
                _fileServiceMock.Object,
                null,
                new FileDestination { Path = "test" },
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("blobStorageService");

            create = () => new SightingFactory(
                _processedDarwinCoreRepositoryMock.Object,
                _fileServiceMock.Object,
                _blobStorageServiceMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("fileDestination");

            create = () => new SightingFactory(
                _processedDarwinCoreRepositoryMock.Object,
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
        [Fact]
        public async Task ExportAllAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _processedDarwinCoreRepositoryMock.Setup(dwcr => dwcr.GetChunkAsync(0, It.IsAny<int>()))
                .ReturnsAsync(new[] { new DarwinCore<DynamicProperties> { 
                    DatasetID = "test", Taxon = new DarwinCoreTaxon
                    {
                        ScientificName = "Biota",
                        VernacularName = "Liv",
                        TaxonID = "0"
                    }
                } });

            _fileServiceMock.Setup(mdr => mdr.CreateFolder(It.IsAny<string>(), It.IsAny<string>()));
            _fileServiceMock.Setup(mdr => mdr.WriteToCsvFileAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IEnumerable<DwC>>(), It.IsAny<DwCMap>()));
            _fileServiceMock.Setup(mdr => mdr.WriteToCsvFileAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IEnumerable<DwCOccurrence>>(), It.IsAny<DwCOccurrenceMap>()));
            _fileServiceMock.Setup(mdr => mdr.WriteToCsvFileAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IEnumerable<DwCMaterialSample>>(), It.IsAny<DwCMaterialSampleMap>()));
            _fileServiceMock.Setup(mdr => mdr.WriteToCsvFileAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IEnumerable<DwCEvent>>(), It.IsAny<DwCEventMap>()));
            _fileServiceMock.Setup(mdr => mdr.WriteToCsvFileAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IEnumerable<DwCLocation>>(), It.IsAny<DwCLocationMap>()));
            _fileServiceMock.Setup(mdr => mdr.WriteToCsvFileAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IEnumerable<DwCGeologicalContext>>(), It.IsAny<DwCGeologicalContextMap>()));
            _fileServiceMock.Setup(mdr => mdr.WriteToCsvFileAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IEnumerable<DwCIdentification>>(), It.IsAny<DwCIdentificationMap>()));
            _fileServiceMock.Setup(mdr => mdr.WriteToCsvFileAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IEnumerable<DwCTaxon>>(), It.IsAny<DwCTaxonMap>()));
            _fileServiceMock.Setup(mdr => mdr.WriteToCsvFileAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IEnumerable<DwCMeasurementOrFact>>(), It.IsAny<DwCMeasurementOrFactMap>()));
            _fileServiceMock.Setup(mdr => mdr.WriteToCsvFileAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IEnumerable<DwCResourceRelationship>>(), It.IsAny<DwCResourceRelationshipMap>()));

            var meta = new XmlDocument();
            meta.Load(@".\DarwinCore\meta.xml");
            _fileServiceMock.Setup(mdr => mdr.GetXmlDocument(It.IsAny<string>())).Returns(meta);
            _fileServiceMock.Setup(mdr => mdr.SaveXmlDocument(It.IsAny<XmlDocument>(), It.IsAny<string>()));
            _fileServiceMock.Setup(mdr => mdr.CopyFiles(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>()));
            _fileServiceMock.Setup(mdr => mdr.CompressFolder(It.IsAny<string>(), It.IsAny<string>()));
            _fileServiceMock.Setup(mdr => mdr.DeleteFolder(It.IsAny<string>()));
            _fileServiceMock.Setup(mdr => mdr.DeleteFile(It.IsAny<string>()));

            _blobStorageServiceMock.Setup(mdr => mdr.CreateContainerAsync(It.IsAny<string>())).ReturnsAsync(true);
            _blobStorageServiceMock.Setup(mdr => mdr.UploadBlobAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var sightingFactory = new SightingFactory(
                _processedDarwinCoreRepositoryMock.Object,
                _fileServiceMock.Object,
                _blobStorageServiceMock.Object,
                new FileDestination { Path = "test" },
                _loggerMock.Object);

            var result = await sightingFactory.ExportAllAsync(JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
        }

        /// <summary>
        /// Test aggregation fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ExportAllAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _fileServiceMock.Setup(mdr => mdr.CreateFolder(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var sightingFactory = new SightingFactory(
                _processedDarwinCoreRepositoryMock.Object,
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
