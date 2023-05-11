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

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.ObservationsController.ObservationsBySearchEndpoint
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class DateFilterTests
    {
        private readonly IntegrationTestFixture _fixture;

        private async Task PopulateDataAsync()
        {
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                .HaveValuesFromPredefinedObservations()
                .TheFirst(20)
                    .IsInDateSpan(DateTime.Parse("2000-01-01T00:00:00"), DateTime.Parse("2000-01-31T23:59:59"))
                .TheNext(60)
                    .IsInDateSpan(DateTime.Parse("2000-02-01T00:00:00"), DateTime.Parse("2000-02-29T23:59:59"))
                .TheNext(20)
                    .IsInDateSpan(DateTime.Parse("2000-03-01T00:00:00"), DateTime.Parse("2000-03-31T23:59:59"))
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
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
            await PopulateDataAsync();

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
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(60);
        }
    }
}
