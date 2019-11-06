using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using SOS.Import.Factories;
using SOS.Import.Models;
using SOS.Import.MongoDb;
using SOS.Import.Repositories.Destination.Kul;
using SOS.Import.Repositories.Destination.Kul.Interfaces;
using SOS.Import.Repositories.Source.Kul;
using SOS.Import.Repositories.Source.Kul.Interfaces;
using SOS.Import.Test.Repositories;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Configuration.Shared;
using Xunit;

namespace SOS.Import.Test.Factories
{
    public class KulSightingFactoryIntegrationTests
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task Test_GetTenThousandObservations_FromKulProvider_And_SaveToMongoDb()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ImportConfiguration importConfiguration = GetImportConfiguration();

            var kulSightingRepository = new KulSightingRepository(
                new Mock<ILogger<KulSightingRepository>>().Object, 
                importConfiguration.KulServiceConfiguration);
            
            var kulSightingVerbatimRepository = new KulSightingVerbatimRepository(
                new ImportClient(
                    importConfiguration.MongoDbConfiguration.GetMongoDbSettings(),
                    importConfiguration.MongoDbConfiguration.DatabaseName,
                    importConfiguration.MongoDbConfiguration.BatchSize), 
                new Mock<ILogger<KulSightingVerbatimRepository>>().Object);
            
            var kulSightingFactory = new KulSightingFactory(
                kulSightingRepository,
                kulSightingVerbatimRepository,
                new Mock<ILogger<KulSightingFactory>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await kulSightingFactory.AggregateAsync(
                new KulAggregationOptions
                {
                    StartHarvestYear = 2015,
                    MaxNumberOfSightingsHarvested = 10000
                });

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task Test_GetTenThousandObservations_FromKulProvider_WithoutSavingToMongoDb()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ImportConfiguration importConfiguration = GetImportConfiguration();
            var kulSightingFactory = new KulSightingFactory(
                new KulSightingRepository(
                    new Mock<ILogger<KulSightingRepository>>().Object,
                    importConfiguration.KulServiceConfiguration),
                new Mock<IKulSightingVerbatimRepository>().Object,
                new Mock<ILogger<KulSightingFactory>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await kulSightingFactory.AggregateAsync(
                new KulAggregationOptions
                {
                    StartHarvestYear = 2015,
                    MaxNumberOfSightingsHarvested = 10000
                });

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }

        private ImportConfiguration GetImportConfiguration()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<KulRepositoryIntegrationTests>()
                .Build();

            ImportConfiguration importConfiguration = config.GetSection(typeof(ImportConfiguration).Name).Get<ImportConfiguration>();
            return importConfiguration;
        }
    }
}