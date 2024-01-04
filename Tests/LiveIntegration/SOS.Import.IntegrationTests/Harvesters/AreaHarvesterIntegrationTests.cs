﻿using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Harvest.Harvesters;
using SOS.Harvest.Services;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Database;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Import.LiveIntegrationTests.Harvesters
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
            var artportalenDataService = new ArtportalenDataService(importConfiguration.ArtportalenConfiguration, new NullLogger<ArtportalenDataService>());

            var verbatimDbConfiguration = GetVerbatimDbConfiguration();
            var areaVerbatimRepository = new Lib.Repositories.Resource.AreaRepository(
                new ProcessClient(
                    verbatimDbConfiguration.GetMongoDbSettings(),
                    verbatimDbConfiguration.DatabaseName,
                    verbatimDbConfiguration.ReadBatchSize,
                    verbatimDbConfiguration.WriteBatchSize),
                new Mock<ILogger<Lib.Repositories.Resource.AreaRepository>>().Object);
            var cacheManager = new Mock<CacheManager>();
            cacheManager.Setup(cm => cm.ClearAsync(Cache.Area)).ReturnsAsync(true);

            var areaHarvester = new AreaHarvester(
                new Harvest.Repositories.Source.Artportalen.AreaRepository(artportalenDataService, new Mock<ILogger<Harvest.Repositories.Source.Artportalen.AreaRepository>>().Object),
                areaVerbatimRepository,
                new AreaHelper(new Mock<IAreaRepository>().Object),
                new GeoRegionApiService(new GeoRegionApiConfiguration { ApiUrl = "https://georegionapi-dev.artdata.slu.se/" }),
                cacheManager.Object,
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
            var artportalenDataService = new ArtportalenDataService(importConfiguration.ArtportalenConfiguration, new NullLogger<ArtportalenDataService>());
            var verbatimDbConfiguration = GetVerbatimDbConfiguration();
            var areaVerbatimRepository = new Lib.Repositories.Resource.AreaRepository(
                new ProcessClient(
                    verbatimDbConfiguration.GetMongoDbSettings(),
                    verbatimDbConfiguration.DatabaseName,
                    verbatimDbConfiguration.ReadBatchSize,
                    verbatimDbConfiguration.WriteBatchSize),
                new Mock<ILogger<Lib.Repositories.Resource.AreaRepository>>().Object);

            var cacheManagerMock = new Mock<ICacheManager>();
            cacheManagerMock.Setup(cm => cm.ClearAsync(Cache.Area)).ReturnsAsync(true);

            var areaHarvester = new AreaHarvester(
                new Harvest.Repositories.Source.Artportalen.AreaRepository(artportalenDataService, new Mock<ILogger<Harvest.Repositories.Source.Artportalen.AreaRepository>>().Object),
                areaVerbatimRepository,
                new AreaHelper(new Mock<IAreaRepository>().Object),
                new GeoRegionApiService(new GeoRegionApiConfiguration { ApiUrl = "https://georegionapi-dev.artdata.slu.se/" }),
                cacheManagerMock.Object,
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