using System.IO.Compression;

namespace SOS.Lib.Configuration.Statistics
{
    /// <summary>
    /// Configuration for statistics 
    /// </summary>
    public class StatisticsConfiguration
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
