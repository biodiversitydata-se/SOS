﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.LiveIntegrationTests.Extensions;
using SOS.Observations.Api.LiveIntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.LiveIntegrationTests.IntegrationTests.ObservationsController.SearchEndpoint
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class ExtendedFilterIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public ExtendedFilterIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_with_months_consider_both_startDate_and_endDate()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilterInternalDto()
            {
                Date = new DateFilterDto()
                {
                    StartDate = new DateTime(2018, 1, 1),
                    EndDate = new DateTime(2020, 12, 31),
                    DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
                },
                ExtendedFilter = new ExtendedFilterDto()
                {
                    Months = new List<int> { 1, 2, 4 },
                    MonthsComparison = ExtendedFilterDto.DateFilterComparisonDto.BothStartDateAndEndDate
                },
                Output = new OutputFilterExtendedDto { Fields = new[] { "Event" } }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(null, null, searchFilter, 0, 1000);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert - prepare
            //-----------------------------------------------------------------------------------------------------------
            var yearsStartDate = new HashSet<int>();
            var monthsStartDate = new HashSet<int>();
            var yearsEndDate = new HashSet<int>();
            var monthsEndDate = new HashSet<int>();
            foreach (var observation in result.Records)
            {
                yearsStartDate.Add(observation.Event.StartDate.Value.Year);
                monthsStartDate.Add(observation.Event.StartDate.Value.Month);
                yearsEndDate.Add(observation.Event.EndDate.Value.Year);
                monthsEndDate.Add(observation.Event.EndDate.Value.Month);
            }

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            yearsStartDate.Should().BeEquivalentTo(new[] { 2018, 2019, 2020 });
            yearsEndDate.Should().BeEquivalentTo(new[] { 2018, 2019, 2020 });
            monthsStartDate.Should().BeEquivalentTo(new[] { 1, 2, 4 });
            monthsEndDate.Should().BeEquivalentTo(new[] { 1, 2, 4 });
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Test_search_female_taxa()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilterInternalDto()
            {
                DataProvider = new DataProviderFilterDto
                {
                    Ids = new List<int>() { 1 }
                },
                //Date = new DateFilterDto()
                //{
                //    StartDate = new DateTime(2016, 1, 1),
                //    EndDate = new DateTime(2017, 12, 31),
                //    DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
                //},
                //Date = new DateFilterDto()
                //{
                //    StartDate = new DateTime(2018, 1, 1),
                //    EndDate = new DateTime(2022, 12, 31),
                //    DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
                //},
                Taxon = new TaxonFilterDto()
                {
                    IncludeUnderlyingTaxa = true,
                    Ids = new int[] { 103046 }
                },
                ExtendedFilter = new ExtendedFilterDto()
                {
                    SexIds = new List<int> { (int)SexId.Female }
                },
                IncludeRealCount = true
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(null, null, searchFilter, 0, 10);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.TotalCount.Should().BeGreaterOrEqualTo(100);
        }


        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_with_months_consider_startDate()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilterInternalDto()
            {
                Date = new DateFilterDto()
                {
                    StartDate = new DateTime(2018, 1, 1),
                    EndDate = new DateTime(2020, 12, 31),
                    DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
                },
                ExtendedFilter = new ExtendedFilterDto()
                {
                    Months = new List<int> { 1, 2, 4 },
                    MonthsComparison = ExtendedFilterDto.DateFilterComparisonDto.StartDate
                },
                Output = new OutputFilterExtendedDto { Fields = new[] { "Event" } }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(null, null, searchFilter, 0, 1000);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert - prepare
            //-----------------------------------------------------------------------------------------------------------
            var yearsStartDate = new HashSet<int>();
            var monthsStartDate = new HashSet<int>();
            var yearsEndDate = new HashSet<int>();
            var monthsEndDate = new HashSet<int>();
            foreach (var observation in result.Records)
            {
                yearsStartDate.Add(observation.Event.StartDate.Value.Year);
                monthsStartDate.Add(observation.Event.StartDate.Value.Month);
                yearsEndDate.Add(observation.Event.EndDate.Value.Year);
                monthsEndDate.Add(observation.Event.EndDate.Value.Month);
            }

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            yearsStartDate.Should().BeEquivalentTo(new[] { 2018, 2019, 2020 });
            monthsStartDate.Should().BeEquivalentTo(new[] { 1, 2, 4 });
            yearsEndDate.Should().Contain(new[] { 2018, 2019, 2020 });
            monthsEndDate.Should().Contain(new[] { 1, 2, 4 });
        }


        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_with_usePeriodForAllYears()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilterInternalDto()
            {
                Date = new DateFilterDto()
                {
                    StartDate = new DateTime(2018, 1, 2),
                    EndDate = new DateTime(2020, 2, 28),
                    DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
                },
                ExtendedFilter = new ExtendedFilterDto()
                {
                    UsePeriodForAllYears = true
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(null, null, searchFilter, 0, 1000);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert - prepare
            //-----------------------------------------------------------------------------------------------------------
            var yearsStartDate = new HashSet<int>();
            var monthsStartDate = new HashSet<int>();
            var yearsEndDate = new HashSet<int>();
            var monthsEndDate = new HashSet<int>();
            foreach (var observation in result.Records)
            {
                yearsStartDate.Add(observation.Event.StartDate.Value.Year);
                monthsStartDate.Add(observation.Event.StartDate.Value.Month);
                yearsEndDate.Add(observation.Event.EndDate.Value.Year);
                monthsEndDate.Add(observation.Event.EndDate.Value.Month);
            }

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            monthsStartDate.Should().BeEquivalentTo(new[] { 1, 2 });
            monthsEndDate.Should().BeEquivalentTo(new[] { 1, 2 });
            yearsStartDate.Count.Should().BeGreaterOrEqualTo(10, "because all years are included in the search when UsePeriodForAllYears=true");
        }
    }
}
