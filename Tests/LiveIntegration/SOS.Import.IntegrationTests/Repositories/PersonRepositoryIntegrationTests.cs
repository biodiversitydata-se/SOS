using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Harvest.Repositories.Source.Artportalen;
using SOS.Harvest.Services;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Import.LiveIntegrationTests.Repositories
{
    public class PersonRepositoryIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task TestGetAllPersons()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var importConfiguration = GetImportConfiguration();
            var artportalenDataService = new ArtportalenDataService(importConfiguration.ArtportalenConfiguration, new NullLogger<ArtportalenDataService>());

            var personRepository = new PersonRepository(
                artportalenDataService,
                new Mock<ILogger<PersonRepository>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var personEntities = await personRepository.GetAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            personEntities.Should().NotBeEmpty();
        }
    }
}