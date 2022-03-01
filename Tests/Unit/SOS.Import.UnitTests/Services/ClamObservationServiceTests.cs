using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Harvest.Services;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Models.Verbatim.ClamPortal;
using SOS.Lib.Services.Interfaces;
using Xunit;

namespace SOS.Import.UnitTests.Services
{
    public class ClamObservationServiceTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public ClamObservationServiceTests()
        {
            _httpClientService = new Mock<IHttpClientService>();
            _clamServiceConfiguration = new ClamServiceConfiguration {BaseAddress = "https://www.test.se"};
            _loggerMock = new Mock<ILogger<ClamObservationService>>();
        }

        private readonly Mock<IHttpClientService> _httpClientService;
        private readonly ClamServiceConfiguration _clamServiceConfiguration;
        private readonly Mock<ILogger<ClamObservationService>> _loggerMock;

        private ClamObservationService TestObject => new ClamObservationService(
            _httpClientService.Object,
            _clamServiceConfiguration,
            _loggerMock.Object);

        /// <summary>
        ///     Get clams observations fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetClamObservationsAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetClamObservationsAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Count().Should().Be(0);
        }

        /// <summary>
        ///     Get clams observations success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetClamObservationsAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _httpClientService.Setup(hc => hc.GetDataAsync<IEnumerable<ClamObservationVerbatim>>(It.IsAny<Uri>()))
                .ReturnsAsync(new[]
                {
                    new ClamObservationVerbatim
                    {
                        DyntaxaTaxonId = 100024
                    }
                });

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetClamObservationsAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Count().Should().Be(1);
        }
    }
}