using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Lib.Cache;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using SOS.TestHelpers.Helpers.Builders;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ObservationsController.CountEndpoint
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class CachedCountIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public CachedCountIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }
        
        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Cached_count_for_mammalia_taxa()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            List<int> taxonIds = new List<int>()
            {
                4000107, // Mammalia (däggdjur)
                3000303, // Carnivora (rovdjur)
                100053 // igelkott
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            
            // Get count first time. The result is not yet cached.
            var notYetCachedStopwatch = Stopwatch.StartNew();
            var notYetCachedResponse = await _fixture.ObservationsController.CachedCount(
                taxonIds,
                true);
            var notYetCachedResult = notYetCachedResponse.GetResult<IEnumerable<TaxonObservationCountDto>>();
            notYetCachedStopwatch.Stop();

            // Get count second time. The result is cached
            var cachedStopwatch = Stopwatch.StartNew();
            var cachedResponse = await _fixture.ObservationsController.CachedCount(
                taxonIds,
                true);
            var cachedResult = cachedResponse.GetResult<IEnumerable<TaxonObservationCountDto>>();
            cachedStopwatch.Stop();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            notYetCachedResult.Should().NotBeNull();
            cachedResult.Should().BeEquivalentTo(notYetCachedResult);
            cachedStopwatch.ElapsedMilliseconds.Should().BeLessThan(notYetCachedStopwatch.ElapsedMilliseconds / 1000, "the cached result should be at least 1000 times faster than the calculated result.");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Count_wolf_without_permissions()
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
            var response = await _fixture.ObservationsController.Count(
                null,
                searchFilter,
                false,
                true);

            var result = response.GetResult<long>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().Be(0,"because wolf is protected and you have no permissions.");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Count_wolf_with_permissions()
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
            var response = await _fixture.ObservationsController.Count(
                null,
                searchFilter,
                false,
                true);

            var result = response.GetResult<long>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeGreaterThan(0, "because the search is executed with permissions.");
        }


        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Count_protected_species_in_Jonkoping_county()
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
            var response = await _fixture.ObservationsController.CountInternal(
                "CountyAdministrationObservation",
                searchFilter,
                false,
                true);
            var result = response.GetResult<long>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeGreaterThan(0);
        }
    }
}