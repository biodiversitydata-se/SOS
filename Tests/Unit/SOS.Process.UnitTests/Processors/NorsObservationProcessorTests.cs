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
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Nors;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Managers.Interfaces;
using SOS.Process.Processors.Nors;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;
using Xunit;

namespace SOS.Process.UnitTests.Processors
{
    /// <summary>
    ///     Tests for Clam Portal processor
    /// </summary>
    public class NorsObservationProcessorTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public NorsObservationProcessorTests()
        {
            _norsObservationVerbatimRepositoryMock = new Mock<INorsObservationVerbatimRepository>();
            _areaHelper = new Mock<IAreaHelper>();
            _processedObservationRepositoryMock = new Mock<IProcessedObservationRepository>();
            _fieldMappingResolverHelperMock = new Mock<IFieldMappingResolverHelper>();
            _dwcArchiveFileWriterCoordinatorMock = new Mock<IDwcArchiveFileWriterCoordinator>();
            _validationManagerMock = new Mock<IValidationManager>();
            _loggerMock = new Mock<ILogger<NorsObservationProcessor>>();
        }

        private readonly Mock<INorsObservationVerbatimRepository> _norsObservationVerbatimRepositoryMock;
        private readonly Mock<IAreaHelper> _areaHelper;
        private readonly Mock<IProcessedObservationRepository> _processedObservationRepositoryMock;
        private readonly Mock<IFieldMappingResolverHelper> _fieldMappingResolverHelperMock;
        private readonly Mock<IDwcArchiveFileWriterCoordinator> _dwcArchiveFileWriterCoordinatorMock;
        private readonly Mock<IValidationManager> _validationManagerMock;
        private readonly Mock<ILogger<NorsObservationProcessor>> _loggerMock;

        private NorsObservationProcessor TestObject => new NorsObservationProcessor(
            _norsObservationVerbatimRepositoryMock.Object,
            _areaHelper.Object,
            _processedObservationRepositoryMock.Object,
            _fieldMappingResolverHelperMock.Object,
            _dwcArchiveFileWriterCoordinatorMock.Object,
            _validationManagerMock.Object,
            _loggerMock.Object);

        private DataProvider CreateDataProvider()
        {
            return new DataProvider
            {
                Name = "NORS",
                Type = DataProviderType.NorsObservations
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
            var result = await TestObject.ProcessAsync(dataProvider, null, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(RunStatus.Failed);
        }

        // todo - delete test?
        // This test doesn't add any value to the unit test suite due to the following reasons:
        // 1) The constructor is always invoked by dependency injection, which means that this test adds no protection against regressions (bugs).
        // 2) This test, tests the code implementation details and not the behavior of the system.
        //
        ///// <summary>
        /////     Test constructor
        ///// </summary>
        //[Fact]
        //public void ConstructorTest()
        //{
        //    TestObject.Should().NotBeNull();

        //    Action create = () => new NorsObservationProcessor(
        //        null,
        //        _areaHelper.Object,
        //        _processedObservationRepositoryMock.Object,
        //        _fieldMappingResolverHelperMock.Object, TODO,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should()
        //        .Be("norsObservationVerbatimRepository");


        //    create = () => new NorsObservationProcessor(
        //        _norsObservationVerbatimRepositoryMock.Object,
        //        null,
        //        _processedObservationRepositoryMock.Object,
        //        _fieldMappingResolverHelperMock.Object, TODO,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("areaHelper");

        //    create = () => new NorsObservationProcessor(
        //        _norsObservationVerbatimRepositoryMock.Object,
        //        _areaHelper.Object,
        //        null,
        //        _fieldMappingResolverHelperMock.Object, TODO,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processedObservationRepository");

        //    create = () => new NorsObservationProcessor(
        //        _norsObservationVerbatimRepositoryMock.Object,
        //        _areaHelper.Object,
        //        _processedObservationRepositoryMock.Object,
        //        _fieldMappingResolverHelperMock.Object, TODO,
        //        null);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        //}

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
            _norsObservationVerbatimRepositoryMock.Setup(r => r.GetAllByCursorAsync())
                .ThrowsAsync(new Exception("Failed"));

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.ProcessAsync(dataProvider, null, JobCancellationToken.Null);

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
            var mockCursor = new Mock<IAsyncCursor<NorsObservationVerbatim>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<NorsObservationVerbatim>());
            mockCursor
                .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Returns(Task.FromResult(false));

            _norsObservationVerbatimRepositoryMock.Setup(r => r.GetAllByCursorAsync())
                .ReturnsAsync(mockCursor.Object);

            _areaHelper.Setup(r => r.AddAreaDataToProcessedObservations(It.IsAny<IEnumerable<ProcessedObservation>>()));

            _processedObservationRepositoryMock.Setup(r => r.DeleteProviderDataAsync(It.IsAny<DataProvider>()))
                .ReturnsAsync(true);

            _processedObservationRepositoryMock
                .Setup(r => r.AddManyAsync(It.IsAny<ICollection<ProcessedObservation>>()))
                .ReturnsAsync(1);

            var taxa = new Dictionary<int, ProcessedTaxon>
            {
                {0, new ProcessedTaxon {Id = 0, TaxonId = "taxon:0", ScientificName = "Biota"}}
            };
            var dataProvider = CreateDataProvider();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.ProcessAsync(dataProvider, taxa, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(RunStatus.Success);
        }
    }
}