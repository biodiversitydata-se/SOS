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
using SOS.Import.Configuration;
using SOS.Import.Entities;
using SOS.Import.Factories;
using SOS.Import.Models;
using SOS.Import.Repositories.Destination.SpeciesPortal;
using SOS.Import.Repositories.Source.SpeciesPortal;
using SOS.Import.Repositories.Source.SpeciesPortal.Interfaces;
using SOS.Import.Services;
using Xunit;

namespace SOS.Import.Test.Factories
{
    public class SightingFactoryIntegrationTests
    {
        private const string ArtportalenTestServerConnectionString = "Server=artsql2-4;Database=SpeciesObservationSwe_debugremote;Trusted_Connection=True;MultipleActiveResultSets=true";
        private const string MongoDbDatabaseName = "sos-verbatim";
        private const string MongoDbConnectionString = "mongodb://localhost";

        [Fact]
        [Trait("Category","Integration")]
        public async Task TestGetAllData()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var configurationDictionary = new Dictionary<string, string>
            {
                { "ConnectionStrings:SpeciesPortal", ArtportalenTestServerConnectionString }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationDictionary)
                .Build();

            SpeciesPortalDataService speciesPortalDataService = new SpeciesPortalDataService(configuration);
            MongoDbConfiguration mongoDbConfiguration = new MongoDbConfiguration
            {
                DatabaseName = MongoDbDatabaseName, AddBatchSize = 1000
            };
            IOptions<MongoDbConfiguration> mongoDbOptions = Options.Create(mongoDbConfiguration);
            IMongoClient mongoClient = new MongoClient(MongoDbConnectionString);
            SightingVerbatimRepository sightingVerbatimRepository = new SightingVerbatimRepository(mongoClient, mongoDbOptions, new Mock<ILogger<SightingVerbatimRepository>>().Object);
            IMetadataRepository metadataRepository = new MetadataRepository(speciesPortalDataService, new Mock<ILogger<MetadataRepository>>().Object);
            IProjectRepository projectRepository = new ProjectRepository(speciesPortalDataService, new Mock<ILogger<ProjectRepository>>().Object);
            ISightingRepository sightingRepository = new SightingRepository(speciesPortalDataService, new Mock<ILogger<SightingRepository>>().Object);
            PersonRepository personRepository = new PersonRepository(speciesPortalDataService, new Mock<ILogger<PersonRepository>>().Object);
            OrganizationRepository organizationRepository = new OrganizationRepository(speciesPortalDataService, new Mock<ILogger<OrganizationRepository>>().Object);
            SightingRelationRelationRepository sightingRelationRelationRepository = new SightingRelationRelationRepository(speciesPortalDataService, new Mock<ILogger<SightingRelationRelationRepository>>().Object);
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
                sightingRelationRelationRepository,
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
