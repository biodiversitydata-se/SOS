using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SOS.Import.Models.TaxonAttributeService;
using SOS.Import.Services;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using Xunit;

namespace SOS.Import.UnitTests.Services
{
    public class TaxonAttributeServiceTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public TaxonAttributeServiceTests()
        {
            _httpClientServiceMock = new Mock<IHttpClientService>();
            _taxonAttributeServiceConfiguration = new TaxonAttributeServiceConfiguration
                {BaseAddress = "https://taxonattribute-service.artdata.alu.se"};
            _loggerMock = new Mock<ILogger<TaxonAttributeService>>();
        }

        private readonly Mock<IHttpClientService> _httpClientServiceMock;
        private readonly TaxonAttributeServiceConfiguration _taxonAttributeServiceConfiguration;
        private readonly Mock<ILogger<TaxonAttributeService>> _loggerMock;

        private TaxonAttributeService TestObject => new TaxonAttributeService(
            _httpClientServiceMock.Object,
            _taxonAttributeServiceConfiguration,
            _loggerMock.Object);

        /// <summary>
        ///     Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new TaxonAttributeService(
                null,
                _taxonAttributeServiceConfiguration,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("httpClientService");

            create = () => new TaxonAttributeService(
                _httpClientServiceMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should()
                .Be("taxonAttributeServiceConfiguration");

            create = () => new TaxonAttributeService(
                _httpClientServiceMock.Object,
                _taxonAttributeServiceConfiguration,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        ///     Failed to get correct red list period id
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetCurrentRedlistPeriodIdAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var redListId = 3;
            var redlistPeriod = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(new {id = 0}));

            _httpClientServiceMock.Setup(ts =>
                    ts.GetDataAsync<dynamic>(It.IsAny<Uri>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(redlistPeriod);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------

            var result = await TestObject.GetCurrentRedlistPeriodIdAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().NotBe(redListId);
        }

        /// <summary>
        ///     Get current red list period success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetCurrentRedlistPeriodIdAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var redListId = 3;
            var redlistPeriod = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(new {id = redListId}));

            _httpClientServiceMock.Setup(ts =>
                    ts.GetDataAsync<dynamic>(It.IsAny<Uri>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(redlistPeriod);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------

            var result = await TestObject.GetCurrentRedlistPeriodIdAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().Be(redListId);
        }

        /// <summary>
        ///     Get current red list period throws an exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetCurrentRedlistPeriodIdAsyncThrow()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _httpClientServiceMock.Setup(ts =>
                    ts.GetDataAsync<dynamic>(It.IsAny<Uri>(), It.IsAny<Dictionary<string, string>>()))
                .Throws(new Exception());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------

            var result = await TestObject.GetCurrentRedlistPeriodIdAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().Be(-1);
        }

        /// <summary>
        ///     Failed call to get taxon attribute
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetGetTaxonAttributesAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _httpClientServiceMock.Setup(ts =>
                    ts.PostDataAsync<IEnumerable<TaxonAttributeModel>>(It.IsAny<Uri>(), It.IsAny<object>(),
                        It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(new TaxonAttributeModel[0]);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------

            var result = await TestObject.GetTaxonAttributesAsync(new[] {0}, new[] {0}, new[] {0});

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(0);
        }

        /// <summary>
        ///     Successful call to get taxon attribute
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetGetTaxonAttributesAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _httpClientServiceMock.Setup(ts =>
                    ts.PostDataAsync<IEnumerable<TaxonAttributeModel>>(It.IsAny<Uri>(), It.IsAny<object>(),
                        It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(new[]
                {
                    new TaxonAttributeModel {CategoryId = 3}
                });

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------

            var result = await TestObject.GetTaxonAttributesAsync(new[] {0}, new[] {0}, new[] {0});

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(1);
        }

        /// <summary>
        ///     Call to get taxon attribute throws
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetGetTaxonAttributesAsyncThrows()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _httpClientServiceMock.Setup(ts =>
                    ts.PostDataAsync<IEnumerable<TaxonAttributeModel>>(It.IsAny<Uri>(), It.IsAny<object>(),
                        It.IsAny<Dictionary<string, string>>()))
                .Throws(new Exception());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------

            var result = await TestObject.GetTaxonAttributesAsync(new[] {0}, new[] {0}, new[] {0});

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }
    }
}