﻿using FluentAssertions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.CompleteIntegrationTests.ObservationsController
{
    [Collection(Collections.CompleteApiIntegrationTestsCollection)]
    public class GetObservationByIdTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public GetObservationByIdTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "CompleteApiIntegrationTest")]
        public async Task TestProcessObservation()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Add observation to Elasticsearch
            //-----------------------------------------------------------------------------------------------------------                
            var verbatimObservation = ArtportalenObservationVerbatimFactory.Create();
            var observation = _fixture.ArtportalenObservationFactory.CreateProcessedObservation(verbatimObservation, false);
            await CreateIntegrationTestIndexAsync();            
            await AddObservationToElasticAsync(observation);            
            string occurrenceId = $"urn:lsid:artportalen.se:sighting:{verbatimObservation.SightingId}";

            //-----------------------------------------------------------------------------------------------------------
            // Act
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

        private async Task CreateIntegrationTestIndexAsync()
        {
            const bool protectedIndex = false;
            var customRepository = _fixture.CustomProcessedObservationRepository;
            await customRepository.ClearCollectionAsync(protectedIndex);
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
    }
}