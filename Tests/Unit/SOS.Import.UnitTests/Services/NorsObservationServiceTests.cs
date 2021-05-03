using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Services;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Services.Interfaces;
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
            _httpClientService = new Mock<IHttpClientService>();
            _norsServiceConfiguration = new NorsServiceConfiguration
                { MaxNumberOfSightingsHarvested = 10, MaxReturnedChangesInOnePage = 10 };
            _loggerMock = new Mock<ILogger<NorsObservationService>>();
        }

        private readonly Mock<IHttpClientService> _httpClientService;
        private readonly NorsServiceConfiguration _norsServiceConfiguration;
        private readonly Mock<ILogger<NorsObservationService>> _loggerMock;

        private NorsObservationService TestObject => new NorsObservationService(
            _httpClientService.Object,
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
            _httpClientService.Setup(s => s.GetFileStreamAsync(It.IsAny<Uri>(), null))
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
        public async Task GetNorsObservationsAsyncSuccess()
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