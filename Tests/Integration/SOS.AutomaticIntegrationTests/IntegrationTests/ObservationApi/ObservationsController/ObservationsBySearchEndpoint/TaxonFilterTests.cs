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
    public class TaxonFilterTests
    {
        private readonly IntegrationTestFixture _fixture;

        public TaxonFilterTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "CompleteApiIntegrationTest")]
        public async Task TestRedListCategoriesFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(20)
                    .HaveRedlistedTaxonId("CR") // Critically Endangered (Akut hotad)
                .TheNext(20)
                    .HaveRedlistedTaxonId("EN") // Endangered (Starkt hotad)
                .TheNext(20)
                    .HaveRedlistedTaxonId("VU") // Vulnerable (Sårbar)
                .TheNext(20)
                    .HaveRedlistedTaxonId("NT") // Near Threatened (Nära hotad)
                .TheNext(20)
                    .HaveRedlistedTaxonId(null) // Not redlisted taxa
                .Build();
            
            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto
                {
                    RedListCategories = new List<string> { "CR", "EN", "VU" }
                }
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