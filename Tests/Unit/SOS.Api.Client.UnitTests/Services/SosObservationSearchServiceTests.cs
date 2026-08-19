using FluentAssertions;
using Moq;
using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Requests;
using SOS.Api.Client.Services;
using SOS.Api.Contracts.Observations.Requests;
using System.Text.Json;
using Xunit;

namespace SOS.Api.Client.UnitTests.Services;

public class SosObservationSearchServiceTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public async Task HasSensitiveObservationsAsync_CountsOnlySensitiveObservations(
        int count,
        bool expected)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new Mock<ISosObservationsClient>();
        string? sentFilter = null;
        client.Setup(value => value.CountInternalJsonAsync(
                It.IsAny<string>(),
                It.Is<CountInternalOptions>(options => options.SensitiveObservations),
                cancellationToken))
            .Callback<string, CountInternalOptions?, CancellationToken>((json, _, _) => sentFilter = json)
            .ReturnsAsync(count);
        var service = new SosObservationSearchService(client.Object);
        var filter = new SearchFilterInternalDto
        {
            ProtectionFilter = ProtectionFilterDto.BothPublicAndSensitive
        };

        var result = await service.HasSensitiveObservationsAsync(filter, cancellationToken: cancellationToken);

        result.Should().Be(expected);
        filter.ProtectionFilter.Should().Be(ProtectionFilterDto.BothPublicAndSensitive);
        using var json = JsonDocument.Parse(sentFilter!);
        json.RootElement.GetProperty("protectionFilter").GetString().Should().Be("Sensitive");
    }
}
