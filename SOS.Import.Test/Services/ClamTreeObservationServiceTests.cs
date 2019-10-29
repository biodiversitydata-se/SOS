using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Services;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Models.Verbatim.ClamTreePortal;
using Xunit;

namespace SOS.Import.Test.Services
{
    public class ClamTreeObservationServiceTests
    {
        private readonly Mock<IHttpClientService> _httpClientService;
        private readonly ClamTreeServiceConfiguration _clamTreeServiceConfiguration;
        private readonly Mock<ILogger<ClamTreeObservationService>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public ClamTreeObservationServiceTests()
        {
            _httpClientService = new Mock<IHttpClientService>();
            _clamTreeServiceConfiguration = new ClamTreeServiceConfiguration{BaseAddress = "https://www.test.se"};
            _loggerMock = new Mock<ILogger<ClamTreeObservationService>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new ClamTreeObservationService(
                _httpClientService.Object,
                _clamTreeServiceConfiguration,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new ClamTreeObservationService(
                null,
                _clamTreeServiceConfiguration,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("httpClientService");

            create = () => new ClamTreeObservationService(
                _httpClientService.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("clamTreeServiceConfiguration");

            create = () => new ClamTreeObservationService(
                _httpClientService.Object,
                _clamTreeServiceConfiguration,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Get clams observations success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetClamObservationsAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _httpClientService.Setup(hc => hc.GetDataAsync<IEnumerable<ClamObservationVerbatim>>(It.IsAny<Uri>()))
                .ReturnsAsync(new[] { new ClamObservationVerbatim
                {
                    DyntaxaTaxonId = 100024
                }  });

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var clamTreeObservationService = new ClamTreeObservationService(
                _httpClientService.Object,
                _clamTreeServiceConfiguration,
                _loggerMock.Object);

            var result = await clamTreeObservationService.GetClamObservationsAsync();
          
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Count().Should().Be(1);
        }

        /// <summary>
        /// Get clams observations fail
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
            var clamTreeObservationService = new ClamTreeObservationService(
                _httpClientService.Object,
                _clamTreeServiceConfiguration,
                _loggerMock.Object);

            var result = await clamTreeObservationService.GetClamObservationsAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Count().Should().Be(0);
        }

        /// <summary>
        /// Get tree observations success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetTreeObservationsAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _httpClientService.Setup(hc => hc.GetDataAsync<IEnumerable<TreeObservationVerbatim>>(It.IsAny<Uri>()))
                .ReturnsAsync(new[] { new TreeObservationVerbatim
                {
                    DyntaxaTaxonId = 100024
                }  });

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var clamTreeObservationService = new ClamTreeObservationService(
                _httpClientService.Object,
                _clamTreeServiceConfiguration,
                _loggerMock.Object);

            var result = await clamTreeObservationService.GetTreeObservationsAsync(It.IsAny<int>(), It.IsAny<int>());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Count().Should().Be(1);
        }

        /// <summary>
        /// Get tree observations fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetTreeObservationsAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------


            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var clamTreeObservationService = new ClamTreeObservationService(
                _httpClientService.Object,
                _clamTreeServiceConfiguration,
                _loggerMock.Object);

            var result = await clamTreeObservationService.GetTreeObservationsAsync(It.IsAny<int>(), It.IsAny<int>());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Count().Should().Be(0);
        }
    }
}
