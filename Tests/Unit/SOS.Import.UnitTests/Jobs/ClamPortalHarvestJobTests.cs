using System;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Import.Jobs;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using Xunit;

namespace SOS.Import.UnitTests.Managers
{
    public class ClamPortalHarvestJobTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public ClamPortalHarvestJobTests()
        {
            _clamPortalObservationHarvesterMock = new Mock<IClamPortalObservationHarvester>();
            _harvestInfoRepositoryMock = new Mock<IHarvestInfoRepository>();
            _loggerMock = new Mock<ILogger<ClamPortalHarvestJob>>();
        }

        private readonly Mock<IClamPortalObservationHarvester> _clamPortalObservationHarvesterMock;
        private readonly Mock<IHarvestInfoRepository> _harvestInfoRepositoryMock;
        private readonly Mock<ILogger<ClamPortalHarvestJob>> _loggerMock;

        private ClamPortalHarvestJob TestObject => new ClamPortalHarvestJob(
            _clamPortalObservationHarvesterMock.Object,
            _harvestInfoRepositoryMock.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Harvest job throw exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddDataProviderException()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _clamPortalObservationHarvesterMock.Setup(ts => ts.HarvestClamsAsync(JobCancellationToken.Null))
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

        /// <summary>
        ///     Fail to run harvest job
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddDataProviderFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _clamPortalObservationHarvesterMock.Setup(ts => ts.HarvestClamsAsync(JobCancellationToken.Null))
                .ReturnsAsync(new HarvestInfo("id", DataProviderType.Taxa, DateTime.Now) {Status = RunStatus.Failed});

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

        //    Action create = () => new ClamPortalHarvestJob(
        //       null,
        //        _harvestInfoRepositoryMock.Object, TODO,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("clamPortalObservationHarvester");

        //    create = () => new ClamPortalHarvestJob(
        //        _clamPortalObservationHarvesterMock.Object,
        //       null, TODO,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("harvestInfoRepository");

        //    create = () => new ClamPortalHarvestJob(
        //        _clamPortalObservationHarvesterMock.Object,
        //        _harvestInfoRepositoryMock.Object, TODO,
        //        null);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        //}

        /// <summary>
        ///     Run harvest job successfully
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RunAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _clamPortalObservationHarvesterMock.Setup(ts => ts.HarvestClamsAsync(JobCancellationToken.Null))
                .ReturnsAsync(new HarvestInfo("id", DataProviderType.Taxa, DateTime.Now) {Status = RunStatus.Success, Count = 1 });

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
    }
}