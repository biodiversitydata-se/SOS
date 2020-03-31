using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Jobs;
using SOS.Process.Managers.Interfaces;
using SOS.Process.Processors.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;
using Xunit;

namespace SOS.Process.UnitTests.Jobs
{
    /// <summary>
    /// Tests for activate instance job
    /// </summary>
    public class ProcessJobTests
    {
        private readonly Mock<IProcessedObservationRepository> _darwinCoreRepository;
        private readonly Mock<IProcessInfoRepository> _processInfoRepository;
        private readonly Mock<IHarvestInfoRepository> _harvestInfoRepository;
        private readonly Mock<IInstanceManager> _instanceManager;
        private readonly Mock<ICopyFieldMappingsJob> _copyFieldMappingsJob;
        private readonly Mock<IProcessTaxaJob> _processTaxaJob;
        private readonly Mock<IClamPortalObservationProcessor> _clamPortalProcessor;
        private readonly Mock<IKulObservationProcessor> _kulProcessor;
        private readonly Mock<IArtportalenObservationProcessor> _artportalenProcessor;
        private readonly Mock<IProcessedTaxonRepository> _taxonProcessedRepository;
        private readonly Mock<IAreaHelper> _areaHelper;
        private readonly Mock<ILogger<ProcessJob>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProcessJobTests()
        {
            _darwinCoreRepository = new Mock<IProcessedObservationRepository>();
            _processInfoRepository = new Mock<IProcessInfoRepository>();
            _harvestInfoRepository = new Mock<IHarvestInfoRepository>();
            _instanceManager = new Mock<IInstanceManager>();
            _taxonProcessedRepository = new Mock<IProcessedTaxonRepository>();
            _copyFieldMappingsJob = new Mock<ICopyFieldMappingsJob>();
            _processTaxaJob = new Mock<IProcessTaxaJob>();
            _clamPortalProcessor = new Mock<IClamPortalObservationProcessor>();
            _kulProcessor = new Mock<IKulObservationProcessor>();
            _artportalenProcessor = new Mock<IArtportalenObservationProcessor>();
            _taxonProcessedRepository = new Mock<IProcessedTaxonRepository>();
            _areaHelper = new Mock<IAreaHelper>();
            _loggerMock = new Mock<ILogger<ProcessJob>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new ProcessJob(
                _darwinCoreRepository.Object,
                _processInfoRepository.Object,
                _harvestInfoRepository.Object,
                _clamPortalProcessor.Object,
                _kulProcessor.Object,
                _artportalenProcessor.Object,
                _taxonProcessedRepository.Object,
                _instanceManager.Object,
                _copyFieldMappingsJob.Object,
                _processTaxaJob.Object,
                _areaHelper.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new ProcessJob(
                null,
                _processInfoRepository.Object,
                _harvestInfoRepository.Object,
                _clamPortalProcessor.Object,
                _kulProcessor.Object,
                _artportalenProcessor.Object,
                _taxonProcessedRepository.Object,
                _instanceManager.Object,
                _copyFieldMappingsJob.Object,
                _processTaxaJob.Object,
                _areaHelper.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("darwinCoreRepository");

            create = () => new ProcessJob(
                _darwinCoreRepository.Object,
                null,
                _harvestInfoRepository.Object,
                _clamPortalProcessor.Object,
                _kulProcessor.Object,
                _artportalenProcessor.Object,
                _taxonProcessedRepository.Object,
                _instanceManager.Object,
                _copyFieldMappingsJob.Object,
                _processTaxaJob.Object,
                _areaHelper.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processInfoRepository");

            create = () => new ProcessJob(
                _darwinCoreRepository.Object,
                _processInfoRepository.Object,
                null,
                _clamPortalProcessor.Object,
                _kulProcessor.Object,
                _artportalenProcessor.Object,
                _taxonProcessedRepository.Object,
                _instanceManager.Object,
                _copyFieldMappingsJob.Object,
                _processTaxaJob.Object,
                _areaHelper.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("harvestInfoRepository");

            create = () => new ProcessJob(
                _darwinCoreRepository.Object,
                _processInfoRepository.Object,
                _harvestInfoRepository.Object,
                _clamPortalProcessor.Object,
                _kulProcessor.Object,
                _artportalenProcessor.Object,
                _taxonProcessedRepository.Object,
                _instanceManager.Object,
                _copyFieldMappingsJob.Object,
                _processTaxaJob.Object,
                _areaHelper.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("harvestInfoRepository");

            create = () => new ProcessJob(
                _darwinCoreRepository.Object,
                _processInfoRepository.Object,
                _harvestInfoRepository.Object,
                null,
                _kulProcessor.Object,
                _artportalenProcessor.Object,
                _taxonProcessedRepository.Object,
                _instanceManager.Object,
                _copyFieldMappingsJob.Object,
                _processTaxaJob.Object,
                _areaHelper.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("clamPortalObservationProcessor");

            create = () => new ProcessJob(
                _darwinCoreRepository.Object,
                _processInfoRepository.Object,
                _harvestInfoRepository.Object,
                _clamPortalProcessor.Object,
                null,
                _artportalenProcessor.Object,
                _taxonProcessedRepository.Object,
                _instanceManager.Object,
                _copyFieldMappingsJob.Object,
                _processTaxaJob.Object,
                _areaHelper.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("kulObservationProcessor");

            create = () => new ProcessJob(
                _darwinCoreRepository.Object,
                _processInfoRepository.Object,
                _harvestInfoRepository.Object,
                _clamPortalProcessor.Object,
                _kulProcessor.Object,
                null,
                _taxonProcessedRepository.Object,
                _instanceManager.Object,
                _copyFieldMappingsJob.Object,
                _processTaxaJob.Object,
                _areaHelper.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("artportalenProcessFactory");

            create = () => new ProcessJob(
                _darwinCoreRepository.Object,
                _processInfoRepository.Object,
                _harvestInfoRepository.Object,
                _clamPortalProcessor.Object,
                _kulProcessor.Object,
                _artportalenProcessor.Object,
                null,
                _instanceManager.Object,
                _copyFieldMappingsJob.Object,
                _processTaxaJob.Object,
                _areaHelper.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("taxonProcessedRepository");

            create = () => new ProcessJob(
                _darwinCoreRepository.Object,
                _processInfoRepository.Object,
                _harvestInfoRepository.Object,
                _clamPortalProcessor.Object,
                _kulProcessor.Object,
                _artportalenProcessor.Object,
                _taxonProcessedRepository.Object,
                null,
                _copyFieldMappingsJob.Object,
                _processTaxaJob.Object,
                _areaHelper.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("instanceFactory");

            create = () => new ProcessJob(
                _darwinCoreRepository.Object,
                _processInfoRepository.Object,
                _harvestInfoRepository.Object,
                _clamPortalProcessor.Object,
                _kulProcessor.Object,
                _artportalenProcessor.Object,
                _taxonProcessedRepository.Object,
                _instanceManager.Object,
                null,
                _processTaxaJob.Object,
                _areaHelper.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("copyFieldMappingsJob");

            create = () => new ProcessJob(
                _darwinCoreRepository.Object,
                _processInfoRepository.Object,
                _harvestInfoRepository.Object,
                _clamPortalProcessor.Object,
                _kulProcessor.Object,
                _artportalenProcessor.Object,
                _taxonProcessedRepository.Object,
                _instanceManager.Object,
                _copyFieldMappingsJob.Object,
                null,
                _areaHelper.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processTaxaJob");

            create = () => new ProcessJob(
                _darwinCoreRepository.Object,
                _processInfoRepository.Object,
                _harvestInfoRepository.Object,
                _clamPortalProcessor.Object,
                _kulProcessor.Object,
                _artportalenProcessor.Object,
                _taxonProcessedRepository.Object,
                _instanceManager.Object,
                _copyFieldMappingsJob.Object,
                _processTaxaJob.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("areaHelper");

            create = () => new ProcessJob(
                _darwinCoreRepository.Object,
                _processInfoRepository.Object,
                _harvestInfoRepository.Object,
                _clamPortalProcessor.Object,
                _kulProcessor.Object,
                _artportalenProcessor.Object,
                _taxonProcessedRepository.Object,
                _instanceManager.Object,
                _copyFieldMappingsJob.Object,
                _processTaxaJob.Object,
                _areaHelper.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Make a successful test of processing
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RunAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _taxonProcessedRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<ProcessedTaxon>
                {
                    new ProcessedTaxon {Id = 100024, ScientificName = "Canus Lupus"},
                });

            _darwinCoreRepository.Setup(r => r.VerifyCollectionAsync());

            _artportalenProcessor.Setup(r => r.ProcessAsync(It.IsAny<IDictionary<int, ProcessedTaxon>>(), JobCancellationToken.Null))
                .ReturnsAsync(ProcessingStatus.Success(ObservationProvider.Artportalen, DateTime.Now, DateTime.Now, 1));

            _clamPortalProcessor.Setup(r => r.ProcessAsync(It.IsAny<IDictionary<int, ProcessedTaxon>>(), JobCancellationToken.Null))
                .ReturnsAsync(ProcessingStatus.Success(ObservationProvider.ClamPortal, DateTime.Now, DateTime.Now, 1));

            _kulProcessor.Setup(r => r.ProcessAsync(It.IsAny<IDictionary<int, ProcessedTaxon>>(), JobCancellationToken.Null))
                .ReturnsAsync(ProcessingStatus.Success(ObservationProvider.KUL, DateTime.Now, DateTime.Now, 1));

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
            var job = new ProcessJob(
                _darwinCoreRepository.Object,
                _processInfoRepository.Object,
                _harvestInfoRepository.Object,
                _clamPortalProcessor.Object,
                _kulProcessor.Object,
                _artportalenProcessor.Object,
                _taxonProcessedRepository.Object,
                _instanceManager.Object,
                _copyFieldMappingsJob.Object,
                _processTaxaJob.Object,
                _areaHelper.Object,
                _loggerMock.Object);

            var sources = (byte) ObservationProvider.Artportalen + (byte)ObservationProvider.ClamPortal + (byte)ObservationProvider.KUL;
            var result = await job.RunAsync(sources, false, false, true, JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
        }

        /// <summary>
        /// Test processing fail
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
            //-----------------------------------------------------------------------------------------------------------
            var job = new ProcessJob(
                _darwinCoreRepository.Object,
                _processInfoRepository.Object,
                _harvestInfoRepository.Object,
                _clamPortalProcessor.Object,
                _kulProcessor.Object,
                _artportalenProcessor.Object,
                _taxonProcessedRepository.Object,
                _instanceManager.Object,
                _copyFieldMappingsJob.Object,
                _processTaxaJob.Object,
                _areaHelper.Object,
                _loggerMock.Object);

            var result = await job.RunAsync(It.IsAny<int>(), false, false, true, JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }

        /// <summary>
        /// Test processing exception
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
            var job = new ProcessJob(
                _darwinCoreRepository.Object,
                _processInfoRepository.Object,
                _harvestInfoRepository.Object,
                _clamPortalProcessor.Object,
                _kulProcessor.Object,
                _artportalenProcessor.Object,
                _taxonProcessedRepository.Object,
                _instanceManager.Object,
                _copyFieldMappingsJob.Object,
                _processTaxaJob.Object,
                _areaHelper.Object,
                _loggerMock.Object);

            var result = await job.RunAsync(It.IsAny<int>(), false, false, true, JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }
    }
}