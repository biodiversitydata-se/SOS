using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MvmService;
using SOS.Import.Services;
using SOS.Import.Services.Interfaces;
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
        ///     Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new NorsObservationService(
                null,
                _norsServiceConfiguration,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should()
                .Be("httpClientService");

            create = () => new NorsObservationService(
                _httpClientService.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("norsServiceConfiguration");

            create = () => new NorsObservationService(
                _httpClientService.Object,
                _norsServiceConfiguration,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

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
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetAsync(0);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
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