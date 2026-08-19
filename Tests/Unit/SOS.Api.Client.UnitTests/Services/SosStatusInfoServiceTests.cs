using FluentAssertions;
using Moq;
using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Services;
using SOS.Api.Contracts.Status.Responses;
using Xunit;

namespace SOS.Api.Client.UnitTests.Services;

public class SosStatusInfoServiceTests
{
    [Fact]
    public async Task GetDataProviderStatusAsync_ReturnsStatusesFromProcessSummary()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var expectedStatuses = new List<DataProviderStatusDto>
        {
            new()
            {
                Id = 7,
                Identifier = "artportalen",
                Name = "Artportalen",
                SwedishName = "Artportalen",
                PublicActive = 12,
                PublicInactive = 10,
                PublicDiff = 2
            }
        };
        var clientMock = new Mock<ISosSystemsStatusInfoClient>();
        clientMock.Setup(client => client.GetProcessSummaryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessSummaryDto { DataProviderStatuses = expectedStatuses });

        var service = new SosStatusInfoService(clientMock.Object);

        var result = await service.GetDataProviderStatusAsync(cancellationToken);

        result.Should().BeSameAs(expectedStatuses);
    }

    [Fact]
    public async Task GetDataProviderStatusAsync_ReturnsNullWhenProcessSummaryIsMissing()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var clientMock = new Mock<ISosSystemsStatusInfoClient>();
        clientMock.Setup(client => client.GetProcessSummaryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProcessSummaryDto?)null);

        var service = new SosStatusInfoService(clientMock.Object);

        var result = await service.GetDataProviderStatusAsync(cancellationToken);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDataProviderStatusRowsAsync_ComputesRowsFromActiveAndInactiveInstances()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var clientMock = new Mock<ISosSystemsStatusInfoClient>();
        clientMock.Setup(client => client.GetActiveInstanceAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ActiveInstanceInfoDto { ActiveInstance = 1, Id = "mongo1" });
        clientMock.Setup(client => client.GetMongoDbProcessInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new MongoDbProcessInfoDto
                {
                    Id = "mongo1",
                    ProvidersInfo =
                    [
                        new MongoDbProcessInfoDto.Provider
                        {
                            DataProviderId = 42,
                            DataProviderIdentifier = "bird-monitor",
                            PublicProcessCount = 120,
                            ProtectedProcessCount = 25,
                            ProcessFailCount = 2
                        }
                    ]
                },
                new MongoDbProcessInfoDto
                {
                    Id = "mongo2",
                    ProvidersInfo =
                    [
                        new MongoDbProcessInfoDto.Provider
                        {
                            DataProviderId = 42,
                            DataProviderIdentifier = "bird-monitor",
                            PublicProcessCount = 100,
                            ProtectedProcessCount = 20,
                            ProcessFailCount = 1
                        }
                    ]
                }
            ]);

        var service = new SosStatusInfoService(clientMock.Object);

        var result = await service.GetDataProviderStatusRowsAsync(cancellationToken);

        result.Should().ContainSingle();
        result[0].Datasource.Should().Be("bird-monitor");
        result[0].PublicDiff.Should().Be(20);
        result[0].ProtectedDiff.Should().Be(5);
        result[0].InvalidDiff.Should().Be(1);
    }

    [Fact]
    public async Task GetDataProviderStatusRowsAsync_ReturnsEmptyListWhenInputsAreMissing()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var clientMock = new Mock<ISosSystemsStatusInfoClient>();
        clientMock.Setup(client => client.GetActiveInstanceAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActiveInstanceInfoDto?)null);
        clientMock.Setup(client => client.GetMongoDbProcessInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<MongoDbProcessInfoDto>?)null);

        var service = new SosStatusInfoService(clientMock.Object);

        var result = await service.GetDataProviderStatusRowsAsync(cancellationToken);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetObservationsApiHealthAsync_RemovesSelfAndAddsDiskEntries()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var clientMock = new Mock<ISosSystemsStatusInfoClient>();
        clientMock.Setup(client => client.GetHealthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HealthReportDto
            {
                Status = "Healthy",
                Entries = new Dictionary<string, HealthReportEntryDto>
                {
                    ["self"] = new() { Status = "Healthy" },
                    ["api"] = new() { Status = "Healthy" }
                }
            });
        clientMock.Setup(client => client.GetMongoDatabaseInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MongoDbInfoDto
            {
                DiskUsed = "50",
                DiskTotal = "200"
            });
        clientMock.Setup(client => client.GetSearchIndexInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchIndexInfoDto
            {
                Allocations =
                [
                    new SearchIndexInfoDto.AllocationInfo
                    {
                        DiskUsed = "30",
                        DiskAvailable = "70"
                    }
                ]
            });

        var service = new SosStatusInfoService(clientMock.Object);

        var result = await service.GetObservationsApiHealthAsync(cancellationToken);

        result.Should().NotBeNull();
        result!.Entries.Should().NotContainKey("self");
        result.Entries.Should().ContainKey("MongoDB disk");
        result.Entries.Should().ContainKey("Elasticsearch disk");
        result.Entries["MongoDB disk"].Description.Should().Be("Disk used: 25%");
        result.Entries["Elasticsearch disk"].Description.Should().Be("Disk used: 30%");
    }

    [Fact]
    public async Task GetObservationsApiHealthAsync_HandlesMalformedDiskDataWithoutThrowing()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var clientMock = new Mock<ISosSystemsStatusInfoClient>();
        clientMock.Setup(client => client.GetHealthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HealthReportDto
            {
                Status = "Healthy",
                Entries = new Dictionary<string, HealthReportEntryDto>
                {
                    ["self"] = new() { Status = "Healthy" }
                }
            });
        clientMock.Setup(client => client.GetMongoDatabaseInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MongoDbInfoDto
            {
                DiskUsed = "not-a-number",
                DiskTotal = "200"
            });
        clientMock.Setup(client => client.GetSearchIndexInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchIndexInfoDto
            {
                Allocations = Array.Empty<SearchIndexInfoDto.AllocationInfo>()
            });

        var service = new SosStatusInfoService(clientMock.Object);

        var result = await service.GetObservationsApiHealthAsync(cancellationToken);

        result.Should().NotBeNull();
        result!.Entries.Should().NotContainKey("self");
        result.Entries.Should().NotContainKey("MongoDB disk");
        result.Entries.Should().NotContainKey("Elasticsearch disk");
    }
}
