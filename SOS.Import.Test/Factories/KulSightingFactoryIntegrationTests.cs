using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
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
using Xunit;

namespace SOS.Import.Test.Factories
{
    public class KulSightingFactoryIntegrationTests
    {
        private const string MongoDbDatabaseName = "sos-verbatim";
        private const string MongoDbConnectionString = "localhost";
        private const int MongoDbAddBatchSize = 1000;

        [Fact]
        [Trait("Category", "Integration")]
        public async Task Test_GetTenThousandObservations_FromKulProvider_And_SaveToMongoDb()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            MongoClientSettings mongoClientSettings = new MongoClientSettings
            {
                Server = new MongoServerAddress(MongoDbConnectionString)
            };
            var importClient = new ImportClient(mongoClientSettings, MongoDbDatabaseName, MongoDbAddBatchSize);
            var kulSightingRepository = new KulSightingRepository();
            var kulSightingVerbatimRepository = new KulSightingVerbatimRepository(importClient, new Mock<ILogger<KulSightingVerbatimRepository>>().Object);
            var kulSightingFactory = new KulSightingFactory(
                kulSightingRepository,
                kulSightingVerbatimRepository,
                new Mock<ILogger<KulSightingFactory>>().Object);
            

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await kulSightingFactory.AggregateAsync(
                new KulAggregationOptions { MaxNumberOfSightingsHarvested = 10000 });

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task Test_GetTenThousandObservations_FromKulProvider_WithoutSaveToMongoDb()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var kulSightingRepository = new KulSightingRepository();
            Mock<IKulSightingVerbatimRepository> kulSightingVerbatimRepositoryMock = new Mock<IKulSightingVerbatimRepository>();
            var kulSightingFactory = new KulSightingFactory(
                kulSightingRepository,
                kulSightingVerbatimRepositoryMock.Object,
                new Mock<ILogger<KulSightingFactory>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await kulSightingFactory.AggregateAsync(
                new KulAggregationOptions { MaxNumberOfSightingsHarvested = 10000 });

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }
    }
}