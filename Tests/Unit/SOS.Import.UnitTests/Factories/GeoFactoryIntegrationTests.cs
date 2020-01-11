using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Factories;
using SOS.Import.MongoDb;
using SOS.Import.Repositories.Destination;
using SOS.Import.Repositories.Destination.Kul;
using SOS.Import.Repositories.Destination.SpeciesPortal;
using SOS.Import.Repositories.Destination.SpeciesPortal.Interfaces;
using SOS.Import.Repositories.Source.SpeciesPortal;
using SOS.Import.Repositories.Source.SpeciesPortal.Interfaces;
using SOS.Import.Services;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using Xunit;

namespace SOS.Import.Test.Factories
{
    public class GeoFactoryIntegrationTests : TestBase
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
                    importConfiguration.MongoDbConfiguration.GetMongoDbSettings(),
                    importConfiguration.MongoDbConfiguration.DatabaseName,
                    importConfiguration.MongoDbConfiguration.BatchSize),
                new Mock<ILogger<AreaVerbatimRepository>>().Object);
            var processConfig = GetProcessConfiguration().ProcessedDbConfiguration;
           
            var geoFactory = new GeoFactory(
                new AreaRepository(speciesPortalDataService, new Mock<ILogger<AreaRepository>>().Object),
                areaVerbatimRepository,
                new Mock<ILogger<GeoFactory>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await geoFactory.HarvestAreasAsync();

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
                    importConfiguration.MongoDbConfiguration.GetMongoDbSettings(),
                    importConfiguration.MongoDbConfiguration.DatabaseName,
                    importConfiguration.MongoDbConfiguration.BatchSize),
                new Mock<ILogger<AreaVerbatimRepository>>().Object);

            var processConfig = GetProcessConfiguration().ProcessedDbConfiguration;

            var geoFactory = new GeoFactory(
                new AreaRepository(speciesPortalDataService, new Mock<ILogger<AreaRepository>>().Object),
                areaVerbatimRepository,
                new Mock<ILogger<GeoFactory>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await geoFactory.HarvestAreasAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(RunStatus.Success);
        }
    }
}
