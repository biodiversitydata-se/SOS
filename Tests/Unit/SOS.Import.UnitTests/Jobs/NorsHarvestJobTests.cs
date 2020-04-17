using System;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Import.Jobs;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Shared;
using Xunit;

namespace SOS.Import.UnitTests.Managers
{
    public class NorsHarvestJobTests
    {
        private readonly Mock<INorsObservationHarvester> _norsObservationHarvesterMock;
        private readonly Mock<IHarvestInfoRepository> _harvestInfoRepositoryMock;
        private readonly Mock<ILogger<NorsHarvestJob>> _loggerMock;

        private NorsHarvestJob TestObject => new NorsHarvestJob(
            _norsObservationHarvesterMock.Object,
            _harvestInfoRepositoryMock.Object,
            _loggerMock.Object);

        /// <summary>
        /// Constructor
        /// </summary>
        public NorsHarvestJobTests()
        {
            _norsObservationHarvesterMock = new Mock<INorsObservationHarvester>();
            _harvestInfoRepositoryMock = new Mock<IHarvestInfoRepository>();
            _loggerMock = new Mock<ILogger<NorsHarvestJob>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new NorsHarvestJob(
               null,
                _harvestInfoRepositoryMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("norsObservationHarvester");

            create = () => new NorsHarvestJob(
                _norsObservationHarvesterMock.Object,
               null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("harvestInfoRepository");

            create = () => new NorsHarvestJob(
                _norsObservationHarvesterMock.Object,
                _harvestInfoRepositoryMock.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Run harvest job successfully
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RunAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _norsObservationHarvesterMock.Setup(ts => ts.HarvestObservationsAsync(JobCancellationToken.Null))
                .ReturnsAsync(new HarvestInfo("id", DataSet.Taxa, DateTime.Now){ Status = RunStatus.Success});

            _harvestInfoRepositoryMock.Setup(ts => ts.AddOrUpdateAsync(It.IsAny<HarvestInfo>()));
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.RunAsync(JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
        }

        /// <summary>
        /// Fail to run harvest job
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddDataProviderFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _norsObservationHarvesterMock.Setup(ts => ts.HarvestObservationsAsync(JobCancellationToken.Null))
                .ReturnsAsync(new HarvestInfo("id", DataSet.Taxa, DateTime.Now) { Status = RunStatus.Failed });

            _harvestInfoRepositoryMock.Setup(ts => ts.AddOrUpdateAsync(It.IsAny<HarvestInfo>()));
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
        /// Harvest job throw exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddDataProviderException()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _norsObservationHarvesterMock.Setup(ts => ts.HarvestObservationsAsync(JobCancellationToken.Null))
               .Throws<Exception>();
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Func<Task> act = async () => { await TestObject.RunAsync(JobCancellationToken.Null); };
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            await act.Should().ThrowAsync<Exception>();
        }

    }
}
