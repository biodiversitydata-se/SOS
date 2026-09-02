using FluentAssertions;
using Moq;
using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Requests;
using SOS.Api.Client.Services;
using SOS.Api.Contracts.Aggregations.Requests;
using Xunit;

namespace SOS.Api.Client.UnitTests.Services;

public class SosAnalysisServiceTests
{
    [Fact]
    public async Task OrderAooEooAsync_CopiesCalculationOptionsAndConfirmsPassword()
    {
        var client = new Mock<ISosAnalysisClient>();
        client.Setup(value => value.OrderAooEooAsync(
                It.IsAny<SearchFilterAggregationInternalDto>(),
                It.IsAny<AooEooOrderOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("job-1");
        var service = new SosAnalysisService(client.Object);
        var calculation = new AooEooOptions
        {
            GridCellSizeInMeters = 5000,
            AlphaValues = [1.2],
            RoleId = 7
        };

        var result = await service.OrderAooEooAsync(
            new SearchFilterAggregationInternalDto(),
            calculation,
            new SosOrderMetadata
            {
                Description = "test",
                EncryptPassword = "secret",
                SendMailFromZendTo = false
            },
            TestContext.Current.CancellationToken);

        result.Should().Be("job-1");
        client.Verify(value => value.OrderAooEooAsync(
            It.IsAny<SearchFilterAggregationInternalDto>(),
            It.Is<AooEooOrderOptions>(options =>
                options.GridCellSizeInMeters == 5000 &&
                options.AlphaValues!.Single() == 1.2 &&
                options.RoleId == 7 &&
                options.EncryptPassword == "secret" &&
                options.ConfirmEncryptPassword == "secret"),
            TestContext.Current.CancellationToken), Times.Once);
    }
}
