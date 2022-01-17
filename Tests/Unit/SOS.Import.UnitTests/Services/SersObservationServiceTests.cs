using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Services;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using Xunit;

namespace SOS.Import.UnitTests.Services
{
    public class SersObservationServiceTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public SersObservationServiceTests()
        {
            _aquaSupportRequestServiceMock = new Mock<IAquaSupportRequestService>();
            _sersServiceConfiguration = new SersServiceConfiguration
                {MaxNumberOfSightingsHarvested = 10, MaxReturnedChangesInOnePage = 10};
            _loggerMock = new Mock<ILogger<SersObservationService>>();
        }

        private readonly Mock<IAquaSupportRequestService> _aquaSupportRequestServiceMock;
        private readonly SersServiceConfiguration _sersServiceConfiguration;
        private readonly Mock<ILogger<SersObservationService>> _loggerMock;

        private SersObservationService TestObject => new SersObservationService(
            _aquaSupportRequestServiceMock.Object,
            _sersServiceConfiguration,
            _loggerMock.Object);

        /// <summary>
        ///     Get clams observations fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetSersObservationsAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _aquaSupportRequestServiceMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<long>()))
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
        public async Task GetSersObservationsAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            //TODO fix test file
            _aquaSupportRequestServiceMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<long>()))
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