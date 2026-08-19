using SOS.Api.Client.Requests;
using SOS.Api.Client.Responses;
using SOS.Api.Contracts.Exports.Requests;
using SOS.Api.Contracts.Exports.Responses;
using SOS.Api.Contracts.Observations.Requests;

namespace SOS.Api.Client.Abstractions;

/// <summary>Client for SOS observation export endpoints.</summary>
public interface ISosExportsClient
{
    /// <summary>Streams an observation export generated synchronously.</summary>
    Task<SosStreamingResponse> DownloadInternalAsync(
        SearchFilterInternalDto filter,
        SosExportType exportType,
        ExportDownloadOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Queues an observation export and returns its job identifier.</summary>
    Task<string> OrderInternalAsync(
        SearchFilterInternalDto filter,
        SosExportType exportType,
        ExportOrderOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>Gets export jobs owned by the current user.</summary>
    Task<IReadOnlyList<ExportJobInfoDto>> GetMyExportsAsync(
        SosRoleRequestOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one export job owned by the current user.</summary>
    Task<ExportJobInfoDto?> GetMyExportAsync(
        string id,
        SosRoleRequestOptions? options = null,
        CancellationToken cancellationToken = default);
}
