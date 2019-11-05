using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Process.Factories;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Verbatim.ClamTreePortal;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;
using Xunit;

namespace SOS.Process.Test.Factories
{
    /// <summary>
    /// Tests for sighting factory
    /// </summary>
    public class ClamTreePortalProcessFactoryTests
    {
        private readonly Mock<IClamObservationVerbatimRepository> _clamObservationVerbatimRepositoryMock;
        private readonly Mock<ITreeObservationVerbatimRepository> _treeObservationVerbatimRepositoryMock;
        private readonly Mock<IAreaHelper> _areaHelper;
        private readonly Mock<IProcessedRepository> _processedRepository;
        private readonly Mock<ILogger<ClamTreePortalProcessFactory>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public ClamTreePortalProcessFactoryTests()
        {
            _clamObservationVerbatimRepositoryMock = new Mock<IClamObservationVerbatimRepository>();
            _treeObservationVerbatimRepositoryMock = new Mock<ITreeObservationVerbatimRepository>();
            _areaHelper = new Mock<IAreaHelper>();
            _processedRepository = new Mock<IProcessedRepository>();
            _loggerMock = new Mock<ILogger<ClamTreePortalProcessFactory>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new ClamTreePortalProcessFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _treeObservationVerbatimRepositoryMock.Object,
                _areaHelper.Object,
                _processedRepository.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new ClamTreePortalProcessFactory(
                null,
                _treeObservationVerbatimRepositoryMock.Object,
                _areaHelper.Object,
                _processedRepository.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("clamObservationVerbatimRepository");

            create = () => new ClamTreePortalProcessFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                null,
                _areaHelper.Object,
                _processedRepository.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("treeObservationVerbatimRepository");

            create = () => new ClamTreePortalProcessFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _treeObservationVerbatimRepositoryMock.Object,
                null,
                _processedRepository.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("areaHelper");

            create = () => new ClamTreePortalProcessFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _treeObservationVerbatimRepositoryMock.Object,
                _areaHelper.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processedRepository");

            create = () => new ClamTreePortalProcessFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _treeObservationVerbatimRepositoryMock.Object,
                _areaHelper.Object,
                _processedRepository.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Make a successful test of processing
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ProcessAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _clamObservationVerbatimRepositoryMock.Setup(r => r.GetBatchAsync(0))
                .ReturnsAsync(new [] { new ClamObservationVerbatim 
                {
                    DyntaxaTaxonId = 0
                } });

            _treeObservationVerbatimRepositoryMock.Setup(r => r.GetBatchAsync(0))
                .ReturnsAsync(new[] { new TreeObservationVerbatim
                {
                    DyntaxaTaxonId = 0
                } });

            _areaHelper.Setup(r => r.AddAreaDataToDarwinCoreAsync(It.IsAny<IEnumerable<DarwinCore<DynamicProperties>>>()));

            _processedRepository.Setup(r => r.AddManyAsync(It.IsAny<IEnumerable<DarwinCore<DynamicProperties>>>()))
                .ReturnsAsync(true);

            var taxa = new Dictionary<string, DarwinCoreTaxon>
            {
                { "0", new DarwinCoreTaxon { TaxonID = "0", ScientificName = "Biota" } }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var clamTreePortalProcessFactory = new ClamTreePortalProcessFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _treeObservationVerbatimRepositoryMock.Object,
                _areaHelper.Object,
                _processedRepository.Object,
                _loggerMock.Object);

            var result = await clamTreePortalProcessFactory.ProcessAsync(taxa);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
        }

        /// <summary>
        /// Test processing fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AggregateAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------


            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var clamTreePortalProcessFactory = new ClamTreePortalProcessFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _treeObservationVerbatimRepositoryMock.Object,
                _areaHelper.Object,
                _processedRepository.Object,
                _loggerMock.Object);

            var result = await clamTreePortalProcessFactory.ProcessAsync(null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }

        /// <summary>
        /// Test processing exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ProcessAsyncException()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _clamObservationVerbatimRepositoryMock.Setup(r => r.GetBatchAsync(0))
                .ThrowsAsync(new Exception("Failed"));
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var clamTreePortalProcessFactory = new ClamTreePortalProcessFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _treeObservationVerbatimRepositoryMock.Object,
                _areaHelper.Object,
                _processedRepository.Object,
                _loggerMock.Object);

            var result = await clamTreePortalProcessFactory.ProcessAsync(null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }
    }
}
