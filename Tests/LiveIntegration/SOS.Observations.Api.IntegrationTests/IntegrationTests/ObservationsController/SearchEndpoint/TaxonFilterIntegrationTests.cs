using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ObservationsController.SearchEndpoint
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class TaxonFilterIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public TaxonFilterIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_for_mammalia_species_that_is_invasive()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto
                {
                    Ids = new List<int> { TestData.TaxonIds.Mammalia }, 
                    IncludeUnderlyingTaxa = true,
                    TaxonListIds = new List<int> {(int)TaxonListId.InvasiveSpecies},
                    TaxonListOperator = TaxonListOperatorDto.Filter                    
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(0, null, searchFilter, 0, 2);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.Count().Should().Be(2, "because the take parameter is 2");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_for_mammalia_species_that_is_invasive_and_species()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto
                {
                    Ids = new List<int> { TestData.TaxonIds.Mammalia },
                    IncludeUnderlyingTaxa = true,
                    TaxonListIds = new List<int> { (int)TaxonListId.InvasiveSpecies },
                    TaxonListOperator = TaxonListOperatorDto.Filter,
                    TaxonCategories = new List<int> { (int)TaxonCategoryId.Species }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(0, null, searchFilter, 0, 2);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.Count().Should().Be(2, "because the take parameter is 2");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_for_biota_species_includeUnderlyingTaxa()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto
                {
                    Ids = new List<int> { TestData.TaxonIds.Biota },
                    IncludeUnderlyingTaxa = true,
                    TaxonCategories = new List<int> { (int)TaxonCategoryId.Species }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(0, null, searchFilter, 0, 2);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.Count().Should().Be(2, "because the take parameter is 2");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_for_taxon_list_with_includeUnderlyingTaxa()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto
                {
                    TaxonListIds = new List<int> { (int)TaxonListId.SwedishForestAgencyNatureConservationSpecies }, // 18
                    IncludeUnderlyingTaxa = true,
                    TaxonListOperator = TaxonListOperatorDto.Merge
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(0, null, searchFilter, 0, 2);
            var resultIncludeUnderlyingTaxa = response.GetResult<PagedResultDto<Observation>>();

            searchFilter.Taxon.IncludeUnderlyingTaxa = false;
            response = await _fixture.ObservationsController.ObservationsBySearch(0, null, searchFilter, 0, 2);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            resultIncludeUnderlyingTaxa.TotalCount.Should().BeGreaterThan(result.TotalCount);
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_for_all_species()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto
                {                    
                    IncludeUnderlyingTaxa = true,
                    TaxonCategories = new List<int> { (int)TaxonCategoryId.Species }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(0, null, searchFilter, 0, 2);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.Count().Should().Be(2, "because the take parameter is 2");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_for_biota_species_without_UnderlyingTaxa()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto
                {
                    Ids = new List<int> { TestData.TaxonIds.Biota },
                    IncludeUnderlyingTaxa = false,
                    TaxonCategories = new List<int> { (int)TaxonCategoryId.Species }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(0, null, searchFilter, 0, 2);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.Count().Should().Be(0, "because the Biota taxon category isn't Species");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_for_species()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto
                {                    
                    TaxonCategories = new List<int> { (int)TaxonCategoryId.Species }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(0, null, searchFilter, 0, 2);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.Count().Should().Be(2, "because the take parameter is 2");
        }
    }
}
