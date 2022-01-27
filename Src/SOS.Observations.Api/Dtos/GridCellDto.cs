namespace SOS.Observations.Api.Dtos
{
    /// <summary>
    /// 
    /// </summary>
    public class GridCellDto
    {
        /// <summary>
        /// Cell coordinates in WGS84
        /// </summary>
        public LatLonBoundingBoxDto BoundingBox { get; set; }

        /// <summary>
        /// Number of observations in cell
        /// </summary>
        public long? ObservationsCount { get; set; }
        
        /// <summary>
        /// Cell coordinates in SWEREF99 TM
        /// </summary>
        public XYBoundingBoxDto Sweref99TmBoundingBox { get; set; }

        /// <summary>
        /// Count of different taxa
        /// </summary>
        public long? TaxaCount { get; set; }
    }
}