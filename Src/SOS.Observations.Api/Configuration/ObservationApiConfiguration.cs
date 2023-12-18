﻿using System.Collections.Generic;
using System.IO.Compression;

namespace SOS.Observations.Api.Configuration
{
    /// <summary>
    /// Observations API configuration.
    /// </summary>
    public class ObservationApiConfiguration
    {
        /// <summary>
        /// Default max ongoing export jobs for a user
        /// </summary>
        public int DefaultUserExportLimit { get; set; }

        /// <summary>
        /// Max number of observations allowed for file download
        /// </summary>
        public int  DownloadExportObservationsLimit { get; set; }

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
}