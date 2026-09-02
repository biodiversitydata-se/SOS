namespace SOS.Api.Contracts.Exports.Responses;

/// <summary>
/// Processing status of an export job.
/// </summary>
public enum ExportJobStatusDto
{
    /// <summary>The status is unavailable or unrecognised.</summary>
    Unknown = 0,
    /// <summary>The job is waiting to be processed.</summary>
    Queued = 1,
    /// <summary>The job is being processed.</summary>
    Processing = 2,
    /// <summary>The job completed successfully.</summary>
    Succeeded = 3,
    /// <summary>The job failed.</summary>
    Failed = 4
}
