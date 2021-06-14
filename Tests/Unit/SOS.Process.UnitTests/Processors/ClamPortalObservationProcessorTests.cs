using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using NetTopologySuite.Geometries;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.ClamPortal;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Managers.Interfaces;
using SOS.Process.Processors.ClamPortal;
using Xunit;

namespace SOS.Process.UnitTests.Processors
{
    /// <summary>
    ///     Tests for Clam Portal processor
    /// </summary>
    public class ClamPortalObservationProcessorTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public ClamPortalObservationProcessorTests()
        {
            _clamObservationVerbatimRepositoryMock = new Mock<IClamObservationVerbatimRepository>();
            _areaHelper = new Mock<IAreaHelper>();
            _processedObservationRepositoryMock = new Mock<IProcessedPublicObservationRepository>();
            _vocabularyResolverMock = new Mock<IVocabularyValueResolver>();
            _dwcArchiveFileWriterCoordinatorMock = new Mock<IDwcArchiveFileWriterCoordinator>();
            _processManagerMock = new Mock<IProcessManager>();
            _validationManagerMock = new Mock<IValidationManager>();
            _geometryManagerMock = new Mock<IGeometryManager>();
            _loggerMock = new Mock<ILogger<ClamPortalObservationProcessor>>();
        }

        private readonly Mock<IClamObservationVerbatimRepository> _clamObservationVerbatimRepositoryMock;
        private readonly Mock<IAreaHelper> _areaHelper;
        private readonly Mock<IProcessedPublicObservationRepository> _processedObservationRepositoryMock;
        private readonly Mock<IVocabularyValueResolver> _vocabularyResolverMock;
        private readonly Mock<IDwcArchiveFileWriterCoordinator> _dwcArchiveFileWriterCoordinatorMock;
        private readonly Mock<IProcessManager> _processManagerMock;
        private readonly Mock<IValidationManager> _validationManagerMock;
        private readonly Mock<IGeometryManager> _geometryManagerMock;
        private readonly Mock<ILogger<ClamPortalObservationProcessor>> _loggerMock;

        private ClamPortalObservationProcessor TestObject => new ClamPortalObservationProcessor(
            _clamObservationVerbatimRepositoryMock.Object,
            _areaHelper.Object,
            _processedObservationRepositoryMock.Object,
            _vocabularyResolverMock.Object, 
            _dwcArchiveFileWriterCoordinatorMock.Object,
            _processManagerMock.Object,
            _validationManagerMock.Object,
            _geometryManagerMock.Object,
            _loggerMock.Object);

        private DataProvider CreateDataProvider()
        {
            return new DataProvider
            {
                Names = new[] { new VocabularyValueTranslation { CultureCode = "en-GB", Value = "Clam portal" } },
                Type = DataProviderType.ClamPortalObservations
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
            _clamObservationVerbatimRepositoryMock.Setup(r => r.GetBatchAsync(It.IsAny<int>()))
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
        [Fact(Skip = "Not working")]
        public async Task ProcessAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _clamObservationVerbatimRepositoryMock.Setup(r => r.GetBatchAsync(It.IsAny<int>()))
                .ReturnsAsync(new[]
                {
                    new ClamObservationVerbatim
                    {
                        DyntaxaTaxonId = 0
                    }
                });

            _areaHelper.Setup(r => r.AddAreaDataToProcessedObservations(It.IsAny<IEnumerable<Observation>>()));

            _processedObservationRepositoryMock
                .Setup(r => r.AddManyAsync(It.IsAny<ICollection<Observation>>()))
                .ReturnsAsync(1);

            _geometryManagerMock.Setup(g => g.GetCircleAsync(It.IsAny<Point>(), It.IsAny<int?>()))
                .ReturnsAsync(null as Polygon);

            var taxa = new Dictionary<int, Taxon>
            {
                {0, new Taxon {Id = 0, TaxonId = "taxon:0", ScientificName = "Biota"}}
            };
            var dataProvider = CreateDataProvider();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.ProcessAsync(dataProvider, taxa,  JobRunModes.Full, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(RunStatus.Success);
        }
    }
}