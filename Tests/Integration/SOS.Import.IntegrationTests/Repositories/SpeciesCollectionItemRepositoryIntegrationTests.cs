using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Repositories.Source.SpeciesPortal;
using SOS.Import.Services;
using Xunit;

namespace SOS.Import.IntegrationTests.Repositories
{
    public class SpeciesCollectionItemRepositoryIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task TestGetAllSightingSpeciesCollectionItems()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var importConfiguration = GetImportConfiguration();
            var speciesPortalDataService = new SpeciesPortalDataService(importConfiguration.SpeciesPortalConfiguration);
            var speciesCollectionItemRepository = new SpeciesCollectionItemRepository(
                speciesPortalDataService,
                new Mock<ILogger<SpeciesCollectionItemRepository>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var speciesCollectionItemEntities = await speciesCollectionItemRepository.GetAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            speciesCollectionItemEntities.Should().NotBeEmpty();
        }
    }
}