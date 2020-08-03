using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Harvesters.Observations;
using SOS.Import.Repositories.Destination.Artportalen;
using SOS.Import.Repositories.Source.Artportalen;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Artportalen;
using Xunit;

namespace SOS.Import.UnitTests.Harvesters.Observations
{
    /// <summary>
    ///     Tests for Artportalen observation harvester
    /// </summary>
    public class ArtportalenObservationHarvesterTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public ArtportalenObservationHarvesterTests()
        {
            _artportalenConfiguration = new ArtportalenConfiguration();
            _metadataRepositoryMock = new Mock<IMetadataRepository>();
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _sightingRepositoryMock = new Mock<ISightingRepository>();
            _siteRepositoryMockMock = new Mock<ISiteRepository>();
            _sightingVerbatimRepository = new Mock<SightingVerbatimRepository>();
            _personRepository = new Mock<PersonRepository>();
            _organizationRepository = new Mock<OrganizationRepository>();
            _sightingRelationRepository = new Mock<SightingRelationRepository>();
            _speciesCollectionItemRepository = new Mock<SpeciesCollectionItemRepository>();
            _loggerMock = new Mock<ILogger<ArtportalenObservationHarvester>>();
        }

        private readonly ArtportalenConfiguration _artportalenConfiguration;
        private readonly Mock<IMetadataRepository> _metadataRepositoryMock;
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<ISightingRepository> _sightingRepositoryMock;
        private readonly Mock<ISiteRepository> _siteRepositoryMockMock;
        private readonly Mock<SightingVerbatimRepository> _sightingVerbatimRepository;
        private readonly Mock<PersonRepository> _personRepository;
        private readonly Mock<OrganizationRepository> _organizationRepository;
        private readonly Mock<SightingRelationRepository> _sightingRelationRepository;
        private readonly Mock<SpeciesCollectionItemRepository> _speciesCollectionItemRepository;
        private readonly Mock<ILogger<ArtportalenObservationHarvester>> _loggerMock;

        private ArtportalenObservationHarvester TestObject => new ArtportalenObservationHarvester(
            _artportalenConfiguration,
            _metadataRepositoryMock.Object,
            _projectRepositoryMock.Object,
            _sightingRepositoryMock.Object,
            _siteRepositoryMockMock.Object,
            _sightingVerbatimRepository.Object,
            _personRepository.Object,
            _organizationRepository.Object,
            _sightingRelationRepository.Object,
            _speciesCollectionItemRepository.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test aggregation fail
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
            var result = await TestObject.HarvestSightingsAsync(false, JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Status.Should().Be(RunStatus.Failed);
        }

        /// <summary>
        ///     Make a successful test of aggregation
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AggregateAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _metadataRepositoryMock.Setup(mdr => mdr.GetActivitiesAsync())
                .ReturnsAsync(new[]
                {
                    new MetadataWithCategoryEntity
                    {
                        Id = 1, CategoryId = 1, CategoryName = "Category", Translation = "Activity",
                        CultureCode = "sv-GB"
                    }
                });
            _metadataRepositoryMock.Setup(mdr => mdr.GetBiotopesAsync())
                .ReturnsAsync(
                    new[] {new MetadataEntity {Id = 1, Translation = "Biotope", CultureCode = Cultures.en_GB}});
            _metadataRepositoryMock.Setup(mdr => mdr.GetGendersAsync())
                .ReturnsAsync(new[]
                    {new MetadataEntity {Id = 1, Translation = "Gender", CultureCode = Cultures.en_GB}});
            _metadataRepositoryMock.Setup(mdr => mdr.GetStagesAsync())
                .ReturnsAsync(new[] {new MetadataEntity {Id = 1, Translation = "Stage", CultureCode = Cultures.en_GB}});
            _metadataRepositoryMock.Setup(mdr => mdr.GetSubstratesAsync())
                .ReturnsAsync(new[]
                    {new MetadataEntity {Id = 1, Translation = "Substrate", CultureCode = Cultures.en_GB}});
            _metadataRepositoryMock.Setup(mdr => mdr.GetUnitsAsync())
                .ReturnsAsync(new[] {new MetadataEntity {Id = 1, Translation = "Unit", CultureCode = Cultures.en_GB}});
            _metadataRepositoryMock.Setup(mdr => mdr.GetValidationStatusAsync())
                .ReturnsAsync(new[]
                    {new MetadataEntity {Id = 1, Translation = "ValidationStatus", CultureCode = Cultures.en_GB}});


            _projectRepositoryMock.Setup(pr => pr.GetProjectsAsync())
                .ReturnsAsync(new[] {new ProjectEntity {Id = 1, Name = "Project"}});

            _sightingRepositoryMock.Setup(sr => sr.GetIdSpanAsync())
                .ReturnsAsync(new Tuple<int, int>(1, 1));
            _sightingRepositoryMock.Setup(sr => sr.GetChunkAsync(It.IsAny<int>(), It.IsAny<int>(), false))
                .ReturnsAsync(
                    new[] {new SightingEntity {Id = 1, ActivityId = 1, GenderId = 1, SiteId = 1, StageId = 1}});
            _sightingRepositoryMock.Setup(sr => sr.GetProjectIdsAsync())
                .ReturnsAsync(new[] {(SightingId: 1, ProjectId: 1)});

            _siteRepositoryMockMock.Setup(sr => sr.GetAsync())
                .ReturnsAsync(new[] {new SiteEntity {Id = 1, Name = "Site"}});

            _sightingVerbatimRepository.Setup(tr => tr.DeleteCollectionAsync())
                .ReturnsAsync(true);
            _sightingVerbatimRepository.Setup(tr => tr.AddCollectionAsync())
                .ReturnsAsync(true);
            _sightingVerbatimRepository
                .Setup(tr => tr.AddManyAsync(It.IsAny<IEnumerable<ArtportalenObservationVerbatim>>()))
                .ReturnsAsync(true);
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.HarvestSightingsAsync(false, JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Status.Should().Be(RunStatus.Success);
        }

        /// <summary>
        ///     Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new ArtportalenObservationHarvester(
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
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("artportalenConfiguration");

            create = () => new ArtportalenObservationHarvester(
                _artportalenConfiguration,
                _metadataRepositoryMock.Object,
                null,
                _sightingRepositoryMock.Object,
                _siteRepositoryMockMock.Object,
                _sightingVerbatimRepository.Object,
                _personRepository.Object,
                _organizationRepository.Object,
                _sightingRelationRepository.Object,
                _speciesCollectionItemRepository.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("projectRepository");

            create = () => new ArtportalenObservationHarvester(
                _artportalenConfiguration,
                _metadataRepositoryMock.Object,
                _projectRepositoryMock.Object,
                null,
                _siteRepositoryMockMock.Object,
                _sightingVerbatimRepository.Object,
                _personRepository.Object,
                _organizationRepository.Object,
                _sightingRelationRepository.Object,
                _speciesCollectionItemRepository.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("sightingRepository");

            create = () => new ArtportalenObservationHarvester(
                _artportalenConfiguration,
                _metadataRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _sightingRepositoryMock.Object,
                null,
                _sightingVerbatimRepository.Object,
                _personRepository.Object,
                _organizationRepository.Object,
                _sightingRelationRepository.Object,
                _speciesCollectionItemRepository.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("siteRepository");

            create = () => new ArtportalenObservationHarvester(
                _artportalenConfiguration,
                _metadataRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _sightingRepositoryMock.Object,
                _siteRepositoryMockMock.Object,
                null,
                _personRepository.Object,
                _organizationRepository.Object,
                _sightingRelationRepository.Object,
                _speciesCollectionItemRepository.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("sightingVerbatimRepository");

            create = () => new ArtportalenObservationHarvester(
                _artportalenConfiguration,
                _metadataRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _sightingRepositoryMock.Object,
                _siteRepositoryMockMock.Object,
                _sightingVerbatimRepository.Object,
                _personRepository.Object,
                _organizationRepository.Object,
                _sightingRelationRepository.Object,
                _speciesCollectionItemRepository.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}