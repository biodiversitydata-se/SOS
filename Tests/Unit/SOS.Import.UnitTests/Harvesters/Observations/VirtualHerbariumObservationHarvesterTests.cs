using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Harvesters.Observations;
using SOS.Import.Repositories.Destination.VirtualHerbarium.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
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
        ///     Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new VirtualHerbariumObservationHarvester(
                null,
                _virtualHerbariumObservationVerbatimRepositoryMock.Object,
                _virtualHerbariumServiceConfiguration,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should()
                .Be("virtualHerbariumObservationService");

            create = () => new VirtualHerbariumObservationHarvester(
                _virtualHerbariumObservationServiceMock.Object,
                null,
                _virtualHerbariumServiceConfiguration,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should()
                .Be("virtualHerbariumObservationVerbatimRepository");

            create = () => new VirtualHerbariumObservationHarvester(
                _virtualHerbariumObservationServiceMock.Object,
                _virtualHerbariumObservationVerbatimRepositoryMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should()
                .Be("virtualHerbariumServiceConfiguration");

            create = () => new VirtualHerbariumObservationHarvester(
                _virtualHerbariumObservationServiceMock.Object,
                _virtualHerbariumObservationVerbatimRepositoryMock.Object,
                _virtualHerbariumServiceConfiguration,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

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