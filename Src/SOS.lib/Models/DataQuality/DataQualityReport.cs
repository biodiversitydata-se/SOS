using System.Collections.Generic;

namespace SOS.Lib.Models.DataQuality
{
    /// <summary>
    /// Data quality report
    /// </summary>
    public class DataQualityReport
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DataQualityReport()
        {
            Records = new HashSet<DataQualityReportRecord>();
        }
        /// <summary>
        /// Duplicate observations
        /// </summary>
        public ICollection<DataQualityReportRecord> Records { get; set; }
    }
}
