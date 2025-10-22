﻿using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Models.Processed.Observation;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Observations.Api.LiveIntegrationTests.Extensions;
using SOS.Observations.Api.LiveIntegrationTests.Fixtures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Observations.Api.LiveIntegrationTests.IntegrationTests.ObservationsController.SearchEndpoint
{
    [Collection(Fixtures.Collections.ApiIntegrationTestsCollection)]
    public class SearchByScrollTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public SearchByScrollTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_for_mammalia_observations_by_using_the_scroll_endpoint()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto
                {
                    Ids = new List<int> { TestData.TaxonIds.Mammalia },
                    IncludeUnderlyingTaxa = true
                },
                Geographics = new GeographicsFilterDto()
                {
                    Areas = new[]
                    {
                        TestData.Areas.JonkopingCounty // Jönköping County
                    },
                },
                Output = new OutputFilterDto
                {
                    Fields = new[] { "event.startDate", "event.endDate", "location.decimalLatitude", "location.decimalLongitude", "location.municipality", "taxon.id", "taxon.scientificName", "occurrence.occurrenceId", "occurrence.recordedBy", "occurrence.occurrenceStatus" }
                }
            };
            var observations = new List<Observation>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var countResponse = await _fixture.ObservationsController.Count(null, null, searchFilter);
            int count = countResponse.GetResult<int>();
            count.Should().BeLessOrEqualTo(100000, "because our limit for the scroll endpoint is 100000");
            string scrollId = null; // no scroll in first request
            bool hasMorePages;
            HashSet<string> ids = new HashSet<string>();
            Stopwatch sp = Stopwatch.StartNew();
            do
            {
                IActionResult response = await _fixture.ObservationsController.ObservationsScroll(
                    0,
                    null, searchFilter, scrollId, 10000);
                ScrollResultDto<Observation> result = response.GetResult<ScrollResultDto<Observation>>();
                observations.AddRange(result.Records);
                ids.UnionWith(result.Records.Select(m => m.Occurrence.OccurrenceId));
                scrollId = result.ScrollId;
                hasMorePages = result.HasMorePages;
            } while (hasMorePages && observations.Count < 100000);
            sp.Stop();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observations.Count().Should().BeGreaterThan(0);
        }
    }
}
