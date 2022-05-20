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
using SOS.Lib.Models.Processed.CheckList;
using SOS.Observations.Api.Dtos.Checklist;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.ObservationsController.GetObservationByIdEndpoint
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class GetChecklistByIdTests
    {
        private readonly IntegrationTestFixture _fixture;

        public GetChecklistByIdTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestGetChecklistById()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------
            const int Id = 123456;
            const string eventId = $"urn:lsid:artportalen.se:Checklist:123456";
            var verbatimChecklists = Builder<ArtportalenCheckListVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedChecklists()                    
                .TheFirst(1)
                    .With(p => p.Id = Id)
                .Build();

            await _fixture.ProcessAndAddChecklistsToElasticSearch(verbatimChecklists);

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ChecklistsController.GetChecklistByIdAsync(eventId);
            var resultChecklist = response.GetResultObject<CheckListDto>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            resultChecklist.Should().NotBeNull();
            resultChecklist.Event.EventId.Should().Be(eventId);
            resultChecklist.Id.Should().Be(eventId);            
        }
    }
}