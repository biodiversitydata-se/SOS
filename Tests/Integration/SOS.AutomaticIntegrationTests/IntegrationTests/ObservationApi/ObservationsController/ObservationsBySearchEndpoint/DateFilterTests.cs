using System;
using FizzWare.NBuilder;
using FluentAssertions;
using SOS.Lib.Models.Processed.Observation;
using System.Threading.Tasks;
using Xunit;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.ObservationsController.ObservationsBySearchEndpoint
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class DateFilterTests
    {
        private readonly IntegrationTestFixture _fixture;

        private async Task<List<ArtportalenObservationVerbatim>> PopulateDataAsync()
        {
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                .HaveValuesFromPredefinedObservations()
                .TheFirst(20)                    
                    .IsInDateSpan(
                        DateTime.SpecifyKind(DateTime.Parse("2000-01-01T00:00:00"), DateTimeKind.Local),
                        DateTime.SpecifyKind(DateTime.Parse("2000-01-31T23:59:59"), DateTimeKind.Local))
                .TheNext(60)
                    .IsInDateSpan(
                        DateTime.SpecifyKind(DateTime.Parse("2000-02-01T00:00:00"), DateTimeKind.Local),
                        DateTime.SpecifyKind(DateTime.Parse("2000-02-29T23:59:59"), DateTimeKind.Local))
                .TheNext(20)
                    .IsInDateSpan(
                        DateTime.SpecifyKind(DateTime.Parse("2000-03-01T00:00:00"), DateTimeKind.Local),
                        DateTime.SpecifyKind(DateTime.Parse("2000-03-31T23:59:59"), DateTimeKind.Local))
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            return verbatimObservations.ToList();
        }

        public DateFilterTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestBetweenStartDateAndEndDateFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            

            await PopulateDataAsync();

            var searchFilter = new SearchFilterDto
            {
                Date = new DateFilterDto
                {
                    StartDate = DateTime.Parse("2000-02-01T00:00:00"),
                    EndDate = DateTime.Parse("2000-02-29T23:59:59"),
                    DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
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

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestOverlappingStartDateAndEndDateFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            await PopulateDataAsync();

            var searchFilter = new SearchFilterDto
            {
                Date = new DateFilterDto
                {
                    StartDate = DateTime.Parse("2000-02-01T00:00:00"),
                    EndDate = DateTime.Parse("2000-02-29T23:59:59"),
                    DateFilterType = DateFilterTypeDto.OverlappingStartDateAndEndDate
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
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

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestOnlyStartDateFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            await PopulateDataAsync();

            var searchFilter = new SearchFilterDto
            {
                Date = new DateFilterDto
                {
                    StartDate = DateTime.Parse("2000-02-01T00:00:00"),
                    EndDate = DateTime.Parse("2000-02-29T23:59:59"),
                    DateFilterType = DateFilterTypeDto.OnlyStartDate
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
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

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestOnlyEndDateFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            List<ArtportalenObservationVerbatim> allVerbatimObservations = await PopulateDataAsync();            

            var searchFilter = new SearchFilterDto
            {
                Date = new DateFilterDto
                {
                    StartDate = DateTime.Parse("2000-02-01T00:00:00"),
                    EndDate = DateTime.Parse("2000-02-29T23:59:59"),
                    DateFilterType = DateFilterTypeDto.OnlyEndDate
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                null,
                null,
                searchFilter,
                0,
                100);
            var result = response.GetResult<PagedResultDto<Observation>>();            

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            //await DebugTestOnlyEndDateFilter(allVerbatimObservations, result.Records);
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(60);
        }

        private async Task DebugTestOnlyEndDateFilter(
            IList<ArtportalenObservationVerbatim> verbatimObservations,
            IEnumerable<Observation> resultObservations)
        {
            var testResultItems = await _fixture.CreateTestResultSummary(verbatimObservations, resultObservations);
            foreach (var item in testResultItems)
            {
                item.VerbatimValue = item.VerbatimObservation.EndDate.Value;
                item.ProcessedValue = item.ProcessedObservation.Event.EndDate.Value;
            }
            testResultItems = testResultItems
                .OrderBy(m => m.ProcessedObservation.Event.EndDate.Value)
                .ToList();
        }
    }
}
