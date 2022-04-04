using FizzWare.NBuilder;
using FluentAssertions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Observations.Api.IntegrationTests.CompleteIntegrationTests.ObservationsController
{ 
    [Collection(Collections.CompleteApiIntegrationTestsCollection)]
    public class GetObservationByIdTests
    {
        private readonly CompleteApiIntegrationTestFixture _fixture;

        public GetObservationByIdTests(CompleteApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
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
        
        [Fact]
        [Trait("Category", "CompleteApiIntegrationTest")]
        public async Task TestGetObservationById()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------
            const int sightingId = 123456;
            const string occurrenceId = $"urn:lsid:artportalen.se:sighting:123456";
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveRandomValidValues()
                .TheFirst(1)
                    .With(p => p.SightingId = sightingId)                
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Process observations
            //-----------------------------------------------------------------------------------------------------------
            var processedObservations = ProcessObservations(verbatimObservations);

            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Add observation to Elasticsearch
            //-----------------------------------------------------------------------------------------------------------
            await _fixture.CustomProcessedObservationRepository.DeleteAllDocumentsAsync(false);
            await AddObservationsToElasticAsync(processedObservations);            

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.CustomObservationsController.GetObservationById(
                null,
                null,
                null,
                occurrenceId,
                Lib.Enums.OutputFieldSet.AllWithValues);
            var resultObservation = response.GetResult<Observation>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            resultObservation.Should().NotBeNull();
            resultObservation.Occurrence.OccurrenceId.Should().Be(occurrenceId);
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