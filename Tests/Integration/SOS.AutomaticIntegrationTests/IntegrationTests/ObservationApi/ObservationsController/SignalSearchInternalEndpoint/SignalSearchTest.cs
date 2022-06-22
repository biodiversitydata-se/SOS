using FizzWare.NBuilder;
using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.Extensions;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.TestHelpers.Helpers.Builders;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.ObservationsController.SignalSearchInternalEndpoint
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class SignalSearchTest
    {
        private readonly IntegrationTestFixture _fixture;

        public SignalSearchTest(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task SignalSearchInternalTest()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------
            const int userId = 15;
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations(true)
                    //.HaveRandomValues()
                .TheFirst(20)
                    .With(p => p.TaxonId = 100051)
                    .With(p => p.StartDate = new DateTime(2000, 1, 1))
                    .With(p => p.EndDate = new DateTime(2000, 1, 1))
                .TheNext(20)
                    .With(p => p.TaxonId = 100057)
                    .With(p => p.StartDate = new DateTime(2000, 1, 15))
                    .With(p => p.EndDate = new DateTime(2000, 1, 18))
                .TheNext(20)
                    .With(p => p.TaxonId = 100069)
                    .With(p => p.StartDate = new DateTime(2000, 1, 30))
                    .With(p => p.EndDate = new DateTime(2000, 1, 30))
                .TheNext(20)
                    .With(p => p.TaxonId = 100070)
                    .With(p => p.StartDate = new DateTime(2000, 1, 1))
                    .With(p => p.EndDate = new DateTime(2000, 2, 1))
                .TheLast(20)
                    .With(p => p.TaxonId = 100051)
                    .With(p => p.StartDate = new DateTime(1999, 4, 1))
                    .With(p => p.EndDate = new DateTime(1999, 4, 15))
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var authorityBuilder = new UserAuthorizationTestBuilder();
            var authority = authorityBuilder
                .WithAuthorityIdentity("SightingIndication")
                .WithMaxProtectionLevel(1)
                .WithAreaAccess(Lib.Enums.AreaType.County, "1")
                .Build();
            _fixture.UseMockUserService(15, authority);

            var searchFilter = new SignalFilterDto
            {
                StartDate = new DateTime(1999, 12, 1),
                Geographics = new GeographicsFilterDto
                {
                   BoundingBox = new LatLonBoundingBoxDto
                   {
                       BottomRight = new LatLonCoordinateDto { Latitude = 50, Longitude = 18 },
                       TopLeft = new LatLonCoordinateDto { Latitude = 80, Longitude = 9 },
                   }
                },
                Taxon = new TaxonFilterBaseDto
                {
                    TaxonListIds = new[] { 1 }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.SignalSearchInternal(null, null, searchFilter, onlyAboveMyClearance: true);

            var result = response.GetResult<bool>();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Should().Be(true);
        }
    }
}