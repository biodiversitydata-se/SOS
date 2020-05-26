using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared;
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
    public class ProcessAreasJobTests
    {
        private readonly Mock<IAreaVerbatimRepository> _areaVerbatimRepositoryMock;
        private readonly Mock<IProcessedAreaRepository> _processedAreaRepositoryMock;
        private readonly Mock<IProcessInfoRepository> _processInfoRepository;
        private readonly Mock<IHarvestInfoRepository> _harvestInfoRepository;
        private readonly Mock<ILogger<ProcessAreasJob>> _loggerMock;

        private ProcessAreasJob TestObject => new ProcessAreasJob(
            _areaVerbatimRepositoryMock.Object,
            _processedAreaRepositoryMock.Object,
            _harvestInfoRepository.Object,
            _processInfoRepository.Object,
            _loggerMock.Object);

        /// <summary>
        /// Constructor
        /// </summary>
        public ProcessAreasJobTests()
        {
            _areaVerbatimRepositoryMock = new Mock<IAreaVerbatimRepository>();
            _processedAreaRepositoryMock = new Mock<IProcessedAreaRepository>();
            _processInfoRepository = new Mock<IProcessInfoRepository>();
            _harvestInfoRepository = new Mock<IHarvestInfoRepository>();
            _loggerMock = new Mock<ILogger<ProcessAreasJob>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new ProcessAreasJob(
                null,
                _processedAreaRepositoryMock.Object,
                _harvestInfoRepository.Object,
                _processInfoRepository.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("areaVerbatimRepository");

            create = () => new ProcessAreasJob(
                _areaVerbatimRepositoryMock.Object,
                null,
                _harvestInfoRepository.Object,
                _processInfoRepository.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processedAreaRepository");

            create = () => new ProcessAreasJob(
                _areaVerbatimRepositoryMock.Object,
                _processedAreaRepositoryMock.Object,
                null,
                _processInfoRepository.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("harvestInfoRepository");

            create = () => new ProcessAreasJob(
                _areaVerbatimRepositoryMock.Object,
                _processedAreaRepositoryMock.Object,
                _harvestInfoRepository.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processInfoRepository");

            create = () => new ProcessAreasJob(
                _areaVerbatimRepositoryMock.Object,
                _processedAreaRepositoryMock.Object,
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
            _areaVerbatimRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Area>
                {
                    new Area(AreaType.County) { Id = 16, Name = "Dalarna" },
                });

            _processedAreaRepositoryMock.Setup(r => r.DeleteCollectionAsync())
                .ReturnsAsync(true);

            _processedAreaRepositoryMock.Setup(r => r.AddManyAsync(It.IsAny<IEnumerable<Area>>()))
                .ReturnsAsync(true);

            _harvestInfoRepository.Setup(r => r.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HarvestInfo("ID", DataProviderType.Taxa, DateTime.Now){ Status = RunStatus.Success});

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
            _areaVerbatimRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Area>
                {
                    new Area(AreaType.County) { Id = 16, Name = "Dalarna" },
                });

            _processedAreaRepositoryMock.Setup(r => r.DeleteCollectionAsync())
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
            _areaVerbatimRepositoryMock.Setup(r => r.GetAllAsync())
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