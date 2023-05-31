using FizzWare.NBuilder;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.Extensions;
using SOS.Lib.Models.Search.Result;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.ObservationsController.CashedCountInternalEndpoint
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class CountTestGet
    {
        private readonly IntegrationTestFixture _fixture;

        public CountTestGet(IntegrationTestFixture fixture)
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
                  .With(p => p.NotPresent = false)
                  .With(p => p.NotRecovered = false)
                  .With(p => p.UnsureDetermination = false)
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

            var taxonId = 100012;

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.CachedCountInternal(taxonId);
            var result = response.GetResult<TaxonSumAggregationItem>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.ObservationCount.Should().Be(40);
            result.ProvinceCount.Should().Be(2);
        }
    }
}