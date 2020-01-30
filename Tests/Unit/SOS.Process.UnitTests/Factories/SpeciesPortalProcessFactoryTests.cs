using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Enums;
using  SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.SpeciesPortal;
using SOS.Process.Factories;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;
using Xunit;

namespace SOS.Process.UnitTests.Factories
{
    /// <summary>
    /// Tests for sighting factory
    /// </summary>
    public class SpeciesPortalProcessFactoryTests
    {
        private readonly Mock<ISpeciesPortalVerbatimRepository> _speciesPortalVerbatimRepository;
        private readonly Mock<IProcessedSightingRepository> _processedSightingRepositoryMock;
        private readonly Mock<ILogger<SpeciesPortalProcessFactory>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public SpeciesPortalProcessFactoryTests()
        {
            _speciesPortalVerbatimRepository = new Mock<ISpeciesPortalVerbatimRepository>();
            _processedSightingRepositoryMock = new Mock<IProcessedSightingRepository>();
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
                _processedSightingRepositoryMock.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new SpeciesPortalProcessFactory(
                null,
                _processedSightingRepositoryMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("speciesPortalVerbatimRepository");

            create = () => new SpeciesPortalProcessFactory(
                _speciesPortalVerbatimRepository.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("DarwinCoreRepository");

            create = () => new SpeciesPortalProcessFactory(
                _speciesPortalVerbatimRepository.Object,
                _processedSightingRepositoryMock.Object,
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

            _processedSightingRepositoryMock.Setup(r => r.AddManyAsync(It.IsAny<ICollection<ProcessedSighting>>()))
                .ReturnsAsync(1);

            var taxa = new Dictionary<int, ProcessedTaxon>
            {
                { 0, new ProcessedTaxon { Id = 0, TaxonId = "0", ScientificName = "Biota" } }
            };

            var fieldMappingById = new Dictionary<int, FieldMapping>
            {
                {0, new FieldMapping {Id = 0, Name = "ActivityId"}}
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var speciesPortalProcessFactory = new SpeciesPortalProcessFactory(
                _speciesPortalVerbatimRepository.Object,
                _processedSightingRepositoryMock.Object,
                _loggerMock.Object);

            var result = await speciesPortalProcessFactory.ProcessAsync(taxa, fieldMappingById, JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Status.Should().Be(RunStatus.Success);
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
                _processedSightingRepositoryMock.Object,
                _loggerMock.Object);

            var result = await speciesPortalProcessFactory.ProcessAsync(null, null, JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Status.Should().Be(RunStatus.Failed);
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
                _processedSightingRepositoryMock.Object,
                _loggerMock.Object);

            var result = await speciesPortalProcessFactory.ProcessAsync(null, null, JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Status.Should().Be(RunStatus.Failed);
        }
    }
}
