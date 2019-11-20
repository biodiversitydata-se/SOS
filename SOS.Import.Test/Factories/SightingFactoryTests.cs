using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Entities;
using SOS.Import.Factories;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Models.Verbatim.SpeciesPortal;
using SOS.Import.Repositories.Destination.SpeciesPortal;
using SOS.Import.Repositories.Source.SpeciesPortal;
using SOS.Import.Repositories.Source.SpeciesPortal.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using Xunit;

namespace SOS.Import.Test.Factories
{
    /// <summary>
    /// Tests for sighting factory
    /// </summary>
    public class SightingFactoryTests
    {
        private readonly SpeciesPortalConfiguration _speciesPortalConfiguration;
        private readonly Mock<IMetadataRepository> _metadataRepositoryMock;
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<ISightingRepository> _sightingRepositoryMock;
        private readonly Mock<ISiteRepository> _siteRepositoryMockMock;
        private readonly Mock<SightingVerbatimRepository> _sightingVerbatimRepository;
        private readonly Mock<PersonRepository> _personRepository;
        private readonly Mock<OrganizationRepository> _organizationRepository;
        private readonly Mock<SightingRelationRepository> _sightingRelationRepository;
        private readonly Mock<SpeciesCollectionItemRepository> _speciesCollectionItemRepository;
        private readonly Mock<IHarvestInfoRepository> _harvestInfoRepositoryMock;
        private readonly Mock<ILogger<SpeciesPortalSightingFactory>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public SightingFactoryTests()
        {
            _speciesPortalConfiguration = new SpeciesPortalConfiguration();
            _metadataRepositoryMock = new Mock<IMetadataRepository>();
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _sightingRepositoryMock = new Mock<ISightingRepository>();
            _siteRepositoryMockMock = new Mock<ISiteRepository>();
            _sightingVerbatimRepository = new Mock<SightingVerbatimRepository>();
            _personRepository = new Mock<PersonRepository>();
            _organizationRepository = new Mock<OrganizationRepository>();
            _sightingRelationRepository = new Mock<SightingRelationRepository>();
            _speciesCollectionItemRepository = new Mock<SpeciesCollectionItemRepository>();
            _harvestInfoRepositoryMock = new Mock<IHarvestInfoRepository>();
            _loggerMock = new Mock<ILogger<SpeciesPortalSightingFactory>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new SpeciesPortalSightingFactory(
                _speciesPortalConfiguration,
                _metadataRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _sightingRepositoryMock.Object,
                _siteRepositoryMockMock.Object,
                _sightingVerbatimRepository.Object,
                _personRepository.Object,
                _organizationRepository.Object, 
                _sightingRelationRepository.Object, 
                _speciesCollectionItemRepository.Object,
                _harvestInfoRepositoryMock.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new SpeciesPortalSightingFactory(
                null,
                _metadataRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _sightingRepositoryMock.Object,
                _siteRepositoryMockMock.Object,
                _sightingVerbatimRepository.Object,
                _personRepository.Object,
                _organizationRepository.Object,
                _sightingRelationRepository.Object,
                _speciesCollectionItemRepository.Object,
                _harvestInfoRepositoryMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("speciesPortalConfiguration");

            create = () => new SpeciesPortalSightingFactory(
                _speciesPortalConfiguration,
                _metadataRepositoryMock.Object,
                null,
                _sightingRepositoryMock.Object,
                _siteRepositoryMockMock.Object,
                _sightingVerbatimRepository.Object,
                _personRepository.Object,
                _organizationRepository.Object,
                _sightingRelationRepository.Object,
                _speciesCollectionItemRepository.Object,
                _harvestInfoRepositoryMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("projectRepository");

            create = () => new SpeciesPortalSightingFactory(
                _speciesPortalConfiguration,
                _metadataRepositoryMock.Object,
                _projectRepositoryMock.Object,
                null,
                _siteRepositoryMockMock.Object,
                _sightingVerbatimRepository.Object,
                _personRepository.Object,
                _organizationRepository.Object,
                _sightingRelationRepository.Object,
                _speciesCollectionItemRepository.Object,
                _harvestInfoRepositoryMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("sightingRepository");

            create = () => new SpeciesPortalSightingFactory(
                _speciesPortalConfiguration,
                _metadataRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _sightingRepositoryMock.Object,
                null,
                _sightingVerbatimRepository.Object,
                _personRepository.Object,
                _organizationRepository.Object,
                _sightingRelationRepository.Object,
                _speciesCollectionItemRepository.Object,
                _harvestInfoRepositoryMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("siteRepository");

            create = () => new SpeciesPortalSightingFactory(
                _speciesPortalConfiguration,
                _metadataRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _sightingRepositoryMock.Object,
                _siteRepositoryMockMock.Object,
                null,
                _personRepository.Object,
                _organizationRepository.Object,
                _sightingRelationRepository.Object,
                _speciesCollectionItemRepository.Object,
                _harvestInfoRepositoryMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("sightingVerbatimRepository");

            create = () => new SpeciesPortalSightingFactory(
                _speciesPortalConfiguration,
                _metadataRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _sightingRepositoryMock.Object,
                _siteRepositoryMockMock.Object,
                _sightingVerbatimRepository.Object,
                _personRepository.Object,
                _organizationRepository.Object,
                _sightingRelationRepository.Object,
                _speciesCollectionItemRepository.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("harvestInfoRepository");

            create = () => new SpeciesPortalSightingFactory(
                _speciesPortalConfiguration,
                _metadataRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _sightingRepositoryMock.Object,
                _siteRepositoryMockMock.Object,
                _sightingVerbatimRepository.Object,
                _personRepository.Object,
                _organizationRepository.Object,
                _sightingRelationRepository.Object,
                _speciesCollectionItemRepository.Object,
                _harvestInfoRepositoryMock.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Make a successful test of aggregation
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AggregateAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _metadataRepositoryMock.Setup(mdr => mdr.GetActivitiesAsync())
                .ReturnsAsync(new [] { new MetadataWithCategoryEntity { Id = 1, Name = "Activity" } });
            _metadataRepositoryMock.Setup(mdr => mdr.GetBiotopesAsync())
                .ReturnsAsync(new[] { new MetadataEntity { Id = 1, Name = "Biotope" } });
            _metadataRepositoryMock.Setup(mdr => mdr.GetGendersAsync())
                .ReturnsAsync(new[] { new MetadataEntity { Id = 1, Name = "Gender" } });
            _metadataRepositoryMock.Setup(mdr => mdr.GetStagesAsync())
                .ReturnsAsync(new[] { new MetadataEntity { Id = 1, Name = "Stage" } });
            _metadataRepositoryMock.Setup(mdr => mdr.GetSubstratesAsync())
                .ReturnsAsync(new[] { new MetadataEntity { Id = 1, Name = "Substrate" } });
            _metadataRepositoryMock.Setup(mdr => mdr.GetUnitsAsync())
                .ReturnsAsync(new[] { new MetadataEntity { Id = 1, Name = "Unit" } });
            _metadataRepositoryMock.Setup(mdr => mdr.GetValidationStatusAsync())
                .ReturnsAsync(new[] { new MetadataEntity { Id = 1, Name = "ValidationStatus" } });
            

            _projectRepositoryMock.Setup(pr => pr.GetAsync())
                .ReturnsAsync(new[] { new ProjectEntity { Id = 1, Name = "Project" } });

            _sightingRepositoryMock.Setup(sr => sr.GetIdSpanAsync())
                .ReturnsAsync( new Tuple<int, int>(1, 1));
            _sightingRepositoryMock.Setup(sr => sr.GetChunkAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new[] { new SightingEntity { Id = 1, ActivityId = 1, GenderId = 1, SiteId = 1, StageId = 1 } });
            _sightingRepositoryMock.Setup(sr => sr.GetProjectIdsAsync())
                .ReturnsAsync(new [] { new Tuple<int, int>(1, 1) });

            _siteRepositoryMockMock.Setup(sr => sr.GetAsync())
                .ReturnsAsync(new[] { new SiteEntity() { Id = 1, Name = "Site"} });

            _sightingVerbatimRepository.Setup(tr => tr.DeleteCollectionAsync())
                .ReturnsAsync(true);
            _sightingVerbatimRepository.Setup(tr => tr.AddCollectionAsync())
                .ReturnsAsync(true);
            _sightingVerbatimRepository.Setup(tr => tr.AddManyAsync(It.IsAny<IEnumerable<APSightingVerbatim>>()))
                .ReturnsAsync(true);
            _harvestInfoRepositoryMock.Setup(hir =>
                    hir.UpdateHarvestInfoAsync(It.IsAny<string>(), DataProviderId.ClamAndTreePortal, It.IsAny<int>()))
                .ReturnsAsync(true);
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var sightingFactory = new SpeciesPortalSightingFactory(
                _speciesPortalConfiguration,
                _metadataRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _sightingRepositoryMock.Object,
                _siteRepositoryMockMock.Object,
                _sightingVerbatimRepository.Object,
                _personRepository.Object, 
                _organizationRepository.Object,
                _sightingRelationRepository.Object,
                _speciesCollectionItemRepository.Object,
                _harvestInfoRepositoryMock.Object,
                _loggerMock.Object);

            var result = await sightingFactory.HarvestSightingsAsync(JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
        }

        /// <summary>
        /// Test aggregation fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AggregateAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var sightingFactory = new SpeciesPortalSightingFactory(
                _speciesPortalConfiguration,
                _metadataRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _sightingRepositoryMock.Object,
                _siteRepositoryMockMock.Object,
                _sightingVerbatimRepository.Object,
                _personRepository.Object, 
                _organizationRepository.Object,
                _sightingRelationRepository.Object,
                _speciesCollectionItemRepository.Object,
                _harvestInfoRepositoryMock.Object,
                _loggerMock.Object);

            var result = await sightingFactory.HarvestSightingsAsync(JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }
    }
}
