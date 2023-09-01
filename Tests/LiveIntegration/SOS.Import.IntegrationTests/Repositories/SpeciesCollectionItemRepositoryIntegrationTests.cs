﻿using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Harvest.Repositories.Source.Artportalen;
using SOS.Harvest.Services;
using SOS.Import.LiveIntegrationTests;
using Xunit;

namespace SOS.Import.LiveIntegrationTests.Repositories
{
    public class SpeciesCollectionItemRepositoryIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task TestGetAllSightingSpeciesCollectionItems()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var importConfiguration = GetImportConfiguration();
            var artportalenDataService = new ArtportalenDataService(importConfiguration.ArtportalenConfiguration, new NullLogger<ArtportalenDataService>());
            var speciesCollectionItemRepository = new SpeciesCollectionItemRepository(
                artportalenDataService,
                new Mock<ILogger<SpeciesCollectionItemRepository>>().Object);

            var sightingIds = new[] { 64768, 3580540, 4164924, 6700000, 2896107 };
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var speciesCollectionItemEntities = await speciesCollectionItemRepository.GetBySightingAsync(sightingIds);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            speciesCollectionItemEntities.Should().NotBeEmpty();
        }
    }
}