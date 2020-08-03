using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Harvesters;
using SOS.Import.Repositories.Destination.Artportalen;
using SOS.Import.Repositories.Source.Artportalen;
using SOS.Import.Services;
using SOS.Lib.Database;
using SOS.Lib.Enums;
using Xunit;

namespace SOS.Import.IntegrationTests.Harvesters
{
    public class AreaHarvesterIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task HarvestAllAreas_And_SaveToMongoDb()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var importConfiguration = GetImportConfiguration();
            var artportalenDataService = new ArtportalenDataService(importConfiguration.ArtportalenConfiguration);

            var verbatimDbConfiguration = GetVerbatimDbConfiguration();
            var areaVerbatimRepository = new AreaVerbatimRepository(
                new VerbatimClient(
                    verbatimDbConfiguration.GetMongoDbSettings(),
                    verbatimDbConfiguration.DatabaseName,
                    verbatimDbConfiguration.ReadBatchSize,
                    verbatimDbConfiguration.WriteBatchSize),
                new Mock<ILogger<AreaVerbatimRepository>>().Object);

            var areaHarvester = new AreaHarvester(
                new AreaRepository(artportalenDataService, new Mock<ILogger<AreaRepository>>().Object),
                areaVerbatimRepository,
                new Mock<ILogger<AreaHarvester>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await areaHarvester.HarvestAreasAsync();

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
            var importConfiguration = GetImportConfiguration();
            var artportalenDataService = new ArtportalenDataService(importConfiguration.ArtportalenConfiguration);
            var verbatimDbConfiguration = GetVerbatimDbConfiguration();
            var areaVerbatimRepository = new AreaVerbatimRepository(
                new VerbatimClient(
                    verbatimDbConfiguration.GetMongoDbSettings(),
                    verbatimDbConfiguration.DatabaseName,
                    verbatimDbConfiguration.ReadBatchSize,
                    verbatimDbConfiguration.WriteBatchSize),
                new Mock<ILogger<AreaVerbatimRepository>>().Object);

            var areaHarvester = new AreaHarvester(
                new AreaRepository(artportalenDataService, new Mock<ILogger<AreaRepository>>().Object),
                areaVerbatimRepository,
                new Mock<ILogger<AreaHarvester>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await areaHarvester.HarvestAreasAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(RunStatus.Success);
        }
    }
}