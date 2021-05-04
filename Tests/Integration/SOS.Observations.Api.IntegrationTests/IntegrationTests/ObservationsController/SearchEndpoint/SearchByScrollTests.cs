using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ObservationsController.SearchEndpoint
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class SearchByScrollTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public SearchByScrollTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_for_mammalia_observations()
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
                Areas = new[]
                {
                    TestData.Areas.JonkopingCounty // Jönköping County
                },
                OutputFields = new List<string> { "event.startDate", "event.endDate", "location.decimalLatitude", "location.decimalLongitude", "location.municipality", "taxon.id", "taxon.scientificName", "occurrence.recordedBy", "occurrence.occurrenceStatus" }
            };
            var observations = new List<Observation>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            string scrollId = null; // no scroll in first request
            bool hasMorePages;
            do
            {
                IActionResult response = await _fixture.ObservationsController.ObservationsScroll(
                    searchFilter, scrollId, 1000);
                ScrollResultDto<Observation> result = response.GetResult<ScrollResultDto<Observation>>();
                observations.AddRange(result.Records);
                scrollId = result.ScrollId;
                hasMorePages = result.HasMorePages;
            } while (hasMorePages);
            
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observations.Count().Should().BeGreaterThan(10000);
        }
    }
}
