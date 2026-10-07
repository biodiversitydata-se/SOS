using Moq;
using SOS.Api.Contracts.Lookups.Responses;
using SOS.Api.Contracts.Metadata.Responses;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Observations.Api.IntegrationTests.Setup;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.DataProvidersEndpoints;

[Collection(TestCollection.Name)]
public class DataProviderDateEndpointTests(TestFixture fixture)
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DataProviders_PreservesUtcDates_IncludingMinimumDates(bool useMinimumDates)
    {
        var date = useMinimumDates
            ? DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc)
            : new DateTime(2026, 9, 30, 12, 34, 0, DateTimeKind.Utc);
        var provider = CreateProvider();
        var processInfo = new ProcessInfo("index", date)
        {
            End = date,
            ProvidersInfo =
            [
                new ProviderInfo(provider)
                {
                    ProcessStart = date,
                    HarvestEnd = date,
                    ProcessEnd = date,
                    LatestIncrementalEnd = date,
                    PublicProcessCount = 8,
                    ProtectedProcessCount = 3
                }
            ]
        };
        using var client = CreateClient(provider, processInfo);

        using var response = await client.GetAsync("/DataProviders", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = (await response.Content.ReadFromJsonAsync<List<DataProviderDto>>(
            cancellationToken: TestContext.Current.CancellationToken))!.Should().ContainSingle().Subject;
        var expectedDate = new DateTimeOffset(date);
        dto.LatestHarvestDate.Should().Be(expectedDate);
        dto.LatestProcessDate.Should().Be(expectedDate);
        dto.LatestIncrementalHarvestDate.Should().Be(expectedDate);
        dto.LatestHarvestDate!.Value.Offset.Should().Be(TimeSpan.Zero);
        dto.LatestProcessDate!.Value.Offset.Should().Be(TimeSpan.Zero);
        dto.LatestIncrementalHarvestDate!.Value.Offset.Should().Be(TimeSpan.Zero);
        dto.NextHarvestFrom.Should().Be(new DateTimeOffset(provider.NextHarvestFrom(date)!.Value));
        dto.PublicObservations.Should().Be(8);
        dto.ProtectedObservations.Should().Be(3);
    }

    [Fact]
    public async Task DataProviders_PreservesNullDates_WhenProviderHasNotBeenHarvested()
    {
        var provider = CreateProvider();
        var date = new DateTime(2026, 9, 30, 12, 34, 0, DateTimeKind.Utc);
        var processInfo = new ProcessInfo("index", date)
        {
            End = date,
            ProvidersInfo = [new ProviderInfo(provider) { ProcessStart = date, PublicProcessCount = 8 }]
        };
        using var client = CreateClient(provider, processInfo);

        using var response = await client.GetAsync("/DataProviders", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = (await response.Content.ReadFromJsonAsync<List<DataProviderDto>>(
            cancellationToken: TestContext.Current.CancellationToken))!.Should().ContainSingle().Subject;
        dto.LatestHarvestDate.Should().BeNull();
        dto.LatestProcessDate.Should().BeNull();
        dto.LatestIncrementalHarvestDate.Should().BeNull();
        dto.NextHarvestFrom!.Value.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public async Task ProcessInformation_ReturnsUtcDefaults_WhenProcessingHasNotFinished()
    {
        var provider = CreateProvider();
        var date = new DateTime(2026, 9, 30, 12, 34, 0, DateTimeKind.Utc);
        // End and ProcessStart retain DateTime.MinValue with Kind=Unspecified.
        var processInfo = new ProcessInfo("index", date)
        {
            ProvidersInfo = [new ProviderInfo(provider) { PublicProcessCount = 8 }],
            MetadataInfo = [new ProcessInfo("metadata", date)]
        };
        using var client = CreateClient(provider, processInfo);

        using var response = await client.GetAsync("/Systems/ProcessInformation?active=true", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<ProcessInfoDto>(
            cancellationToken: TestContext.Current.CancellationToken);
        dto.Should().NotBeNull();
        dto!.Start.Should().Be(new DateTimeOffset(date));
        dto.End.Should().Be(DateTimeOffset.MinValue);
        dto.ProvidersInfo.Should().ContainSingle().Subject.ProcessStart.Should().Be(DateTimeOffset.MinValue);
        dto.MetadataInfo.Should().ContainSingle().Subject.End.Should().Be(DateTimeOffset.MinValue);
    }

    [Fact]
    public async Task DataProviderHealth_ReturnsReport_WhenProcessDatesAreUnset()
    {
        var provider = CreateProvider();
        var processInfo = new ProcessInfo("index", DateTime.UnixEpoch)
        {
            ProvidersInfo = [new ProviderInfo(provider) { ProcessStatus = RunStatus.Success, PublicProcessCount = 8 }]
        };
        using var client = CreateClient(provider, processInfo);

        using var response = await client.GetAsync("/DataProviders/Health", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        json.RootElement.GetProperty("entries").GetProperty(provider.Identifier)
            .GetProperty("description").GetString().Should().Be("Public: 8, Protected: ");
    }

    private HttpClient CreateClient(DataProvider provider, ProcessInfo processInfo)
    {
        var cache = new Mock<IDataProviderCache>();
        cache.Setup(value => value.GetAllAsync()).ReturnsAsync([provider]);
        var repository = new Mock<IProcessInfoRepository>();
        repository.Setup(value => value.GetAsync(It.IsAny<string>())).ReturnsAsync(processInfo);

        return fixture.ApiFactory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            // Keep the real managers, controller, JSON formatter and HTTP pipeline.
            services.Replace(ServiceDescriptor.Singleton(cache.Object));
            services.Replace(ServiceDescriptor.Singleton(repository.Object));
            // SystemsController creates a separate ES client even for metadata-only actions.
            // The fixture normally leaves Clusters null because searches use its test client manager.
            var elasticConfiguration = fixture.ServiceProvider.GetRequiredService<ElasticSearchConfiguration>();
            services.Replace(ServiceDescriptor.Singleton(new ElasticSearchConfiguration
            {
                IndexPrefix = elasticConfiguration.IndexPrefix,
                IndexSettings = elasticConfiguration.IndexSettings,
                Clusters = []
            }));
        })).CreateClient();
    }

    private static DataProvider CreateProvider() => new()
    {
        Id = 12,
        Identifier = "test-provider",
        IsActive = true,
        Names = [new VocabularyValueTranslation { CultureCode = "sv-SE", Value = "Testnamn" }],
        HarvestSchedule = "0 0 * * *"
    };
}
