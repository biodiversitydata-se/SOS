using System.Threading.Tasks;
using FluentAssertions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;
using SOS.Lib.Models.Search.Filters;
using System.Collections.Generic;
using System.Linq;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ObservationRepository
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class DistinctValuesTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public DistinctValuesTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }       

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Get_all_distinct_organismQuantity()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilter searchFilter = new SearchFilter(0)
            {                
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------            
            //string field = "occurrence.individualCount";
            string field = "occurrence.organismQuantity";
            var items = await _fixture.ProcessedObservationRepository.GetAllAggregationItemsAsync(searchFilter, field);

            List<string> errors = new List<string>();
            foreach (var pair in items)
            {
                if (double.TryParse(pair.AggregationKey, out var val))
                {

                }
                else
                {
                    errors.Add(pair.AggregationKey);
                }
            }

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            items.Should().NotBeNull();
        }
    }
}