﻿using System.IO.Compression;

namespace SOS.Analysis.Api.Configuration
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
        /// Max number of buckets created by aggregations
        /// </summary>
        public int MaxNrAggregationBuckets { get; set; }

        /// <summary>
        /// Protected scope
        /// </summary>
        public string ProtectedScope { get; set; }

        /// <summary>
        /// Response compression level.
        /// </summary>
        public CompressionLevel ResponseCompressionLevel { get; set; } = CompressionLevel.Optimal;
    }
}