using System;
using System.Collections.Generic;
using System.Linq;
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
    public class SightingRelationRepositoryIntegrationTests
    {
        private const string ArtportalenTestServerConnectionString = "Server=artsql2-4;Database=SpeciesObservationSwe_debugremote;Trusted_Connection=True;MultipleActiveResultSets=true";

        [Fact]
        [Trait("Category","Integration")]
        public async Task TestGetSightingRelations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SpeciesPortalDataService speciesPortalDataService = new SpeciesPortalDataService(new ConnectionStrings
            {
                SpeciesPortal = ArtportalenTestServerConnectionString
            });

            SightingRelationRepository sightingRelationRepository = new SightingRelationRepository(
                speciesPortalDataService,
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
