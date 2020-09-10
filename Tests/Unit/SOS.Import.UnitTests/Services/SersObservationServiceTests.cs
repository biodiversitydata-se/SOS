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
    public class SersObservationServiceTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public SersObservationServiceTests()
        {
            _httpClientService = new Mock<IHttpClientService>();
            _sersServiceConfiguration = new SersServiceConfiguration
                {MaxNumberOfSightingsHarvested = 10, MaxReturnedChangesInOnePage = 10};
            _loggerMock = new Mock<ILogger<SersObservationService>>();
        }

        private readonly Mock<IHttpClientService> _httpClientService;
        private readonly SersServiceConfiguration _sersServiceConfiguration;
        private readonly Mock<ILogger<SersObservationService>> _loggerMock;

        private SersObservationService TestObject => new SersObservationService(
            _httpClientService.Object,
            _sersServiceConfiguration,
            _loggerMock.Object);

        /// <summary>
        ///     Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new SersObservationService(
                null,
                _sersServiceConfiguration,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should()
                .Be("httpClientService");

            create = () => new SersObservationService(
                _httpClientService.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("sersServiceConfiguration");

            create = () => new SersObservationService(
                _httpClientService.Object,
                _sersServiceConfiguration,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

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
            _httpClientService.Setup(s => s.GetFileStreamAsync(It.IsAny<Uri>(), null))
                .Throws(new Exception("Exception"));
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
        public async Task GetSersObservationsAsyncSuccess()
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