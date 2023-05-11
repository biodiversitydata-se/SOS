using FizzWare.NBuilder;
using FluentAssertions;
using SOS.Lib.Models.Processed.Observation;
using System.Threading.Tasks;
using Xunit;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.Extensions;
using SOS.TestHelpers.Helpers.Builders;
using SOS.Lib.Models.Shared;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.ObservationsController.ObservationsBySearchEndpoint
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class MyObservationsTests
    {
        private readonly IntegrationTestFixture _fixture;

        public MyObservationsTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task Search_observations_ObservedByMe()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            const int userId = 15;
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(60)
                    .With(m => m.ObserversInternal = m.ObserversInternal = new[] { new UserInternal() { Id = userId, PersonId = userId, UserServiceUserId = userId, ViewAccess = true } })
                    .With(m => m.Observers = "Tom Volgers")
                    .With(m => m.ReportedByUserServiceUserId = userId)
                    .With(m => m.ReportedBy = "Tom Volgers")
                .Build();

            var authorityBuilder = new UserAuthorizationTestBuilder();
            var authority = authorityBuilder
                .WithAuthorityIdentity("Sighting")
                .WithMaxProtectionLevel(1)
                .Build();
            _fixture.UseMockUserService(userId, authority);
            _fixture.UseMockUser(_fixture.ObservationsController, userId, "user@test.xx");

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterDto
            {
                ObservedByMe = true
            };
            
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                null,
                null,
                searchFilter,
                0,
                100);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(60);
            _fixture.RestoreUserService();
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task Search_observations_ObservedByMe_that_have_ReportedBy_but_not_ObservedBy_values()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            const int userId = 15;
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(60)
                    .With(m => m.ObserversInternal = null)
                    .With(m => m.Observers = "Via Tom Volgers")                    
                    .With(m => m.ReportedByUserServiceUserId = userId)
                    .With(m => m.ReportedBy = "Tom Volgers")
                .Build();

            var authorityBuilder = new UserAuthorizationTestBuilder();
            var authority = authorityBuilder
                .WithAuthorityIdentity("Sighting")
                .WithMaxProtectionLevel(1)
                .Build();
            _fixture.UseMockUserService(userId, authority);
            _fixture.UseMockUser(_fixture.ObservationsController, userId, "user@test.xx");
            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterDto
            {
                ObservedByMe = true
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                null,
                null,
                searchFilter,
                0,
                100);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(0);
            _fixture.RestoreUserService();
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task Search_observations_ReportedByMe_that_have_ReportedBy_but_not_ObservedBy_values()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            const int userId = 15;
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(60)
                    .With(m => m.ObserversInternal = null)
                    .With(m => m.Observers = "Via Tom Volgers")
                    .With(m => m.ReportedByUserServiceUserId = userId)
                    .With(m => m.ReportedBy = "Tom Volgers")
                .Build();

            var authorityBuilder = new UserAuthorizationTestBuilder();
            var authority = authorityBuilder
                .WithAuthorityIdentity("Sighting")
                .WithMaxProtectionLevel(1)
                .Build();
            _fixture.UseMockUserService(userId, authority);
            _fixture.UseMockUser(_fixture.ObservationsController, userId, "user@test.xx");

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterDto
            {
                ReportedByMe = true
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                null,
                null,
                searchFilter,
                0,
                100);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(60);
            _fixture.RestoreUserService();
        }
    }
}