using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Harvest.Containers;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Harvesters.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Harvest.Services;
using SOS.Lib.Database;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Repositories.Verbatim.Interfaces;
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

            var artportalenDataService = new ArtportalenDataService(importConfiguration.ArtportalenConfiguration, new NullLogger<ArtportalenDataService>());

            var verbatimDbConfiguration = GetVerbatimDbConfiguration();
            var importClient = new VerbatimClient(
                verbatimDbConfiguration.GetMongoDbSettings(),
                verbatimDbConfiguration.DatabaseName,
                verbatimDbConfiguration.ReadBatchSize,
                verbatimDbConfiguration.WriteBatchSize);
            var sightingVerbatimRepository =
                new ArtportalenVerbatimRepository(importClient, new Mock<ILogger<ArtportalenVerbatimRepository>>().Object);
            var mediadataRepository =
                new MediaRepository(artportalenDataService, new Mock<ILogger<MediaRepository>>().Object);
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
            var taxonRepository = new TaxonRepository(artportalenDataService, new Mock<ILogger<TaxonRepository>>().Object);

            var processedObservationRepository = new Mock<IProcessedObservationRepository>().Object;
            siteRepositoryMock.Setup(foo => foo.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>())).ReturnsAsync(new List<SiteEntity>());

            var processedDbConfiguration = GetProcessDbConfiguration();
            var processedClient = new ProcessClient(processedDbConfiguration.GetMongoDbSettings(), processedDbConfiguration.DatabaseName, processedDbConfiguration.ReadBatchSize, processedDbConfiguration.WriteBatchSize);
            var areaRepository = new Lib.Repositories.Resource.AreaRepository(processedClient, new Mock<ILogger<Lib.Repositories.Resource.AreaRepository>>().Object);
            var areaHelper = new AreaHelper(areaRepository);


            var observationHarvester = new ArtportalenObservationHarvester(
                importConfiguration.ArtportalenConfiguration,
                mediadataRepository,
                metadataRepository,
                projectRepository,
                sightingRepository,
                siteRepositoryMock.Object,
                sightingVerbatimRepository,
                personRepository,
                organizationRepository,
                sightingRelationRepository,
                speciesCollectionItemRepository,
                processedObservationRepository,
                taxonRepository,
                new ArtportalenMetadataContainer(),
                areaHelper,
                new Mock<ILogger<ArtportalenObservationHarvester>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var res = await observationHarvester.HarvestObservationsAsync(JobRunModes.Full, JobCancellationToken.Null);

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
            var artportalenDataService = new ArtportalenDataService(importConfiguration.ArtportalenConfiguration, new NullLogger<ArtportalenDataService>());
            var sightingVerbatimRepositoryMock = new Mock<IArtportalenVerbatimRepository>();
            var mediadataRepository =
                new MediaRepository(artportalenDataService, new Mock<ILogger<MediaRepository>>().Object);
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
            siteRepositoryMock.Setup(foo => foo.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>()));
            var processedObservationRepository = new Mock<IProcessedObservationRepository>().Object;
            var taxonRepositoryMock = new Mock<ITaxonRepository>().Object;

            var processedDbConfiguration = GetProcessDbConfiguration();
            var processedClient = new ProcessClient(processedDbConfiguration.GetMongoDbSettings(), processedDbConfiguration.DatabaseName, processedDbConfiguration.ReadBatchSize, processedDbConfiguration.WriteBatchSize);
            var areaRepository = new Lib.Repositories.Resource.AreaRepository(processedClient, new Mock<ILogger<Lib.Repositories.Resource.AreaRepository>>().Object);
            var areaHelper = new AreaHelper(areaRepository);

            var observationHarvester = new ArtportalenObservationHarvester(
                importConfiguration.ArtportalenConfiguration,
                mediadataRepository,
                metadataRepository,
                projectRepository,
                sightingRepository,
                siteRepositoryMock.Object,
                sightingVerbatimRepositoryMock.Object,
                personRepository,
                organizationRepository,
                sightingRelationRepository,
                speciesCollectionItemRepository,
                processedObservationRepository,
                taxonRepositoryMock,
                new ArtportalenMetadataContainer(),
                areaHelper,
                new Mock<ILogger<ArtportalenObservationHarvester>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var res = await observationHarvester.HarvestObservationsAsync(JobRunModes.Full, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            res.Status.Should().Be(RunStatus.Success);
        }
    }
}