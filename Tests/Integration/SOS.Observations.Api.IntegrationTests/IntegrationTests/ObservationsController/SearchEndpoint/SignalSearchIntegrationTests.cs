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
using SOS.TestHelpers.Helpers.Builders;
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
        public async Task Test_SignalSearch_in_Tranas_municipality_when_having_access_to_Jonkoping_county()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var authorityBuilder = new UserAuthorizationTestBuilder();
            var authority = authorityBuilder
                .WithAuthorityIdentity("SightingIndication")
                .WithMaxProtectionLevel(3)
                .WithAreaAccess(TestData.AreaAuthority.JonkopingCounty)
                .Build();
            _fixture.UseMockUserService(authority);

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

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Test_SignalSearch_in_Tranas_municipality_when_having_access_to_Ostergotland_county_and_consider_observation_accuracy()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var authorityBuilder = new UserAuthorizationTestBuilder();
            var authority = authorityBuilder
                .WithAuthorityIdentity("SightingIndication")
                .WithMaxProtectionLevel(3)
                .WithAreaAccess(TestData.AreaAuthority.OstergotlandCounty)
                .Build();
            _fixture.UseMockUserService(authority);

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
            result.Should().BeTrue("because when consider observation accuracy some observations in Östergötland could intersect Tranås municipality in Jönköping county.");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Test_SignalSearch_in_Tranas_municipality_when_having_access_to_Ostergotland_county_and_not_consider_observation_accuracy()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var authorityBuilder = new UserAuthorizationTestBuilder();
            var authority = authorityBuilder
                .WithAuthorityIdentity("SightingIndication")
                .WithMaxProtectionLevel(3)
                .WithAreaAccess(TestData.AreaAuthority.OstergotlandCounty)
                .Build();
            _fixture.UseMockUserService(authority);

            var searchFilter = new SignalFilterDto()
            {
                Geographics = new GeographicsFilterDto()
                {
                    Areas = new List<AreaFilterDto>
                    {
                        TestData.Areas.TranasMunicipality
                    },
                    ConsiderObservationAccuracy = false,
                    ConsiderDisturbanceRadius = false,
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
            result.Should().BeFalse();
        }


    }
}
