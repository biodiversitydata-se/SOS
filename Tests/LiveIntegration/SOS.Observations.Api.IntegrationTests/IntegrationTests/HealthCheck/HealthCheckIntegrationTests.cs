using System.Threading.Tasks;
using FluentAssertions;
using SOS.Observations.Api.LiveIntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.LiveIntegrationTests.IntegrationTests.HealthCheck
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class HealthCheckIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public HealthCheckIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task SearchPerformanceHealthCheck()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await _fixture.SearchPerformanceHealthCheck.CheckHealthAsync(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy);
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task SearchDataProviderHealthCheck()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await _fixture.SearchDataProvidersHealthCheck.CheckHealthAsync(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy);
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task AzureSearchHealthCheck()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await _fixture.AzureSearchHealthCheck.CheckHealthAsync(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy);
        }
    }
}