using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Containers.Interfaces;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Harvesters.Observations;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
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
            _artportalenVerbatimRepositoryMock = new Mock<IArtportalenVerbatimRepository>();
            _personRepository = new Mock<IPersonRepository>();
            _organizationRepository = new Mock<IOrganizationRepository>();
            _sightingRelationRepository = new Mock<ISightingRelationRepository>();
            _speciesCollectionItemRepository = new Mock<ISpeciesCollectionItemRepository>();
            _processedObservationRepositoryMock = new Mock<IProcessedObservationRepository>();
            _artportalenMetadataContainerMock = new Mock<IArtportalenMetadataContainer>();
            _loggerMock = new Mock<ILogger<ArtportalenObservationHarvester>>();
        }

        private readonly ArtportalenConfiguration _artportalenConfiguration;
        private readonly Mock<IMetadataRepository> _metadataRepositoryMock;
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<ISightingRepository> _sightingRepositoryMock;
        private readonly Mock<ISiteRepository> _siteRepositoryMockMock;
        private readonly Mock<IArtportalenVerbatimRepository> _artportalenVerbatimRepositoryMock;
        private readonly Mock<IPersonRepository> _personRepository;
        private readonly Mock<IOrganizationRepository> _organizationRepository;
        private readonly Mock<ISightingRelationRepository> _sightingRelationRepository;
        private readonly Mock<ISpeciesCollectionItemRepository> _speciesCollectionItemRepository;
        private readonly Mock<IProcessedObservationRepository> _processedObservationRepositoryMock;
        private readonly Mock<IArtportalenMetadataContainer> _artportalenMetadataContainerMock;
        private readonly Mock<ILogger<ArtportalenObservationHarvester>> _loggerMock;

        private ArtportalenObservationHarvester TestObject => new ArtportalenObservationHarvester(
            _artportalenConfiguration,
            _metadataRepositoryMock.Object,
            _projectRepositoryMock.Object,
            _sightingRepositoryMock.Object,
            _siteRepositoryMockMock.Object,
            _artportalenVerbatimRepositoryMock.Object,
            _personRepository.Object,
            _organizationRepository.Object,
            _sightingRelationRepository.Object,
            _speciesCollectionItemRepository.Object,
            _processedObservationRepositoryMock.Object,
            _artportalenMetadataContainerMock.Object,
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
            var result = await TestObject.HarvestObservationsAsync(JobRunModes.Full, JobCancellationToken.Null);
            
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(RunStatus.Failed);
        }

        /// <summary>
        ///     Make a successful test of aggregation
        /// </summary>
        /// <returns></returns>
        [Fact(Skip="Doesn't work")]
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
            _sightingRepositoryMock.Setup(sr => sr.GetChunkAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(
                    new[] {new SightingEntity {Id = 1, ActivityId = 1, GenderId = 1, SiteId = 1, StageId = 1}});
            _sightingRepositoryMock.Setup(sr => sr.GetSightingProjectIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new[] {(SightingId: 1, ProjectId: 1)});

            _siteRepositoryMockMock.Setup(sr => sr.GetAsync())
                .ReturnsAsync(new[] {new SiteEntity {Id = 1, Name = "Site"}});

            _artportalenVerbatimRepositoryMock.Setup(tr => tr.DeleteCollectionAsync())
                .ReturnsAsync(true);
            _artportalenVerbatimRepositoryMock.Setup(tr => tr.AddCollectionAsync())
                .ReturnsAsync(true);
            _artportalenVerbatimRepositoryMock
                .Setup(tr => tr.AddManyAsync(It.IsAny<IEnumerable<ArtportalenObservationVerbatim>>()))
                .ReturnsAsync(true);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.HarvestObservationsAsync(JobRunModes.Full, JobCancellationToken.Null);
            
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(RunStatus.Success);
        }
    }
}