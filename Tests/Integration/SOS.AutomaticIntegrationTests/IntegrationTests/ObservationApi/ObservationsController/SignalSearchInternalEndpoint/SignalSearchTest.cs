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
        [Fact(Skip = "Must have autorization")]
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

            var searchFilter = new SignalFilterDto
            {
                StartDate = new DateTime(2020, 1, 1),
                Geographics = new GeographicsFilterDto
                {
                    Areas = new[] { new AreaFilterDto { AreaType = Observations.Api.Dtos.Enum.AreaTypeDto.Municipality, FeatureId = "380" } }
                },
                Taxon = new TaxonFilterBaseDto
                {
                    TaxonListIds = new[] {1}
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.SignalSearchInternal(null, null, searchFilter, onlyAboveMyClearance: true);

            var resultObservation = response.GetResult<PagedResultDto<dynamic>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            var records = resultObservation.Records;
            resultObservation.Should().NotBeNull();
            resultObservation.TotalCount.Should().Be(2);
            records.Sum(r => r.ObservationCount).Should().Be(60);
        }
    }
}