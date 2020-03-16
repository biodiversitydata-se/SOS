using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Factories;
using SOS.Import.MongoDb;
using SOS.Import.Repositories.Destination.SpeciesPortal;
using SOS.Import.Repositories.Source.SpeciesPortal;
using SOS.Import.Services;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using Xunit;

namespace SOS.Import.IntegrationTests.Factories
{
    public class AreaFactoryIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task HarvestAllAreas_And_SaveToMongoDb()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ImportConfiguration importConfiguration = GetImportConfiguration();
            var speciesPortalDataService = new SpeciesPortalDataService(importConfiguration.SpeciesPortalConfiguration);
            var areaVerbatimRepository = new AreaVerbatimRepository(
                new ImportClient(
                    importConfiguration.VerbatimDbConfiguration.GetMongoDbSettings(),
                    importConfiguration.VerbatimDbConfiguration.DatabaseName,
                    importConfiguration.VerbatimDbConfiguration.BatchSize),
                new Mock<ILogger<AreaVerbatimRepository>>().Object);

            var areaFactory = new AreaFactory(
                new AreaRepository(speciesPortalDataService, new Mock<ILogger<AreaRepository>>().Object),
                areaVerbatimRepository,
                new Mock<ILogger<AreaFactory>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await areaFactory.HarvestAreasAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(RunStatus.Success);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task HarvestAllAreas_WithoutSavingToMongoDb()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ImportConfiguration importConfiguration = GetImportConfiguration();
            var speciesPortalDataService = new SpeciesPortalDataService(importConfiguration.SpeciesPortalConfiguration);
            var areaVerbatimRepository = new AreaVerbatimRepository(
                new ImportClient(
                    importConfiguration.VerbatimDbConfiguration.GetMongoDbSettings(),
                    importConfiguration.VerbatimDbConfiguration.DatabaseName,
                    importConfiguration.VerbatimDbConfiguration.BatchSize),
                new Mock<ILogger<AreaVerbatimRepository>>().Object);

            var areaFactory = new AreaFactory(
                new AreaRepository(speciesPortalDataService, new Mock<ILogger<AreaRepository>>().Object),
                areaVerbatimRepository,
                new Mock<ILogger<AreaFactory>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await areaFactory.HarvestAreasAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(RunStatus.Success);
        }
    }
}
