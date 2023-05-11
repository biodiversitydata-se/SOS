using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Harvest.Harvesters.VirtualHerbarium;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using Xunit;

namespace SOS.Import.UnitTests.Harvesters.Observations
{
    public class VirtualHerbariumObservationHarvesterTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public VirtualHerbariumObservationHarvesterTests()
        {
            _virtualHerbariumObservationVerbatimRepositoryMock =
                new Mock<IVirtualHerbariumObservationVerbatimRepository>();
            _virtualHerbariumObservationServiceMock = new Mock<IVirtualHerbariumObservationService>();
            _virtualHerbariumServiceConfiguration = new VirtualHerbariumServiceConfiguration
                {MaxNumberOfSightingsHarvested = 1};
            _loggerMock = new Mock<ILogger<VirtualHerbariumObservationHarvester>>();
        }

        private readonly Mock<IVirtualHerbariumObservationVerbatimRepository>
            _virtualHerbariumObservationVerbatimRepositoryMock;

        private readonly Mock<IVirtualHerbariumObservationService> _virtualHerbariumObservationServiceMock;
        private readonly VirtualHerbariumServiceConfiguration _virtualHerbariumServiceConfiguration;
        private readonly Mock<ILogger<VirtualHerbariumObservationHarvester>> _loggerMock;

        private VirtualHerbariumObservationHarvester TestObject => new VirtualHerbariumObservationHarvester(
            _virtualHerbariumObservationServiceMock.Object,
            _virtualHerbariumObservationVerbatimRepositoryMock.Object,
            _virtualHerbariumServiceConfiguration,
            _loggerMock.Object);

        /// <summary>
        ///     Test aggregation fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HarvestVirtualHerbariumAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _virtualHerbariumObservationVerbatimRepositoryMock.Setup(cts => cts.DeleteCollectionAsync())
                .ThrowsAsync(new Exception("Fail"));

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.HarvestObservationsAsync(JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Status.Should().Be(RunStatus.Failed);
        }

        /// <summary>
        ///     Make a successful virtualHerbariums harvest
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HarvestVirtualHerbariumAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _virtualHerbariumObservationVerbatimRepositoryMock.Setup(tr => tr.DeleteCollectionAsync())
                .ReturnsAsync(true);
            _virtualHerbariumObservationVerbatimRepositoryMock.Setup(tr => tr.AddCollectionAsync())
                .ReturnsAsync(true);
            XDocument doc = null;
            _virtualHerbariumObservationServiceMock.Setup(cts => cts.GetLocalitiesAsync())
                .ReturnsAsync(doc);
            _virtualHerbariumObservationServiceMock.Setup(cts => cts.GetAsync(It.IsAny<DateTime>(), 0, It.IsAny<int>()))
                .ReturnsAsync(doc);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.HarvestObservationsAsync(JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Status.Should().Be(RunStatus.Success);
        }
    }
}