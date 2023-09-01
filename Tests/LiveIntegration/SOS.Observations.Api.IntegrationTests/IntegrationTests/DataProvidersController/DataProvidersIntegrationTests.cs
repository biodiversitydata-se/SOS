﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Vocabulary;
using SOS.Observations.Api.LiveIntegrationTests.Extensions;
using SOS.Observations.Api.LiveIntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.LiveIntegrationTests.IntegrationTests.DataProvidersController
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class DataProvidersIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public DataProvidersIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Get_all_active_data_providers_except_the_ones_with_no_observations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.DataProvidersController.GetDataProvidersAsync("sv-SE", true);
            var dataProviders = response.GetResult<List<DataProviderDto>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            dataProviders.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Get_all_active_data_providers()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.DataProvidersController.GetDataProvidersAsync();
            var dataProviders = response.GetResult<List<DataProviderDto>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            dataProviders.Should().NotBeNull();
        }
    }
}