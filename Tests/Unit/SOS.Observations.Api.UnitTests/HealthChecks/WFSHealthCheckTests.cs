using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Services.Interfaces;
using SOS.Observations.Api.HealthChecks;
using System.Text.Json;
using Xunit;

namespace SOS.Observations.Api.UnitTests.HealthChecks;

public class WFSHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_Should_Be_Healthy_When_All_Requests_Return_Ten_Observations()
    {
        // Arrange
        var httpClientService = new Mock<IHttpClientService>();
        var response = JsonSerializer.Deserialize<JsonElement>("{\"numberReturned\":10}");
        httpClientService
            .Setup(service => service.GetDataAsync<JsonElement>(
                It.IsAny<Uri>(),
                null,
                It.IsAny<TimeSpan?>(),
                It.IsAny<int?>()))
            .ReturnsAsync(response);
        var healthCheck = CreateHealthCheck(httpClientService);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        httpClientService.Verify(service => service.GetDataAsync<JsonElement>(
            It.IsAny<Uri>(),
            null,
            TimeSpan.FromSeconds(10),
            null), Times.Exactly(4));
    }

    [Fact]
    public async Task CheckHealthAsync_Should_Finish_When_Cancelled()
    {
        // Arrange
        var httpClientService = new Mock<IHttpClientService>();
        var pendingResponse = new TaskCompletionSource<JsonElement>();
        httpClientService
            .Setup(service => service.GetDataAsync<JsonElement>(
                It.IsAny<Uri>(),
                null,
                It.IsAny<TimeSpan?>(),
                It.IsAny<int?>()))
            .Returns(pendingResponse.Task);
        var healthCheck = CreateHealthCheck(httpClientService);
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), cancellationTokenSource.Token);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("WFS health check timed out");
    }

    private static WFSHealthCheck CreateHealthCheck(Mock<IHttpClientService> httpClientService)
    {
        return new WFSHealthCheck(
            httpClientService.Object,
            Mock.Of<ILogger<WFSHealthCheck>>());
    }
}
