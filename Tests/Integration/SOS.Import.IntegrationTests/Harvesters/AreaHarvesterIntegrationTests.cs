using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Harvesters;
using SOS.Import.Services;
using SOS.Lib.Database;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;
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
            var areaVerbatimRepository = new Lib.Repositories.Resource.AreaRepository(
                new ProcessClient(
                    verbatimDbConfiguration.GetMongoDbSettings(),
                    verbatimDbConfiguration.DatabaseName,
                    verbatimDbConfiguration.ReadBatchSize,
                    verbatimDbConfiguration.WriteBatchSize),
                new Mock<ILogger<Lib.Repositories.Resource.AreaRepository>>().Object);

            var areaHarvester = new AreaHarvester(
                new Import.Repositories.Source.Artportalen.AreaRepository(artportalenDataService, new Mock<ILogger<Import.Repositories.Source.Artportalen.AreaRepository>>().Object),
                areaVerbatimRepository,
                new AreaHelper(new Mock<IAreaRepository>().Object, new Mock<IVocabularyRepository>().Object), 
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
            var areaVerbatimRepository = new Lib.Repositories.Resource.AreaRepository(
                new ProcessClient(
                    verbatimDbConfiguration.GetMongoDbSettings(),
                    verbatimDbConfiguration.DatabaseName,
                    verbatimDbConfiguration.ReadBatchSize,
                    verbatimDbConfiguration.WriteBatchSize),
                new Mock<ILogger<Lib.Repositories.Resource.AreaRepository>>().Object);

            var areaHarvester = new AreaHarvester(
                new Import.Repositories.Source.Artportalen.AreaRepository(artportalenDataService, new Mock<ILogger<Import.Repositories.Source.Artportalen.AreaRepository>>().Object),
                areaVerbatimRepository,
                new AreaHelper(new Mock<IAreaRepository>().Object, new Mock<IVocabularyRepository>().Object),
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