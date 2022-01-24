namespace SOS.Observations.Api.Dtos
{
    /// <summary>
    /// 
    /// </summary>
    public class GridCellDto
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
        public LatLonBoundingBoxDto BoundingBox { get; set; }
    }
}