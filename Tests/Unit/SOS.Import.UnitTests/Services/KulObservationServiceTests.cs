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

namespace SOS.Process.UnitTests.Services
{
    public class KulObservationServiceTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public KulObservationServiceTests()
        {
            _aquaSupportRequestServiceMock = new Mock<IAquaSupportRequestService>();
            _clamServiceConfiguration = new KulServiceConfiguration
            {
                MaxNumberOfSightingsHarvested = 10,
                StartHarvestYear = DateTime.Now.Year
            };
            _loggerMock = new Mock<ILogger<KulObservationService>>();
        }

        private readonly Mock<IAquaSupportRequestService> _aquaSupportRequestServiceMock;
        private readonly KulServiceConfiguration _clamServiceConfiguration;
        private readonly Mock<ILogger<KulObservationService>> _loggerMock;

        private KulObservationService TestObject => new KulObservationService(
            _aquaSupportRequestServiceMock.Object,
            _clamServiceConfiguration,
            _loggerMock.Object);

        /// <summary>
        ///     Get clams observations fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetKulObservationsAsyncFail()
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
        public async Task GetKulObservationsAsyncSuccess()
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