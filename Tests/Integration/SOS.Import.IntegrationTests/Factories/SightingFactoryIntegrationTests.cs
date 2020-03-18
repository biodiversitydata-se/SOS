using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Entities;
using SOS.Import.Factories;
using SOS.Import.MongoDb;
using SOS.Import.ObservationHarvesters;
using SOS.Import.Repositories.Destination.Artportalen;
using SOS.Import.Repositories.Destination.Artportalen.Interfaces;
using SOS.Import.Repositories.Source.Artportalen;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Import.Services;
using SOS.Lib.Enums;
using Xunit;

namespace SOS.Import.IntegrationTests.Factories
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
            importConfiguration.ArtportalenConfiguration.ChunkSize = 125000;
            importConfiguration.ArtportalenConfiguration.MaxNumberOfSightingsHarvested = 100000;

            var artportalenDataService = new ArtportalenDataService(importConfiguration.ArtportalenConfiguration);
            var importClient = new ImportClient(
                importConfiguration.VerbatimDbConfiguration.GetMongoDbSettings(),
                importConfiguration.VerbatimDbConfiguration.DatabaseName,
                importConfiguration.VerbatimDbConfiguration.BatchSize);
            var sightingVerbatimRepository = new SightingVerbatimRepository(importClient, new Mock<ILogger<SightingVerbatimRepository>>().Object);
            var metadataRepository = new MetadataRepository(artportalenDataService, new Mock<ILogger<MetadataRepository>>().Object);
            var projectRepository = new ProjectRepository(artportalenDataService, new Mock<ILogger<ProjectRepository>>().Object);
            var sightingRepository = new SightingRepository(artportalenDataService, new Mock<ILogger<SightingRepository>>().Object);
            var personRepository = new PersonRepository(artportalenDataService, new Mock<ILogger<PersonRepository>>().Object);
            var organizationRepository = new OrganizationRepository(artportalenDataService, new Mock<ILogger<OrganizationRepository>>().Object);
            var sightingRelationRepository = new SightingRelationRepository(artportalenDataService, new Mock<ILogger<SightingRelationRepository>>().Object);
            var speciesCollectionItemRepository = new SpeciesCollectionItemRepository(artportalenDataService, new Mock<ILogger<SpeciesCollectionItemRepository>>().Object);
            var siteRepositoryMock = new Mock<ISiteRepository>();
            siteRepositoryMock.Setup(foo => foo.GetAsync()).ReturnsAsync(new List<SiteEntity>());

            ArtportalenObservationHarvester observationHarvester = new ArtportalenObservationHarvester(
                importConfiguration.ArtportalenConfiguration,
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
                new Mock<ILogger<ArtportalenObservationHarvester>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var res = await observationHarvester.HarvestSightingsAsync(JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            res.Status.Should().Be(RunStatus.Success);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task HarvestOneHundredThousandSightingsFromArtportalen_WithoutSavingToMongoDb()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var importConfiguration = GetImportConfiguration();
            importConfiguration.ArtportalenConfiguration.ChunkSize = 125000;
            importConfiguration.ArtportalenConfiguration.MaxNumberOfSightingsHarvested = 100000;
            var artportalenDataService = new ArtportalenDataService(importConfiguration.ArtportalenConfiguration);
            var sightingVerbatimRepositoryMock = new Mock<ISightingVerbatimRepository>();
            IMetadataRepository metadataRepository = new MetadataRepository(artportalenDataService, new Mock<ILogger<MetadataRepository>>().Object);
            IProjectRepository projectRepository = new ProjectRepository(artportalenDataService, new Mock<ILogger<ProjectRepository>>().Object);
            ISightingRepository sightingRepository = new SightingRepository(artportalenDataService, new Mock<ILogger<SightingRepository>>().Object);
            PersonRepository personRepository = new PersonRepository(artportalenDataService, new Mock<ILogger<PersonRepository>>().Object);
            OrganizationRepository organizationRepository = new OrganizationRepository(artportalenDataService, new Mock<ILogger<OrganizationRepository>>().Object);
            SightingRelationRepository sightingRelationRepository = new SightingRelationRepository(artportalenDataService, new Mock<ILogger<SightingRelationRepository>>().Object);
            SpeciesCollectionItemRepository speciesCollectionItemRepository = new SpeciesCollectionItemRepository(artportalenDataService, new Mock<ILogger<SpeciesCollectionItemRepository>>().Object);
            var siteRepositoryMock = new Mock<ISiteRepository>();
            siteRepositoryMock.Setup(foo => foo.GetAsync()).ReturnsAsync(new List<SiteEntity>());

            ArtportalenObservationHarvester observationHarvester = new ArtportalenObservationHarvester(
                importConfiguration.ArtportalenConfiguration,
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
                new Mock<ILogger<ArtportalenObservationHarvester>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var res = await observationHarvester.HarvestSightingsAsync(JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            res.Status.Should().Be(RunStatus.Success);
        }
    }
}