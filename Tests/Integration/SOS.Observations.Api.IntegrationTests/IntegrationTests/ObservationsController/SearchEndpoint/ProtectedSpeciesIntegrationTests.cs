using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using SOS.TestHelpers.Helpers.Builders;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ObservationsController.SearchEndpoint
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class ProtectedSpeciesIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public ProtectedSpeciesIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_for_wolf_in_public_observations_should_return_0_observations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto { Ids = new List<int> { TestData.TaxonIds.Wolf }, IncludeUnderlyingTaxa = true },
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(null, null, searchFilter, 0, 10);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.Should().BeEmpty("because Wolf is protected");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_for_wolf_without_permissions()
        {
            // To test with a specific user, change SOS.Lib.Managers.FilterManager.AddAuthorizationAsync() to use:
            // Remove: var user = await _userService.GetUserAsync();
            // Add:    var user = await _userService.GetUserByIdAsync([userId]);

            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var authorityBuilder = new UserAuthorizationTestBuilder();
            var authority = authorityBuilder
                .WithAuthorityIdentity("Sighting")
                .WithMaxProtectionLevel(5)
                .WithTaxonIdsAccess(TestData.TaxonIds.Otter)
                .WithAreaAccess(TestData.AreaAuthority.Sweden)
                .Build();
            _fixture.UseMockUserService(authority);
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto { Ids = new List<int> { TestData.TaxonIds.Wolf }, IncludeUnderlyingTaxa = true },
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                0,
                null,
                searchFilter,
                0,
                1000,
                "",
                SearchSortOrder.Asc,
                false,
                "sv-SE",
                true);

            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.Should().BeEmpty("because wolf is protected and you have no permissions.");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_for_wolf_with_permissions()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var authorityBuilder = new UserAuthorizationTestBuilder();
            var authority = authorityBuilder
                .WithAuthorityIdentity("Sighting")
                .WithMaxProtectionLevel(5)
                .WithTaxonIdsAccess(TestData.TaxonIds.Wolf)
                .WithAreaAccess(TestData.AreaAuthority.Sweden)
                .Build();
            _fixture.UseMockUserService(authority);
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto { Ids = new List<int> { TestData.TaxonIds.Wolf }, IncludeUnderlyingTaxa = true },
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                null,
                null,
                searchFilter,
                0,
                1000,
                "",
                SearchSortOrder.Asc,
                false,
                "sv-SE",
                true);

            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.Should().NotBeEmpty("because the search is executed with permissions.");
        }


        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Get_protected_species_in_Jonkoping_county()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var authorityBuilder = new UserAuthorizationTestBuilder();
            var authority = authorityBuilder
                .WithAuthorityIdentity("Sighting")
                .WithMaxProtectionLevel(3)
                .WithAreaAccess(TestData.AreaAuthority.JonkopingCounty)
                .Build();
            _fixture.UseMockUserService(authority);
            var searchFilter = new SearchFilterInternalDto
            {
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present,
                Geographics = new GeographicsFilterDto
                {
                    Areas = new List<AreaFilterDto>
                    {
                        TestData.Areas.JonkopingCounty,
                        TestData.Areas.OstergotlandCounty,
                    }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
                null,
                "CountyAdministrationObservation",
                searchFilter,
                0,
                10000,
                "",
                SearchSortOrder.Asc,
                false,
                "sv-SE",
                true);
            var result = response.GetResult<GeoPagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.TotalCount.Should().BeGreaterThan(0);
            result.Records.All(m => m.Location.County.FeatureId == TestData.Areas.JonkopingCounty.FeatureId).Should()
                .BeTrue("Search is done with permission to Jönköping county");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Try_get_protected_species_in_Jonkoping_county_with_too_low_protection_level_authority()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var authorityBuilder = new UserAuthorizationTestBuilder();
            var jonkopingAuthority = authorityBuilder
                .WithAuthorityIdentity("Sighting")
                .WithMaxProtectionLevel(3)
                .WithAreaAccess(TestData.AreaAuthority.JonkopingCounty)
                .WithTaxonIdsAccess(new List<int> { 100054 })// Pilgrimsfalk (protection level 5)
                .Build();
            var ostergotlandAuthority = authorityBuilder
                .WithAuthorityIdentity("Sighting")
                .WithMaxProtectionLevel(5)
                .WithAreaAccess(TestData.AreaAuthority.OstergotlandCounty)
                .Build();
            _fixture.UseMockUserService(jonkopingAuthority, ostergotlandAuthority);
            var searchFilter = new SearchFilterInternalDto
            {
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present,
                Geographics = new GeographicsFilterDto
                {
                    Areas = new List<AreaFilterDto>
                    {
                        TestData.Areas.JonkopingCounty,
                    }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
                0,
                "CountyAdministrationObservation",
                searchFilter,
                0,
                10,
                "",
                SearchSortOrder.Asc,
                false,
                "sv-SE",
                true);
            var result = response.GetResult<GeoPagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.TotalCount.Should().Be(0, "because Pilgrimsfalk has ProtectionLevel 5, but the authority is max protection level 3");
        }
    }
}