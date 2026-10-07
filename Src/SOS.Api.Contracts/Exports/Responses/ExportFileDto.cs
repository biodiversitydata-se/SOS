using System;

namespace SOS.Api.Contracts.Exports.Responses;

/// <summary>
/// File object
/// </summary>
public class ExportFileDto
{
    /// <summary>
    /// Name Of file
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Date time when file was created
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Download URL.
    /// </summary>
    public string DownloadUrl { get; set; } = null!;
}
