namespace SOS.Lib.Models.Gis
{
    /// <summary>
    /// 
    /// </summary>
    public class GridCell
    {
        /// <summary>
        /// Number of observations in cell
        /// </summary>
        public long? ObservationsCount { get; set; }

        /// <summary>
        /// Count of different taxa
        /// </summary>
        public long? TaxaCount { get; set; }

        /// <summary>
        /// Cell coordinates
        /// </summary>
        public XYBoundingBox MetricBoundingBox { get; set; }
    }
}