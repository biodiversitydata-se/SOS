using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Models.Verbatim.Kul;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Harvest.Jobs;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Artportalen.Interfaces;
using SOS.Harvest.Processors.DarwinCoreArchive.Interfaces;
using SOS.Harvest.Processors.FishData.Interfaces;
using SOS.Harvest.Processors.Kul.Interfaces;
using SOS.Harvest.Processors.Mvm.Interfaces;
using SOS.Harvest.Processors.Nors.Interfaces;
using SOS.Harvest.Processors.ObservationDatabase.Interfaces;
using SOS.Harvest.Processors.Sers.Interfaces;
using SOS.Harvest.Processors.Shark.Interfaces;
using SOS.Harvest.Processors.VirtualHerbarium.Interfaces;
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
            _processedObservationRepositoryMock = new Mock<IProcessedObservationRepository>();
            _processInfoRepository = new Mock<IProcessInfoRepository>();
            _harvestInfoRepository = new Mock<IHarvestInfoRepository>();
            _cacheManager = new Mock<ICacheManager>();
            _instanceManager = new Mock<IInstanceManager>();
            _processTimeManagerMock = new Mock<IProcessTimeManager>();
            _validationManager = new Mock<IValidationManager>();
            _taxonCache =new Mock<ICache<int, Taxon>>();
            _processTaxaJob = new Mock<IProcessTaxaJob>();
            _fishDataProcessor = new Mock<IFishDataObservationProcessor>();
            _kulProcessor = new Mock<IKulObservationProcessor>();
            _mvmProcessor = new Mock<IMvmObservationProcessor>();
            _norsProcessor = new Mock<INorsObservationProcessor>();
            _observationDatabaseProcessorMock = new Mock<IObservationDatabaseProcessor>();
            _sersProcessor = new Mock<ISersObservationProcessor>();
            _sharkProcessor = new Mock<ISharkObservationProcessor>();
            _virtualHerbariumProcessor = new Mock<IVirtualHerbariumObservationProcessor>();
            _artportalenProcessor = new Mock<IArtportalenObservationProcessor>();
            _areaHelper = new Mock<IAreaHelper>();
            _loggerMock = new Mock<ILogger<ProcessObservationsJob>>();
            _dwcaObservationProcessor = new Mock<IDwcaObservationProcessor>();
            _dwcArchiveFileWriterCoordinatorMock = new Mock<IDwcArchiveFileWriterCoordinator>();
            _dataProviderCache = new Mock<IDataProviderCache>();
            _processConfigurationMock = new Mock<ProcessConfiguration>();
        }

        private readonly Mock<IProcessedObservationRepository> _processedObservationRepositoryMock;
        private readonly Mock<IProcessInfoRepository> _processInfoRepository;
        private readonly Mock<IHarvestInfoRepository> _harvestInfoRepository;
        private readonly Mock<IProcessTaxaJob> _processTaxaJob;
        private readonly Mock<IFishDataObservationProcessor> _fishDataProcessor;
        private readonly Mock<IKulObservationProcessor> _kulProcessor;
        private readonly Mock<IMvmObservationProcessor> _mvmProcessor;
        private readonly Mock<INorsObservationProcessor> _norsProcessor;
        private readonly Mock<IObservationDatabaseProcessor> _observationDatabaseProcessorMock;
        private readonly Mock<ISersObservationProcessor> _sersProcessor;
        private readonly Mock<ISharkObservationProcessor> _sharkProcessor;
        private readonly Mock<IVirtualHerbariumObservationProcessor> _virtualHerbariumProcessor;
        private readonly Mock<IArtportalenObservationProcessor> _artportalenProcessor;
        private readonly Mock<ICache<int, Taxon>> _taxonCache;
        private readonly Mock<ICacheManager> _cacheManager;
        private readonly Mock<IProcessTimeManager> _processTimeManagerMock;
        private readonly Mock<IValidationManager> _validationManager;
        private readonly Mock<IInstanceManager> _instanceManager;
        private readonly Mock<IDataProviderCache> _dataProviderCache;
        private readonly Mock<IDwcaObservationProcessor> _dwcaObservationProcessor;
        private readonly Mock<IAreaHelper> _areaHelper;
        private readonly Mock<IDwcArchiveFileWriterCoordinator> _dwcArchiveFileWriterCoordinatorMock;
        private readonly Mock<ILogger<ProcessObservationsJob>> _loggerMock;
        private readonly Mock<ProcessConfiguration> _processConfigurationMock;

        private ProcessObservationsJob TestObject => new ProcessObservationsJob(
            _processedObservationRepositoryMock.Object,
            _processInfoRepository.Object,
            _harvestInfoRepository.Object,
            _artportalenProcessor.Object,
            _fishDataProcessor.Object,
            _kulProcessor.Object,
            _mvmProcessor.Object,
            _norsProcessor.Object,
            _observationDatabaseProcessorMock.Object,
            _sersProcessor.Object,
            _sharkProcessor.Object,
            _virtualHerbariumProcessor.Object,
            _dwcaObservationProcessor.Object,
            _taxonCache.Object,
            _dataProviderCache.Object,
            _cacheManager.Object,
            _processTimeManagerMock.Object,
            _validationManager.Object,
            _processTaxaJob.Object,
            _areaHelper.Object,
            _dwcArchiveFileWriterCoordinatorMock.Object,
            _processConfigurationMock.Object,
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
            Func<Task> act = async () => { await TestObject.RunAsync(It.IsAny<List<string>>(), JobRunModes.Full, JobCancellationToken.Null); };

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
            Func<Task> act = async () => { await TestObject.RunAsync(It.IsAny<List<string>>(), JobRunModes.Full, JobCancellationToken.Null); };

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

            _dataProviderCache.Setup(dpm => dpm.GetAllAsync()).ReturnsAsync(new List<DataProvider> {new DataProvider{ Id = 1, Names = new []{ new VocabularyValueTranslation{ CultureCode = "en-GB", Value = "Artportalen" } }, Identifier = "Artportalen", Type = DataProviderType.ArtportalenObservations} });
            
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
                .ReturnsAsync(new HarvestInfo(DateTime.Now));
            _artportalenProcessor.Setup(r =>
                    r.ProcessAsync(null, It.IsAny<IDictionary<int, Taxon>>(),  JobRunModes.Full, JobCancellationToken.Null))
                .ReturnsAsync(ProcessingStatus.Success(DataProviderIdentifiers.Artportalen,
                    DataProviderType.ArtportalenObservations, DateTime.Now, DateTime.Now, 1, 1, 0));

            _harvestInfoRepository.Setup(r => r.GetAsync(nameof(KulObservationVerbatim)))
                .ReturnsAsync(new HarvestInfo(
                    DateTime.Now));
            _kulProcessor.Setup(r =>
                    r.ProcessAsync(null, It.IsAny<IDictionary<int, Taxon>>(),JobRunModes.Full,  JobCancellationToken.Null))
                .ReturnsAsync(ProcessingStatus.Success(DataProviderIdentifiers.KUL, DataProviderType.KULObservations,
                    DateTime.Now, DateTime.Now, 1, 1, 0));

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
                new List<string>
                {
                    DataProviderIdentifiers.Artportalen, 
                    DataProviderIdentifiers.KUL
                },
                JobRunModes.Full,
                JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
        }
    }
}