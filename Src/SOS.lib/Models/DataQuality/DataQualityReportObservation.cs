namespace SOS.Lib.Models.DataQuality
{
    /// <summary>
    /// Observation data 
    /// </summary>
    public class DataQualityReportObservation
    {
        /// <summary>
        /// Id of data provider
        /// </summary>
        public string DataProviderId{ get; set; }

        /// <summary>
        /// Id of observation
        /// </summary>
        public string OccurrenceId { get; set; }
    }
}
