using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Repositories.Source.SpeciesPortal;
using SOS.Import.Services;
using Xunit;

namespace SOS.Import.IntegrationTests.Repositories
{
    /// <summary>
    /// Test sighting repository
    /// </summary>
    public class SightingRepositoryIntegrationTests : TestBase
    {
        /// <summary>
        /// Test get chunk of specific sighting ids.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetChunkAsync_ForSpecificSightingIds_Success()
        {
            IEnumerable<int> sightingIds = new []
            {
                72109918,
                53584868,
                53584859,
                53688179
            };

            var importConfiguration = GetImportConfiguration();
            var speciesPortalDataService = new SpeciesPortalDataService(importConfiguration.SpeciesPortalConfiguration);
            var sightingRepository = new SightingRepository(
                speciesPortalDataService,
                new Mock<ILogger<SightingRepository>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await sightingRepository.GetChunkAsync(sightingIds);
            
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(4);
        }
    }
}
