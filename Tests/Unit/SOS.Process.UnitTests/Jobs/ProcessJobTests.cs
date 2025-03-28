using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Harvest.Jobs;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Artportalen.Interfaces;
using SOS.Harvest.Processors.DarwinCoreArchive.Interfaces;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Models.Verbatim.Kul;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Process.UnitTests.Jobs
{
    /// <summary>
    ///     Tests for activate instance job
    /// </summary>
    public class ProcessJobTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public ProcessJobTests()
        {
            _processedObservationRepositoryMock = new Mock<IProcessedObservationCoreRepository>();
            _userObservationRepositoryMock = new Mock<IUserObservationRepository>();
            _observationDatasetRepository = new Mock<IDatasetRepository>();
            _observationEventRepository = new Mock<IEventRepository>();
            _processInfoRepository = new Mock<IProcessInfoRepository>();
            _harvestInfoRepository = new Mock<IHarvestInfoRepository>();
            _cacheManager = new Mock<ICacheManager>();
            _instanceManager = new Mock<IInstanceManager>();
            _processTimeManagerMock = new Mock<IProcessTimeManager>();
            _validationManager = new Mock<IValidationManager>();
            _taxonCache = new Mock<ICache<int, Taxon>>();
            _processTaxaJob = new Mock<IProcessTaxaJob>();
            _observationProcessorManagerMock = new Mock<IObservationProcessorManager>();
            _areaHelper = new Mock<IAreaHelper>();
            _loggerMock = new Mock<ILogger<ProcessObservationsJobFull>>();
            _dwcaObservationProcessor = new Mock<IDwcaObservationProcessor>();
            _dwcArchiveFileWriterCoordinatorMock = new Mock<IDwcArchiveFileWriterCoordinator>();
            _dataProviderCache = new Mock<IDataProviderCache>();
            _processConfigurationMock = new Mock<ProcessConfiguration>();
            _dwcaDatasetProcessorMock = new Mock<IDwcaDatasetProcessor>();
            _artportalenDatasetProcessorMock = new Mock<IArtportalenDatasetProcessor>();
            _dwcaEventProcessorMock = new Mock<IDwcaEventProcessor>();
            _artportalenEventProcessorMock = new Mock<IArtportalenEventProcessor>();
            _observationsHarvestJobIncrementalMock = new Mock<IObservationsHarvestJobIncremental>();
        }

        private readonly Mock<IProcessedObservationCoreRepository> _processedObservationRepositoryMock;
        private readonly Mock<IUserObservationRepository> _userObservationRepositoryMock;
        private readonly Mock<IDatasetRepository> _observationDatasetRepository;
        private readonly Mock<IEventRepository> _observationEventRepository;
        private readonly Mock<IProcessInfoRepository> _processInfoRepository;
        private readonly Mock<IHarvestInfoRepository> _harvestInfoRepository;
        private readonly Mock<IProcessTaxaJob> _processTaxaJob;
        private readonly Mock<IObservationProcessorManager> _observationProcessorManagerMock;
        private readonly Mock<ICache<int, Taxon>> _taxonCache;
        private readonly Mock<ICacheManager> _cacheManager;
        private readonly Mock<IProcessTimeManager> _processTimeManagerMock;
        private readonly Mock<IValidationManager> _validationManager;
        private readonly Mock<IInstanceManager> _instanceManager;
        private readonly Mock<IDataProviderCache> _dataProviderCache;
        private readonly Mock<IDwcaObservationProcessor> _dwcaObservationProcessor;
        private readonly Mock<IAreaHelper> _areaHelper;
        private readonly Mock<IDwcArchiveFileWriterCoordinator> _dwcArchiveFileWriterCoordinatorMock;
        private readonly Mock<IDwcaDatasetProcessor> _dwcaDatasetProcessorMock;
        private readonly Mock<IArtportalenDatasetProcessor> _artportalenDatasetProcessorMock;
        private readonly Mock<IArtportalenEventProcessor> _artportalenEventProcessorMock;
        private readonly Mock<IDwcaEventProcessor> _dwcaEventProcessorMock;
        private readonly Mock<IObservationsHarvestJobIncremental> _observationsHarvestJobIncrementalMock;
        private readonly Mock<ILogger<ProcessObservationsJobFull>> _loggerMock;
        private readonly Mock<ProcessConfiguration> _processConfigurationMock;

        private ProcessObservationsJobFull TestObject => new ProcessObservationsJobFull(
            _processedObservationRepositoryMock.Object,
            _processInfoRepository.Object,
            _harvestInfoRepository.Object,
            _observationProcessorManagerMock.Object,
            _taxonCache.Object,
            new Mock<ICache<VocabularyId, Vocabulary>>().Object,
            _dataProviderCache.Object,
            _cacheManager.Object,
            _processTimeManagerMock.Object,
            _validationManager.Object,
            _processTaxaJob.Object,
            _areaHelper.Object,
            _dwcArchiveFileWriterCoordinatorMock.Object,
            _processConfigurationMock.Object,
            _userObservationRepositoryMock.Object,
            _observationDatasetRepository.Object,
            _observationEventRepository.Object,
            _dwcaDatasetProcessorMock.Object,
            _artportalenDatasetProcessorMock.Object,
            _artportalenEventProcessorMock.Object,
            _dwcaEventProcessorMock.Object,
            _observationsHarvestJobIncrementalMock.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test processing exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RunAsyncException()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _taxonCache.Setup(r => r.GetAllAsync())
                .ThrowsAsync(new Exception("Failed"));
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Func<Task> act = async () => { await TestObject.RunAsync(JobCancellationToken.Null); };

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            await act.Should().ThrowAsync<Exception>();
        }

        /// <summary>
        ///     Test processing fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RunAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------


            //-----------------------------------------------------------------------------------------------------------
            // Act
            //----------------------------------------------------------------------------------------------------------
            Func<Task> act = async () => { await TestObject.RunAsync(JobCancellationToken.Null); };

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            await act.Should().ThrowAsync<Exception>();
        }

        /// <summary>
        ///     Make a successful test of processing
        /// </summary>
        /// <returns></returns>
        [Fact(Skip = "Not working")]
        public async Task RunAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------

            _dataProviderCache.Setup(dpm => dpm.GetAllAsync()).ReturnsAsync(new List<DataProvider> { new DataProvider { Id = 1, Names = new[] { new VocabularyValueTranslation { CultureCode = "en-GB", Value = "Artportalen" } }, Identifier = "Artportalen", Type = DataProviderType.ArtportalenObservations } });

            _processTaxaJob.Setup(r => r.RunAsync())
                .ReturnsAsync(true);

            _taxonCache.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Taxon>
                {
                    new Taxon {Id = 100024, ScientificName = "Canus Lupus"}
                });

            _processedObservationRepositoryMock.Setup(r => r.VerifyCollectionAsync(It.IsAny<bool>()))
                .ReturnsAsync(true);

            _harvestInfoRepository.Setup(r => r.GetAsync(nameof(ArtportalenObservationVerbatim)))
                .ReturnsAsync(new HarvestInfo("Identifier", DateTime.Now));

            _harvestInfoRepository.Setup(r => r.GetAsync(nameof(KulObservationVerbatim)))
                .ReturnsAsync(new HarvestInfo("Identifier",
                    DateTime.Now));

            _processedObservationRepositoryMock.Setup(r => r.SetActiveInstanceAsync(It.IsAny<byte>()));
            _processInfoRepository.Setup(r => r.VerifyCollectionAsync());

            _processInfoRepository.Setup(r => r.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new ProcessInfo(It.IsAny<string>(), It.IsAny<DateTime>()));

            _processInfoRepository.Setup(r => r.AddOrUpdateAsync(It.IsAny<ProcessInfo>()))
                .ReturnsAsync(true);

            _areaHelper.Setup(r => r.InitializeAsync());
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.RunAsync(
                JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
        }
    }
}