using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Enums;
using  SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Factories.Interfaces;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Jobs;
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
        private readonly Mock<IProcessedSightingRepository> _darwinCoreRepository;
        private readonly Mock<IProcessInfoRepository> _processInfoRepository;
        private readonly Mock<IHarvestInfoRepository> _harvestInfoRepository;
        private readonly Mock<IClamPortalProcessFactory> _clamPortalProcessFactory;
        private readonly Mock<IKulProcessFactory> _kulProcessFactory;
        private readonly Mock<ISpeciesPortalProcessFactory> _speciesPortalProcessFactory;
        private readonly Mock<ITaxonProcessedRepository> _taxonProcessedRepository;
        private readonly Mock<IAreaHelper> _areaHelper;
        private readonly Mock<ILogger<ProcessJob>> _loggerMock;



        /// <summary>
        /// Constructor
        /// </summary>
        public ProcessJobTests()
        {
            _darwinCoreRepository = new Mock<IProcessedSightingRepository>();
            _processInfoRepository = new Mock<IProcessInfoRepository>();
            _harvestInfoRepository = new Mock<IHarvestInfoRepository>();
            _clamPortalProcessFactory = new Mock<IClamPortalProcessFactory>();
            _kulProcessFactory = new Mock<IKulProcessFactory>();
            _speciesPortalProcessFactory = new Mock<ISpeciesPortalProcessFactory>();
            _taxonProcessedRepository = new Mock<ITaxonProcessedRepository>();
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
                _clamPortalProcessFactory.Object,
                _kulProcessFactory.Object,
                _speciesPortalProcessFactory.Object,
                _taxonProcessedRepository.Object,
                _areaHelper.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new ProcessJob(
                null,
                _processInfoRepository.Object,
                _harvestInfoRepository.Object,
                _clamPortalProcessFactory.Object,
                _kulProcessFactory.Object,
                _speciesPortalProcessFactory.Object,
                _taxonProcessedRepository.Object,
                _areaHelper.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("darwinCoreRepository");

            create = () => new ProcessJob(
                _darwinCoreRepository.Object,
                null,
                _harvestInfoRepository.Object,
                _clamPortalProcessFactory.Object,
                _kulProcessFactory.Object,
                _speciesPortalProcessFactory.Object,
                _taxonProcessedRepository.Object,
                _areaHelper.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processInfoRepository");

            create = () => new ProcessJob(
                _darwinCoreRepository.Object,
                _processInfoRepository.Object,
                null,
                _clamPortalProcessFactory.Object,
                _kulProcessFactory.Object,
                _speciesPortalProcessFactory.Object,
                _taxonProcessedRepository.Object,
                _areaHelper.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("harvestInfoRepository");

            create = () => new ProcessJob(
                _darwinCoreRepository.Object,
                _processInfoRepository.Object,
                _harvestInfoRepository.Object,
                null,
                _kulProcessFactory.Object,
                _speciesPortalProcessFactory.Object,
                _taxonProcessedRepository.Object,
                _areaHelper.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("clamPortalProcessFactory");

            create = () => new ProcessJob(
                _darwinCoreRepository.Object,
                _processInfoRepository.Object,
                _harvestInfoRepository.Object,
                _clamPortalProcessFactory.Object,
                null,
                _speciesPortalProcessFactory.Object,
                _taxonProcessedRepository.Object,
                _areaHelper.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("kulProcessFactory");

            create = () => new ProcessJob(
                _darwinCoreRepository.Object,
                _processInfoRepository.Object,
                _harvestInfoRepository.Object,
                _clamPortalProcessFactory.Object,
                _kulProcessFactory.Object,
                null,
                _taxonProcessedRepository.Object,
                _areaHelper.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("speciesPortalProcessFactory");

            create = () => new ProcessJob(
                _darwinCoreRepository.Object,
                _processInfoRepository.Object,
                _harvestInfoRepository.Object,
                _clamPortalProcessFactory.Object,
                _kulProcessFactory.Object,
                _speciesPortalProcessFactory.Object,
                null,
                _areaHelper.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("taxonVerbatimRepository");

            create = () => new ProcessJob(
                _darwinCoreRepository.Object,
                _processInfoRepository.Object,
                _harvestInfoRepository.Object,
                _clamPortalProcessFactory.Object,
                _kulProcessFactory.Object,
                _speciesPortalProcessFactory.Object,
                _taxonProcessedRepository.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("areaHelper");

            create = () => new ProcessJob(
                _darwinCoreRepository.Object,
                _processInfoRepository.Object,
                _harvestInfoRepository.Object,
                _clamPortalProcessFactory.Object,
                _kulProcessFactory.Object,
                _speciesPortalProcessFactory.Object,
                _taxonProcessedRepository.Object,
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
            _taxonProcessedRepository.Setup(r => r.GetTaxaAsync())
                .ReturnsAsync(new []
                {
                    new ProcessedTaxon() {Id = 100024, ScientificName = "Canus Lupus"},
                });

            _darwinCoreRepository.Setup(r => r.VerifyCollectionAsync());

            _harvestInfoRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new []
                {
                    new HarvestInfo("0", DataProvider.Artdatabanken, DateTime.Now)
                });

            _speciesPortalProcessFactory.Setup(r => r.ProcessAsync(It.IsAny<IDictionary<int, ProcessedTaxon>>(), JobCancellationToken.Null))
                .ReturnsAsync(new RunInfo(DataProvider.Artdatabanken) {Count = 1, Status = RunStatus.Success, Start = DateTime.Now, End = DateTime.Now});

            _clamPortalProcessFactory.Setup(r => r.ProcessAsync(It.IsAny<IDictionary<int, ProcessedTaxon>>(), JobCancellationToken.Null))
                .ReturnsAsync(new RunInfo(DataProvider.ClamPortal) { Count = 1, Status = RunStatus.Success, Start = DateTime.Now, End = DateTime.Now });

            _kulProcessFactory.Setup(r => r.ProcessAsync(It.IsAny<IDictionary<int, ProcessedTaxon>>(), JobCancellationToken.Null))
                .ReturnsAsync(new RunInfo(DataProvider.KUL) { Count = 1, Status = RunStatus.Success, Start = DateTime.Now, End = DateTime.Now });

            _darwinCoreRepository.Setup(r => r.DropIndexAsync());
            _darwinCoreRepository.Setup(r => r.CreateIndexAsync());

            _darwinCoreRepository.Setup(r => r.SetActiveInstanceAsync(It.IsAny<byte>()));

            _processInfoRepository.Setup(r => r.VerifyCollectionAsync());

            _processInfoRepository.Setup(r => r.GetAsync(It.IsAny<byte>()))
                .ReturnsAsync(new ProcessInfo(It.IsAny<byte>()));

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
                _clamPortalProcessFactory.Object,
                _kulProcessFactory.Object,
                _speciesPortalProcessFactory.Object,
                _taxonProcessedRepository.Object,
                _areaHelper.Object,
                _loggerMock.Object);

            var sources = (byte) DataProvider.Artdatabanken + (byte) DataProvider.ClamPortal + (byte) DataProvider.KUL;
            var result = await job.RunAsync(sources, true, JobCancellationToken.Null);
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
                _clamPortalProcessFactory.Object,
                _kulProcessFactory.Object,
                _speciesPortalProcessFactory.Object,
                _taxonProcessedRepository.Object,
                _areaHelper.Object,
                _loggerMock.Object);

            var result = await job.RunAsync(It.IsAny<int>(), true, JobCancellationToken.Null);
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
            _taxonProcessedRepository.Setup(r => r.GetTaxaAsync())
                .ThrowsAsync(new Exception("Failed"));
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var job = new ProcessJob(
                _darwinCoreRepository.Object,
                _processInfoRepository.Object,
                _harvestInfoRepository.Object,
                _clamPortalProcessFactory.Object,
                _kulProcessFactory.Object,
                _speciesPortalProcessFactory.Object,
                _taxonProcessedRepository.Object,
                _areaHelper.Object,
                _loggerMock.Object);

            var result = await job.RunAsync(It.IsAny<int>(), true, JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }
    }
}
