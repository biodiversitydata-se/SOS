using System;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Harvest.Harvesters.Shark;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using Xunit;

namespace SOS.Import.UnitTests.Harvesters.Observations
{
    public class SharkObservationHarvesterTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public SharkObservationHarvesterTests()
        {
            _sharkObservationVerbatimRepositoryMock = new Mock<ISharkObservationVerbatimRepository>();
            _sharkObservationServiceMock = new Mock<ISharkObservationService>();
            _sharkServiceConfiguration = new SharkServiceConfiguration {MaxNumberOfSightingsHarvested = 1};
            _loggerMock = new Mock<ILogger<SharkObservationHarvester>>();
        }

        private readonly Mock<ISharkObservationVerbatimRepository> _sharkObservationVerbatimRepositoryMock;
        private readonly Mock<ISharkObservationService> _sharkObservationServiceMock;
        private readonly SharkServiceConfiguration _sharkServiceConfiguration;
        private readonly Mock<ILogger<SharkObservationHarvester>> _loggerMock;

        private SharkObservationHarvester TestObject => new SharkObservationHarvester(
            _sharkObservationServiceMock.Object,
            _sharkObservationVerbatimRepositoryMock.Object,
            _sharkServiceConfiguration,
            _loggerMock.Object);

        /// <summary>
        ///     Test aggregation fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HarvestSharkAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _sharkObservationServiceMock.Setup(cts => cts.GetAsync(It.IsAny<string>()))
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
    }
}