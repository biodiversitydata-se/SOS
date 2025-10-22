﻿using FluentAssertions;
using SOS.Lib.Models.Search.Filters;
using SOS.Observations.Api.LiveIntegrationTests.Fixtures;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Observations.Api.LiveIntegrationTests.IntegrationTests.Events
{
    [Collection(Fixtures.Collections.ApiIntegrationTestsCollection)]
    public class EventRepositoryIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public EventRepositoryIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Get_all_events_grouped_by_datasetId()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var filter = new EventSearchFilter();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var datasetEventIds = await _fixture.EventRepository.GetAllAggregationItemsListAsync<string, string>(filter, "dataset.identifier", "eventId");

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            datasetEventIds.Should().NotBeEmpty();
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Get_all_nils_events_grouped_by_datasetId()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var filter = new EventSearchFilter();
            filter.DataProviderIds = new List<int> { 25 };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var datasetEventIds = await _fixture.EventRepository.GetAllAggregationItemsListAsync<string, string>(filter, "dataset.identifier", "eventId");

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            datasetEventIds.Should().NotBeEmpty();
        }
    }
}
