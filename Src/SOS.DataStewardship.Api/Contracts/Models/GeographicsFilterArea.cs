namespace SOS.DataStewardship.Api.Contracts.Models
{    
    /// <summary>
	/// Geometry filter
	/// </summary>
    public class GeometryFilter
    {        
        /// <summary>
		/// GeoJSON geometry
		/// </summary>
        public IGeoShape GeographicArea { get; set; }        
        
        /// <summary>
		/// The offset in meters from the geometries. This variable is required if geometries type is point
		/// </summary>
        public double? MaxDistanceFromGeometries { get; set; }
    }
}