using SOS.DataStewardship.Api.Contracts.Enums;
using SOS.Lib.Swagger;
using Swashbuckle.AspNetCore.Annotations;

namespace SOS.DataStewardship.Api.Contracts.Models
{    
    [SwaggerSchema("Geographics filter")]
    public class GeographicsFilter
    {
        [SwaggerExclude]
        public GeometryFilter Area { get; set; }

        [SwaggerSchema("County filter")]
        public County? County { get; set; }

        [SwaggerSchema("Geometry filter")]
        public GeometryFilter Geometry { get; set; }

        [SwaggerSchema("Municipality filter")]
        public Municipality? Municipality { get; set; }

        [SwaggerSchema("Parish filter")]
        public Parish? Parish { get; set; }

        [SwaggerSchema("Province filter")]
        public Province? Province { get; set; }

       
    }
}