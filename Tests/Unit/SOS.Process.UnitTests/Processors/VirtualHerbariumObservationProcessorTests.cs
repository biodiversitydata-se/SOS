using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.VirtualHerbarium;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Processors.VirtualHerbarium;
using Xunit;

namespace SOS.Process.UnitTests.Processors
{
    /// <summary>
    ///     Tests for Clam Portal processor
    /// </summary>
    public class VirtualHerbariumObservationProcessorTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public VirtualHerbariumObservationProcessorTests()
        {
            _virtualHerbariumObservationVerbatimRepositoryMock =
                new Mock<IVirtualHerbariumObservationVerbatimRepository>();
            _areaHelper = new Mock<IAreaHelper>();
            _processedObservationRepositoryMock = new Mock<IProcessedPublicObservationRepository>();
            _vocabularyResolverMock = new Mock<IVocabularyValueResolver>();
            _dwcArchiveFileWriterCoordinatorMock = new Mock<IDwcArchiveFileWriterCoordinator>();
            _validationManagerMock = new Mock<IValidationManager>();
            _loggerMock = new Mock<ILogger<VirtualHerbariumObservationProcessor>>();
        }

        private readonly Mock<IVirtualHerbariumObservationVerbatimRepository>
            _virtualHerbariumObservationVerbatimRepositoryMock;

        private readonly Mock<IAreaHelper> _areaHelper;
        private readonly Mock<IProcessedPublicObservationRepository> _processedObservationRepositoryMock;
        private readonly Mock<IVocabularyValueResolver> _vocabularyResolverMock;
        private readonly Mock<IDwcArchiveFileWriterCoordinator> _dwcArchiveFileWriterCoordinatorMock;
        private readonly Mock<IValidationManager> _validationManagerMock;
        private readonly Mock<ILogger<VirtualHerbariumObservationProcessor>> _loggerMock;

        private VirtualHerbariumObservationProcessor TestObject => new VirtualHerbariumObservationProcessor(
            _virtualHerbariumObservationVerbatimRepositoryMock.Object,
            _areaHelper.Object,
            _processedObservationRepositoryMock.Object,
            _vocabularyResolverMock.Object, 
            _dwcArchiveFileWriterCoordinatorMock.Object,
            _validationManagerMock.Object,
            _loggerMock.Object);

        private DataProvider CreateDataProvider()
        {
            return new DataProvider
            {
                Name = "Virtual Herbarium",
                Type = DataProviderType.VirtualHerbariumObservations
            };
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
            var dataprovider = CreateDataProvider();
            _processedObservationRepositoryMock.Setup(r => r.DeleteProviderDataAsync(It.IsAny<DataProvider>()))
                .ThrowsAsync(new Exception("Failed"));

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.ProcessAsync(dataprovider, null, JobRunModes.Full, JobCancellationToken.Null);

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
            var mockCursor = new Mock<IAsyncCursor<VirtualHerbariumObservationVerbatim>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<VirtualHerbariumObservationVerbatim>());
            mockCursor
                .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Returns(Task.FromResult(false));

            _virtualHerbariumObservationVerbatimRepositoryMock.Setup(r => r.GetAllByCursorAsync())
                .ReturnsAsync(mockCursor.Object);

            _areaHelper.Setup(r => r.AddAreaDataToProcessedObservations(It.IsAny<IEnumerable<Observation>>()));

            _processedObservationRepositoryMock.Setup(r => r.DeleteProviderDataAsync(It.IsAny<DataProvider>()))
                .ReturnsAsync(true);

            _processedObservationRepositoryMock
                .Setup(r => r.AddManyAsync(It.IsAny<ICollection<Observation>>()))
                .ReturnsAsync(1);

            var dataProvider = CreateDataProvider();
            var taxa = new Dictionary<int, Taxon>
            {
                {0, new Taxon {Id = 0, TaxonId = "taxon:0", ScientificName = "Biota"}}
            };

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