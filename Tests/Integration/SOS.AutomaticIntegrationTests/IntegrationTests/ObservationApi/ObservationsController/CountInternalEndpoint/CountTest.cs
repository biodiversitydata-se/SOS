using FizzWare.NBuilder;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.Extensions;
using SOS.Observations.Api.Dtos.Filter;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.ObservationsController.CountInternalEndpoint
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class CountTest
    {
        private readonly IntegrationTestFixture _fixture;

        public CountTest(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }
        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task SumObservationCountInternalTest()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------
            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                    //.HaveRandomValues()
                .TheFirst(20)
                    .With(p => p.TaxonId = 100012)
                    .With(p => p.StartDate = new DateTime(2000, 1, 1))
                    .With(p => p.EndDate = new DateTime(2000, 1, 1))
                .TheNext(20)
                    .With(p => p.TaxonId = 100013)
                    .With(p => p.StartDate = new DateTime(2000, 1, 15))
                    .With(p => p.EndDate = new DateTime(2000, 1, 18))
                .TheNext(20)
                    .With(p => p.TaxonId = 100012)
                    .With(p => p.StartDate = new DateTime(2000, 1, 30))
                    .With(p => p.EndDate = new DateTime(2000, 1, 30))
                .TheNext(20)
                    .With(p => p.TaxonId = 100011)
                    .With(p => p.StartDate = new DateTime(2000, 1, 1))
                    .With(p => p.EndDate = new DateTime(2000, 2, 1))
                .TheLast(20)
                    .With(p => p.TaxonId = 100011)
                    .With(p => p.StartDate = new DateTime(2000, 4, 1))
                    .With(p => p.EndDate = new DateTime(2000, 4, 15))
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterInternalBaseDto
            {
                Date = new DateFilterDto
                {
                    StartDate = DateTime.Parse("2000-01-01T00:00:00"),
                    EndDate = DateTime.Parse("2000-01-31T23:59:59"),
                    DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
                },
                Taxon = new TaxonFilterDto
                {
                    Ids = new[] { 100012, 100013 }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.CountInternal(null, null, searchFilter);
            var count = response.GetResult<long>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            count.Should().Be(60);
        }
    }
}