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
using SOS.Import.Services;
using Xunit;

namespace SOS.Import.Test.Repositories
{
    public class OrganizationRepositoryTests
    {
        private const string ArtportalenTestServerConnectionString = "Server=artsql2-4;Database=SpeciesObservationSwe_debugremote;Trusted_Connection=True;MultipleActiveResultSets=true";

        [Fact]
        [Trait("Category", "Integration")]
        public async Task TestGetAllOrganizations()
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
            OrganizationRepository organizationRepository = new OrganizationRepository(
                speciesPortalDataService, 
                new Mock<ILogger<OrganizationRepository>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var organizationEntities = await organizationRepository.GetAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            organizationEntities.Should().NotBeEmpty();
        }
    }
}