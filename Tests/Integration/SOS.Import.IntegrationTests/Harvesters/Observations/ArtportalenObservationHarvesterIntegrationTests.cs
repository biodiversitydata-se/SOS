using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Containers;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Harvesters.Observations;
using SOS.Import.Repositories.Source.Artportalen;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Import.Services;
using SOS.Lib.Database;
using SOS.Lib.Enums;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using Xunit;

namespace SOS.Import.IntegrationTests.Harvesters.Observations
{
    public class ArtportalenObservationHarvesterIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task HarvestOneHundredThousandSightingsFromArtportalen_And_SaveToMongoDb()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var importConfiguration = GetImportConfiguration();
            importConfiguration.ArtportalenConfiguration.ChunkSize = 125000;
            importConfiguration.ArtportalenConfiguration.MaxNumberOfSightingsHarvested = 100000;

            var artportalenDataService = new ArtportalenDataService(importConfiguration.ArtportalenConfiguration);

            var verbatimDbConfiguration = GetVerbatimDbConfiguration();
            var importClient = new VerbatimClient(
                verbatimDbConfiguration.GetMongoDbSettings(),
                verbatimDbConfiguration.DatabaseName,
                verbatimDbConfiguration.ReadBatchSize,
                verbatimDbConfiguration.WriteBatchSize);
            var sightingVerbatimRepository =
                new ArtportalenVerbatimRepository(importClient, new Mock<ILogger<ArtportalenVerbatimRepository>>().Object);
            var metadataRepository =
                new MetadataRepository(artportalenDataService, new Mock<ILogger<MetadataRepository>>().Object);
            var projectRepository =
                new ProjectRepository(artportalenDataService, new Mock<ILogger<ProjectRepository>>().Object);
            var sightingRepository =
                new SightingRepository(artportalenDataService, new Mock<ILogger<SightingRepository>>().Object);
            var personRepository =
                new PersonRepository(artportalenDataService, new Mock<ILogger<PersonRepository>>().Object);
            var organizationRepository = new OrganizationRepository(artportalenDataService,
                new Mock<ILogger<OrganizationRepository>>().Object);
            var sightingRelationRepository = new SightingRelationRepository(artportalenDataService,
                new Mock<ILogger<SightingRelationRepository>>().Object);
            var speciesCollectionItemRepository = new SpeciesCollectionItemRepository(artportalenDataService,
                new Mock<ILogger<SpeciesCollectionItemRepository>>().Object);
            var siteRepositoryMock = new Mock<ISiteRepository>();

            var _processedObservationRepository = new Mock<IProcessedObservationRepository>().Object;
            siteRepositoryMock.Setup(foo => foo.GetAsync()).ReturnsAsync(new List<SiteEntity>());

            var observationHarvester = new ArtportalenObservationHarvester(
                importConfiguration.ArtportalenConfiguration,
                metadataRepository,
                projectRepository,
                sightingRepository,
                siteRepositoryMock.Object,
                sightingVerbatimRepository,
                personRepository,
                organizationRepository,
                sightingRelationRepository,
                speciesCollectionItemRepository,
                _processedObservationRepository,
                new ArtportalenMetadataContainer(), 
                new Mock<ILogger<ArtportalenObservationHarvester>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var res = await observationHarvester.HarvestSightingsAsync(JobRunModes.Full, JobCancellationToken.Null);

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
            var sightingVerbatimRepositoryMock = new Mock<ArtportalenVerbatimRepository>();
            IMetadataRepository metadataRepository =
                new MetadataRepository(artportalenDataService, new Mock<ILogger<MetadataRepository>>().Object);
            IProjectRepository projectRepository =
                new ProjectRepository(artportalenDataService, new Mock<ILogger<ProjectRepository>>().Object);
            ISightingRepository sightingRepository =
                new SightingRepository(artportalenDataService, new Mock<ILogger<SightingRepository>>().Object);
            var personRepository =
                new PersonRepository(artportalenDataService, new Mock<ILogger<PersonRepository>>().Object);
            var organizationRepository = new OrganizationRepository(artportalenDataService,
                new Mock<ILogger<OrganizationRepository>>().Object);
            var sightingRelationRepository = new SightingRelationRepository(artportalenDataService,
                new Mock<ILogger<SightingRelationRepository>>().Object);
            var speciesCollectionItemRepository = new SpeciesCollectionItemRepository(artportalenDataService,
                new Mock<ILogger<SpeciesCollectionItemRepository>>().Object);
            var siteRepositoryMock = new Mock<ISiteRepository>();
            siteRepositoryMock.Setup(foo => foo.GetAsync()).ReturnsAsync(new List<SiteEntity>());
            var _processedObservationRepository = new Mock<IProcessedObservationRepository>().Object;

            var observationHarvester = new ArtportalenObservationHarvester(
                importConfiguration.ArtportalenConfiguration,
                metadataRepository,
                projectRepository,
                sightingRepository,
                siteRepositoryMock.Object,
                sightingVerbatimRepositoryMock.Object,
                personRepository,
                organizationRepository,
                sightingRelationRepository,
                speciesCollectionItemRepository,
                _processedObservationRepository,
                new ArtportalenMetadataContainer(), 
                new Mock<ILogger<ArtportalenObservationHarvester>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var res = await observationHarvester.HarvestSightingsAsync(JobRunModes.Full, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            res.Status.Should().Be(RunStatus.Success);
        }
    }
}