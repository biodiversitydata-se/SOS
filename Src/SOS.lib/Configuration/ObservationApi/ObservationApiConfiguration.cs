using System.Collections.Generic;

namespace SOS.Lib.Configuration.ObservationApi
{
    public class ObservationApiConfiguration
    {
        /// <summary>
        /// Max number of observations allowed for file download
        /// </summary>
        public int  DownloadExportObservationsLimit { get; set; }

        /// <summary>
        /// File Export path
        /// </summary>
        public string ExportPath { get; set; }

        /// <summary>
        /// Max number of observations allowed for file order
        /// </summary>
        public int OrderExportObservationsLimit { get; set; }

        /// <summary>
        /// Max calculated tiles returned
        /// </summary>
        public int TilesLimit { get; set; }

        /// <summary>
        /// Taxon list id's allowed in signal search
        /// </summary>
        public IEnumerable<int> SignalSearchTaxonListIds { get; set; }
    }
}