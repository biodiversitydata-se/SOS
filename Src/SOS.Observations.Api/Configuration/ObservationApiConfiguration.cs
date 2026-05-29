using System.Collections.Generic;
using System.IO.Compression;

namespace SOS.Observations.Api.Configuration;

/// <summary>
/// Observations API configuration.
/// </summary>
public class ObservationApiConfiguration
{
    /// <summary>
    /// Cache monitoring configuration.
    /// </summary>
    public CacheMonitoringConfiguration CacheMonitoring { get; set; } = new();

    /// <summary>
    /// Default max ongoing export jobs for a user
    /// </summary>
    public int DefaultUserExportLimit { get; set; }

    /// <summary>
    /// Max number of observations allowed for file download
    /// </summary>
    public int DownloadExportObservationsLimit { get; set; }

    /// <summary>
    /// If true, make response compression enabled.
    /// </summary>
    public bool EnableResponseCompression { get; set; } = false;

    /// <summary>
    /// File Export path
    /// </summary>
    public string ExportPath { get; set; }

    /// <summary>
    /// Max number of observations allowed for file order
    /// </summary>
    public int OrderExportObservationsLimit { get; set; }

    /// <summary>
    /// Protected scope
    /// </summary>
    public string ProtectedScope { get; set; }

    /// <summary>
    /// Response compression level.
    /// </summary>
    public CompressionLevel ResponseCompressionLevel { get; set; } = CompressionLevel.Optimal;
}

/// <summary>
/// Cache monitoring configuration.
/// </summary>
public class CacheMonitoringConfiguration
{
    /// <summary>
    /// If true, cache monitoring is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Log AdaptiveSearchLocations cache usage after this many cache writes. Set to 0 or less to disable periodic write logging.
    /// </summary>
    public int AdaptiveSearchLocationsLogEveryWriteCount { get; set; } = 100;

    /// <summary>
    /// Log location geometry cache usage after this many cache writes. Set to 0 or less to disable periodic write logging.
    /// </summary>
    public int LocationGeometryLogEveryWriteCount { get; set; } = 50;
}
