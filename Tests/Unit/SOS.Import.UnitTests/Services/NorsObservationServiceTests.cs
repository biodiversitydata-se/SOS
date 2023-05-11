using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Harvest.Services;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using Xunit;

namespace SOS.Import.UnitTests.Services
{
    public class NorsObservationServiceTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public NorsObservationServiceTests()
        {
            _aquaSupportRequestServiceMock = new Mock<IAquaSupportRequestService>();
            _norsServiceConfiguration = new NorsServiceConfiguration
                { MaxNumberOfSightingsHarvested = 10 };
            _loggerMock = new Mock<ILogger<NorsObservationService>>();
        }
        private readonly Mock<IAquaSupportRequestService> _aquaSupportRequestServiceMock;
        private readonly NorsServiceConfiguration _norsServiceConfiguration;
        private readonly Mock<ILogger<NorsObservationService>> _loggerMock;

        private NorsObservationService TestObject => new NorsObservationService(
            _aquaSupportRequestServiceMock.Object,
            _norsServiceConfiguration,
            _loggerMock.Object);

        /// <summary>
        ///     Get clams observations fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetNorsObservationsAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _aquaSupportRequestServiceMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<long>(), It.IsAny<int>()))
                .Throws(new Exception("Exception"));
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Func<Task> act = async () => { await TestObject.GetAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 0); };
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            await act.Should().ThrowAsync<Exception>();
        }

        /// <summary>
        ///     Get clams observations success
        /// </summary>
        /// <returns></returns>
        [Fact(Skip = "Not working")]
        public async Task GetNorsObservationsAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            //TODO fix test file
            _aquaSupportRequestServiceMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<long>(), It.IsAny<int>()))
                .ReturnsAsync(new XDocument());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 0);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }
    }
}