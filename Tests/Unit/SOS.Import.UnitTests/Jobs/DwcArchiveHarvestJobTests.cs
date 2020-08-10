using System;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Import.Jobs;
using SOS.Import.Managers.Interfaces;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using Xunit;

namespace SOS.Import.UnitTests.Managers
{
    public class DwcArchiveHarvestJobTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public DwcArchiveHarvestJobTests()
        {
            _dwcObservationHarvesterMock = new Mock<IDwcObservationHarvester>();
            _dataProviderManagerMock = new Mock<IDataProviderManager>();
            _harvestInfoRepositoryMock = new Mock<IHarvestInfoRepository>();
            _loggerMock = new Mock<ILogger<DwcArchiveHarvestJob>>();
        }

        private readonly Mock<IDwcObservationHarvester> _dwcObservationHarvesterMock;
        private readonly Mock<IDataProviderManager> _dataProviderManagerMock;
        private readonly Mock<IHarvestInfoRepository> _harvestInfoRepositoryMock;
        private readonly Mock<ILogger<DwcArchiveHarvestJob>> _loggerMock;

        private DwcArchiveHarvestJob TestObject => new DwcArchiveHarvestJob(
            _dwcObservationHarvesterMock.Object,
            _harvestInfoRepositoryMock.Object,
            _dataProviderManagerMock.Object,
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
            _dataProviderManagerMock.Setup(ts => ts.GetDataProviderByIdAsync(It.IsAny<int>()))
                .Throws<Exception>();
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Func<Task> act = async () => { await TestObject.RunAsync(0, "", JobCancellationToken.Null); };
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
            _dataProviderManagerMock.Setup(ts => ts.GetDataProviderByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new DataProvider());

            _dwcObservationHarvesterMock.Setup(ts =>
                    ts.HarvestObservationsAsync(It.IsAny<string>(), It.IsAny<DataProvider>(),
                        JobCancellationToken.Null))
                .ReturnsAsync(new HarvestInfo("id", DataProviderType.Taxa, DateTime.Now) {Status = RunStatus.Failed});

            _harvestInfoRepositoryMock.Setup(ts => ts.AddOrUpdateAsync(It.IsAny<HarvestInfo>()));
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Func<Task> act = async () => { await TestObject.RunAsync(0, "", JobCancellationToken.Null); };
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            await act.Should().ThrowAsync<Exception>();
        }

        /// <summary>
        ///     Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new DwcArchiveHarvestJob(
                null,
                _harvestInfoRepositoryMock.Object,
                _dataProviderManagerMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("dwcObservationHarvester");

            create = () => new DwcArchiveHarvestJob(
                _dwcObservationHarvesterMock.Object,
                null,
                _dataProviderManagerMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("harvestInfoRepository");

            create = () => new DwcArchiveHarvestJob(
                _dwcObservationHarvesterMock.Object,
                _harvestInfoRepositoryMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("dataProviderManager");

            create = () => new DwcArchiveHarvestJob(
                _dwcObservationHarvesterMock.Object,
                _harvestInfoRepositoryMock.Object,
                _dataProviderManagerMock.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

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
            _dataProviderManagerMock.Setup(ts => ts.GetDataProviderByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new DataProvider());

            _dwcObservationHarvesterMock.Setup(ts =>
                    ts.HarvestObservationsAsync(It.IsAny<string>(), It.IsAny<DataProvider>(),
                        JobCancellationToken.Null))
                .ReturnsAsync(new HarvestInfo("id", DataProviderType.Taxa, DateTime.Now) {Status = RunStatus.Success, Count = 1 });

            _harvestInfoRepositoryMock.Setup(ts => ts.AddOrUpdateAsync(It.IsAny<HarvestInfo>()));
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.RunAsync(0, "", JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
        }
    }
}