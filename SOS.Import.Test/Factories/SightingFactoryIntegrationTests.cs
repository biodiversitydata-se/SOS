using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using SOS.Import.Entities;
using SOS.Import.Factories;
using SOS.Import.Models;
using SOS.Import.MongoDb;
using SOS.Import.Repositories.Destination.SpeciesPortal;
using SOS.Import.Repositories.Destination.SpeciesPortal.Interfaces;
using SOS.Import.Repositories.Source.SpeciesPortal;
using SOS.Import.Repositories.Source.SpeciesPortal.Interfaces;
using SOS.Import.Services;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Configuration.Shared;
using Xunit;

namespace SOS.Import.Test.Factories
{
    public class SightingFactoryIntegrationTests
    {
        private const string ArtportalenTestServerConnectionString = "Server=artsql2-4;Database=SpeciesObservationSwe_debugremote;Trusted_Connection=True;MultipleActiveResultSets=true";
        private const string MongoDbDatabaseName = "sos-verbatim";
        private const string MongoDbConnectionString = "localhost";
        private const int MongoDbAddBatchSize = 1000;

        [Fact]
        [Trait("Category","Integration")]
        public async Task TestGetAllData()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ConnectionStrings connectionStrings = new ConnectionStrings();
            connectionStrings.SpeciesPortal = ArtportalenTestServerConnectionString;
            SpeciesPortalDataService speciesPortalDataService = new SpeciesPortalDataService(new ConnectionStrings
            {
                SpeciesPortal = ArtportalenTestServerConnectionString
            });
            MongoClientSettings mongoClientSettings = new MongoClientSettings
            {
                Server = new MongoServerAddress(MongoDbConnectionString)
            };

            var importClient = new ImportClient(mongoClientSettings, MongoDbDatabaseName, MongoDbAddBatchSize);
            SightingVerbatimRepository sightingVerbatimRepository = new SightingVerbatimRepository(importClient, new Mock<ILogger<SightingVerbatimRepository>>().Object);
            IMetadataRepository metadataRepository = new MetadataRepository(speciesPortalDataService, new Mock<ILogger<MetadataRepository>>().Object);
            IProjectRepository projectRepository = new ProjectRepository(speciesPortalDataService, new Mock<ILogger<ProjectRepository>>().Object);
            ISightingRepository sightingRepository = new SightingRepository(speciesPortalDataService, new Mock<ILogger<SightingRepository>>().Object);
            PersonRepository personRepository = new PersonRepository(speciesPortalDataService, new Mock<ILogger<PersonRepository>>().Object);
            OrganizationRepository organizationRepository = new OrganizationRepository(speciesPortalDataService, new Mock<ILogger<OrganizationRepository>>().Object);
            SightingRelationRepository sightingRelationRepository = new SightingRelationRepository(speciesPortalDataService, new Mock<ILogger<SightingRelationRepository>>().Object);
            SpeciesCollectionItemRepository speciesCollectionItemRepository = new SpeciesCollectionItemRepository(speciesPortalDataService, new Mock<ILogger<SpeciesCollectionItemRepository>>().Object);
            //ISiteRepository siteRepository = new SiteRepository(speciesPortalDataService, new Mock<ILogger<SiteRepository>>().Object);
            var siteRepositoryMock = new Mock<ISiteRepository>();
            siteRepositoryMock.Setup(foo => foo.GetAsync()).ReturnsAsync(new List<SiteEntity>());
            var areaRepositoryMock = new Mock<IAreaRepository>();
            areaRepositoryMock.Setup(foo => foo.GetAsync()).ReturnsAsync(new List<AreaEntity>());
            var areaVerbatimRepositoryMock = new Mock<IAreaVerbatimRepository>();

            SpeciesPortalSightingFactory sightingFactory = new SpeciesPortalSightingFactory(
                areaRepositoryMock.Object,
                metadataRepository,
                projectRepository,
                sightingRepository,
                siteRepositoryMock.Object,
                //siteRepository,
                areaVerbatimRepositoryMock.Object,
                sightingVerbatimRepository,
                personRepository, 
                organizationRepository,
                sightingRelationRepository,
                speciesCollectionItemRepository,
                new Mock<ILogger<SpeciesPortalSightingFactory>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var res = await sightingFactory.AggregateAsync(new SpeciesPortalAggregationOptions
            {
                AddSightingsToVerbatimDatabase = true,
                ChunkSize = 100000,
                MaxNumberOfSightingsHarvested = 100000
            });

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            res.Should().BeTrue();
        }
    }
}
