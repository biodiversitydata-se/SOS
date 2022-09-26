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

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.ObservationsController.SearchAggregatedInternalEndpoint
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class AggregationTypeTests
    {
        private readonly IntegrationTestFixture _fixture;

        public AggregationTypeTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestSearchAggregatedInternalSightingsPerWeek()
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
                    .With(p => p.TaxonId = 100013)
                    .With(p => p.StartDate = new DateTime(2000, 2, 1))
                    .With(p => p.EndDate = new DateTime(2000, 2, 1))
                .TheLast(20)
                    .With(p => p.TaxonId = 100012)
                    .With(p => p.StartDate = new DateTime(2000, 1, 1))
                    .With(p => p.EndDate = new DateTime(2000, 1, 15))
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterAggregationInternalDto
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
            var response = await _fixture.ObservationsController.SearchAggregatedInternal(null, null,
                searchFilter,
                Lib.Enums.AggregationType.SightingsPerWeek);

            var resultObservation = response.GetResult<PagedResultDto<dynamic>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            var records = resultObservation.Records;
            resultObservation.Should().NotBeNull();
            resultObservation.TotalCount.Should().Be(60);
            records.Count().Should().Be(5);
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestSearchAggregatedInternalSightingsPerYear()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------

            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               //.HaveRandomValues()
               .TheFirst(20)
                   .With(p => p.TaxonId = 100011)
                   .With(p => p.StartDate = new DateTime(2000, 1, 1))
                   .With(p => p.EndDate = new DateTime(2000, 1, 1))
                   .With(p => p.Site.Province = new GeographicalArea { FeatureId = "1" })
               .TheNext(20)
                   .With(p => p.TaxonId = 100012)
                   .With(p => p.StartDate = new DateTime(2000, 1, 15))
                   .With(p => p.EndDate = new DateTime(2000, 1, 18))
                   .With(p => p.Site.Province = new GeographicalArea { FeatureId = "2" })
               .TheNext(20)
                   .With(p => p.TaxonId = 100012)
                   .With(p => p.StartDate = new DateTime(2000, 1, 30))
                   .With(p => p.EndDate = new DateTime(2000, 1, 30))
                   .With(p => p.Site.Province = new GeographicalArea { FeatureId = "3" })
               .TheNext(20)
                   .With(p => p.TaxonId = 100016)
                   .With(p => p.StartDate = new DateTime(2000, 1, 1))
                   .With(p => p.EndDate = new DateTime(2000, 2, 1))
               .TheLast(20)
                   .With(p => p.TaxonId = 100017)
                   .With(p => p.StartDate = new DateTime(2000, 4, 1))
                   .With(p => p.EndDate = new DateTime(2000, 4, 15))
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterAggregationInternalDto
            {
                Date = new DateFilterDto
                {
                    StartDate = DateTime.Parse("2000-01-01T00:00:00"),
                    EndDate = DateTime.Parse("2000-01-31T23:59:59"),
                    DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
                },
                Taxon = new TaxonFilterDto
                {
                    Ids = new[] { 100011, 100012 }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.SearchAggregatedInternal(null, null,
                searchFilter,
                Lib.Enums.AggregationType.SightingsPerYear);

            var resultObservation = response.GetResult<PagedResultDto<dynamic>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            var records = resultObservation.Records;
            resultObservation.Should().NotBeNull();
            resultObservation.TotalCount.Should().Be(60);
            records.Count().Should().Be(1);
        }
    }
}