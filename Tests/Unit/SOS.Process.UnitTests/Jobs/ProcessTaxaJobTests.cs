using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Jobs;
using SOS.Process.Processors.Taxon.Interfaces;
using Xunit;

namespace SOS.Process.UnitTests.Jobs
{
    /// <summary>
    ///     Tests for activate instance job
    /// </summary>
    public class ProcessTaxaJobTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public ProcessTaxaJobTests()
        {
            _taxonProcessorMock = new Mock<ITaxonProcessor>();
            _processInfoRepository = new Mock<IProcessInfoRepository>();
            _harvestInfoRepository = new Mock<IHarvestInfoRepository>();
            _loggerMock = new Mock<ILogger<ProcessTaxaJob>>();
        }

        private readonly Mock<ITaxonProcessor> _taxonProcessorMock;
        private readonly Mock<IProcessInfoRepository> _processInfoRepository;
        private readonly Mock<IHarvestInfoRepository> _harvestInfoRepository;
        private readonly Mock<ILogger<ProcessTaxaJob>> _loggerMock;

        private ProcessTaxaJob TestObject => new ProcessTaxaJob(
            _taxonProcessorMock.Object,
            _harvestInfoRepository.Object,
            _processInfoRepository.Object,
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
            _taxonProcessorMock.Setup(r => r.ProcessTaxaAsync())
                .Throws<Exception>();
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Func<Task> act = async () => { await TestObject.RunAsync(); };
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
            _taxonProcessorMock.Setup(r => r.ProcessTaxaAsync())
                .ReturnsAsync(-1);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Func<Task> run =
                () => TestObject.RunAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            run.Should().Throw<Exception>()
                .WithMessage("Process taxa job failed");
        }

        /// <summary>
        ///     Make a successful test of processing
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RunAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _taxonProcessorMock.Setup(r => r.ProcessTaxaAsync())
                .ReturnsAsync(1);

            _harvestInfoRepository.Setup(r => r.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HarvestInfo("ID", DataProviderType.Taxa, DateTime.Now) {Status = RunStatus.Success});

            _processInfoRepository.Setup(r => r.VerifyCollectionAsync());

            _processInfoRepository.Setup(r => r.AddOrUpdateAsync(It.IsAny<ProcessInfo>()));

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.RunAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
        }
    }
}