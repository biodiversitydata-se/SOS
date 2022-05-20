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
using SOS.Lib.Models.Processed.Checklist;
using SOS.Observations.Api.Dtos.Filter;
using System;
using SOS.Lib.Models.Statistics;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.ObservationsController.GetObservationByIdEndpoint
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class CalculateTrendTests
    {
        private readonly IntegrationTestFixture _fixture;

        public CalculateTrendTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestCalculateTrend()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create checklist data
            //-----------------------------------------------------------------------------------------------------------                        
            var verbatimChecklists = Builder<ArtportalenChecklistVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedChecklists()
                    .With(m => m.StartDate = DateTime.Now - TimeSpan.FromMinutes(30)) // 30 minutes search effort time.
                    .With(m => m.EndDate = DateTime.Now)
                .TheFirst(60) // 60 talgoxe found
                    .With(m => m.TaxonIds = new List<int>() { 103026, 102998 })
                    .With(m => m.TaxonIdsFound = new List<int>() { 103026 })
                .TheNext(20) // 20 talgoxe not found
                    .With(m => m.TaxonIds = new List<int>() { 103026, 102998 })
                    .With(m => m.TaxonIdsFound = new List<int>() { 102998 })
                .TheNext(20) // didn't look for talgoxe
                    .With(m => m.TaxonIds = new List<int>() { 102998 })
                    .With(m => m.TaxonIdsFound = new List<int>() { })
                .Build();
            await _fixture.ProcessAndAddChecklistsToElasticSearch(verbatimChecklists);

            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Specify filter
            //-----------------------------------------------------------------------------------------------------------            
            var calculateTrendFilter = new CalculateTrendFilterDto
            {
                Checklist = new TrendChecklistFilterDto
                {
                    MinEffortTime = "00:15:00"
                },
                TaxonId = 103026
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ChecklistsController.CalculateTrendAsync(calculateTrendFilter);
            var trend = response.GetResultObject<TaxonTrendResult>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------                        
            trend.Quotient.Should().BeApproximately(60.0/80, 0.0001, "because there are 60 present observations and 20 absent");
            trend.NrPresentObservations.Should().Be(60, "because there are 60 checklists matching the search criteria where talgoxe was found");
            trend.NrAbsentObservations.Should().Be(20, "because there are 20 checklists matching the search criteria where talgoxe was not found");
            trend.NrChecklists.Should().Be(80, "because there are 80 checklists matching the search criteria");
            trend.TaxonId.Should().Be(103026);
        }
    }
}