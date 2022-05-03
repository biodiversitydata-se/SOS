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

        [Fact]
        [Trait("Category", "CompleteApiIntegrationTest")]
        public async Task TestTaxonIdFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(20)
                    .HaveTaxonId(103032)  // Pica pica
                .TheNext(20)
                    .HaveTaxonId(222135)  // Abies alba
                .TheNext(20)
                    .HaveTaxonId(221100)  // Impatiens glandulifera
                .TheNext(20)
                    .HaveTaxonId(103026)  // Parus major
                .TheNext(20)
                    .HaveTaxonId(103025)  // Cyanistes caeruleus
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto
                {
                    Ids = new [] { 222135, 103025, 103026 }
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

        [Fact]
        [Trait("Category", "CompleteApiIntegrationTest")]
        public async Task TestTaxonIdUnderlyingFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(20)
                    .HaveTaxonId(103024)  // Svartmes
                .TheNext(20)
                    .HaveTaxonId(103025)  // Blåmes
                .TheNext(20)
                    .HaveTaxonId(103023)  // Tofsmes
                .TheNext(20)
                    .HaveTaxonId(221100)  // Jättebalsamin
                .TheNext(20)
                    .HaveTaxonId(100001)  // Duvhök
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto
                {
                    Ids = new[] { 2002112 }, // mesar
                    IncludeUnderlyingTaxa = true
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

        [Fact]
        [Trait("Category", "CompleteApiIntegrationTest")]
        public async Task TestTaxonListMergeIdFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(20)
                    .HaveTaxonId(103025)  // Blåmes
                .TheNext(20)
                    .HaveTaxonId(100943)  // Asknätfjäril
                .TheNext(20)
                    .HaveTaxonId(101260)  // Svartfläckig blåvinge
                .TheNext(20)
                    .HaveTaxonId(221100)  // Jättebalsamin
                .TheNext(20)
                    .HaveTaxonId(219680)  // Jätteloka
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto
                {
                    TaxonListIds = new[] { 1 }, // Fridlysta arter
                    Ids = new[] { 103025 }, // Blåmes
                    TaxonListOperator = TaxonListOperatorDto.Merge
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

        [Fact]
        [Trait("Category", "CompleteApiIntegrationTest")]
        public async Task TestTaxonListFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(40)
                    .HaveTaxonId(101248)  // Violett guldvinge
                .TheNext(20)
                    .HaveTaxonId(100943)  // Asknätfjäril
                .TheNext(20)
                    .HaveTaxonId(101260)  // Svartfläckig blåvinge
                .TheNext(20)
                    .HaveTaxonId(221100)  // Jättebalsamin

                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto
                {
                    TaxonListIds = new[] { 1 }, // Fridlysta arter
                    Ids = new[] { 101248, 100943 }, // Violett guldvinge, Jättebalsamin
                    TaxonListOperator = TaxonListOperatorDto.Filter
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

        [Fact]
        [Trait("Category", "CompleteApiIntegrationTest")]
        public async Task TestTaxonCategoryFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(40)
                    .HaveTaxonCategoryTaxonId(17)  // Art
                .TheNext(20)
                    .HaveTaxonCategoryTaxonId(18)  // 
                .TheNext(20)
                    .HaveTaxonCategoryTaxonId(14)  // 
                .TheNext(20)
                    .HaveTaxonCategoryTaxonId(11)  // 
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto
                {
                    TaxonCategories = new List<int> { 17, 18 } // art
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