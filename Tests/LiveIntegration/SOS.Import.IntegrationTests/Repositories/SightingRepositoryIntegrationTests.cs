using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Harvest.Repositories.Source.Artportalen;
using SOS.Harvest.Services;
using Xunit;

namespace SOS.Import.IntegrationTests.Repositories
{
    /// <summary>
    ///     Test sighting repository
    /// </summary>
    public class SightingRepositoryIntegrationTests : TestBase
    {
        /// <summary>
        ///     Test get chunk of specific sighting ids.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetChunkAsync_ForSpecificSightingIds_Success()
        {
            IEnumerable<int> sightingIds = new[]
            {
                72109918,
                53584868,
                53584859,
                53688179
            };

            var importConfiguration = GetImportConfiguration();
            var artportalenDataService = new ArtportalenDataService(importConfiguration.ArtportalenConfiguration, new NullLogger<ArtportalenDataService>());
            var sightingRepository = new SightingRepository(
                artportalenDataService,
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