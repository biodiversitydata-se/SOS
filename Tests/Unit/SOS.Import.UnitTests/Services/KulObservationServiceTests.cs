using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Services;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Services.Interfaces;
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
            _httpClientService = new Mock<IHttpClientService>();
            _clamServiceConfiguration = new KulServiceConfiguration
            {
                MaxNumberOfSightingsHarvested = 10, MaxReturnedChangesInOnePage = 10,
                StartHarvestYear = DateTime.Now.Year
            };
            _loggerMock = new Mock<ILogger<KulObservationService>>();
        }

        private readonly Mock<IHttpClientService> _httpClientService;
        private readonly KulServiceConfiguration _clamServiceConfiguration;
        private readonly Mock<ILogger<KulObservationService>> _loggerMock;

        private KulObservationService TestObject => new KulObservationService(
            _httpClientService.Object,
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
            _httpClientService.Setup(s => s.GetFileStreamAsync(It.IsAny<Uri>(), It.IsAny<Dictionary<string, string>>()))
                .Throws(new Exception("Exception"));
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Func<Task> act = async () => { await TestObject.GetAsync(0); };
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
            _httpClientService.Setup(s => s.GetFileStreamAsync(It.IsAny<Uri>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(new FileStream("", FileMode.Open));

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetAsync(0);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }
    }
}