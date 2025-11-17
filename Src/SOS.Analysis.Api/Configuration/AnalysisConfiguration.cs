using System.IO.Compression;

namespace SOS.Analysis.Api.Configuration;

/// <summary>
/// Configuration for analysis API 
/// </summary>
public class AnalysisConfiguration
{
    /// <summary>
    /// True if response compression shuld be enabled
    /// </summary>
    public bool EnableResponseCompression { get; set; }

    /// <summary>
    /// Protected scope
    /// </summary>
    public string? ProtectedScope { get; set; }

    /// <summary>
    /// Response compression level.
    /// </summary>
    public CompressionLevel ResponseCompressionLevel { get; set; } = CompressionLevel.Optimal;

    /// <summary>
    /// Default max ongoing export jobs for a user
    /// </summary>
    public int DefaultUserExportLimit { get; set; } = 5;
}
