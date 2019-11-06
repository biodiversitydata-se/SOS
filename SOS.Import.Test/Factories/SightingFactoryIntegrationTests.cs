using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
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
        public async Task Test_GetOneHundredThousandSightingsFromArtportalen_And_SaveToMongoDb()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
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

            SpeciesPortalSightingFactory sightingFactory = new SpeciesPortalSightingFactory(
                metadataRepository,
                projectRepository,
                sightingRepository,
                siteRepositoryMock.Object,
                //siteRepository,
                sightingVerbatimRepository,
                personRepository, 
                organizationRepository,
                sightingRelationRepository,
                speciesCollectionItemRepository,
                new Mock<ILogger<SpeciesPortalSightingFactory>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var res = await sightingFactory.HarvestSightingsAsync(new SpeciesPortalAggregationOptions
            {
                ChunkSize = 125000,
                MaxNumberOfSightingsHarvested = 100000
            });

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            res.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task Test_GetOneHundredThousandSightingsFromArtportalen_WithoutSaveToMongoDb()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SpeciesPortalDataService speciesPortalDataService = new SpeciesPortalDataService(new ConnectionStrings
            {
                SpeciesPortal = ArtportalenTestServerConnectionString
            });
            
            var sightingVerbatimRepositoryMock = new Mock<ISightingVerbatimRepository>();
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

            SpeciesPortalSightingFactory sightingFactory = new SpeciesPortalSightingFactory(
                metadataRepository,
                projectRepository,
                sightingRepository,
                siteRepositoryMock.Object,
                //siteRepository,
                sightingVerbatimRepositoryMock.Object,
                personRepository,
                organizationRepository,
                sightingRelationRepository,
                speciesCollectionItemRepository,
                new Mock<ILogger<SpeciesPortalSightingFactory>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var res = await sightingFactory.HarvestSightingsAsync(new SpeciesPortalAggregationOptions
            {
                ChunkSize = 125000,
                MaxNumberOfSightingsHarvested = 100000
            });

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            res.Should().BeTrue();
        }

    }
}
