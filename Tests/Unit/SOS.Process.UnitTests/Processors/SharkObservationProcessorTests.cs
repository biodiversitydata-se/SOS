using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shark;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Shark;
using Xunit;

namespace SOS.Process.UnitTests.Processors
{
    /// <summary>
    ///     Tests for Clam Portal processor
    /// </summary>
    public class SharkObservationProcessorTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public SharkObservationProcessorTests()
        {
            _sharkObservationVerbatimRepositoryMock = new Mock<ISharkObservationVerbatimRepository>();
            _areaHelper = new Mock<IAreaHelper>();
            _processedObservationRepository = new Mock<IProcessedObservationCoreRepository>();
            _vocabularyResolverMock = new Mock<IVocabularyValueResolver>();
            _dwcArchiveFileWriterCoordinatorMock = new Mock<IDwcArchiveFileWriterCoordinator>();
            _processManagerMock = new Mock<IProcessManager>();
            _validationManagerMock = new Mock<IValidationManager>();
            _diffusionManagerMock = new Mock<IDiffusionManager>();
            _processTimeManagerMock = new Mock<IProcessTimeManager>();
            _loggerMock = new Mock<ILogger<SharkObservationProcessor>>();
        }

        private readonly Mock<ISharkObservationVerbatimRepository> _sharkObservationVerbatimRepositoryMock;
        private readonly Mock<IAreaHelper> _areaHelper;
        private readonly Mock<IProcessedObservationCoreRepository> _processedObservationRepository;
        private readonly Mock<IVocabularyValueResolver> _vocabularyResolverMock;
        private readonly Mock<IDwcArchiveFileWriterCoordinator> _dwcArchiveFileWriterCoordinatorMock;
        private readonly Mock<IProcessManager> _processManagerMock;
        private readonly Mock<IValidationManager> _validationManagerMock;
        private readonly Mock<IDiffusionManager> _diffusionManagerMock;
        private readonly Mock<IProcessTimeManager> _processTimeManagerMock;
        private readonly Mock<ILogger<SharkObservationProcessor>> _loggerMock;

        private SharkObservationProcessor TestObject => new SharkObservationProcessor(
            _sharkObservationVerbatimRepositoryMock.Object,
            _areaHelper.Object,
            _processedObservationRepository.Object,
            _vocabularyResolverMock.Object, 
            _dwcArchiveFileWriterCoordinatorMock.Object,
            _processManagerMock.Object,
            _validationManagerMock.Object,
            _diffusionManagerMock.Object,
            _processTimeManagerMock.Object,
            new ProcessConfiguration(),
            _loggerMock.Object);

        private DataProvider CreateDataProvider()
        {
            return new DataProvider
            {
                Names = new[] { new VocabularyValueTranslation { CultureCode = "en-GB", Value = "SHARK" } },
                Type = DataProviderType.SharkObservations
            };
        }

        /// <summary>
        ///     Test processing fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AggregateAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dataProvider = CreateDataProvider();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.ProcessAsync(dataProvider, null, JobRunModes.Full, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(RunStatus.Failed);
        }

        /// <summary>
        ///     Test processing exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ProcessAsyncException()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dataProvider = CreateDataProvider();
            _sharkObservationVerbatimRepositoryMock.Setup(r => r.GetAllByCursorAsync())
                .ThrowsAsync(new Exception("Failed"));

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.ProcessAsync(dataProvider, null, JobRunModes.Full, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(RunStatus.Failed);
        }

        /// <summary>
        ///     Make a successful test of processing
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ProcessAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var mockCursor = new Mock<IAsyncCursor<SharkObservationVerbatim>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<SharkObservationVerbatim>());
            mockCursor
                .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Returns(Task.FromResult(false));

            _sharkObservationVerbatimRepositoryMock.Setup(r => r.GetAllByCursorAsync())
                .ReturnsAsync(mockCursor.Object);

            _areaHelper.Setup(r => r.AddAreaDataToProcessedLocations(It.IsAny<IEnumerable<Location>>()));

            _processedObservationRepository.Setup(r => r.DeleteProviderDataAsync(It.IsAny<DataProvider>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            
            _processedObservationRepository
                .Setup(r => r.AddManyAsync(It.IsAny<ICollection<Observation>>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(1);
           
            var taxa = new Dictionary<int, Taxon>
            {
                {0, new Taxon {Id = 0, TaxonId = "taxon:0", ScientificName = "Biota"}}
            };

            var dataProvider = CreateDataProvider();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.ProcessAsync(dataProvider, taxa, JobRunModes.Full, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(RunStatus.Success);
        }
    }
}