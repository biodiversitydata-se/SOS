using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Nest;
using SOS.Lib.Enums;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ObservationsController.SearchEndpoint
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class SignalSearchIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public SignalSearchIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Test_SignalSearch()
        {
            // To test with a specific user, change SOS.Lib.Managers.FilterManager.AddAuthorizationAsync() to use
            // Remove: var user = await _userService.GetUserAsync();
            // Add:    var user = await _userService.GetUserByIdAsync([userId]);

            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SignalFilterDto()
            {
                Geographics = new GeographicsFilterDto()
                {
                    Areas = new List<AreaFilterDto>
                    {
                        TestData.Areas.TranasMunicipality
                    },
                    ConsiderObservationAccuracy = true,
                    ConsiderDisturbanceRadius = true,
                    MaxAccuracy = 5000
                },
                Taxon = new TaxonFilterBaseDto()
                {
                    TaxonListIds = new[]
                    {
                        (int)TaxonListId.RedlistedSpecies, 
                        (int)TaxonListId.HabitatsDirective, 
                        (int)TaxonListId.ProtectedByLaw, 
                        (int)TaxonListId.ActionPlan, 
                        (int)TaxonListId.SwedishForestAgencyNatureConservationSpecies
                    }
                },
                StartDate = new DateTime(1950, 1, 1)
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.SignalSearchInternal(
                "CountyAdministrationObservation",
                searchFilter,
                false,
                0,
                true);
            var result = response.GetResult<bool>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }
    }
}
