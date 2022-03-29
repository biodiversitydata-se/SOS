using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTestsBase
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class ProcessAndTestObservationIntegrationTest
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public ProcessAndTestObservationIntegrationTest(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task TestProcessObservation()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------                
            var customRepository = _fixture.CustomProcessedObservationRepository;
            await CreateIntegrationTestIndexAsync();
            var observation = await ProcessObservationAsync();
            await AddObservationToElasticAsync(observation);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------                
            SearchFilter filter = new SearchFilter();
            var obs = await customRepository.GetObservationAsync("obs1", filter);
            var response = await _fixture.CustomObservationsController.GetObservationById(null, null, null, "obs1", Lib.Enums.OutputFieldSet.AllWithValues);            
            var result = response.GetResult<Observation>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            obs.Should().NotBeNull();
        }

        private async Task CreateIntegrationTestIndexAsync()
        {
            const bool protectedIndex = false;
            var customRepository = _fixture.CustomProcessedObservationRepository;
            await customRepository.ClearCollectionAsync(protectedIndex);
        }

        private async Task<Observation> ProcessObservationAsync()
        {
            var observation = new Observation
            {
                Event = new Event
                {
                    PlainStartTime = new TimeSpan(10, 30, 0).ToString("hh\\:mm"),
                    PlainEndTime = new TimeSpan(13, 0, 0).ToString("hh\\:mm")
                },
                Occurrence = new Occurrence
                {
                    OccurrenceId = "obs1",
                    OccurrenceRemarks = "testobs"
                }
            };

            return observation;
        }

        private async Task AddObservationToElasticAsync(Observation observation)
        {
            const bool protectedIndex = false;
            var customRepository = _fixture.CustomProcessedObservationRepository;
            //await customRepository.DisableIndexingAsync(protectedIndex);
            await customRepository.AddManyAsync(new[] { observation }, protectedIndex);
            //await customRepository.EnableIndexingAsync(protectedIndex);
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
                    Event = new Event
                    {
                        PlainStartTime = new TimeSpan(10, 30, 0).ToString("hh\\:mm"),
                        PlainEndTime = new TimeSpan(13, 0, 0).ToString("hh\\:mm")
                    },
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