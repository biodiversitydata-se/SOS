using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Harvest.Containers.Interfaces;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Harvesters.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            _mediaRepositoryMock = new Mock<IMediaRepository>();
            _metadataRepositoryMock = new Mock<IMetadataRepository>();
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _sightingRepositoryMock = new Mock<ISightingRepository>();
            _siteRepositoryMockMock = new Mock<ISiteRepository>();
            _artportalenVerbatimRepositoryMock = new Mock<IArtportalenVerbatimRepository>();
            _personRepository = new Mock<IPersonRepository>();
            _sightingRelationRepository = new Mock<ISightingRelationRepository>();
            _speciesCollectionItemRepository = new Mock<ISpeciesCollectionItemRepository>();
            _processedObservationRepositoryMock = new Mock<IProcessedObservationCoreRepository>();
            _taxonRepositoryMock = new Mock<ITaxonRepository>();
            _artportalenMetadataContainerMock = new Mock<IArtportalenMetadataContainer>();
            _areaHelperMock = new Mock<IAreaHelper>();
            _loggerMock = new Mock<ILogger<ArtportalenObservationHarvester>>();
        }

        private readonly ArtportalenConfiguration _artportalenConfiguration;
        private readonly Mock<IMediaRepository> _mediaRepositoryMock;
        private readonly Mock<IMetadataRepository> _metadataRepositoryMock;
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<ISightingRepository> _sightingRepositoryMock;
        private readonly Mock<ISiteRepository> _siteRepositoryMockMock;
        private readonly Mock<IArtportalenVerbatimRepository> _artportalenVerbatimRepositoryMock;
        private readonly Mock<IPersonRepository> _personRepository;
        private readonly Mock<ISightingRelationRepository> _sightingRelationRepository;
        private readonly Mock<ISpeciesCollectionItemRepository> _speciesCollectionItemRepository;
        private readonly Mock<IProcessedObservationCoreRepository> _processedObservationRepositoryMock;
        private readonly Mock<ITaxonRepository> _taxonRepositoryMock;
        private readonly Mock<IArtportalenMetadataContainer> _artportalenMetadataContainerMock;
        private readonly Mock<IAreaHelper> _areaHelperMock;
        private readonly Mock<ILogger<ArtportalenObservationHarvester>> _loggerMock;

        private ArtportalenObservationHarvester TestObject => new ArtportalenObservationHarvester(
            _artportalenConfiguration,
            _mediaRepositoryMock.Object,
            _metadataRepositoryMock.Object,
            _projectRepositoryMock.Object,
            _sightingRepositoryMock.Object,
            _siteRepositoryMockMock.Object,
            _artportalenVerbatimRepositoryMock.Object,
            _sightingRelationRepository.Object,
            _speciesCollectionItemRepository.Object,
            _processedObservationRepositoryMock.Object,
            _artportalenMetadataContainerMock.Object,
            _areaHelperMock.Object,
            _loggerMock.Object);

        [Fact]
        public void VerifyDistinctBatch()
        {
            // Arrange
            var harvestIds = new HashSet<int>();
            var batch1 = new List<NewAndEditedSightingId> {
                new NewAndEditedSightingId {Id = 1, EditDate = new DateTime(2000,1,1)},
                new NewAndEditedSightingId {Id = 2, EditDate = new DateTime(2001,1,1)},
                new NewAndEditedSightingId {Id = 3, EditDate = new DateTime(2002,1,1)}
            };

            var batch2 = new List<NewAndEditedSightingId> {
                new NewAndEditedSightingId {Id = 4, EditDate = new DateTime(2003,1,1)},
                new NewAndEditedSightingId {Id = 5, EditDate = new DateTime(2004,1,1)},
                new NewAndEditedSightingId {Id = 6, EditDate = new DateTime(2005,1,1)}
            };

            var batch3 = new List<NewAndEditedSightingId> {
                new NewAndEditedSightingId {Id = 1, EditDate = new DateTime(2003,1,1)},
                new NewAndEditedSightingId {Id = 4, EditDate = new DateTime(2004,1,1)},
                new NewAndEditedSightingId {Id = 7, EditDate = new DateTime(2006,1,1)}
            };

            var batch4 = new List<NewAndEditedSightingId> {
                new NewAndEditedSightingId {Id = 7, EditDate = new DateTime(2006,1,1)},
                new NewAndEditedSightingId {Id = 8, EditDate = new DateTime(2007,1,1)},
                new NewAndEditedSightingId {Id = 9, EditDate = new DateTime(2008,1,1)}
            };

            // Act
            var modifiedBatch1 = ArtportalenObservationHarvester.GetDistinctBatch(harvestIds, batch1.ToArray());
            var modifiedBatch2 = ArtportalenObservationHarvester.GetDistinctBatch(harvestIds, batch2.ToArray());
            var modifiedBatch3 = ArtportalenObservationHarvester.GetDistinctBatch(harvestIds, batch3.ToArray());
            var modifiedBatch4 = ArtportalenObservationHarvester.GetDistinctBatch(harvestIds, batch4.ToArray());

            // Assert
            modifiedBatch1.Should().BeEquivalentTo(new List<NewAndEditedSightingId> {
                new NewAndEditedSightingId {Id = 1, EditDate = new DateTime(2000,1,1)},
                new NewAndEditedSightingId {Id = 2, EditDate = new DateTime(2001,1,1)},
                new NewAndEditedSightingId {Id = 3, EditDate = new DateTime(2002,1,1)}
            });

            modifiedBatch2.Should().BeEquivalentTo(new List<NewAndEditedSightingId> {
                new NewAndEditedSightingId {Id = 4, EditDate = new DateTime(2003,1,1)},
                new NewAndEditedSightingId {Id = 5, EditDate = new DateTime(2004,1,1)},
                new NewAndEditedSightingId {Id = 6, EditDate = new DateTime(2005,1,1)}
            });

            modifiedBatch3.Should().BeEquivalentTo(new List<NewAndEditedSightingId> {
                new NewAndEditedSightingId {Id = 7, EditDate = new DateTime(2006,1,1)}
            });

            modifiedBatch4.Should().BeEquivalentTo(new List<NewAndEditedSightingId> {                
                new NewAndEditedSightingId {Id = 8, EditDate = new DateTime(2007,1,1)},
                new NewAndEditedSightingId {Id = 9, EditDate = new DateTime(2008,1,1)}
            });
        }


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
            var result = await TestObject.HarvestObservationsAsync(new DataProvider(), JobRunModes.Full, null, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(RunStatus.Failed);
        }

        /// <summary>
        ///     Make a successful test of aggregation
        /// </summary>
        /// <returns></returns>
        [Fact(Skip = "Doesn't work")]
        public async Task AggregateAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _metadataRepositoryMock.Setup(mdr => mdr.GetActivitiesAsync())
                .ReturnsAsync(new[]
                {
                    new MetadataWithCategoryEntity<int>(1)
                    {
                        CategoryId = 1, CategoryName = "Category", Translation = "Activity",
                        CultureCode = "sv-GB"
                    }
                });
            _mediaRepositoryMock.Setup(mdr => mdr.GetAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(
                    new[] { new MediaEntity() { SightingId = 1, FileType = "image", UploadDateTime = DateTime.Now } });
            _metadataRepositoryMock.Setup(mdr => mdr.GetBiotopesAsync())
                .ReturnsAsync(
                    new[] { new MetadataEntity<int>(1) { Translation = "Biotope", CultureCode = Cultures.en_GB } });
            _metadataRepositoryMock.Setup(mdr => mdr.GetGendersAsync())
                .ReturnsAsync(new[]
                    {new MetadataEntity<int>(1) { Translation = "Gender", CultureCode = Cultures.en_GB}});
            _metadataRepositoryMock.Setup(mdr => mdr.GetStagesAsync())
                .ReturnsAsync(new[] { new MetadataEntity<int>(1) { Translation = "Stage", CultureCode = Cultures.en_GB } });
            _metadataRepositoryMock.Setup(mdr => mdr.GetSubstratesAsync())
                .ReturnsAsync(new[]
                    {new MetadataEntity<int>(1) { Translation = "Substrate", CultureCode = Cultures.en_GB}});
            _metadataRepositoryMock.Setup(mdr => mdr.GetUnitsAsync())
                .ReturnsAsync(new[] { new MetadataEntity<int>(1) { Translation = "Unit", CultureCode = Cultures.en_GB } });
            _metadataRepositoryMock.Setup(mdr => mdr.GetValidationStatusAsync())
                .ReturnsAsync(new[]
                    {new MetadataEntity<int>(1) { Translation = "ValidationStatus", CultureCode = Cultures.en_GB}});
            _projectRepositoryMock.Setup(pr => pr.GetProjectsAsync())
                .ReturnsAsync(new[] { new ProjectEntity { Id = 1, Name = "Project" } });

            _sightingRepositoryMock.Setup(sr => sr.GetIdSpanAsync())
                .ReturnsAsync((1, 1));
            _sightingRepositoryMock.Setup(sr => sr.GetChunkAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(
                    new[] { new SightingEntity { Id = 1, ActivityId = 1, GenderId = 1, SiteId = 1, StageId = 1 } });
            _sightingRepositoryMock.Setup(sr => sr.GetSightingProjectIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new[] { (SightingId: 1, ProjectId: 1) });

            _siteRepositoryMockMock.Setup(sr => sr.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new[] { new SiteEntity { Id = 1, Name = "Site" } });

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
            var result = await TestObject.HarvestObservationsAsync(new DataProvider(), JobRunModes.Full, null, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(RunStatus.Success);
        }
    }
}