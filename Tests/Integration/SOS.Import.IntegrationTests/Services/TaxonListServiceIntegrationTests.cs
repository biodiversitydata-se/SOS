using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Import.Services;
using SOS.Lib.Services;
using Xunit;

namespace SOS.Import.IntegrationTests.Services
{
    public class TaxonListServiceIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task Get_taxon_list_definitions()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var importConfiguration = GetImportConfiguration();
            var taxonListService = new TaxonListService(
                new HttpClientService(new Mock<ILogger<HttpClientService>>().Object), 
                importConfiguration.TaxonListServiceConfiguration,
                new NullLogger<TaxonListService>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = (await taxonListService.GetDefinitionsAsync());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task Get_taxa_for_two_conservation_lists()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var importConfiguration = GetImportConfiguration();
            var taxonListService = new TaxonListService(
                new HttpClientService(new Mock<ILogger<HttpClientService>>().Object),
                importConfiguration.TaxonListServiceConfiguration,
                new NullLogger<TaxonListService>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = (await taxonListService.GetTaxaAsync(new []{34,35}));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().NotBeNull();
        }
    }
}