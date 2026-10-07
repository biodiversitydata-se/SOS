using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Api.Contracts.Lookups.Responses;
using SOS.Api.Contracts.Metadata.Responses;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Managers;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;
using Xunit;

namespace SOS.Observations.Api.UnitTests.Services;

public class DataProviderManagerTests
{
    [Fact]
    public async Task GetDataProvidersAsync_ReturnsContractDtoWithTranslatedAndProcessFields()
    {
        var provider = new DataProvider
        {
            Id = 12,
            Identifier = "test-provider",
            IsActive = true,
            Names = [new VocabularyValueTranslation { CultureCode = "sv-SE", Value = "Testnamn" }],
            Descriptions = [new VocabularyValueTranslation { CultureCode = "sv-SE", Value = "Beskrivning" }],
            Organizations = [new VocabularyValueTranslation { CultureCode = "sv-SE", Value = "Organisation" }],
            Paths = [new DataProviderPath { CultureCode = "sv-SE", Path = ["Rot", "Gren"] }],
            Url = "https://example.org",
            HarvestSchedule = "0 0 * * *",
            IncludeInSearchByDefault = true
        };
        var harvestEnd = new DateTimeOffset(2025, 1, 2, 3, 4, 0, TimeSpan.Zero);
        var processEnd = new DateTimeOffset(2025, 1, 3, 3, 4, 0, TimeSpan.Zero);
        var incrementalEnd = new DateTimeOffset(2025, 1, 4, 3, 4, 0, TimeSpan.Zero);
        var processInfo = new ProcessInfoDto
        {
            ProvidersInfo =
            [
                new ProviderInfoDto
                {
                    DataProviderId = provider.Id,
                    PublicProcessCount = 8,
                    ProtectedProcessCount = 3,
                    HarvestEnd = harvestEnd,
                    ProcessEnd = processEnd,
                    LatestIncrementalEnd = incrementalEnd,
                    HarvestNotes = "Klart"
                }
            ]
        };
        var cache = new Mock<IDataProviderCache>();
        cache.Setup(x => x.GetAllAsync()).ReturnsAsync([provider]);
        var processInfoManager = new Mock<IProcessInfoManager>();
        processInfoManager.Setup(x => x.GetProcessInfoAsync("index")).ReturnsAsync(processInfo);
        var repository = new Mock<IProcessedObservationRepository>();
        repository.SetupGet(x => x.UniquePublicIndexName).Returns("index");
        var manager = new DataProviderManager(
            cache.Object,
            processInfoManager.Object,
            repository.Object,
            NullLogger<DataProviderManager>.Instance);

        var result = Assert.Single(await manager.GetDataProvidersAsync(false, "sv-SE"));

        Assert.IsType<DataProviderDto>(result);
        Assert.Equal(provider.Id, result.Id);
        Assert.Equal(provider.Identifier, result.Identifier);
        Assert.Equal("Testnamn", result.Name);
        Assert.Equal("Beskrivning", result.Description);
        Assert.Equal("Organisation", result.Organization);
        Assert.Equal(["Rot", "Gren"], result.Path);
        Assert.Equal(provider.Url, result.Url);
        Assert.Equal(8, result.PublicObservations);
        Assert.Equal(3, result.ProtectedObservations);
        Assert.Equal(harvestEnd, result.LatestHarvestDate);
        Assert.Equal(processEnd, result.LatestProcessDate);
        Assert.Equal(incrementalEnd, result.LatestIncrementalHarvestDate);
        Assert.Equal((DateTimeOffset?)provider.NextHarvestFrom(harvestEnd.UtcDateTime), result.NextHarvestFrom);
        Assert.Equal("Klart", result.HarvestNotes);
        Assert.Equal(provider.HarvestSchedule, result.HarvestSchedule);
        Assert.True(result.IncludeInSearchByDefault);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(-3)]
    public async Task GetDataProvidersAsync_PreservesOffsetsAndSchedulesFromTheUtcInstant(int offsetHours)
    {
        var provider = new DataProvider { Id = 12, IsActive = true, HarvestSchedule = "0 0 * * *" };
        // At +02:00 this instant is still on the previous UTC day.
        var date = new DateTimeOffset(2026, 10, 1, 0, 30, 0, TimeSpan.FromHours(offsetHours));
        var processInfo = new ProcessInfoDto
        {
            ProvidersInfo =
            [
                new ProviderInfoDto
                {
                    DataProviderId = provider.Id,
                    HarvestEnd = date,
                    ProcessEnd = date,
                    LatestIncrementalEnd = date
                }
            ]
        };
        var cache = new Mock<IDataProviderCache>();
        cache.Setup(value => value.GetAllAsync()).ReturnsAsync([provider]);
        var processInfoManager = new Mock<IProcessInfoManager>();
        processInfoManager.Setup(value => value.GetProcessInfoAsync("index")).ReturnsAsync(processInfo);
        var repository = new Mock<IProcessedObservationRepository>();
        repository.SetupGet(value => value.UniquePublicIndexName).Returns("index");
        var manager = new DataProviderManager(cache.Object, processInfoManager.Object, repository.Object,
            NullLogger<DataProviderManager>.Instance);

        var result = Assert.Single(await manager.GetDataProvidersAsync(false, "sv-SE"));

        Assert.Equal(date, result.LatestHarvestDate);
        Assert.Equal(date, result.LatestProcessDate);
        Assert.Equal(date, result.LatestIncrementalHarvestDate);
        Assert.Equal(date.Offset, result.LatestHarvestDate!.Value.Offset);
        Assert.Equal(date.Offset, result.LatestProcessDate!.Value.Offset);
        Assert.Equal(date.Offset, result.LatestIncrementalHarvestDate!.Value.Offset);
        Assert.Equal((DateTimeOffset?)provider.NextHarvestFrom(date.UtcDateTime), result.NextHarvestFrom);
        Assert.Equal(TimeSpan.Zero, result.NextHarvestFrom!.Value.Offset);
    }
}
