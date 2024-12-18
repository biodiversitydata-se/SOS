﻿using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Harvest.Repositories.Source.Artportalen;
using SOS.Harvest.Services;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Import.LiveIntegrationTests.Repositories
{
    public class AreaRepositoryIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task TestGetAllAreas()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var importConfiguration = GetImportConfiguration();
            var artportalenDataService = new ArtportalenDataService(importConfiguration.ArtportalenConfiguration, new NullLogger<ArtportalenDataService>());
            var areaRepository = new AreaRepository(
                artportalenDataService,
                new Mock<ILogger<AreaRepository>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var areaEntities = await areaRepository.GetAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            areaEntities.Should().NotBeEmpty();
        }
    }
}