using SOS.Api.Contracts.Exports.Responses;

namespace SOS.Api.Client.Requests;

/// <summary>
/// Options for filtering and paging the current user's export jobs.
/// </summary>
public sealed class MyExportsOptions : SosRoleRequestOptions
{
    /// <summary>The statuses included in the result.</summary>
    public IReadOnlyCollection<ExportJobStatusDto>? Statuses { get; set; }

    /// <summary>The number of matching jobs to skip after newest-first sorting.</summary>
    public int Skip { get; set; }

    /// <summary>The maximum number of jobs to return.</summary>
    public int Take { get; set; } = 100;
}
