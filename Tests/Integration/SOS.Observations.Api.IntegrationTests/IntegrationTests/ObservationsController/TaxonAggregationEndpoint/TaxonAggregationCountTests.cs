using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ObservationsController.TaxonAggregationEndpoint
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class TaxonAggregationCountTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public TaxonAggregationCountTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;        
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Dictionary_vs_Array_performance_test()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            Random random = new Random();
            const int NrIterations = 100000;

            //-----------------------------------------------------------------------------------------------------------
            // Act - Dictionary
            //-----------------------------------------------------------------------------------------------------------
            var spDictionary = Stopwatch.StartNew();
            var dictionary = new Dictionary<int, Dictionary<int, int>>();
            for (int i=0; i<NrIterations; i++)
            {
                var dic = new Dictionary<int, int>();
                for (int j=0; j < 100; j++)
                {
                    var provinceId = random.Next(0, 33);
                    if (!dic.TryAdd(provinceId, i))
                    {
                        dic[provinceId] += 1;
                    }                                        
                }
                dictionary.Add(i, dic);
            }
            spDictionary.Stop();

            //-----------------------------------------------------------------------------------------------------------
            // Act - Array
            //-----------------------------------------------------------------------------------------------------------
            var spArray = Stopwatch.StartNew();
            var dictionaryArray = new Dictionary<int, int[]>();
            for (int i = 0; i < NrIterations; i++)
            {
                var array = new int[34];
                for (int j = 0; j < 100; j++)
                {
                    var provinceId = random.Next(0, 33);
                    array[provinceId] += 1;                    
                }
                dictionaryArray.Add(i, array);
            }
            spArray.Stop();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            spArray.ElapsedMilliseconds.Should().BeLessThan(spDictionary.ElapsedMilliseconds, "because array type is much faster than dictionary type");
        }


        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task GetCachedCount()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            int taxonId = TestData.TaxonIds.Mammalia;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.CachedCountInternal(taxonId);
            var result = response.GetResult<TaxonSumAggregationItem>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.SumObservationCount.Should().BeGreaterThan(50000);
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task GetMultipleCachedCount()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            int[] taxonIds = new int[] {
                TestData.TaxonIds.Mammalia,
                TestData.TaxonIds.Otter,
                TestData.TaxonIds.Aves,
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.MultipleCachedCountInternal(taxonIds);
            var result = response.GetResult<IEnumerable<TaxonSumAggregationItem>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Count().Should().Be(3);
            result.Single(m => m.TaxonId == TestData.TaxonIds.Aves).SumObservationCount.Should().BeGreaterThan(1000000);
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task TaxonAggregationCount_include_all_taxa_with_paging()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var taxonFilter = new TaxonFilterDto
            {
                IncludeUnderlyingTaxa = true
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.TaxonSumAggregationInternal(taxonFilter,
                0,
                10);
            var result = response.GetResult<PagedResultDto<TaxonSumAggregationItem>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.TotalCount.Should().BeGreaterThan(100000, "because there exists more than 100 000 taxa");
            result.Records.Count().Should().Be(10);
            result.Records.First().TaxonId.Should().Be(TestData.TaxonIds.Biota, "because Biota should have the highest sum of observations");
            result.Records.First().SumObservationCount.Should().BeGreaterThan(10000000, "because there are more than 10 000 000 present observations");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task TaxonAggregationCount_mammalia_taxa_without_paging()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var taxonFilter = new TaxonFilterDto
            {
                Ids = new int[] { TestData.TaxonIds.Mammalia },
                IncludeUnderlyingTaxa = true
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.TaxonSumAggregationInternal(taxonFilter);
            var result = response.GetResult<PagedResultDto<TaxonSumAggregationItem>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.TotalCount.Should().BeGreaterThan(250, "because there exists more than 250 taxa");
            result.Records.Count().Should().Be(Convert.ToInt32(result.TotalCount));
            result.Records.First().TaxonId.Should().Be(TestData.TaxonIds.Mammalia, "because Mammalia should have the highest sum of observations");
            result.Records.First().SumObservationCount.Should().BeGreaterThan(50000, "because there are more than 50 000 present observations");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task TaxonAggregationCount_only_mammalia_taxa_without_paging()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var taxonFilter = new TaxonFilterDto
            {
                Ids = new int[] { TestData.TaxonIds.Mammalia },
                IncludeUnderlyingTaxa = false
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.TaxonSumAggregationInternal(taxonFilter);
            var result = response.GetResult<PagedResultDto<TaxonSumAggregationItem>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.TotalCount.Should().Be(1);
            result.Records.Count().Should().Be(1);
            result.Records.First().TaxonId.Should().Be(TestData.TaxonIds.Mammalia, "because Mammalia should have the highest sum of observations");
            result.Records.First().SumObservationCount.Should().BeGreaterThan(50000, "because there are more than 50 000 present observations");
        }


        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task TaxonAggregationCount_with_no_taxa_and_without_underlyingTaxa()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var taxonFilter = new TaxonFilterDto
            {                
                IncludeUnderlyingTaxa = false
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.TaxonSumAggregationInternal(taxonFilter);
            var result = response.GetResult<PagedResultDto<TaxonSumAggregationItem>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.TotalCount.Should().Be(1, "only Biota is returned");            
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task TaxonAggregationCount_with_no_taxa_and_with_underlyingTaxa()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var taxonFilter = new TaxonFilterDto
            {
                IncludeUnderlyingTaxa = true
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.TaxonSumAggregationInternal(taxonFilter);
            var result = response.GetResult<PagedResultDto<TaxonSumAggregationItem>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.TotalCount.Should().BeGreaterThan(100000, "because there exists more than 100 000 taxa");
            result.Records.Count().Should().BeGreaterThan(100000);
            result.Records.First().TaxonId.Should().Be(TestData.TaxonIds.Biota, "because Biota should have the highest sum of observations");
            result.Records.First().SumObservationCount.Should().BeGreaterThan(10000000, "because there are more than 10 000 000 present observations");
            result.Records.First().SumProvinceCount.Should().BeGreaterOrEqualTo(34);
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task TaxonAggregationCount_without_TaxonFilter_is_the_same_as_Biota_with_underlying_taxa()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            TaxonFilterDto taxonFilter = null;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------            
            var response = await _fixture.ObservationsController.TaxonSumAggregationInternal(taxonFilter);
            var result = response.GetResult<PagedResultDto<TaxonSumAggregationItem>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.TotalCount.Should().BeGreaterThan(100000, "because there exists more than 100 000 taxa");
            result.Records.Count().Should().BeGreaterThan(100000);
            result.Records.First().TaxonId.Should().Be(TestData.TaxonIds.Biota, "because Biota should have the highest sum of observations");
            result.Records.First().SumObservationCount.Should().BeGreaterThan(10000000, "because there are more than 10 000 000 present observations");
            result.Records.First().SumProvinceCount.Should().BeGreaterOrEqualTo(34);
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task TaxonAggregationCount_with_only_species()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            TaxonFilterDto taxonFilter = new TaxonFilterDto
            {
                IncludeUnderlyingTaxa = true,
                TaxonCategories = new List<int> { (int)TaxonCategoryId.Species }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------            
            var response = await _fixture.ObservationsController.TaxonSumAggregationInternal(taxonFilter, 0, 100);
            var result = response.GetResult<PagedResultDto<TaxonSumAggregationItem>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.TotalCount.Should().BeGreaterThan(70000, "because there exists more than 70 000 species");
            result.Records.Count().Should().Be(100);
            var commonBlackBird = result.Records.SingleOrDefault(m => m.TaxonId == TestData.TaxonIds.CommonBlackbird);
            commonBlackBird.Should().NotBeNull("Common Blackbird (koltrast) should be one of the 100 most observed species in Sweden");
            commonBlackBird.ObservationCount.Should().BeGreaterThan(50000);
            commonBlackBird.SumObservationCount.Should().BeGreaterOrEqualTo(commonBlackBird.ObservationCount);
            commonBlackBird.ProvinceCount.Should().BeGreaterOrEqualTo(25);
            commonBlackBird.SumProvinceCount.Should().BeGreaterOrEqualTo(25);            
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task TaxonAggregationCount_sort_by_observationCount()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            TaxonFilterDto taxonFilter = null;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------            
            var response = await _fixture.ObservationsController.TaxonSumAggregationInternal(taxonFilter, 0, 100, "ObservationCount");
            var result = response.GetResult<PagedResultDto<TaxonSumAggregationItem>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.TotalCount.Should().BeGreaterThan(100000, "because there exists more than 100 000 taxa");
            result.Records.Count().Should().Be(100);
            var commonBlackBird = result.Records.SingleOrDefault(m => m.TaxonId == TestData.TaxonIds.CommonBlackbird);
            commonBlackBird.Should().NotBeNull("Common Blackbird (koltrast) should be one of the 100 most observed species in Sweden");
            commonBlackBird.ObservationCount.Should().BeGreaterThan(50000);
            commonBlackBird.SumObservationCount.Should().BeGreaterOrEqualTo(commonBlackBird.ObservationCount);
            commonBlackBird.ProvinceCount.Should().BeGreaterOrEqualTo(25);
            commonBlackBird.SumProvinceCount.Should().BeGreaterOrEqualTo(25);
        }
    }
}