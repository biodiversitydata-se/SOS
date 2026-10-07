using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Observations.Api.Managers;
using Xunit;

namespace SOS.Observations.Api.UnitTests.Services;

public class ProcessInfoManagerTests
{
    [Theory]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Unspecified)]
    public async Task GetProcessInfoAsync_PreservesUtcDatesAndDefaultValues(DateTimeKind kind)
    {
        var minimum = DateTime.SpecifyKind(DateTime.MinValue, kind);
        var date = new DateTime(2026, 9, 30, 12, 34, 0, kind);
        var provider = new DataProvider { Id = 12 };
        var processInfo = new ProcessInfo("index", minimum)
        {
            End = minimum,
            MetadataInfo = [new ProcessInfo("metadata", minimum) { End = minimum }],
            ProvidersInfo =
            [
                new ProviderInfo(provider)
                {
                    ProcessStart = minimum,
                    ProcessEnd = date,
                    HarvestStart = minimum,
                    HarvestEnd = date,
                    LatestIncrementalStart = minimum,
                    LatestIncrementalEnd = date
                }
            ]
        };
        var repository = new Mock<IProcessInfoRepository>();
        repository.Setup(value => value.GetAsync("index")).ReturnsAsync(processInfo);
        var manager = new ProcessInfoManager(repository.Object, NullLogger<ProcessInfoManager>.Instance);

        var result = await manager.GetProcessInfoAsync("index");

        Assert.NotNull(result);
        Assert.Equal(DateTimeOffset.MinValue, result.Start);
        Assert.Equal(DateTimeOffset.MinValue, result.End);
        Assert.Equal(DateTimeOffset.MinValue, Assert.Single(result.MetadataInfo).End);
        var mappedProvider = Assert.Single(result.ProvidersInfo);
        Assert.Equal(DateTimeOffset.MinValue, mappedProvider.ProcessStart);
        Assert.Equal(DateTimeOffset.MinValue, mappedProvider.HarvestStart);
        Assert.Equal(DateTimeOffset.MinValue, mappedProvider.LatestIncrementalStart);
        var expected = new DateTimeOffset(DateTime.SpecifyKind(date, DateTimeKind.Utc));
        Assert.Equal(expected, mappedProvider.ProcessEnd);
        Assert.Equal(expected, mappedProvider.HarvestEnd);
        Assert.Equal(expected, mappedProvider.LatestIncrementalEnd);
        Assert.Equal(TimeSpan.Zero, mappedProvider.ProcessEnd!.Value.Offset);
    }

    [Fact]
    public async Task GetProcessInfoAsync_PreservesMissingOptionalDates()
    {
        var processInfo = new ProcessInfo("index", DateTime.UnixEpoch)
        {
            End = DateTime.UnixEpoch,
            ProvidersInfo = [new ProviderInfo(new DataProvider { Id = 12 })]
        };
        var repository = new Mock<IProcessInfoRepository>();
        repository.Setup(value => value.GetAsync("index")).ReturnsAsync(processInfo);
        var manager = new ProcessInfoManager(repository.Object, NullLogger<ProcessInfoManager>.Instance);

        var result = await manager.GetProcessInfoAsync("index");

        Assert.NotNull(result);
        var mappedProvider = Assert.Single(result.ProvidersInfo);
        Assert.Null(mappedProvider.ProcessEnd);
        Assert.Null(mappedProvider.HarvestStart);
        Assert.Null(mappedProvider.HarvestEnd);
        Assert.Null(mappedProvider.LatestIncrementalStart);
        Assert.Null(mappedProvider.LatestIncrementalEnd);
    }
}
