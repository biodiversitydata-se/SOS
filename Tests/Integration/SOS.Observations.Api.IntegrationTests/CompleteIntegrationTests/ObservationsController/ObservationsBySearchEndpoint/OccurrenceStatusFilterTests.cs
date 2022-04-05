using FizzWare.NBuilder;
using FluentAssertions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos;
using System.Linq;
using LinqStatistics;

namespace SOS.Observations.Api.IntegrationTests.CompleteIntegrationTests.ObservationsController.ObservationsBySearchEndpoint
{
    [Collection(Collections.CompleteApiIntegrationTestsCollection)]
    public class OccurrenceStatusFilterTests
    {
        private readonly CompleteApiIntegrationTestFixture _fixture;

        public OccurrenceStatusFilterTests(CompleteApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "CompleteApiIntegrationTest")]
        public async Task GetOnlyPresentObservations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()                    
                    .HaveRandomValidValues()
                .TheFirst(60)
                    .With(v => v.NotPresent = false)
                    .With(v => v.NotRecovered = false)
                .TheNext(20)
                    .With(v => v.NotPresent = true)
                .TheNext(20)
                    .With(v => v.NotRecovered = true)
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Process observations
            //-----------------------------------------------------------------------------------------------------------
            var processedObservations = ProcessObservations(verbatimObservations);
            
            // Assert 60 observations has IsPositiveObservation=true after processing
            processedObservations
                .Select(m => m.Occurrence.IsPositiveObservation)
                .CountEach()
                .OrderByDescending(m => m.Count)
                .ToList()
                .Should().BeEquivalentTo(
                    new List<ItemCount<bool?>> { 
                        new ItemCount<bool?>(true, 60), 
                        new ItemCount<bool?>(false, 40) });

            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Add observation to Elasticsearch
            //-----------------------------------------------------------------------------------------------------------
            await _fixture.CustomProcessedObservationRepository.DeleteAllDocumentsAsync(false);
            await AddObservationsToElasticAsync(processedObservations);

            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Search filter
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilterDto
            {
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
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

        private List<Observation> ProcessObservations(IEnumerable<ArtportalenObservationVerbatim> verbatimObservations)
        {
            var processedObservations = new List<Observation>();
            bool diffuseIfSupported = false;
            foreach (var verbatimObservation in verbatimObservations)
            {
                var processedObservation = _fixture.ArtportalenObservationFactory.CreateProcessedObservation(verbatimObservation, diffuseIfSupported);
                processedObservations.Add(processedObservation);
            }

            return processedObservations;
        }

        private async Task AddObservationToElasticAsync(Observation observation)
        {
            const bool protectedIndex = false;
            var customRepository = _fixture.CustomProcessedObservationRepository;
            //await customRepository.DisableIndexingAsync(protectedIndex);
            await customRepository.AddManyAsync(new[] { observation }, protectedIndex);
            //await customRepository.EnableIndexingAsync(protectedIndex);
            Thread.Sleep(1000);
        }

        private async Task AddObservationsToElasticAsync(IEnumerable<Observation> observations)
        {
            const bool protectedIndex = false;
            var customRepository = _fixture.CustomProcessedObservationRepository;
            await customRepository.DisableIndexingAsync(protectedIndex);
            await customRepository.AddManyAsync(observations, protectedIndex);
            await customRepository.EnableIndexingAsync(protectedIndex);
            Thread.Sleep(1000);
        }
    }
}