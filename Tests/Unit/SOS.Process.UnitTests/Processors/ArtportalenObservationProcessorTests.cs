using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Managers.Interfaces;
using SOS.Process.Processors.Artportalen;
using SOS.Process.Repositories.Source.Interfaces;
using Xunit;

namespace SOS.Process.UnitTests.Processors
{
    /// <summary>
    ///     Tests for Artportalen processor
    /// </summary>
    public class ArtportalenObservationProcessorTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public ArtportalenObservationProcessorTests()
        {
            _artportalenVerbatimRepository = new Mock<IArtportalenVerbatimRepository>();
            _processedObservationRepositoryMock = new Mock<IProcessedObservationRepository>();
            _processedFieldMappingRepositoryMock = new Mock<IProcessedFieldMappingRepository>();
            _fieldMappingResolverHelperMock = new Mock<IFieldMappingResolverHelper>();
            _processConfiguration = new ProcessConfiguration();
            _dwcArchiveFileWriterCoordinatorMock = new Mock<IDwcArchiveFileWriterCoordinator>();
            _validationManagerMock = new Mock<IValidationManager>();
            _loggerMock = new Mock<ILogger<ArtportalenObservationProcessor>>();
        }

        private readonly Mock<IArtportalenVerbatimRepository> _artportalenVerbatimRepository;
        private readonly Mock<IProcessedObservationRepository> _processedObservationRepositoryMock;
        private readonly Mock<IProcessedFieldMappingRepository> _processedFieldMappingRepositoryMock;
        private readonly Mock<IFieldMappingResolverHelper> _fieldMappingResolverHelperMock;
        private readonly ProcessConfiguration _processConfiguration;
        private readonly Mock<IDwcArchiveFileWriterCoordinator> _dwcArchiveFileWriterCoordinatorMock;
        private readonly Mock<IValidationManager> _validationManagerMock;
        private readonly Mock<ILogger<ArtportalenObservationProcessor>> _loggerMock;

        private ArtportalenObservationProcessor TestObject => new ArtportalenObservationProcessor(
            _artportalenVerbatimRepository.Object,
            _processedObservationRepositoryMock.Object,
            _processedFieldMappingRepositoryMock.Object,
            _fieldMappingResolverHelperMock.Object,
            _processConfiguration,
            _dwcArchiveFileWriterCoordinatorMock.Object,
            _validationManagerMock.Object,
            _loggerMock.Object);

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
            var dataProvider = new DataProvider
            {
                Name = "Artportalen",
                Type = DataProviderType.ArtportalenObservations
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.ProcessAsync(dataProvider, null, false, JobCancellationToken.Null);

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
            var dataProvider = new DataProvider
            {
                Name = "Artportalen",
                Type = DataProviderType.ArtportalenObservations
            };

            _artportalenVerbatimRepository.Setup(r => r.GetBatchAsync(0, 0))
                .ThrowsAsync(new Exception("Failed"));

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.ProcessAsync(dataProvider, null, false, JobCancellationToken.Null);
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
        ///// Test constructor
        ///// </summary>
        //[Fact]
        //public void ConstructorTest()
        //{
        //    TestObject.Should().NotBeNull();

        //    Action create = () => new ArtportalenObservationProcessor(
        //        null,
        //        _processedObservationRepositoryMock.Object,
        //        _processedFieldMappingRepositoryMock.Object,
        //        _fieldMappingResolverHelperMock.Object,
        //        _processConfiguration,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("artportalenVerbatimRepository");

        //    create = () => new ArtportalenObservationProcessor(
        //        _artportalenVerbatimRepository.Object,
        //        null,
        //        _processedFieldMappingRepositoryMock.Object,
        //        _fieldMappingResolverHelperMock.Object,
        //        _processConfiguration,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processedObservationRepository");

        //    create = () => new ArtportalenObservationProcessor(
        //        _artportalenVerbatimRepository.Object,
        //        _processedObservationRepositoryMock.Object,
        //        _processedFieldMappingRepositoryMock.Object,
        //        _fieldMappingResolverHelperMock.Object,
        //        null,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processConfiguration");

        //    create = () => new ArtportalenObservationProcessor(
        //        _artportalenVerbatimRepository.Object,
        //        _processedObservationRepositoryMock.Object,
        //        _processedFieldMappingRepositoryMock.Object,
        //        _fieldMappingResolverHelperMock.Object,
        //        _processConfiguration,
        //        null);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        //}

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
            _processedObservationRepositoryMock.Setup(por => por.DeleteProviderDataAsync(It.IsAny<DataProvider>()))
                .ReturnsAsync(true);
            _artportalenVerbatimRepository.Setup(r => r.GetBatchAsync(0, 0))
                .ReturnsAsync(new[]
                {
                    new ArtportalenObservationVerbatim
                    {
                        Id = 1
                    }
                });

            _processedObservationRepositoryMock
                .Setup(r => r.AddManyAsync(It.IsAny<ICollection<ProcessedObservation>>()))
                .ReturnsAsync(1);

            var dataProvider = new DataProvider
            {
                Name = "Artportalen",
                Type = DataProviderType.ArtportalenObservations
            };

            var taxa = new Dictionary<int, ProcessedTaxon>
            {
                {0, new ProcessedTaxon {Id = 0, TaxonId = "0", ScientificName = "Biota"}}
            };

            var fieldMappingById = new Dictionary<int, FieldMapping>
            {
                {0, new FieldMapping {Id = 0, Name = "ActivityId"}}
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.ProcessAsync(dataProvider, taxa, false, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Status.Should().Be(RunStatus.Success);
        }
    }
}