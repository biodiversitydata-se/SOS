using System.IO.Compression;

namespace SOS.Lib.Configuration.Analysis
{
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
        /// Response compression level.
        /// </summary>
        public CompressionLevel ResponseCompressionLevel { get; set; } = CompressionLevel.Optimal;
    }
}
