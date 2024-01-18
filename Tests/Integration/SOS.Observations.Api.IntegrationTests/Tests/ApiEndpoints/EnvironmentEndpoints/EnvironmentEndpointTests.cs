﻿using SOS.Shared.Api.Dtos;
using SOS.Observations.Api.IntegrationTests.Setup;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.EnvironmentEndpoints;

/// <summary>
/// Integration tests for the get Environment endpoint.
/// </summary>
[Collection(TestCollection.Name)]
public class EnvironmentEndpointTests : TestBase
{
    public EnvironmentEndpointTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    /// <summary>
    /// Integration test for the get Environment endpoint.
    /// </summary>
    [Fact]
    public async Task EnvironmentEndpoint_ReturnsServerEnvironmentInformation()
    {
        // Arrange
        var apiClient = TestFixture.CreateApiClient();
        var expectedEnvironmentInformation = new EnvironmentInformationDto
        {
            EnvironmentType = "Dev",
            CurrentCulture = "sv-SE"
        };

        // Act
        var response = await apiClient.GetAsync($"/environment");
        var result = await response.Content.ReadFromJsonAsync<EnvironmentInformationDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeEquivalentTo(expectedEnvironmentInformation, options => options
            .Excluding(m => m.AspDotnetVersion)
            .Excluding(m => m.HostingServerName)
            .Excluding(m => m.OsPlatform));
    }
}