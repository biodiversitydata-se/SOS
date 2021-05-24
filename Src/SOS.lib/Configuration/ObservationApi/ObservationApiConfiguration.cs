using System.Collections.Generic;

namespace SOS.Lib.Configuration.ObservationApi
{
    public class ObservationApiConfiguration
    {
        public int  ExportObservationsLimit { get; set; }

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