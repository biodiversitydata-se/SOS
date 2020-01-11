using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Repositories.Source.SpeciesPortal;
using SOS.Import.Services;
using Xunit;

namespace SOS.Import.IntegrationTests.Repositories
{
    public class AreaRepositoryIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task TestGetAllAreas()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var importConfiguration = GetImportConfiguration();
            var speciesPortalDataService = new SpeciesPortalDataService(importConfiguration.SpeciesPortalConfiguration);
            var areaRepository = new AreaRepository(
                speciesPortalDataService,
                new Mock<ILogger<AreaRepository>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var areaEntities = await areaRepository.GetAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            areaEntities.Should().NotBeEmpty();
        }
    }
}
