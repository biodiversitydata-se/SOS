﻿using System.Linq;
using System.Threading.Tasks;
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
    public class SightingRelationRepositoryIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task TestGetSightingRelations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var importConfiguration = GetImportConfiguration();
            var artportalenDataService = new ArtportalenDataService(importConfiguration.ArtportalenConfiguration, new NullLogger<ArtportalenDataService>());
            var sightingRelationRepository = new SightingRelationRepository(
                artportalenDataService,
                new Mock<ILogger<SightingRelationRepository>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var sightingRelationEntities = await sightingRelationRepository.GetAsync(Enumerable.Range(1, 100));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            sightingRelationEntities.Should().NotBeEmpty();
        }
    }
}