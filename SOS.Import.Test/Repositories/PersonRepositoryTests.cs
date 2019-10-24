using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Entities;
using SOS.Import.Repositories.Source.SpeciesPortal;
using SOS.Import.Repositories.Source.SpeciesPortal.Interfaces;
using SOS.Import.Services;
using SOS.Lib.Configuration.Import;
using Xunit;

namespace SOS.Import.Test.Repositories
{
    public class PersonRepositoryTests
    {
        private const string ArtportalenTestServerConnectionString = "Server=artsql2-4;Database=SpeciesObservationSwe_debugremote;Trusted_Connection=True;MultipleActiveResultSets=true";

        [Fact]
        [Trait("Category", "Integration")]
        public async Task TestGetAllPersons()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SpeciesPortalDataService speciesPortalDataService = new SpeciesPortalDataService(new ConnectionStrings
            {
                SpeciesPortal = ArtportalenTestServerConnectionString
            });

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