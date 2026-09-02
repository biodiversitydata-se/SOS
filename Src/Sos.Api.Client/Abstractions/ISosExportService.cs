using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Exports.Responses;

namespace SOS.Api.Client.Abstractions;

/// <summary>
/// High-level workflows for SOS export jobs.
/// </summary>
public interface ISosExportService
{
    /// <summary>Gets a filtered, newest-first page of the current user's export jobs.</summary>
    Task<PagedResultDto<ExportJobInfoDto>> GetMyExportsPageAsync(
        MyExportsOptions? options = null,
        CancellationToken cancellationToken = default);
}
