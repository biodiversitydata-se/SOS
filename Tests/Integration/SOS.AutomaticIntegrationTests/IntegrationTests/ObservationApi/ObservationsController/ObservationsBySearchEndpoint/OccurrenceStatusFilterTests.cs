using FizzWare.NBuilder;
using FluentAssertions;
using SOS.Lib.Models.Processed.Observation;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos;
using System.Linq;
using LinqStatistics;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.Extensions;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.ObservationsController.ObservationsBySearchEndpoint
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class OccurrenceStatusFilterTests
    {
        private readonly IntegrationTestFixture _fixture;

        public OccurrenceStatusFilterTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "CompleteApiIntegrationTest")]
        public async Task GetOnlyPresentObservations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                    //.HaveRandomValues()
                .TheFirst(60)
                    .With(v => v.NotPresent = false)
                    .With(v => v.NotRecovered = false)
                .TheNext(20)
                    .With(v => v.NotPresent = true)
                .TheNext(20)
                    .With(v => v.NotRecovered = true)
                .Build();
            
            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterDto
            {
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.CustomObservationsController.ObservationsBySearch(
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
        }
    }
}