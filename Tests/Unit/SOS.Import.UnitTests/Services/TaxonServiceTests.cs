using System;
using System.Threading.Tasks;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Services;
using SOS.Lib.Configuration.Import;
using Xunit;

namespace SOS.Import.Test.Services
{
    public class TaxonServiceTests
    {
        private readonly TaxonServiceConfiguration _taxonServiceConfiguration;
        private readonly Mock<ILogger<TaxonService>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public TaxonServiceTests()
        {
            _taxonServiceConfiguration = new TaxonServiceConfiguration { BaseAddress = "https://taxonservice.artdata.slu.se/DarwinCore/DarwinCoreArchiveFile" };
            _loggerMock = new Mock<ILogger<TaxonService>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new TaxonService(
                _taxonServiceConfiguration,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new TaxonService(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("taxonServiceConfiguration");

            create = () => new TaxonService(
                _taxonServiceConfiguration,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Get taxa success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetTaxaAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var TaxonService = new TaxonService(
                _taxonServiceConfiguration,
                _loggerMock.Object);

            var result = await TaxonService.GetTaxaAsync();
          
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Count().Should().BeGreaterThan(0);
        }

        /// <summary>
        /// Get taxa fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetTaxaAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------


            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var TaxonService = new TaxonService(
                new TaxonServiceConfiguration{BaseAddress = "Tom"}, 
                _loggerMock.Object);

            var result = await TaxonService.GetTaxaAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }
    }
}
