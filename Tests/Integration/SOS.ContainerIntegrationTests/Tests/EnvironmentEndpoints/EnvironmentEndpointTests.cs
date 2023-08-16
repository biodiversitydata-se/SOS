using SOS.ContainerIntegrationTests.Setup;
using SOS.Observations.Api.Dtos;

namespace SOS.ContainerIntegrationTests.Tests.EnvironmentEndpoints;

/// <summary>
/// Integration tests for the get Environment endpoint.
/// </summary>
[Collection(IntegrationTestsCollection.Name)]
public class EnvironmentEndpointTests : IntegrationTestsBase
{
    public EnvironmentEndpointTests(IntegrationTestsFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    /// <summary>
    /// Integration test for the get Environment endpoint.
    /// </summary>
    [Fact]
    public async Task DateEndpoint_ReturnsParsedDate_GivenValidDate()
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