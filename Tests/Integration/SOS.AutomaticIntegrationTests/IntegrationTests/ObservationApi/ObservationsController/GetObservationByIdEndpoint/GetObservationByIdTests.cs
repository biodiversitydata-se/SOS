using FizzWare.NBuilder;
using FluentAssertions;
using SOS.Lib.Models.Processed.Observation;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.Extensions;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.ObservationsController.GetObservationByIdEndpoint
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class GetObservationByIdTests
    {
        private readonly IntegrationTestFixture _fixture;

        public GetObservationByIdTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "CompleteApiIntegrationTest")]
        public async Task TestGetObservationById()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------
            const int SightingId = 123456;
            const string occurrenceId = $"urn:lsid:artportalen.se:sighting:123456";
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    //.HaveValuesFromRandomPredefinedObservation()
                    .HaveRandomValues()
                .TheFirst(1)
                    .With(p => p.SightingId = SightingId)
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

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
    }
}