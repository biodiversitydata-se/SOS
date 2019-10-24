using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Repositories.Source.SpeciesPortal;
using SOS.Import.Services;
using Xunit;

namespace SOS.Import.Test.Repositories
{
    public class SpeciesCollectionItemRepositoryTests
    {
        private const string ArtportalenTestServerConnectionString = "Server=artsql2-4;Database=SpeciesObservationSwe_debugremote;Trusted_Connection=True;MultipleActiveResultSets=true";

        [Fact]
        [Trait("Category", "Integration")]
        public async Task TestGetAllSightingSpeciesCollectionItems()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var configurationDictionary = new Dictionary<string, string>
            {
                { "ConnectionStrings:SpeciesPortal", ArtportalenTestServerConnectionString }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationDictionary)
                .Build();

            SpeciesPortalDataService speciesPortalDataService = new SpeciesPortalDataService(configuration);
            SpeciesCollectionItemRepository speciesCollectionItemRepository = new SpeciesCollectionItemRepository(
                speciesPortalDataService,
                new Mock<ILogger<SpeciesCollectionItemRepository>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var speciesCollectionItemEntities = await speciesCollectionItemRepository.GetAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            speciesCollectionItemEntities.Should().NotBeEmpty();
        }
    }
}