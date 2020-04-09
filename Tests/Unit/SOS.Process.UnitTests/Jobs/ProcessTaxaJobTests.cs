using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Enums;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Jobs;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;
using Xunit;

namespace SOS.Process.UnitTests.Jobs
{
    /// <summary>
    /// Tests for activate instance job
    /// </summary>
    public class ProcessTaxaJobTests
    {
        private readonly Mock<ITaxonVerbatimRepository> _taxonVerbatimRepositoryMock;
        private readonly Mock<IProcessedTaxonRepository> _processedTaxonRepositoryMock;
        private readonly Mock<IProcessInfoRepository> _processInfoRepository;
        private readonly Mock<IHarvestInfoRepository> _harvestInfoRepository;
        private readonly Mock<ILogger<ProcessTaxaJob>> _loggerMock;

        private ProcessTaxaJob TestObject => new ProcessTaxaJob(
            _taxonVerbatimRepositoryMock.Object,
            _processedTaxonRepositoryMock.Object,
            _harvestInfoRepository.Object,
            _processInfoRepository.Object,
            _loggerMock.Object);

        /// <summary>
        /// Constructor
        /// </summary>
        public ProcessTaxaJobTests()
        {
            _taxonVerbatimRepositoryMock = new Mock<ITaxonVerbatimRepository>();
            _processedTaxonRepositoryMock = new Mock<IProcessedTaxonRepository>();
            _processInfoRepository = new Mock<IProcessInfoRepository>();
            _harvestInfoRepository = new Mock<IHarvestInfoRepository>();
            _loggerMock = new Mock<ILogger<ProcessTaxaJob>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new ProcessTaxaJob(
                null,
                _processedTaxonRepositoryMock.Object,
                _harvestInfoRepository.Object,
                _processInfoRepository.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("taxonVerbatimRepository");

            create = () => new ProcessTaxaJob(
                _taxonVerbatimRepositoryMock.Object,
                null,
                _harvestInfoRepository.Object,
                _processInfoRepository.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processedTaxonRepository");

            create = () => new ProcessTaxaJob(
                _taxonVerbatimRepositoryMock.Object,
                _processedTaxonRepositoryMock.Object,
                null,
                _processInfoRepository.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("harvestInfoRepository");

            create = () => new ProcessTaxaJob(
                _taxonVerbatimRepositoryMock.Object,
                _processedTaxonRepositoryMock.Object,
                _harvestInfoRepository.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processInfoRepository");

            create = () => new ProcessTaxaJob(
                _taxonVerbatimRepositoryMock.Object,
                _processedTaxonRepositoryMock.Object,
                _harvestInfoRepository.Object,
                _processInfoRepository.Object,
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
            _taxonVerbatimRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<DarwinCoreTaxon>
                {
                    new DarwinCoreTaxon { Id = 100024, ScientificName = "Canus Lupus" },
                });

            _processedTaxonRepositoryMock.Setup(r => r.DeleteCollectionAsync())
                .ReturnsAsync(true);

            _processedTaxonRepositoryMock.Setup(r => r.AddManyAsync(It.IsAny<IEnumerable<ProcessedTaxon>>()))
                .ReturnsAsync(true);

            _harvestInfoRepository.Setup(r => r.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HarvestInfo("ID", DataSet.Taxa, DateTime.Now){ Status = RunStatus.Success});

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
            _taxonVerbatimRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<DarwinCoreTaxon>
                {
                    new DarwinCoreTaxon { Id = 100024, ScientificName = "Canus Lupus" },
                });

            _processedTaxonRepositoryMock.Setup(r => r.DeleteCollectionAsync())
                .ReturnsAsync(false);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.RunAsync();
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
            _taxonVerbatimRepositoryMock.Setup(r => r.GetAllAsync())
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
    }
}