using Swashbuckle.AspNetCore.Annotations;

namespace SOS.DataStewardship.Api.Contracts.Models
{    
    [SwaggerSchema("Geometry filter")]
    public class GeometryFilter
    {        
        [SwaggerSchema("GeoJSON geometry")]
        public IGeoShape GeographicArea { get; set; }        
        
        [SwaggerSchema("The offset in meters from the geometries. This variable is required if geometries type is point")]
        public double? MaxDistanceFromGeometries { get; set; }
    }
}