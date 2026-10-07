using FluentAssertions;
using Moq;
using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Requests;
using SOS.Api.Client.Services;
using SOS.Api.Contracts.Exports.Responses;
using Xunit;

namespace SOS.Api.Client.UnitTests.Services;

public class SosExportServiceTests
{
    [Fact]
    public async Task GetMyExportsPageAsync_FiltersAndSortsBeforePaging()
    {
        var client = new Mock<ISosExportsClient>();
        client.Setup(value => value.GetMyExportsAsync(
                It.IsAny<SosRoleRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new ExportJobInfoDto { Id = "old", Status = ExportJobStatus.Succeeded, CreatedDate = new DateTime(2024, 1, 1) },
                new ExportJobInfoDto { Id = "failed", Status = ExportJobStatus.Failed, CreatedDate = new DateTime(2026, 1, 1) },
                new ExportJobInfoDto { Id = "new", Status = ExportJobStatus.Succeeded, CreatedDate = new DateTime(2025, 1, 1) }
            ]);
        var service = new SosExportService(client.Object);

        var result = await service.GetMyExportsPageAsync(
            new MyExportsOptions
            {
                Statuses = [ExportJobStatus.Succeeded],
                Skip = 0,
                Take = 1
            },
            TestContext.Current.CancellationToken);

        result.TotalCount.Should().Be(2);
        result.Records.Should().ContainSingle().Which.Id.Should().Be("new");
    }
}
