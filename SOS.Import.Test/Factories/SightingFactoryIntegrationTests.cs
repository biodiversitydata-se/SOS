using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
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
    public class SightingFactoryIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category","Integration")]
        public async Task HarvestOneHundredThousandSightingsFromArtportalen_And_SaveToMongoDb()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var importConfiguration = GetImportConfiguration();
            importConfiguration.SpeciesPortalConfiguration.ChunkSize = 125000;
            importConfiguration.SpeciesPortalConfiguration.MaxNumberOfSightingsHarvested = 100000;

            var speciesPortalDataService = new SpeciesPortalDataService(importConfiguration.SpeciesPortalConfiguration);
            var importClient = new ImportClient(
                importConfiguration.MongoDbConfiguration.GetMongoDbSettings(),
                importConfiguration.MongoDbConfiguration.DatabaseName,
                importConfiguration.MongoDbConfiguration.BatchSize);
            var sightingVerbatimRepository = new SightingVerbatimRepository(importClient, new Mock<ILogger<SightingVerbatimRepository>>().Object);
            var metadataRepository = new MetadataRepository(speciesPortalDataService, new Mock<ILogger<MetadataRepository>>().Object);
            var projectRepository = new ProjectRepository(speciesPortalDataService, new Mock<ILogger<ProjectRepository>>().Object);
            var sightingRepository = new SightingRepository(speciesPortalDataService, new Mock<ILogger<SightingRepository>>().Object);
            var personRepository = new PersonRepository(speciesPortalDataService, new Mock<ILogger<PersonRepository>>().Object);
            var organizationRepository = new OrganizationRepository(speciesPortalDataService, new Mock<ILogger<OrganizationRepository>>().Object);
            var sightingRelationRepository = new SightingRelationRepository(speciesPortalDataService, new Mock<ILogger<SightingRelationRepository>>().Object);
            var speciesCollectionItemRepository = new SpeciesCollectionItemRepository(speciesPortalDataService, new Mock<ILogger<SpeciesCollectionItemRepository>>().Object);
            //ISiteRepository siteRepository = new SiteRepository(speciesPortalDataService, new Mock<ILogger<SiteRepository>>().Object);
            var siteRepositoryMock = new Mock<ISiteRepository>();
            siteRepositoryMock.Setup(foo => foo.GetAsync()).ReturnsAsync(new List<SiteEntity>());

            SpeciesPortalSightingFactory sightingFactory = new SpeciesPortalSightingFactory(
                importConfiguration.SpeciesPortalConfiguration,
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
            var res = await sightingFactory.HarvestSightingsAsync(JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            res.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task HarvestOneHundredThousandSightingsFromArtportalen_WithoutSavingToMongoDb()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var importConfiguration = GetImportConfiguration();
            importConfiguration.SpeciesPortalConfiguration.ChunkSize = 125000;
            importConfiguration.SpeciesPortalConfiguration.MaxNumberOfSightingsHarvested = 100000;
            var speciesPortalDataService = new SpeciesPortalDataService(importConfiguration.SpeciesPortalConfiguration);
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
                importConfiguration.SpeciesPortalConfiguration,
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
            var res = await sightingFactory.HarvestSightingsAsync(JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            res.Should().BeTrue();
        }
    }
}