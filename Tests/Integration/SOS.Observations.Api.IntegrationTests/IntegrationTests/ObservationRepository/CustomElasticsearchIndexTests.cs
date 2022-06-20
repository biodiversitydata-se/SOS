using System;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ObservationRepository
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class CustomElasticsearchIndexTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public CustomElasticsearchIndexTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Test_create_Elasticsearch_index_from_current_observation_model()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var customRepository = _fixture.CustomProcessedObservationRepository;
            const bool protectedIndex = false;
            var observations = new[]
            {
                new Observation
                {
                    Event = new Event(DateTime.Now.AddHours(-2), new TimeSpan(10, 30, 0), DateTime.Now.AddHours(-1), new TimeSpan(13, 0, 0)),
                    Occurrence = new Occurrence
                    {
                        OccurrenceId = "obs1"
                    }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            await customRepository.ClearCollectionAsync(protectedIndex);
            await customRepository.DisableIndexingAsync(protectedIndex);
            await customRepository.AddManyAsync(observations, protectedIndex);
            await customRepository.EnableIndexingAsync(protectedIndex);
            SearchFilter filter = new SearchFilter();
            var obs = await customRepository.GetObservationAsync("obs1", filter);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            obs.Should().NotBeNull();
        }
    }
}