using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Models.Verbatim.ClamPortal;
using SOS.Lib.Models.Verbatim.Kul;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Jobs;
using SOS.Process.Managers.Interfaces;
using SOS.Process.Processors.Artportalen.Interfaces;
using SOS.Process.Processors.ClamPortal.Interfaces;
using SOS.Process.Processors.FishData.Interfaces;
using SOS.Process.Processors.Interfaces;
using SOS.Process.Processors.Kul.Interfaces;
using SOS.Process.Processors.Mvm.Interfaces;
using SOS.Process.Processors.Nors.Interfaces;
using SOS.Process.Processors.Sers.Interfaces;
using SOS.Process.Processors.Shark.Interfaces;
using SOS.Process.Processors.VirtualHerbarium.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;
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
            _darwinCoreRepository = new Mock<IProcessedObservationRepository>();
            _processInfoRepository = new Mock<IProcessInfoRepository>();
            _harvestInfoRepository = new Mock<IHarvestInfoRepository>();
            _instanceManager = new Mock<IInstanceManager>();
            _validationManager = new Mock<IValidationManager>();
            _taxonProcessedRepository = new Mock<IProcessedTaxonRepository>();
            _processTaxaJob = new Mock<IProcessTaxaJob>();
            _clamPortalProcessor = new Mock<IClamPortalObservationProcessor>();
            _fishDataProcessor = new Mock<IFishDataObservationProcessor>();
            _kulProcessor = new Mock<IKulObservationProcessor>();
            _mvmProcessor = new Mock<IMvmObservationProcessor>();
            _norsProcessor = new Mock<INorsObservationProcessor>();
            _sersProcessor = new Mock<ISersObservationProcessor>();
            _sharkProcessor = new Mock<ISharkObservationProcessor>();
            _virtualHerbariumProcessor = new Mock<IVirtualHerbariumObservationProcessor>();
            _artportalenProcessor = new Mock<IArtportalenObservationProcessor>();
            _taxonProcessedRepository = new Mock<IProcessedTaxonRepository>();
            _areaHelper = new Mock<IAreaHelper>();
            _loggerMock = new Mock<ILogger<ProcessJob>>();
            _dwcaObservationProcessor = new Mock<IDwcaObservationProcessor>();
            _dwcArchiveFileWriterCoordinatorMock = new Mock<IDwcArchiveFileWriterCoordinator>();
            _dataProviderManager = new Mock<IDataProviderManager>();
        }

        private readonly Mock<IProcessedObservationRepository> _darwinCoreRepository;
        private readonly Mock<IProcessInfoRepository> _processInfoRepository;
        private readonly Mock<IHarvestInfoRepository> _harvestInfoRepository;
        private readonly Mock<IProcessTaxaJob> _processTaxaJob;
        private readonly Mock<IClamPortalObservationProcessor> _clamPortalProcessor;
        private readonly Mock<IFishDataObservationProcessor> _fishDataProcessor;
        private readonly Mock<IKulObservationProcessor> _kulProcessor;
        private readonly Mock<IMvmObservationProcessor> _mvmProcessor;
        private readonly Mock<INorsObservationProcessor> _norsProcessor;
        private readonly Mock<ISersObservationProcessor> _sersProcessor;
        private readonly Mock<ISharkObservationProcessor> _sharkProcessor;
        private readonly Mock<IVirtualHerbariumObservationProcessor> _virtualHerbariumProcessor;
        private readonly Mock<IArtportalenObservationProcessor> _artportalenProcessor;
        private readonly Mock<IProcessedTaxonRepository> _taxonProcessedRepository;
        private readonly Mock<IValidationManager> _validationManager;
        private readonly Mock<IInstanceManager> _instanceManager;
        private readonly Mock<IDataProviderManager> _dataProviderManager;
        private readonly Mock<IDwcaObservationProcessor> _dwcaObservationProcessor;
        private readonly Mock<IAreaHelper> _areaHelper;
        private readonly Mock<IDwcArchiveFileWriterCoordinator> _dwcArchiveFileWriterCoordinatorMock;
        private readonly Mock<ILogger<ProcessJob>> _loggerMock;

        private ProcessJob TestObject => new ProcessJob(
            _darwinCoreRepository.Object,
            _processInfoRepository.Object,
            _harvestInfoRepository.Object,
            _artportalenProcessor.Object,
            _clamPortalProcessor.Object,
            _fishDataProcessor.Object,
            _kulProcessor.Object,
            _mvmProcessor.Object,
            _norsProcessor.Object,
            _sersProcessor.Object,
            _sharkProcessor.Object,
            _virtualHerbariumProcessor.Object,
            _dwcaObservationProcessor.Object,
            _taxonProcessedRepository.Object,
            _dataProviderManager.Object,
            _instanceManager.Object,
            _validationManager.Object,
            _processTaxaJob.Object,
            _areaHelper.Object,
            _dwcArchiveFileWriterCoordinatorMock.Object,
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
            _taxonProcessedRepository.Setup(r => r.GetAllAsync())
                .ThrowsAsync(new Exception("Failed"));
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Func<Task> act = async () => { await TestObject.RunAsync(It.IsAny<List<string>>(), JobRunModes.Full, JobCancellationToken.Null); };

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<Exception>();
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
            act.Should().Throw<Exception>();
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

        //    Action create = () => new ProcessJob(
        //        null,
        //        _processInfoRepository.Object,
        //        _harvestInfoRepository.Object,
        //        _clamPortalProcessor.Object,
        //        _kulProcessor.Object,
        //        _mvmProcessor.Object,
        //        _norsProcessor.Object,
        //        _sersProcessor.Object,
        //        _sharkProcessor.Object,
        //        _virtualHerbariumProcessor.Object,
        //        _artportalenProcessor.Object,
        //        _taxonProcessedRepository.Object,
        //        _instanceManager.Object,
        //        _copyFieldMappingsJob.Object,
        //        _processTaxaJob.Object,
        //        _areaHelper.Object,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processedObservationRepository");

        //    create = () => new ProcessJob(
        //        _darwinCoreRepository.Object,
        //        null,
        //        _harvestInfoRepository.Object,
        //        _clamPortalProcessor.Object,
        //        _kulProcessor.Object,
        //        _mvmProcessor.Object,
        //        _norsProcessor.Object,
        //        _sersProcessor.Object,
        //        _sharkProcessor.Object,
        //        _virtualHerbariumProcessor.Object,
        //        _artportalenProcessor.Object,
        //        _taxonProcessedRepository.Object,
        //        _instanceManager.Object,
        //        _copyFieldMappingsJob.Object,
        //        _processTaxaJob.Object,
        //        _areaHelper.Object,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processInfoRepository");

        //    create = () => new ProcessJob(
        //        _darwinCoreRepository.Object,
        //        _processInfoRepository.Object,
        //        null,
        //        _clamPortalProcessor.Object,
        //        _kulProcessor.Object,
        //        _mvmProcessor.Object,
        //        _norsProcessor.Object,
        //        _sersProcessor.Object,
        //        _sharkProcessor.Object,
        //        _virtualHerbariumProcessor.Object,
        //        _artportalenProcessor.Object,
        //        _taxonProcessedRepository.Object,
        //        _instanceManager.Object,
        //        _copyFieldMappingsJob.Object,
        //        _processTaxaJob.Object,
        //        _areaHelper.Object,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("harvestInfoRepository");

        //    create = () => new ProcessJob(
        //        _darwinCoreRepository.Object,
        //        _processInfoRepository.Object,
        //        _harvestInfoRepository.Object,
        //        null,
        //        _kulProcessor.Object,
        //        _mvmProcessor.Object,
        //        _norsProcessor.Object,
        //        _sersProcessor.Object,
        //        _sharkProcessor.Object,
        //        _virtualHerbariumProcessor.Object,
        //        _artportalenProcessor.Object,
        //        _taxonProcessedRepository.Object,
        //        _instanceManager.Object,
        //        _copyFieldMappingsJob.Object,
        //        _processTaxaJob.Object,
        //        _areaHelper.Object,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("clamPortalObservationProcessor");

        //    create = () => new ProcessJob(
        //        _darwinCoreRepository.Object,
        //        _processInfoRepository.Object,
        //        _harvestInfoRepository.Object,
        //        _clamPortalProcessor.Object,
        //        null,
        //        _mvmProcessor.Object,
        //        _norsProcessor.Object,
        //        _sersProcessor.Object,
        //        _sharkProcessor.Object,
        //        _virtualHerbariumProcessor.Object,
        //        _artportalenProcessor.Object,
        //        _taxonProcessedRepository.Object,
        //        _instanceManager.Object,
        //        _copyFieldMappingsJob.Object,
        //        _processTaxaJob.Object,
        //        _areaHelper.Object,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("kulObservationProcessor");

        //    create = () => new ProcessJob(
        //        _darwinCoreRepository.Object,
        //        _processInfoRepository.Object,
        //        _harvestInfoRepository.Object,
        //        _clamPortalProcessor.Object,
        //        _kulProcessor.Object,
        //        null,
        //        _norsProcessor.Object,
        //        _sersProcessor.Object,
        //        _sharkProcessor.Object,
        //        _virtualHerbariumProcessor.Object,
        //        _artportalenProcessor.Object,
        //        _taxonProcessedRepository.Object,
        //        _instanceManager.Object,
        //        _copyFieldMappingsJob.Object,
        //        _processTaxaJob.Object,
        //        _areaHelper.Object,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("mvmObservationProcessor");

        //    create = () => new ProcessJob(
        //        _darwinCoreRepository.Object,
        //        _processInfoRepository.Object,
        //        _harvestInfoRepository.Object,
        //        _clamPortalProcessor.Object,
        //        _kulProcessor.Object,
        //        _mvmProcessor.Object,
        //        null,
        //        _sersProcessor.Object,
        //        _sharkProcessor.Object,
        //        _virtualHerbariumProcessor.Object,
        //        _artportalenProcessor.Object,
        //        _taxonProcessedRepository.Object,
        //        _instanceManager.Object,
        //        _copyFieldMappingsJob.Object,
        //        _processTaxaJob.Object,
        //        _areaHelper.Object,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("norsObservationProcessor");

        //    create = () => new ProcessJob(
        //        _darwinCoreRepository.Object,
        //        _processInfoRepository.Object,
        //        _harvestInfoRepository.Object,
        //        _clamPortalProcessor.Object,
        //        _kulProcessor.Object,
        //        _mvmProcessor.Object,
        //        _norsProcessor.Object,
        //        null,
        //        _sharkProcessor.Object,
        //        _virtualHerbariumProcessor.Object,
        //        _artportalenProcessor.Object,
        //        _taxonProcessedRepository.Object,
        //        _instanceManager.Object,
        //        _copyFieldMappingsJob.Object,
        //        _processTaxaJob.Object,
        //        _areaHelper.Object,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("sersObservationProcessor");

        //    create = () => new ProcessJob(
        //        _darwinCoreRepository.Object,
        //        _processInfoRepository.Object,
        //        _harvestInfoRepository.Object,
        //        _clamPortalProcessor.Object,
        //        _kulProcessor.Object,
        //        _mvmProcessor.Object,
        //        _norsProcessor.Object,
        //        _sersProcessor.Object,
        //        null,
        //        _virtualHerbariumProcessor.Object,
        //        _artportalenProcessor.Object,
        //        _taxonProcessedRepository.Object,
        //        _instanceManager.Object,
        //        _copyFieldMappingsJob.Object,
        //        _processTaxaJob.Object,
        //        _areaHelper.Object,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("sharkObservationProcessor");

        //    create = () => new ProcessJob(
        //        _darwinCoreRepository.Object,
        //        _processInfoRepository.Object,
        //        _harvestInfoRepository.Object,
        //        _clamPortalProcessor.Object,
        //        _kulProcessor.Object,
        //        _mvmProcessor.Object,
        //        _norsProcessor.Object,
        //        _sersProcessor.Object,
        //        _sharkProcessor.Object,
        //        null,
        //        _artportalenProcessor.Object,
        //        _taxonProcessedRepository.Object,
        //        _instanceManager.Object,
        //        _copyFieldMappingsJob.Object,
        //        _processTaxaJob.Object,
        //        _areaHelper.Object,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("virtualHerbariumObservationProcessor");


        //    create = () => new ProcessJob(
        //        _darwinCoreRepository.Object,
        //        _processInfoRepository.Object,
        //        _harvestInfoRepository.Object,
        //        _clamPortalProcessor.Object,
        //        _kulProcessor.Object,
        //        _mvmProcessor.Object,
        //        _norsProcessor.Object,
        //        _sersProcessor.Object,
        //        _sharkProcessor.Object,
        //        _virtualHerbariumProcessor.Object,
        //        null,
        //        _taxonProcessedRepository.Object,
        //        _instanceManager.Object,
        //        _copyFieldMappingsJob.Object,
        //        _processTaxaJob.Object,
        //        _areaHelper.Object,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("artportalenObservationProcessor");

        //    create = () => new ProcessJob(
        //        _darwinCoreRepository.Object,
        //        _processInfoRepository.Object,
        //        _harvestInfoRepository.Object,
        //        _clamPortalProcessor.Object,
        //        _kulProcessor.Object,
        //        _mvmProcessor.Object,
        //        _norsProcessor.Object,
        //        _sersProcessor.Object,
        //        _sharkProcessor.Object,
        //        _virtualHerbariumProcessor.Object,
        //        _artportalenProcessor.Object,
        //        null,
        //        _instanceManager.Object,
        //        _copyFieldMappingsJob.Object,
        //        _processTaxaJob.Object,
        //        _areaHelper.Object,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processedTaxonRepository");

        //    create = () => new ProcessJob(
        //        _darwinCoreRepository.Object,
        //        _processInfoRepository.Object,
        //        _harvestInfoRepository.Object,
        //        _clamPortalProcessor.Object,
        //        _kulProcessor.Object,
        //        _mvmProcessor.Object,
        //        _norsProcessor.Object,
        //        _sersProcessor.Object,
        //        _sharkProcessor.Object,
        //        _virtualHerbariumProcessor.Object,
        //        _artportalenProcessor.Object,
        //        _taxonProcessedRepository.Object,
        //        null,
        //        _copyFieldMappingsJob.Object,
        //        _processTaxaJob.Object,
        //        _areaHelper.Object,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("instanceManager");

        //    create = () => new ProcessJob(
        //        _darwinCoreRepository.Object,
        //        _processInfoRepository.Object,
        //        _harvestInfoRepository.Object,
        //        _clamPortalProcessor.Object,
        //        _kulProcessor.Object,
        //        _mvmProcessor.Object,
        //        _norsProcessor.Object,
        //        _sersProcessor.Object,
        //        _sharkProcessor.Object,
        //        _virtualHerbariumProcessor.Object,
        //        _artportalenProcessor.Object,
        //        _taxonProcessedRepository.Object,
        //        _instanceManager.Object,
        //        null,
        //        _processTaxaJob.Object,
        //        _areaHelper.Object,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("copyFieldMappingsJob");

        //    create = () => new ProcessJob(
        //        _darwinCoreRepository.Object,
        //        _processInfoRepository.Object,
        //        _harvestInfoRepository.Object,
        //        _clamPortalProcessor.Object,
        //        _kulProcessor.Object,
        //        _mvmProcessor.Object,
        //        _norsProcessor.Object,
        //        _sersProcessor.Object,
        //        _sharkProcessor.Object,
        //        _virtualHerbariumProcessor.Object,
        //        _artportalenProcessor.Object,
        //        _taxonProcessedRepository.Object,
        //        _instanceManager.Object,
        //        _copyFieldMappingsJob.Object,
        //        null,
        //        _areaHelper.Object,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processTaxaJob");

        //    create = () => new ProcessJob(
        //        _darwinCoreRepository.Object,
        //        _processInfoRepository.Object,
        //        _harvestInfoRepository.Object,
        //        _clamPortalProcessor.Object,
        //        _kulProcessor.Object,
        //        _mvmProcessor.Object,
        //        _norsProcessor.Object,
        //        _sersProcessor.Object,
        //        _sharkProcessor.Object,
        //        _virtualHerbariumProcessor.Object,
        //        _artportalenProcessor.Object,
        //        _taxonProcessedRepository.Object,
        //        _instanceManager.Object,
        //        _copyFieldMappingsJob.Object,
        //        _processTaxaJob.Object,
        //        null,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("areaHelper");

        //    create = () => new ProcessJob(
        //        _darwinCoreRepository.Object,
        //        _processInfoRepository.Object,
        //        _harvestInfoRepository.Object,
        //        _clamPortalProcessor.Object,
        //        _kulProcessor.Object,
        //        _mvmProcessor.Object,
        //        _norsProcessor.Object,
        //        _sersProcessor.Object,
        //        _sharkProcessor.Object,
        //        _virtualHerbariumProcessor.Object,
        //        _artportalenProcessor.Object,
        //        _taxonProcessedRepository.Object,
        //        _instanceManager.Object,
        //        _copyFieldMappingsJob.Object,
        //        _processTaxaJob.Object,
        //        _areaHelper.Object,
        //        null);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        //}

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

            _dataProviderManager.Setup(dpm => dpm.GetAllDataProvidersAsync()).ReturnsAsync(new List<DataProvider> {new DataProvider{ Id = 1, Name = "Artportalen", Identifier = "Artportalen", Type = DataProviderType.ArtportalenObservations} });
            
            _processTaxaJob.Setup(r => r.RunAsync())
                .ReturnsAsync(true);

            _taxonProcessedRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<ProcessedTaxon>
                {
                    new ProcessedTaxon {Id = 100024, ScientificName = "Canus Lupus"}
                });

            _darwinCoreRepository.Setup(r => r.VerifyCollectionAsync());


            _harvestInfoRepository.Setup(r => r.GetAsync(nameof(ArtportalenObservationVerbatim)))
                .ReturnsAsync(new HarvestInfo(nameof(ArtportalenObservationVerbatim),
                    DataProviderType.ArtportalenObservations, DateTime.Now));
            _artportalenProcessor.Setup(r =>
                    r.ProcessAsync(null, It.IsAny<IDictionary<int, ProcessedTaxon>>(), JobRunModes.Full, JobCancellationToken.Null))
                .ReturnsAsync(ProcessingStatus.Success(DataProviderIdentifiers.Artportalen,
                    DataProviderType.ArtportalenObservations, DateTime.Now, DateTime.Now, 1));

            _harvestInfoRepository.Setup(r => r.GetAsync(nameof(ClamObservationVerbatim)))
                .ReturnsAsync(new HarvestInfo(nameof(ClamObservationVerbatim), DataProviderType.ClamPortalObservations,
                    DateTime.Now));
            _clamPortalProcessor.Setup(r =>
                    r.ProcessAsync(null, It.IsAny<IDictionary<int, ProcessedTaxon>>(), JobRunModes.Full, JobCancellationToken.Null))
                .ReturnsAsync(ProcessingStatus.Success(DataProviderIdentifiers.ClamGateway,
                    DataProviderType.ClamPortalObservations, DateTime.Now, DateTime.Now, 1));

            _harvestInfoRepository.Setup(r => r.GetAsync(nameof(KulObservationVerbatim)))
                .ReturnsAsync(new HarvestInfo(nameof(KulObservationVerbatim), DataProviderType.KULObservations,
                    DateTime.Now));
            _kulProcessor.Setup(r =>
                    r.ProcessAsync(null, It.IsAny<IDictionary<int, ProcessedTaxon>>(), JobRunModes.Full, JobCancellationToken.Null))
                .ReturnsAsync(ProcessingStatus.Success(DataProviderIdentifiers.KUL, DataProviderType.KULObservations,
                    DateTime.Now, DateTime.Now, 1));

            _darwinCoreRepository.Setup(r => r.SetActiveInstanceAsync(It.IsAny<byte>()));

            _processInfoRepository.Setup(r => r.VerifyCollectionAsync());

            _processInfoRepository.Setup(r => r.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new ProcessInfo(It.IsAny<string>(), It.IsAny<DateTime>()));

            _processInfoRepository.Setup(r => r.AddOrUpdateAsync(It.IsAny<ProcessInfo>()))
                .ReturnsAsync(true);

            _areaHelper.Setup(r => r.PersistCache());
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.RunAsync(
                new List<string>
                {
                    DataProviderIdentifiers.Artportalen, DataProviderIdentifiers.ClamGateway,
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