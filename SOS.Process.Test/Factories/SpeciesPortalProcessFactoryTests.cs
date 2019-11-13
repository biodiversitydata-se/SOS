using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Process.Factories;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Verbatim.SpeciesPortal;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;
using Xunit;

namespace SOS.Process.Test.Factories
{
    /// <summary>
    /// Tests for sighting factory
    /// </summary>
    public class SpeciesPortalProcessFactoryTests
    {
        private readonly Mock<ISpeciesPortalVerbatimRepository> _speciesPortalVerbatimRepository;
        private readonly Mock<IProcessedRepository> _processedRepository;
        private readonly Mock<ILogger<SpeciesPortalProcessFactory>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public SpeciesPortalProcessFactoryTests()
        {
            _speciesPortalVerbatimRepository = new Mock<ISpeciesPortalVerbatimRepository>();
            _processedRepository = new Mock<IProcessedRepository>();
            _loggerMock = new Mock<ILogger<SpeciesPortalProcessFactory>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new SpeciesPortalProcessFactory(
                _speciesPortalVerbatimRepository.Object,
                _processedRepository.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new SpeciesPortalProcessFactory(
                null,
                _processedRepository.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("speciesPortalVerbatimRepository");

            create = () => new SpeciesPortalProcessFactory(
                _speciesPortalVerbatimRepository.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processedRepository");

            create = () => new SpeciesPortalProcessFactory(
                _speciesPortalVerbatimRepository.Object,
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
            _speciesPortalVerbatimRepository.Setup(r => r.GetBatchAsync(0))
                .ReturnsAsync(new [] { new APSightingVerbatim
                {
                    Id = 1
                } });

            _processedRepository.Setup(r => r.AddManyAsync(It.IsAny<IEnumerable<DarwinCore<DynamicProperties>>>()))
                .ReturnsAsync(true);

            var taxa = new Dictionary<int, DarwinCoreTaxon>
            {
                { 0, new DarwinCoreTaxon { TaxonID = "0", ScientificName = "Biota" } }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var speciesPortalProcessFactory = new SpeciesPortalProcessFactory(
                _speciesPortalVerbatimRepository.Object,
                _processedRepository.Object,
                _loggerMock.Object);

            var result = await speciesPortalProcessFactory.ProcessAsync(taxa);
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
            var speciesPortalProcessFactory = new SpeciesPortalProcessFactory(
                _speciesPortalVerbatimRepository.Object,
                _processedRepository.Object,
                _loggerMock.Object);

            var result = await speciesPortalProcessFactory.ProcessAsync(null);
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
            _speciesPortalVerbatimRepository.Setup(r => r.GetBatchAsync(0))
                .ThrowsAsync(new Exception("Failed"));
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var speciesPortalProcessFactory = new SpeciesPortalProcessFactory(
                _speciesPortalVerbatimRepository.Object,
                _processedRepository.Object,
                _loggerMock.Object);

            var result = await speciesPortalProcessFactory.ProcessAsync(null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }
    }
}
