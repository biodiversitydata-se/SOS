using System.Collections.Generic;

namespace SOS.Lib.Models.DataQuality
{
    /// <summary>
    /// Data quality report record
    /// </summary>
    public class DataQualityReportRecord
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DataQualityReportRecord()
        {

        }

        /// <summary>
        /// Observation rnd date
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// Observation locality
        /// </summary>
        public string Locality { get; set; }

        /// <summary>
        /// Duplicate observations
        /// </summary>
        public IEnumerable<DataQualityReportObservation> Observations { get; set; }

        /// <summary>
        /// Observation start date
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// Id of observed taxon
        /// </summary>
        public string TaxonId { get; set; }

        /// <summary>
        /// Scientific name of taxon
        /// </summary>
        public string TaxonScientificName { get; set; }

        /// <summary>
        /// Unique key
        /// </summary>
        public string UniqueKey { get; set; }
    }
}
