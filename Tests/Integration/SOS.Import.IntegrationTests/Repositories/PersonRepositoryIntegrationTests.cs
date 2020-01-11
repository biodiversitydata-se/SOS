using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Repositories.Source.SpeciesPortal;
using SOS.Import.Services;
using Xunit;

namespace SOS.Import.IntegrationTests.Repositories
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
            var speciesPortalDataService = new SpeciesPortalDataService(importConfiguration.SpeciesPortalConfiguration);

            PersonRepository personRepository = new PersonRepository(
                speciesPortalDataService, 
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