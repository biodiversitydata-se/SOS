using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Managers.Interfaces;
using SOS.Process.Processors.Artportalen;
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
            _vocabularyRepositoryMock = new Mock<IVocabularyRepository>();
            _vocabularyResolverMock = new Mock<IVocabularyValueResolver>();
            _processConfiguration = new ProcessConfiguration() { ArtportalenUrl = "https://www.artportalen.se" };
            _dwcArchiveFileWriterCoordinatorMock = new Mock<IDwcArchiveFileWriterCoordinator>();
            _diffusionManagerMock = new Mock<IDiffusionManager>();
            _processManagerMock = new Mock<IProcessManager>();
            _processTimeManagerMock = new Mock<IProcessTimeManager>();
            _validationManagerMock = new Mock<IValidationManager>();
            _loggerMock = new Mock<ILogger<ArtportalenObservationProcessor>>();
        }

        private readonly Mock<IArtportalenVerbatimRepository> _artportalenVerbatimRepository;
        private readonly Mock<IProcessedObservationRepository> _processedObservationRepositoryMock;
        private readonly Mock<IVocabularyRepository> _vocabularyRepositoryMock;
        private readonly Mock<IVocabularyValueResolver> _vocabularyResolverMock;
        private readonly ProcessConfiguration _processConfiguration;
        private readonly Mock<IDwcArchiveFileWriterCoordinator> _dwcArchiveFileWriterCoordinatorMock;
        private readonly Mock<IProcessManager> _processManagerMock;
        private readonly Mock<IValidationManager> _validationManagerMock;
        private readonly Mock<ILogger<ArtportalenObservationProcessor>> _loggerMock;
        private readonly Mock<IDiffusionManager> _diffusionManagerMock;
        private readonly Mock<IProcessTimeManager> _processTimeManagerMock;

        private ArtportalenObservationProcessor TestObject => new ArtportalenObservationProcessor(
            _artportalenVerbatimRepository.Object,
            _processedObservationRepositoryMock.Object,
            _vocabularyRepositoryMock.Object,
            _vocabularyResolverMock.Object,
            _dwcArchiveFileWriterCoordinatorMock.Object,
            _processManagerMock.Object,
            _validationManagerMock.Object,
            _diffusionManagerMock.Object,
            _processTimeManagerMock.Object,
            _processConfiguration,
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
                Names = new[] { new VocabularyValueTranslation { CultureCode = "en-GB", Value = "Artportalen" } },
                Type = DataProviderType.ArtportalenObservations
            };

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
            var dataProvider = new DataProvider
            {
                Names = new[] { new VocabularyValueTranslation { CultureCode = "en-GB", Value = "Artportalen" } },
                Type = DataProviderType.ArtportalenObservations
            };

            _artportalenVerbatimRepository.Setup(r => r.GetBatchAsync(0, 0))
                .ThrowsAsync(new Exception("Failed"));

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.ProcessAsync(dataProvider, null,  JobRunModes.Full, JobCancellationToken.Null);
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
            _processedObservationRepositoryMock.Setup(por => por.DeleteProviderDataAsync(It.IsAny<DataProvider>(), It.IsAny<bool>()))
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
                .Setup(r => r.AddManyAsync(It.IsAny<ICollection<Observation>>(), It.IsAny<bool>()))
                .ReturnsAsync(1);

            var dataProvider = new DataProvider
            {
                Names = new[] { new VocabularyValueTranslation { CultureCode = "en-GB", Value = "Artportalen" } },
                Type = DataProviderType.ArtportalenObservations
            };

            var taxa = new Dictionary<int, Taxon>
            {
                {0, new Taxon {Id = 0, TaxonId = "0", ScientificName = "Biota"}}
            };

            var vocabularyById = new Dictionary<int, Vocabulary>
            {
                {0, new Vocabulary {Id = 0, Name = "ActivityId"}}
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