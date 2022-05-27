using FizzWare.NBuilder;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.Extensions;
using SOS.Lib.Models.Search;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.ObservationsController.CashedCountInternalEndpoint
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class CountTestPost
    {
        private readonly IntegrationTestFixture _fixture;

        public CountTestPost(IntegrationTestFixture fixture)
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
                    .With(p => p.TaxonId = 100011)
                    .With(p => p.Site.Province = new GeographicalArea { FeatureId = "1" })
                .TheNext(20)
                    .With(p => p.TaxonId = 100012)
                    .With(p => p.Site.Province = new GeographicalArea { FeatureId = "2" })
                .TheNext(20)
                    .With(p => p.TaxonId = 100012)
                    .With(p => p.Site.Province = new GeographicalArea { FeatureId = "3" })
                .TheNext(20)
                    .With(p => p.TaxonId = 100013)
                .TheLast(20)
                    .With(p => p.TaxonId = 100014)
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var taxonIds = new[] { 100011, 100012 };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.MultipleCachedCountInternal(taxonIds);
            var result = response.GetResult<IEnumerable<TaxonSumAggregationItem>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Count().Should().Be(2);
            result.Sum(r => r.ObservationCount).Should().Be(60);
        }
    }
}