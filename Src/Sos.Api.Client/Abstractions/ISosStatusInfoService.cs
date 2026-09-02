using SOS.Api.Contracts.Status.Responses;

namespace SOS.Api.Client.Abstractions;

/// <summary>High-level SOS status-information workflows.</summary>
public interface ISosStatusInfoService
{
    /// <summary>Gets per-provider processing status.</summary>
    Task<List<DataProviderStatusDto>?> GetDataProviderStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets flattened per-provider processing status rows.</summary>
    Task<List<DataProviderStatusRow>> GetDataProviderStatusRowsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the Observations API health report.</summary>
    Task<HealthReportDto?> GetObservationsApiHealthAsync(CancellationToken cancellationToken = default);
}
